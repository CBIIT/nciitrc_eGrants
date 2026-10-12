"""Microsoft Graph transport for the upgraded Router (no Outlook COM)."""

import json
import logging
import re
import time
from dataclasses import dataclass
from html import unescape
from typing import Any, Callable
from urllib.error import HTTPError
from urllib.parse import quote, urlencode, urlparse
from urllib.request import Request, urlopen

from app.config import RouterConfig
from app.models import IncomingEmail, OutgoingEmail
from app.processor import Processor

GRAPH_URL = "https://graph.microsoft.com/v1.0/"
logger = logging.getLogger(__name__)


def _request(method: str, url: str, payload: bytes | None, headers: dict[str, str]) -> Any:
    request = Request(url, data=payload, headers=headers, method=method)
    try:
        with urlopen(request, timeout=30) as response:
            content = response.read()
            return json.loads(content) if content else {}
    except HTTPError as error:
        # Graph errors may contain identifiers or private content; do not log response bodies.
        raise RuntimeError(f"Graph {method} failed (HTTP {error.code})") from error


Transport = Callable[[str, str, bytes | None, dict[str, str]], Any]


@dataclass(frozen=True)
class GraphFolder:
    id: str
    name: str


@dataclass(frozen=True)
class GraphMail:
    id: str
    subject: str
    body: str
    html_body: str
    sender: str


def environment_prefix(environment: str) -> str:
    return "" if environment.lower() == "production" else f"({environment}) "


class GraphEmailService:
    def __init__(self, config: RouterConfig, transport: Transport = _request) -> None:
        for name in ("graph_tenant_id", "graph_client_id", "graph_client_secret", "graph_mailbox"):
            if not getattr(config, name):
                raise ValueError(f"Graph setting {name} is required")
        self._config = config
        self._transport = transport
        self._token: str | None = None

    def connect(self) -> None:
        endpoint = (
            f"https://login.microsoftonline.com/{quote(self._config.graph_tenant_id)}/"
            "oauth2/v2.0/token"
        )
        form = urlencode({
            "grant_type": "client_credentials",
            "client_id": self._config.graph_client_id,
            "client_secret": self._config.graph_client_secret,
            "scope": "https://graph.microsoft.com/.default",
        }).encode("utf-8")
        token = self._transport(
            "POST", endpoint, form,
            {"Content-Type": "application/x-www-form-urlencoded"},
        )
        self._token = str(token["access_token"])

    def _mailbox_path(self) -> str:
        return f"users/{quote(self._config.graph_mailbox, safe='')}"

    def _call(self, method: str, path: str, data: Any = None) -> Any:
        if not self._token:
            raise RuntimeError("Graph connection has not been authenticated")
        url = path if path.startswith("https://") else GRAPH_URL + path
        if urlparse(url).hostname != "graph.microsoft.com":
            raise ValueError("Graph pagination URL must stay on graph.microsoft.com")
        payload = json.dumps(data).encode("utf-8") if data is not None else None
        return self._transport(
            method, url, payload,
            {"Authorization": f"Bearer {self._token}", "Accept": "application/json",
             "Content-Type": "application/json"},
        )

    def _pages(self, path: str) -> list[dict[str, Any]]:
        values: list[dict[str, Any]] = []
        while path:
            page = self._call("GET", path)
            values.extend(page.get("value", []))
            path = page.get("@odata.nextLink", "")
        return values

    def _child(self, parent: GraphFolder | None, name: str) -> GraphFolder:
        path = f"{self._mailbox_path()}/mailFolders"
        if parent:
            path += f"/{quote(parent.id, safe='')}/childFolders"
        matches = self._pages(path + "?$top=200")
        for folder in matches:
            if folder.get("displayName", "").casefold() == name.casefold():
                return GraphFolder(str(folder["id"]), name)
        raise ValueError(f"Mail folder not found: {name}")

    def folder(self, folder_path: str) -> GraphFolder:
        segments = [
            part.strip() for part in folder_path.replace("\\", "/").split("/") if part.strip()
        ]
        if not segments:
            raise ValueError("FolderPaths__dirpathRouter is required")
        known = {
            "inbox": "inbox", "drafts": "drafts", "archive": "archive",
            "deleted items": "deleteditems", "sent items": "sentitems", "junk email": "junkemail",
        }
        first = known.get(segments[0].casefold())
        if first:
            raw = self._call("GET", f"{self._mailbox_path()}/mailFolders/{first}")
            current = GraphFolder(str(raw["id"]), segments[0])
        else:
            current = self._child(None, segments[0])
        for part in segments[1:]:
            current = self._child(current, part)
        return current

    def subfolder(self, folder: GraphFolder, name: str) -> GraphFolder:
        return self._child(folder, name)

    def emails(self, folder: GraphFolder) -> list[GraphMail]:
        path = (f"{self._mailbox_path()}/mailFolders/{quote(folder.id, safe='')}/messages"
                "?$top=200&$select=id,subject,body,bodyPreview,from,receivedDateTime")
        messages: list[GraphMail] = []
        for msg in self._pages(path):
            html = msg.get("body", {}).get("content") or ""
            preview = msg.get("bodyPreview") or ""
            body = preview if preview.strip() else unescape(re.sub(r"<.*?>", "", html))
            sender = msg.get("from") or {}
            address = (sender.get("emailAddress") or {}).get("address") or ""
            messages.append(GraphMail(str(msg["id"]), msg.get("subject") or "", body,
                                      html, address))
        return messages

    def send_forward(self, original: GraphMail, output: OutgoingEmail) -> None:
        path = f"{self._mailbox_path()}/messages/{quote(original.id, safe='')}"
        draft = self._call("POST", path + "/createForward", {})
        draft_id = quote(str(draft["id"]), safe="")
        # The source GraphEmailService starts a forward with 'FW: ', unless
        # the routing branch replaces the subject with its metadata prefix.
        subject = (
            output.subject if output.subject != original.subject else "FW: " + original.subject
        )
        body = original.html_body if original.html_body.strip() else original.body
        self._call("PATCH", f"{self._mailbox_path()}/messages/{draft_id}", {
            "subject": environment_prefix(self._config.environment) + subject,
            "toRecipients": [{"emailAddress": {"address": address}}
                             for address in output.recipients],
            "body": {"contentType": "HTML", "content": body},
        })
        self._call("POST", f"{self._mailbox_path()}/messages/{draft_id}/send", {})

    def send_email(self, recipients: str, subject: str, body: str) -> None:
        self._call("POST", f"{self._mailbox_path()}/sendMail", {
            "message": {
                "subject": subject, "body": {"contentType": "HTML", "content": body},
                "toRecipients": [{"emailAddress": {"address": address.strip()}}
                                 for address in recipients.split(";") if address.strip()],
            },
            "saveToSentItems": True,
        })

    def move(self, original: GraphMail, destination: GraphFolder) -> None:
        self._call("POST", f"{self._mailbox_path()}/messages/{quote(original.id, safe='')}/move",
                   {"destinationId": destination.id})


def process_graph_folder(
    service: GraphEmailService, processor: Processor, config: RouterConfig, debug: str,
    *, dry_run: bool = True, sleep: Callable[[float], None] = time.sleep,
) -> int:
    """Walk upgraded Router folders. Never send or move in dry-run mode."""
    service.connect()
    folder = service.folder(config.folder_path)
    archive = service.subfolder(folder, "Old emails")
    emails = service.emails(folder)
    count = 0
    for mail in emails:
        if count >= 50:
            logger.warning("50-item safety limit reached; remaining mail left in place")
            if not dry_run:
                service.send_email(
                    config.legacy_error_recipient,
                    environment_prefix(config.environment) + "Router processed 50 mail items",
                    "Check whether there is duplicate item processing.",
                )
            break
        try:
            # Source GetSenderId returns the address portion before @.
            sender_id = mail.sender.split("@", 1)[0]
            result = processor.handle_single_email(
                IncomingEmail(mail.subject, mail.body, sender_id), debug
            )
            logger.info("Route %s: %s -> %s", result.route_name, mail.subject,
                        [output.recipients for output in result.outgoing_messages])
            if not dry_run:
                for output in result.outgoing_messages:
                    service.send_forward(mail, output)
                service.move(mail, archive)
                sleep(max(config.routing_break_duration, 0) / 1000)
            count += 1
        except Exception:
            logger.exception("Failed to process message; leaving it in source folder")
            if not dry_run:
                try:
                    service.send_email(
                        config.legacy_error_recipient,
                        environment_prefix(config.environment)
                        + f"Error Occured! PROD eMailRouter vbs  >>(Subj: {mail.subject} )",
                        "Router failed to process a message. Inspect logs and source folder.",
                    )
                except Exception:
                    logger.exception("Failed to send admin notification")
    return count
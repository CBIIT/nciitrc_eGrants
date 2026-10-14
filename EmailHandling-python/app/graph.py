"""Microsoft Graph transport for the upgraded Router (no Outlook COM)."""

import json
import logging
import re
import time
from base64 import b64decode
from binascii import Error as Base64Error
from dataclasses import dataclass
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
    received_at: str = ""
    sender_name: str = ""
    to_addresses: tuple[str, ...] = ()


@dataclass(frozen=True)
class GraphAttachment:
    name: str
    content: bytes


def environment_prefix(environment: str) -> str:
    if not environment.strip() or environment.lower() == "production":
        return ""
    return f"({environment}) "


def _config_prefix(config: RouterConfig) -> str:
    # Settings overlay defaults to Development, but unset DOTNET_ENVIRONMENT
    # does not prefix outgoing subjects in Processor.Send / Program.Main.
    environment = (
        config.outgoing_environment
        if config.outgoing_environment is not None
        else config.environment
    )
    return environment_prefix(environment)


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
        form = urlencode(
            {
                "grant_type": "client_credentials",
                "client_id": self._config.graph_client_id,
                "client_secret": self._config.graph_client_secret,
                "scope": "https://graph.microsoft.com/.default",
            }
        ).encode("utf-8")
        token = self._transport(
            "POST",
            endpoint,
            form,
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
        if not urlparse(url).path.startswith("/v1.0/" + self._mailbox_path() + "/"):
            raise ValueError("Graph request must stay within configured mailbox")
        payload = json.dumps(data).encode("utf-8") if data is not None else None
        return self._transport(
            method,
            url,
            payload,
            {
                "Authorization": f"Bearer {self._token}",
                "Accept": "application/json",
                "Content-Type": "application/json",
            },
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
        next_page = path + "?$top=200"
        while next_page:
            page = self._call("GET", next_page)
            for folder in page.get("value", []):
                if folder.get("displayName", "").casefold() == name.casefold():
                    return GraphFolder(str(folder["id"]), name)
            next_page = page.get("@odata.nextLink", "")
        raise ValueError(f"Mail folder not found: {name}")

    def folder(self, folder_path: str) -> GraphFolder:
        segments = [
            part.strip() for part in folder_path.replace("\\", "/").split("/") if part.strip()
        ]
        if not segments:
            raise ValueError("FolderPaths__dirpathRouter is required")
        known = {
            "inbox": "inbox",
            "drafts": "drafts",
            "archive": "archive",
            "deleted items": "deleteditems",
            "sent items": "sentitems",
            "junk email": "junkemail",
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
        path = (
            f"{self._mailbox_path()}/mailFolders/{quote(folder.id, safe='')}/messages"
            "?$top=200&$select=id,subject,body,bodyPreview,from,receivedDateTime,toRecipients"
        )
        messages: list[GraphMail] = []
        for msg in self._pages(path):
            html = msg.get("body", {}).get("content") or ""
            preview = msg.get("bodyPreview") or ""
            body = preview if preview.strip() else re.sub(r"<.*?>", "", html)
            sender = msg.get("from") or {}
            address = (sender.get("emailAddress") or {}).get("address") or ""
            messages.append(
                GraphMail(
                    str(msg["id"]),
                    msg.get("subject") or "",
                    body,
                    html,
                    address,
                    msg.get("receivedDateTime") or "",
                    (sender.get("emailAddress") or {}).get("name") or "",
                    tuple(
                        item.get("emailAddress", {}).get("address", "")
                        for item in (msg.get("toRecipients") or [])
                    ),
                )
            )
        return messages

    def attachments(self, mail: GraphMail) -> list[GraphAttachment]:
        """Read file attachments only; fail closed for item/reference or incomplete content."""
        path = f"{self._mailbox_path()}/messages/{quote(mail.id, safe='')}/attachments?$top=200"
        attachments: list[GraphAttachment] = []
        for item in self._pages(path):
            if item.get("@odata.type") != "#microsoft.graph.fileAttachment":
                raise ValueError("Non-file Graph attachment requires a separate migration strategy")
            if "contentBytes" not in item:
                raise ValueError("Graph file attachment lacks contentBytes")
            try:
                content = b64decode(item["contentBytes"], validate=True)
            except (ValueError, Base64Error) as error:
                raise ValueError("Invalid Graph file attachment content") from error
            attachments.append(GraphAttachment(str(item.get("name") or ""), content))
        return attachments

    def send_forward(self, original: GraphMail, output: OutgoingEmail) -> None:
        path = f"{self._mailbox_path()}/messages/{quote(original.id, safe='')}"
        draft = self._call("POST", path + "/createForward", {})
        draft_id = quote(str(draft["id"]), safe="")
        # The source GraphEmailService starts a forward with 'FW: ', unless
        # the routing branch replaces the subject with its metadata prefix.
        subject = (
            output.subject
            if output.preserve_subject or output.subject != original.subject
            else "FW: " + original.subject
        )
        body = original.html_body if original.html_body.strip() else original.body
        self._call(
            "PATCH",
            f"{self._mailbox_path()}/messages/{draft_id}",
            {
                "subject": _config_prefix(self._config) + subject,
                "toRecipients": [
                    {"emailAddress": {"address": address}}
                    for address in (
                        [self._config.test_recipient]
                        if self._config.test_recipient
                        else output.recipients
                    )
                ],
                "body": {"contentType": "HTML", "content": body},
            },
        )
        self._call("POST", f"{self._mailbox_path()}/messages/{draft_id}/send", {})

    def send_email(self, recipients: str, subject: str, body: str) -> None:
        addresses = (
            [self._config.test_recipient]
            if self._config.test_recipient
            else [address.strip() for address in recipients.split(";") if address.strip()]
        )
        self._call(
            "POST",
            f"{self._mailbox_path()}/sendMail",
            {
                "message": {
                    "subject": subject,
                    "body": {"contentType": "HTML", "content": body},
                    "toRecipients": [
                        {"emailAddress": {"address": address}} for address in addresses
                    ],
                },
                "saveToSentItems": True,
            },
        )

    def move(self, original: GraphMail, destination: GraphFolder) -> None:
        self._call(
            "POST",
            f"{self._mailbox_path()}/messages/{quote(original.id, safe='')}/move",
            {"destinationId": destination.id},
        )


def process_graph_folder(
    service: GraphEmailService,
    processor: Processor,
    config: RouterConfig,
    debug: str,
    *,
    dry_run: bool = True,
    sleep: Callable[[float], None] = time.sleep,
) -> int:
    """Walk upgraded Router folders. Never send or move in dry-run mode."""
    service.connect()
    folder = service.folder(config.folder_path)
    archive = service.subfolder(folder, "Old emails")
    emails = service.emails(folder)
    count = 0
    for mail in emails:
        try:
            result = processor.handle_single_email(
                IncomingEmail(mail.subject, mail.body, mail.sender), debug
            )
            logger.info("Route %s: %s forward(s)", result.route_name, len(result.outgoing_messages))
            if not dry_run:
                for output in result.outgoing_messages:
                    service.send_forward(mail, output)
                try:
                    service.move(mail, archive)
                except Exception:
                    logger.error("Failed to archive message; leaving it in source folder")
                    try:
                        service.send_email(
                            config.error_recipients,
                            _config_prefix(config) + "Failed to move an item to old.",
                            "Failed to move an item to Old emails. Inspect logs.",
                        )
                    except Exception:
                        logger.error("Failed to send archive-failure notification")
                    # A successfully forwarded message may be retried: leave it for review.
                    continue
                sleep(max(config.routing_break_duration, 0) / 1000)
            count += 1
        except Exception:
            logger.error("Failed to process message; leaving it in source folder")
            if not dry_run:
                try:
                    service.send_forward(
                        mail,
                        OutgoingEmail(
                            subject=(
                                f"Error Occured! PROD eMailRouter vbs  >>(Subj: {mail.subject} )"
                            ),
                            body=mail.body,
                            recipients=[config.legacy_error_recipient] * 2,
                        ),
                    )
                except Exception:
                    logger.error("Failed to send admin notification")
            continue
        if count >= 50:
            logger.warning("50-item safety limit reached; remaining mail left in place")
            if not dry_run:
                try:
                    service.send_email(
                        config.legacy_error_recipient,
                        _config_prefix(config)
                        + "Warning! PROD eMailRouter vbs has processed 50 mail items "
                        "in one instance!",
                        "Hello Admin, 50 items have been processed in one instance "
                        "and the application is now exiting. Please check whether "
                        "there is duplicate items processing.",
                    )
                except Exception:
                    logger.error("Failed to send 50-item warning")
            break
    return count

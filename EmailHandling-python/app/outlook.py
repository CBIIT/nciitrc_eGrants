"""Windows Outlook COM public-folder processing. No COM import until live execution."""

import logging
import time
from typing import Any

from app.config import RouterConfig
from app.models import IncomingEmail, RouteResult
from app.processor import Processor

logger = logging.getLogger(__name__)


def environment_prefix(environment: str) -> str:
    return f"({environment}) " if environment.lower() != "production" else ""


def sender_id(item: Any) -> str:
    address = str(item.SenderEmailAddress or "")
    if not address:
        return ""
    kind = str(item.SenderEmailType or "").lower()
    if kind == "smtp":
        return address
    if kind == "ex" and item.Sender:
        user = item.Sender.GetExchangeUser()
        return str(user.Alias or "") if user else ""
    return ""


def notify(app: Any, recipients: str, subject: str, body: str) -> None:
    message = app.CreateItem(0)
    message.To = recipients
    message.Subject = subject
    message.Body = body
    message.Send()


def forward_result(item: Any, result: RouteResult, environment: str) -> None:
    for output in result.outgoing_messages:
        forwarded = item.Forward()  # Preserve original attachments and Outlook MIME content.
        forwarded.Subject = environment_prefix(environment) + output.subject
        for recipient in output.recipients:
            forwarded.Recipients.Add(recipient)
        forwarded.Send()


def process_folder(
    app: Any,
    processor: Processor,
    config: RouterConfig,
    debug: str,
    *,
    dry_run: bool = True,
    sleep: Any = time.sleep,
) -> int:
    """Process a snapshot, never archive during dry-run or after a routing/send failure."""
    if not config.folder_path:
        raise ValueError("FolderPaths__dirpathRouter is required")
    namespace = app.GetNamespace("MAPI")
    namespace.Logon("", "", False, True)
    components = [part for part in config.folder_path.split("\\") if part.strip()]
    folder = namespace.Folders[components[0]]
    for component in components[1:]:
        folder = folder.Folders[component]
    archive = folder.Folders["Old emails"] if not dry_run else None
    items: list[Any] = []
    for item in folder.Items:
        try:
            if int(item.Class) == 43:
                items.append(item)
        except (AttributeError, TypeError, ValueError):
            continue

    count = 0
    for item in items:
        if count >= 50:
            logger.warning("50-item safety limit reached; remaining messages left in place")
            if not dry_run:
                notify(
                    app,
                    config.legacy_error_recipient,
                    environment_prefix(config.environment) + "Router processed 50 mail items",
                    "Check for duplicate item processing.",
                )
            break
        try:
            email = IncomingEmail(
                subject=str(item.Subject or ""), body=str(item.Body or ""), sender=sender_id(item)
            )
            result = processor.handle_single_email(email, debug=debug)
            logger.info(
                "Route %s: %s -> %s",
                result.route_name,
                email.subject,
                [message.recipients for message in result.outgoing_messages],
            )
            if not dry_run:
                forward_result(item, result, config.environment)
                item.Move(archive)
                sleep(max(config.routing_break_duration, 0) / 1000)
            count += 1
        except Exception:
            logger.exception("Failed to process mail item; leaving it in the source folder")
            if not dry_run:
                try:
                    notify(
                        app,
                        config.error_recipients,
                        environment_prefix(config.environment) + "Router processing failure",
                        "A mail item failed to process. Check router logs and source folder.",
                    )
                except Exception:
                    logger.exception("Failed to notify administrators")
    return count


def connect_outlook() -> Any:
    import win32com.client

    try:
        return win32com.client.GetActiveObject("Outlook.Application")
    except Exception:
        return win32com.client.Dispatch("Outlook.Application")

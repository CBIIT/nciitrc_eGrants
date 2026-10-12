"""SMTP global-failure notification used by the upgraded .NET Router."""

import smtplib
from email.message import EmailMessage
from email.utils import formataddr

from app.config import RouterConfig
from app.graph import environment_prefix


def send_global_error(config: RouterConfig, details: str) -> None:
    if not config.smtp_host or not config.smtp_from_address:
        raise ValueError("SMTP host and from address must be configured")
    message = EmailMessage()
    message["From"] = formataddr((config.smtp_from_name, config.smtp_from_address))
    recipients = [item.strip() for item in config.error_recipients.split(";") if item.strip()]
    message["To"] = ", ".join(recipients)
    message["Subject"] = environment_prefix(config.environment) + "Global level email failure."
    message.set_content(details, subtype="html")
    with smtplib.SMTP(config.smtp_host, config.smtp_port, timeout=30) as client:
        client.send_message(message)
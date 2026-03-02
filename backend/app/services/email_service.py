"""Email service -- SMTP-based email sending (replaces Outlook COM from VBScript)."""

import logging
import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

from ..config import settings

logger = logging.getLogger(__name__)


def send_email(
    to: str | list[str],
    subject: str,
    body: str,
    cc: str | list[str] | None = None,
    html: bool = True,
) -> bool:
    """Send an email via SMTP.

    In dev mode with send_mail=False, logs the email instead of sending.
    When mail_override_address is set, redirects all emails there.
    """
    if isinstance(to, str):
        to = [to]
    if isinstance(cc, str):
        cc = [cc]
    if cc is None:
        cc = []

    # Override recipients in non-prod
    if settings.mail_override_address:
        original_to = ", ".join(to)
        to = [settings.mail_override_address]
        cc = []
        subject = f"[OVERRIDE from {original_to}] {subject}"

    if not settings.send_mail:
        logger.info(
            "Email suppressed (send_mail=False): to=%s, subject=%s",
            ", ".join(to),
            subject,
        )
        return True

    msg = MIMEMultipart("alternative")
    msg["From"] = settings.smtp_from
    msg["To"] = ", ".join(to)
    if cc:
        msg["Cc"] = ", ".join(cc)
    msg["Subject"] = subject

    content_type = "html" if html else "plain"
    msg.attach(MIMEText(body, content_type))

    try:
        with smtplib.SMTP(settings.smtp_host) as server:
            all_recipients = to + cc
            server.sendmail(settings.smtp_from, all_recipients, msg.as_string())
        logger.info("Email sent to %s: %s", ", ".join(to), subject)
        return True
    except Exception:
        logger.exception("Failed to send email to %s", ", ".join(to))
        return False

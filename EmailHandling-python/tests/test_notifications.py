from email.message import EmailMessage
from typing import Any

import pytest

from app.config import RouterConfig
from app.notifications import send_global_error


def test_global_notification_uses_smtp_config(monkeypatch: pytest.MonkeyPatch) -> None:
    delivered: list[EmailMessage] = []

    class FakeSmtp:
        def __init__(self, host: str, port: int, timeout: int) -> None:
            assert (host, port, timeout) == ("relay.example.org", 25, 30)

        def __enter__(self) -> "FakeSmtp":
            return self

        def __exit__(self, *args: Any) -> None:
            pass

        def send_message(self, message: EmailMessage) -> None:
            delivered.append(message)

    monkeypatch.setattr("app.notifications.smtplib.SMTP", FakeSmtp)
    config = RouterConfig(
        smtp_host="relay.example.org", smtp_from_address="router@example.org",
        error_recipients="admin@example.org;backup@example.org", environment="Development",
    )
    send_global_error(config, "Router failed")
    assert delivered[0]["Subject"] == "(Development) Global level email failure."
    assert delivered[0]["To"] == "admin@example.org, backup@example.org"
    assert delivered[0]["From"] == "eGrants Router <router@example.org>"
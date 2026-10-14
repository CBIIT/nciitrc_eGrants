import json
from typing import Any

import pytest

from app.config import RouterConfig
from app.graph import GraphEmailService, GraphFolder, GraphMail, process_graph_folder
from app.models import IncomingEmail
from app.processor import Processor


class MockGraph:
    def __init__(self) -> None:
        self.calls: list[tuple[str, str, Any]] = []

    def __call__(self, method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        payload = json.loads(data) if data and "graph.microsoft.com" in url else None
        self.calls.append((method, url, payload))
        if url.endswith("/oauth2/v2.0/token"):
            assert b"client_secret=secret" in (data or b"")
            return {"access_token": "mock-token"}
        assert headers["Authorization"] == "Bearer mock-token"
        if url.endswith("/mailFolders/inbox"):
            return {"id": "inbox-id"}
        if "childFolders" in url:
            return {"value": [{"id": "archive-id", "displayName": "Old emails"}]}
        if "?$top=200&$select=" in url:
            return {
                "value": [
                    {
                        "id": "message-id",
                        "subject": "Prior Approval: Grant",
                        "bodyPreview": "Body",
                        "body": {"content": "<p>Body</p>"},
                        "from": {"emailAddress": {"address": "sender@example.org"}},
                    }
                ]
            }
        if url.endswith("/createForward"):
            return {"id": "draft-id"}
        return {}


def settings() -> RouterConfig:
    return RouterConfig(
        graph_tenant_id="tenant",
        graph_client_id="client",
        graph_client_secret="secret",
        graph_mailbox="mailbox@example.org",
        folder_path="INBOX",
        environment="Development",
        routing_break_duration=0,
    )


def test_graph_dry_run_never_posts_mail() -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    assert process_graph_folder(service, Processor(config), config, "n", dry_run=True) == 1
    writes = [
        url
        for method, url, _ in transport.calls
        if method != "GET" and "graph.microsoft.com" in url
    ]

    assert writes == []


def test_graph_forward_updates_draft_then_sends_then_moves() -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    count = process_graph_folder(
        service, Processor(config), config, "n", dry_run=False, sleep=lambda _: None
    )
    assert count == 1
    writes = [
        (method, url.rsplit("/", 1)[-1], body)
        for method, url, body in transport.calls
        if "graph.microsoft.com" in url and method != "GET"
    ]
    assert [(method, endpoint) for method, endpoint, _ in writes] == [
        ("POST", "createForward"),
        ("PATCH", "draft-id"),
        ("POST", "send"),
        ("POST", "move"),
    ]
    assert writes[1][2]["subject"] == "(Development) FW: Prior Approval: Grant"
    assert writes[1][2]["body"]["content"] == "<p>Body</p>"
    assert writes[1][2]["toRecipients"] == [
        {"emailAddress": {"address": config.nci_grants_post_award_email}}
    ]
    assert writes[3][2] == {"destinationId": "archive-id"}


def test_local_recipient_overrides_graph_forward_and_alert() -> None:
    config = settings()
    config.test_recipient = "daryl.dehuff@nih.gov"
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    service.connect()
    message = GraphMail("id", "Prior Approval: Grant", "body", "", "sender@example.org")
    output = Processor(config).handle_single_email(IncomingEmail(message.subject, "body"), "n")
    service.send_forward(message, output.outgoing_messages[0])
    service.send_email("somebody@example.org;other@example.org", "Alert", "Details")
    writes = [
        payload
        for method, url, payload in transport.calls
        if method == "PATCH" or url.endswith("/sendMail")
    ]
    expected = [{"emailAddress": {"address": "daryl.dehuff@nih.gov"}}]
    assert writes[0]["toRecipients"] == expected
    assert writes[1]["message"]["toRecipients"] == expected


def test_graph_enriched_subject_does_not_get_fw_prefix() -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    service.connect()
    mail = GraphMail("id", "JIT Request for Grant", "body", "", "sender@example.org")
    output = Processor(config).handle_single_email(IncomingEmail(mail.subject, mail.body), "n")
    service.send_forward(mail, output.outgoing_messages[0])
    patch = next(body for method, url, body in transport.calls if method == "PATCH")
    assert patch["subject"] == (
        "(Development) category=JIT Info, sub=Reminder, extract=1, JIT Request for Grant"
    )


def test_change_of_institution_does_not_get_fw_prefix() -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    service.connect()
    subject = "Change of Institution request for Grant"
    mail = GraphMail("id", subject, "body", "", "sender@example.org")
    output = Processor(config).handle_single_email(IncomingEmail(subject, "body"), "n")
    service.send_forward(mail, output.outgoing_messages[0])
    patch = next(body for method, url, body in transport.calls if method == "PATCH")
    assert patch["subject"] == "(Development) " + subject


def test_graph_never_follows_pagination_to_another_host() -> None:
    config = settings()

    def transport(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "token"}
        return {"value": [], "@odata.nextLink": "https://untrusted.example/steal"}

    service = GraphEmailService(config, transport)
    service.connect()
    with pytest.raises(ValueError, match="pagination URL"):
        service.folder("Custom Root")


def test_graph_never_follows_pagination_to_another_mailbox() -> None:
    config = settings()

    def transport(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "token"}
        return {
            "value": [],
            "@odata.nextLink": "https://graph.microsoft.com/v1.0/users/other/mailFolders?$top=200",
        }

    service = GraphEmailService(config, transport)
    service.connect()
    with pytest.raises(ValueError, match="configured mailbox"):
        service.folder("Custom Root")


def test_graph_body_fallback_matches_source_html_stripping() -> None:
    config = settings()

    def transport(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "token"}
        return {"value": [{"id": "m", "body": {"content": "<p>A &amp; B</p>"}}]}

    service = GraphEmailService(config, transport)
    service.connect()
    assert service.emails(GraphFolder("folder", "folder")).pop().body == "A &amp; B"


def test_failed_routing_forwards_original_to_admin_without_archiving(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    processor = Processor(config)

    def fail(*args: Any) -> Any:
        raise ValueError("synthetic routing failure")

    monkeypatch.setattr(processor, "handle_single_email", fail)
    assert (
        process_graph_folder(service, processor, config, "n", dry_run=False, sleep=lambda _: None)
        == 0
    )
    writes = [
        (method, url, payload)
        for method, url, payload in transport.calls
        if "graph.microsoft.com" in url and method != "GET"
    ]
    assert [url.rsplit("/", 1)[-1] for _, url, _ in writes] == ["createForward", "draft-id", "send"]
    assert (
        writes[1][2]["toRecipients"]
        == [{"emailAddress": {"address": config.legacy_error_recipient}}] * 2
    )


def test_archive_failure_notifies_error_recipients_without_retrying_send(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)

    def fail_move(mail: GraphMail, folder: GraphFolder) -> None:
        raise ValueError("synthetic move failure")

    monkeypatch.setattr(service, "move", fail_move)
    assert (
        process_graph_folder(
            service, Processor(config), config, "n", dry_run=False, sleep=lambda _: None
        )
        == 0
    )
    writes = [
        (method, url, body)
        for method, url, body in transport.calls
        if "graph.microsoft.com" in url and method != "GET"
    ]
    assert [url.rsplit("/", 1)[-1] for _, url, _ in writes] == [
        "createForward",
        "draft-id",
        "send",
        "sendMail",
    ]
    assert writes[-1][2]["message"]["subject"].endswith("Failed to move an item to old.")


def test_dry_run_hard_cap_at_fifty_without_mail_writes(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    config = settings()
    transport = MockGraph()
    service = GraphEmailService(config, transport)
    messages = [
        GraphMail(str(n), "Prior Approval: Synthetic", "", "", "sender@example.org")
        for n in range(51)
    ]
    monkeypatch.setattr(service, "emails", lambda folder: messages)
    assert process_graph_folder(service, Processor(config), config, "n", dry_run=True) == 50
    assert not [
        url
        for method, url, _ in transport.calls
        if method != "GET" and "graph.microsoft.com" in url
    ]

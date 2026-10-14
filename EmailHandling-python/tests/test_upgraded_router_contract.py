"""Offline regression evidence from upgraded Router/Processor.cs and GraphEmailService.cs.

Source expectations below are independent of the Python router's return values.
Explicit xfails are unresolved source-parity gaps, not desired policy changes.
"""

import json
from pathlib import Path
from typing import Any

import pytest

from app.config import RouterConfig, load_config
from app.database import SqlGrantLookup
from app.graph import GraphEmailService, GraphFolder, GraphMail, process_graph_folder
from app.models import IncomingEmail
from app.notifications import send_global_error
from app.processor import Processor


def settings(**overrides: Any) -> RouterConfig:
    config = RouterConfig(
        graph_tenant_id="tenant",
        graph_client_id="client",
        graph_client_secret="synthetic-secret",
        graph_mailbox="router@example.invalid",
        folder_path="INBOX",
        outgoing_environment="QA",
        routing_break_duration=0,
        efile_email="file@example.invalid",
        egrants_dev_email="dev@example.invalid",
        egrants_test_email="test@example.invalid",
        egrants_stage_email="stage@example.invalid",
        debug_email="debug@example.invalid",
        nci_grants_post_award_email="post@example.invalid",
        public_access_recipients="public1@example.invalid;public2@example.invalid",
        legacy_error_recipient="admin@example.invalid",
        error_recipients="ops@example.invalid",
    )
    for key, value in overrides.items():
        setattr(config, key, value)
    return config


class Transport:
    """All OAuth, Graph reads and Graph writes are intercepted; no socket is opened."""

    def __init__(self, messages: list[dict[str, Any]] | None = None) -> None:
        self.messages = messages if messages is not None else [self.message(0)]
        self.calls: list[tuple[str, str, Any]] = []
        self.fail: str | None = None
        self.draft = 0

    @staticmethod
    def message(index: int, subject: str = "Prior Approval: Synthetic") -> dict[str, Any]:
        return {
            "id": f"m{index}",
            "subject": subject,
            "bodyPreview": "plain text",
            "body": {"content": "<p>synthetic HTML</p>"},
            "from": {"emailAddress": {"address": "agent@example.invalid"}},
        }

    def __call__(self, method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            assert method == "POST"
            assert b"scope=https%3A%2F%2Fgraph.microsoft.com%2F.default" in (data or b"")
            assert "tenant/oauth2/v2.0/token" in url
            return {"access_token": "fake-token"}
        assert headers["Authorization"] == "Bearer fake-token"
        payload = json.loads(data) if data else None
        self.calls.append((method, url, payload))
        if self.fail and url.endswith(self.fail):
            raise RuntimeError("synthetic transport failure")
        if url.endswith("/mailFolders/inbox"):
            return {"id": "inbox-id"}
        if "/childFolders" in url:
            return {"value": [{"id": "archive-id", "displayName": "Old emails"}]}
        if "/messages?$top=200" in url:
            return {"value": self.messages}
        if url.endswith("/createForward"):
            self.draft += 1
            return {"id": f"draft-{self.draft}"}
        return {}

    def writes(self) -> list[tuple[str, str, Any]]:
        return [(method, url, body) for method, url, body in self.calls if method != "GET"]


class Lookup:
    def __init__(self) -> None:
        self.inputs: list[str] = []

    def appl_id(self, text: str) -> str:
        self.inputs.append(text)
        return "42"

    def officer_emails(self, appl_id: str) -> tuple[str, str]:
        raise AssertionError("not an FCOI message")


@pytest.mark.parametrize(
    "debug,recipients",
    [
        (
            "n",
            [
                "file@example.invalid",
                "dev@example.invalid",
                "test@example.invalid",
                "stage@example.invalid",
            ],
        ),
        ("y", ["debug@example.invalid", "dev@example.invalid"]),
    ],
)
def test_sender_alias_not_domain_and_sql_parameter(debug: str, recipients: list[str]) -> None:
    config = settings()
    lookup = Lookup()
    subject = "Notice: 5R01CA000001 - 01"
    result = Processor(config, lookup).handle_single_email(
        IncomingEmail(subject, "body", "PuBlIcTeam@example.invalid"), debug
    )
    assert lookup.inputs == ["5R01CA000001"]
    assert [(m.subject, m.recipients) for m in result.outgoing_messages] == [
        ("category=PublicAccess, sub=, applid=42, extract=1, " + subject, recipients)
    ]
    lookup = Lookup()
    assert (
        not Processor(config, lookup)
        .handle_single_email(IncomingEmail(subject, "body", "agent@public.example.invalid"), debug)
        .outgoing_messages
    )
    assert lookup.inputs == []
    assert (
        not Processor(config, lookup)
        .handle_single_email(IncomingEmail("", "body", "public@example.invalid"), debug)
        .outgoing_messages
    )
    assert lookup.inputs == []


@pytest.mark.parametrize("environment,expected", [(None, ""), ("Production", ""), ("QA", "(QA) ")])
def test_unset_environment_loads_development_but_does_not_prefix(
    tmp_path: Path,
    monkeypatch: pytest.MonkeyPatch,
    environment: str | None,
    expected: str,
) -> None:
    (tmp_path / "appsettings.json").write_text(
        json.dumps(
            {
                "Graph": {
                    "TenantId": "tenant",
                    "ClientId": "client",
                    "ClientSecret": "synthetic-secret",
                    "Mailbox": "router@example.invalid",
                },
                "EmailRecipients": {"NCIGrantsPostAwardEmail": "base@example.invalid"},
            }
        ),
        encoding="utf-8",
    )
    (tmp_path / "appsettings.Development.json").write_text(
        json.dumps(
            {
                "EmailRecipients": {"NCIGrantsPostAwardEmail": "overlay@example.invalid"},
            }
        ),
        encoding="utf-8",
    )
    if environment is None:
        monkeypatch.delenv("DOTNET_ENVIRONMENT", raising=False)
    else:
        monkeypatch.setenv("DOTNET_ENVIRONMENT", environment)
    config = load_config(tmp_path)
    assert config.nci_grants_post_award_email == (
        "overlay@example.invalid" if environment is None else "base@example.invalid"
    )
    transport = Transport()
    service = GraphEmailService(config, transport)
    service.connect()
    mail = GraphMail("m", "Prior Approval: Synthetic", "text", "", "agent@example.invalid")
    output = Processor(config).handle_single_email(IncomingEmail(mail.subject, mail.body), "n")
    service.send_forward(mail, output.outgoing_messages[0])
    patch = next(body for method, _, body in transport.calls if method == "PATCH")
    assert patch["subject"] == expected + "FW: Prior Approval: Synthetic"
    assert patch["toRecipients"] == [
        {"emailAddress": {"address": config.nci_grants_post_award_email}}
    ]


@pytest.mark.parametrize("debug", ["n", "y"])
@pytest.mark.parametrize(
    "r15,suffix",
    [(False, ""), (True, "category=eRANotification, sub=Late Progress Report, extract=1, ")],
)
def test_late_progress_exact_patch_subject_even_when_blank(
    debug: str,
    r15: bool,
    suffix: str,
) -> None:
    config = settings()
    transport = Transport()
    service = GraphEmailService(config, transport)
    service.connect()
    subject = "NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant X"
    if r15:
        subject += " R15 123"
    mail = GraphMail("m", subject, "text", "<p>HTML</p>", "agent@example.invalid")
    output = Processor(config).handle_single_email(IncomingEmail(subject, mail.body), debug)
    service.send_forward(mail, output.outgoing_messages[0])
    patch = next(body for method, _, body in transport.calls if method == "PATCH")
    assert patch["subject"] == "(QA) " + (suffix + subject if r15 else "")
    assert patch["body"] == {"contentType": "HTML", "content": "<p>HTML</p>"}
    assert [r["emailAddress"]["address"] for r in patch["toRecipients"]] == (
        [
            "file@example.invalid",
            "dev@example.invalid",
            "test@example.invalid",
            "stage@example.invalid",
        ]
        if debug == "n"
        else ["debug@example.invalid", "dev@example.invalid"]
    )


@pytest.mark.parametrize(
    "html,text,expected",
    [
        ("<b>synthetic</b>", "preview", "<b>synthetic</b>"),
        ("  ", "plain preview", "plain preview"),
        ("", "", ""),
    ],
)
def test_forward_body_uses_nonblank_html_else_text(
    html: str,
    text: str,
    expected: str,
) -> None:
    config = settings()
    transport = Transport()
    service = GraphEmailService(config, transport)
    service.connect()
    mail = GraphMail("m", "Prior Approval: Synthetic", text, html, "agent@example.invalid")
    output = Processor(config).handle_single_email(IncomingEmail(mail.subject, text), "n")
    service.send_forward(mail, output.outgoing_messages[0])
    assert [method for method, _, _ in transport.writes()] == ["POST", "PATCH", "POST"]
    assert transport.writes()[1][2]["body"] == {"contentType": "HTML", "content": expected}


def test_first_matching_folder_page_wins_and_missing_child_raises() -> None:
    config = settings()
    seen: list[str] = []
    first = "https://graph.microsoft.com/v1.0/users/router%40example.invalid/mailFolders?$top=200"
    second = first + "&$skiptoken=next"

    def transport(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "fake-token"}
        seen.append(url)
        if url == first:
            return {"value": [{"id": "wrong", "displayName": "Other"}], "@odata.nextLink": second}
        if url == second:
            return {
                "value": [
                    {"id": "first-match", "displayName": "rOuTeR"},
                    {"id": "later-match", "displayName": "Router"},
                ]
            }
        return {"value": []}

    service = GraphEmailService(config, transport)
    service.connect()
    assert service.folder("Router").id == "first-match"
    assert seen == [first, second]
    with pytest.raises(ValueError, match="not found"):
        service.folder("Missing")


def test_message_pagination_loads_all_201_and_preserves_sender_and_preview() -> None:
    config = settings()
    page_one = [Transport.message(i) for i in range(200)]
    page_two = [Transport.message(200)]
    page_two[0]["bodyPreview"] = " "
    next_page = (
        "https://graph.microsoft.com/v1.0/users/router%40example.invalid/"
        "mailFolders/folder/messages?$skiptoken=synthetic"
    )
    seen: list[str] = []

    def fake(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "fake-token"}
        seen.append(url)
        return (
            {"value": page_two}
            if url == next_page
            else {"value": page_one, "@odata.nextLink": next_page}
        )

    service = GraphEmailService(config, fake)
    service.connect()
    emails = service.emails(GraphFolder("folder", "Folder"))
    assert [mail.id for mail in emails] == [f"m{i}" for i in range(201)]
    assert emails[0].body == "plain text"
    assert emails[-1].body == "synthetic HTML"
    assert emails[-1].html_body == "<p>synthetic HTML</p>"
    assert emails[-1].sender == "agent@example.invalid"
    assert len(seen) == 2 and seen[1] == next_page


@pytest.mark.parametrize("debug", ["n", "y"])
def test_esnap_two_separate_forward_drafts_before_single_archive(debug: str) -> None:
    subject = "eSNAP Received at NIH submitted to NIH with a Non-Compliance: Synthetic"
    config = settings()
    transport = Transport([Transport.message(0, subject)])
    count = process_graph_folder(
        GraphEmailService(config, transport),
        Processor(config),
        config,
        debug,
        dry_run=False,
        sleep=lambda _: None,
    )
    writes = transport.writes()
    assert count == 1
    assert [url.rsplit("/", 1)[-1] for _, url, _ in writes] == [
        "createForward",
        "draft-1",
        "send",
        "createForward",
        "draft-2",
        "send",
        "move",
    ]
    first, second = writes[1][2], writes[4][2]
    assert [first["subject"], second["subject"]] == [
        "(QA) category=eRANotification, sub=RPPR Non-Compliance, extract=1," + subject,
        "(QA) FW: " + subject,
    ]

    def addresses(patch: dict[str, Any]) -> list[str]:
        return [r["emailAddress"]["address"] for r in patch["toRecipients"]]

    assert addresses(first) == (
        [
            "file@example.invalid",
            "dev@example.invalid",
            "test@example.invalid",
            "stage@example.invalid",
        ]
        if debug == "n"
        else ["debug@example.invalid", "dev@example.invalid"]
    )
    assert addresses(second) == (
        ["public1@example.invalid", "public2@example.invalid"]
        if debug == "n"
        else ["debug@example.invalid", "dev@example.invalid"]
    )
    assert writes[-1][2] == {"destinationId": "archive-id"}


@pytest.mark.parametrize(
    "subject,expected",
    [
        ("Undeliverable: Prior Approval: Synthetic", 0),
        ("re: ffr notification FFR NOTIFICATION : REJECTED", 0),
        ("fw: eRA Commons: PRAM for Grant", 0),
        ("unknown synthetic message", 0),
        ("RE: FFR NOTIFICATION : REJECTED", 0),
        ("RE: eRA Commons: PRAM for Grant", 1),
    ],
)
def test_suppressed_and_unknown_messages_still_archive(
    subject: str,
    expected: int,
) -> None:
    # FFR suppression lowercases; upper-case RE still suppresses, unlike PRAM.
    config = settings()
    transport = Transport([Transport.message(0, subject)])
    assert (
        process_graph_folder(
            GraphEmailService(config, transport),
            Processor(config),
            config,
            "n",
            dry_run=False,
            sleep=lambda _: None,
        )
        == 1
    )
    assert sum(url.endswith("/createForward") for _, url, _ in transport.writes()) == expected
    assert sum(url.endswith("/move") for _, url, _ in transport.writes()) == 1


def test_dry_run_inspects_only_fifty_without_any_graph_writes() -> None:
    config = settings()
    transport = Transport([Transport.message(i) for i in range(51)])
    assert (
        process_graph_folder(
            GraphEmailService(config, transport), Processor(config), config, "n", dry_run=True
        )
        == 50
    )
    assert transport.writes() == []  # Graph OAuth and GET are faked too.


@pytest.mark.parametrize(
    "size,expected_warnings",
    [
        (49, 0),
        (50, 1),
        pytest.param(
            51,
            2,
            marks=pytest.mark.xfail(
                strict=True, reason="approved decision pending: Python hard-caps at 50; C# does not"
            ),
        ),
    ],
)
def test_source_threshold_and_warning_after_each_completed_item(
    size: int,
    expected_warnings: int,
) -> None:
    # Processor.cs increments after move/sleep; EmailMe fires for every count >= 50.
    config = settings()
    transport = Transport([Transport.message(i) for i in range(size)])
    count = process_graph_folder(
        GraphEmailService(config, transport),
        Processor(config),
        config,
        "n",
        dry_run=False,
        sleep=lambda _: None,
    )
    writes = transport.writes()
    moves = [i for i, (_, url, _) in enumerate(writes) if url.endswith("/move")]
    sends = [i for i, (_, url, _) in enumerate(writes) if url.endswith("/send")]
    warnings = [(i, body) for i, (_, url, body) in enumerate(writes) if url.endswith("/sendMail")]
    assert (count, len(sends), len(moves), len(warnings)) == (size, size, size, expected_warnings)
    for index, body in warnings:
        assert index > moves[49]
        assert body["message"] == {
            "subject": (
                "(QA) Warning! PROD eMailRouter vbs has processed 50 mail items in one instance!"
            ),
            "body": {
                "contentType": "HTML",
                "content": "Hello Admin, 50 items have been processed in one instance and the "
                "application is now exiting. Please check whether there is duplicate "
                "items processing.",
            },
            "toRecipients": [{"emailAddress": {"address": "admin@example.invalid"}}],
        }


@pytest.mark.parametrize("failure", ["/createForward", "draft-1", "/send"])
def test_transport_send_failures_do_not_move_and_attempt_admin_alert(failure: str) -> None:
    config = settings()
    transport = Transport()
    transport.fail = failure
    count = process_graph_folder(
        GraphEmailService(config, transport),
        Processor(config),
        config,
        "n",
        dry_run=False,
        sleep=lambda _: None,
    )
    writes = transport.writes()
    assert count == 0  # target safety policy: failed items stay in place
    assert not any(url.endswith("/move") for _, url, _ in writes)
    assert sum(url.endswith("/createForward") for _, url, _ in writes) == 2


def test_admin_alert_failure_does_not_move_or_increment(monkeypatch: pytest.MonkeyPatch) -> None:
    config = settings()
    transport = Transport()
    transport.fail = "/createForward"
    processor = Processor(config)

    def broken(*args: Any) -> Any:
        raise ValueError("synthetic invalid positional token")

    monkeypatch.setattr(processor, "handle_single_email", broken)
    assert (
        process_graph_folder(
            GraphEmailService(config, transport),
            processor,
            config,
            "n",
            dry_run=False,
            sleep=lambda _: None,
        )
        == 0
    )
    assert [url.rsplit("/", 1)[-1] for _, url, _ in transport.writes()] == ["createForward"]


def test_move_alert_failure_has_no_retry_or_second_forward() -> None:
    config = settings()
    transport = Transport()
    transport.fail = "/move"
    # The alert transport also fails, without calling Graph or SQL outside the fake.
    original = transport.__call__

    def fail_move_and_alert(
        method: str, url: str, data: bytes | None, headers: dict[str, str]
    ) -> Any:
        if url.endswith("/sendMail"):
            transport.calls.append((method, url, json.loads(data or b"{}")))
            raise RuntimeError("synthetic alert failure")
        return original(method, url, data, headers)

    service = GraphEmailService(config, fail_move_and_alert)
    assert (
        process_graph_folder(
            service, Processor(config), config, "n", dry_run=False, sleep=lambda _: None
        )
        == 0
    )
    assert [url.rsplit("/", 1)[-1] for _, url, _ in transport.writes()] == [
        "createForward",
        "draft-1",
        "send",
        "move",
        "sendMail",
    ]


@pytest.mark.parametrize(
    "rows,expected", [([], ""), ([(None,), (42,)], ""), ([(42,), (99,)], "42")]
)
def test_sql_udf_first_row_null_and_no_rows(
    rows: list[tuple[int | None]],
    expected: str,
) -> None:
    calls: list[tuple[Any, ...]] = []

    class Cursor:
        def execute(self, *args: Any) -> "Cursor":
            calls.append(args)
            return self

        def fetchone(self) -> tuple[int | None] | None:
            return rows[0] if rows else None

        def close(self) -> None:
            calls.append(("closed",))

    class Connection:
        def cursor(self) -> Cursor:
            return Cursor()

    db = object.__new__(SqlGrantLookup)  # bypass pyodbc constructor; no real connection
    db._connection = Connection()
    assert db.appl_id("synthetic input") == expected
    assert calls == [
        ("SELECT dbo.Imm_fn_applid_match(?) AS applid", "synthetic input"),
        ("closed",),
    ]


def test_sql_failure_propagates_and_closes_cursor() -> None:
    closed: list[bool] = []

    class Cursor:
        def execute(self, *args: Any) -> Any:
            raise RuntimeError("synthetic SQL failure")

        def close(self) -> None:
            closed.append(True)

    class Connection:
        def cursor(self) -> Cursor:
            return Cursor()

    db = object.__new__(SqlGrantLookup)
    db._connection = Connection()
    with pytest.raises(RuntimeError, match="synthetic SQL failure"):
        db.appl_id("synthetic")
    assert closed == [True]


def test_smtp_global_failure_uses_html_and_single_fake_attempt(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    attempts: list[Any] = []

    class FakeSmtp:
        def __init__(self, host: str, port: int, timeout: int) -> None:
            assert (host, port, timeout) == ("relay.example.invalid", 25, 30)
            attempts.append("connect")

        def __enter__(self) -> "FakeSmtp":
            return self

        def __exit__(self, *args: Any) -> None:
            pass

        def send_message(self, message: Any) -> None:
            attempts.append(message)
            raise RuntimeError("synthetic relay failure")

    monkeypatch.setattr("app.notifications.smtplib.SMTP", FakeSmtp)
    config = settings(
        smtp_host="relay.example.invalid",
        smtp_port=25,
        smtp_from_address="router@example.invalid",
        smtp_from_name="Synthetic Router",
        error_recipients="ops@example.invalid;backup@example.invalid",
    )
    with pytest.raises(RuntimeError, match="synthetic relay failure"):
        send_global_error(config, "synthetic failure details")
    assert len(attempts) == 2  # no retry by the adapter
    message = attempts[1]
    assert message["Subject"] == "(QA) Global level email failure."
    assert message["From"] == "Synthetic Router <router@example.invalid>"
    assert message["To"] == "ops@example.invalid, backup@example.invalid"
    assert message.get_content_type() == "text/html"
    assert message.get_content().strip() == "synthetic failure details"


@pytest.mark.xfail(strict=True, reason="source archives after admin alert; target leaves in place")
def test_source_admin_alert_success_archives_and_counts_failed_route(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    config = settings()
    transport = Transport()
    processor = Processor(config)

    def broken(*args: Any) -> Any:
        raise ValueError("synthetic positional parse failure")

    monkeypatch.setattr(processor, "handle_single_email", broken)
    count = process_graph_folder(
        GraphEmailService(config, transport),
        processor,
        config,
        "n",
        dry_run=False,
        sleep=lambda _: None,
    )
    writes = transport.writes()
    assert [url.rsplit("/", 1)[-1] for _, url, _ in writes] == ["createForward", "draft-1", "send"]
    assert writes[1][2]["toRecipients"] == [
        {"emailAddress": {"address": "admin@example.invalid"}},
        {"emailAddress": {"address": "admin@example.invalid"}},
    ]
    assert writes[1][2]["subject"] == (
        "(QA) Error Occured! PROD eMailRouter vbs  >>(Subj: Prior Approval: Synthetic )"
    )
    assert writes[1][2]["body"]["content"] == "<p>synthetic HTML</p>"
    # C# Process moves and increments after successful RaiseErrorToAdmin.
    assert count == 1 and any(url.endswith("/move") for _, url, _ in writes)


@pytest.mark.xfail(strict=True, reason="source counts successful move alert; target skips count")
def test_source_move_failure_alert_still_counts_and_does_not_retry_forward() -> None:
    config = settings()
    transport = Transport()
    transport.fail = "/move"
    count = process_graph_folder(
        GraphEmailService(config, transport),
        Processor(config),
        config,
        "n",
        dry_run=False,
        sleep=lambda _: None,
    )
    writes = transport.writes()
    assert [url.rsplit("/", 1)[-1] for _, url, _ in writes] == [
        "createForward",
        "draft-1",
        "send",
        "move",
        "sendMail",
    ]
    assert writes[-1][2]["message"]["subject"] == "(QA) Failed to move an item to old."
    assert writes[-1][2]["message"]["toRecipients"] == [
        {"emailAddress": {"address": "ops@example.invalid"}}
    ]
    # C# counts this iteration after successful sendMail; Python currently skips increment.
    assert count == 1

"""Graph-backed ExchangeFixed/AddSuppProd migration contract (offline only)."""

from base64 import b64encode
from typing import Any

import pytest

from app.add_supp_prod import notification_id, plan_supplement, sender_id, subject_parameters
from app.config import RouterConfig
from app.exchange_fixed import parse_subject, plan_documents
from app.graph import GraphAttachment, GraphEmailService, GraphFolder, GraphMail
from app.graph_project_preview import preview_project


class Lookup:
    def __init__(self) -> None:
        self.inputs: list[str] = []

    def appl_id(self, text: str) -> str:
        self.inputs.append(text)
        return "42"

    def notification_appl_id(self, value: str) -> str:
        self.inputs.append("notification:" + value)
        return "42"

    def pa_code(self, value: str) -> str:
        self.inputs.append("pa:" + value)
        return "PA-1"


def mail(subject: str, *, sender: str = "staff@example.org", body: str = "body") -> GraphMail:
    return GraphMail("message-id", subject, body, "<p>body</p>", sender, "2025-01-02T00:00:00Z")


def test_exchange_fixed_subject_and_grant_lookup_priority() -> None:
    email = mail("grantnumber=1R01CA000001, category=PublicAccess, sub=Compliant, extract=3")
    db = Lookup()
    plans = plan_documents(email, [GraphAttachment("report.pdf", b"%PDF")], db)
    assert parse_subject(email.subject).category == "PublicAccess"
    assert db.inputs == [" 1R01CA000001 "]
    # Source's extract=3 PublicAccess writes a combined PDF; it does not
    # register the attachment separately (unlike extract=2).
    assert [(plan.format, plan.attachment) for plan in plans] == [("pdf", None)]


def test_exchange_fixed_extract_two_filters_att_and_flags_unknown_extension() -> None:
    email = mail("applid=42,category=Correspondence,extract=2")
    plans = plan_documents(
        email,
        [GraphAttachment("ATT00001.png", b"x"), GraphAttachment("archive.xyz", b"y")],
        Lookup(),
    )
    assert len(plans) == 1
    assert plans[0].attachment is not None and plans[0].attachment.content == b"y"
    assert plans[0].format == "xyz" and plans[0].qc_required


def test_exchange_fixed_unknown_extract_fails_closed() -> None:
    with pytest.raises(ValueError, match="extract mode"):
        plan_documents(mail("applid=42, extract=5"), [], Lookup())


def test_supplement_system_and_missing_notification() -> None:
    db = Lookup()
    aliases = {"nciogaegrantsprod@example.org": "nciogaegrantsprod"}
    plan = plan_supplement(
        mail(
            "Change in Status", sender="nciogaegrantsprod@example.org", body="Notification Id=123"
        ),
        [],
        db,
        trusted_aliases=aliases,
    )
    assert plan.branch == "system" and plan.subcategory == "Supplement Status Change"
    assert plan.notification_id == "123" and db.inputs == ["notification:123"]
    assert notification_id("NOTIFICATION ID=987") == "987"
    assert plan_supplement(
        mail("Change in Status", sender="nciogaegrantsprod@example.org"),
        [],
        db,
        trusted_aliases=aliases,
    ).admin_alert


def test_supplement_staff_first_attachment_and_params() -> None:
    aliases = {"jonesni@example.org": "jonesni"}
    plan = plan_supplement(
        mail("category=application file,applid=42", sender="jonesni@example.org"),
        [GraphAttachment("original.docx", b"data"), GraphAttachment("second.txt", b"x")],
        Lookup(),
        trusted_aliases=aliases,
    )
    assert plan.branch == "staff" and plan.file_type == "docx"
    assert plan.attachment is not None and plan.attachment.content == b"data"
    assert subject_parameters("Category=correspondence, sub=Progress") == {
        "category": "correspondence",
        "sub": "Progress",
    }
    missing = plan_supplement(
        mail("category=correspondence,applid=42", sender="jonesni@example.org"),
        [],
        Lookup(),
        trusted_aliases=aliases,
    )
    assert missing.admin_alert == "Correspondence requires subcategory"


def test_supplement_reply_and_era_are_distinct() -> None:
    db = Lookup()
    reply = plan_supplement(mail("Re: hello", body="Notification Id=9"), [], db)
    assert reply.branch == "reply" and reply.notification_id == "9"
    assert reply.pa_code == "PA-1" and reply.appl_id == "42"
    era = plan_supplement(
        mail("Supplement Requested", sender="caeranotifications@example.org"),
        [],
        db,
        trusted_aliases={"caeranotifications@example.org": "caeranotifications"},
    )
    assert era.branch == "era" and era.subcategory == "Supplement Requested"


def test_unmapped_or_spoofed_staff_address_is_not_privileged() -> None:
    assert sender_id("jonesni@untrusted.example") == "jonesni@untrusted.example"
    plan = plan_supplement(
        mail("category=application file,applid=42", sender="jonesni@untrusted.example"),
        [],
        Lookup(),
        trusted_aliases={"jonesni@example.org": "jonesni"},
    )
    assert plan.branch == "reply" and plan.admin_alert == "Unidentified email"


def test_graph_file_attachments_decode_all_pages_and_reject_reference_attachments() -> None:
    urls: list[str] = []

    def transport(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        urls.append(url)
        if "login.microsoftonline.com" in url:
            return {"access_token": "fixture-token"}
        assert method == "GET"
        if url.endswith("page=2"):
            return {
                "value": [
                    {
                        "@odata.type": "#microsoft.graph.fileAttachment",
                        "name": "second.txt",
                        "contentBytes": b64encode(b"two").decode(),
                    }
                ]
            }
        return {
            "value": [
                {
                    "@odata.type": "#microsoft.graph.fileAttachment",
                    "name": "first.pdf",
                    "contentBytes": b64encode(b"one").decode(),
                }
            ],
            "@odata.nextLink": "https://graph.microsoft.com/v1.0/users/mail%40example.org/"
            "messages/message-id/attachments?page=2",
        }

    config = RouterConfig(
        graph_mailbox="mail@example.org",
        graph_tenant_id="tenant",
        graph_client_id="client",
        graph_client_secret="secret",
    )
    service = GraphEmailService(config, transport)
    service.connect()
    files = service.attachments(mail("subject"))
    assert [(file.name, file.content) for file in files] == [
        ("first.pdf", b"one"),
        ("second.txt", b"two"),
    ]
    assert len(urls) == 3

    def reference(method: str, url: str, data: bytes | None, headers: dict[str, str]) -> Any:
        if "login.microsoftonline.com" in url:
            return {"access_token": "fixture-token"}
        return {"value": [{"@odata.type": "#microsoft.graph.referenceAttachment"}]}

    service = GraphEmailService(config, reference)
    service.connect()
    with pytest.raises(ValueError, match="Non-file"):
        service.attachments(mail("subject"))


def test_graph_project_preview_only_reads_and_requires_mapping() -> None:
    class FakeGraph:
        def __init__(self) -> None:
            self.calls: list[str] = []

        def connect(self) -> None:
            self.calls.append("connect")

        def folder(self, name: str) -> GraphFolder:
            self.calls.append("folder:" + name)
            return GraphFolder("test-folder", name)

        def emails(self, folder: GraphFolder) -> list[GraphMail]:
            self.calls.append("emails")
            return [mail("applid=42,extract=2")]

        def attachments(self, message: GraphMail) -> list[GraphAttachment]:
            self.calls.append("attachments")
            return [GraphAttachment("report.pdf", b"fixture")]

    service = FakeGraph()
    with pytest.raises(ValueError, match="Outlook public-folder"):
        preview_project(service, "exchange-fixed", "Public Folders - old\\efile", Lookup())
    assert service.calls == []
    plans = preview_project(service, "exchange-fixed", "Inbox/efile", Lookup())
    assert len(plans) == 1 and plans[0].message_id == "message-id"
    assert service.calls == ["connect", "folder:Inbox/efile", "emails", "attachments"]

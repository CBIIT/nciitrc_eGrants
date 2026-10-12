"""Behavior contracts extracted from the ordered C# HandleSingleEmail branches."""

from dataclasses import dataclass, field
from typing import Any

import pytest

from app.config import RouterConfig
from app.models import IncomingEmail
from app.outlook import forward_result, process_folder
from app.processor import Processor
from app.routing_rules import remove_sp_characters


class FakeLookup:
    def __init__(self) -> None:
        self.inputs: list[str] = []

    def appl_id(self, text: str) -> str:
        self.inputs.append(text)
        return "12345"

    def officer_emails(self, appl_id: str) -> tuple[str, str]:
        assert appl_id == "12345"
        return "primary@example.org", "backup@example.org"


# subject, sender, body, branch, subject prefix, production recipient set
CASES = [
    ("eSNAP Received at NIH submitted to NIH with a Non-Compliance", "", "", "esnap",
     "category=eRANotification, sub=RPPR Non-Compliance, extract=1,", "efile"),
    ("IC ACTION REQUIRED - Relinquishing Statement", "", "", "relinquishing", "", "relinquishing"),
    ("A Supplement Requested through system", "", "", "supplement", "", "supplement"),
    ("award FCOI 123", "", "", "fcoi", "", "bob"),
    ("No Cost Extension Submitted", "", "", "no_cost_extension",
     "category=eRANotification, sub=No Cost Extension, extract=1,", "efile"),
    ("Change of Institution request for Grant", "", "", "change_of_institution",
     "", "relinquishing"),
    ("Compliant PASC: 5R01CA258784 - 04 - SEN", "public@nih.gov", "", "public_access",
     "category=PublicAccess, sub=Compliant, applid=12345, extract=1, ", "efile"),
    ("JIT Request for Grant", "", "", "jit_request",
     "category=JIT Info, sub=Reminder, extract=1, ", "efile"),
    ("JIT Documents Have Been Submitted for Grant", "", "", "jit_submitted",
     "category=eRA Notification, sub=JIT Submitted, extract=1, ", "efile"),
    ("NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant R15 123", "", "",
     "late_progress", "category=eRANotification, sub=Late Progress Report, extract=1, ", "efile"),
    ("Expiring Funds", "", "", "expiring_funds", "category=Closeout, extract=2, ", "efile"),
    ("Prior Approval: Carryover", "", "", "prior_approval", "", "postaward"),
    ("FFR NOTIFICATION : REJECTED", "", "", "ffr_rejected",
     "category=Notification, sub=FFR Rejection, extract=1, ", "efile"),
    ("eRA Commons: The Final RPPR - Additional Materials for Award", "", "", "final_rppr",
     "category=FRAM: Request, sub=The Final RPPR, extract=1, ", "efile"),
    ("RPPR Unobligated Balance: Additional Information Needed for 5R01CA258784", "", "",
    "rppr_balance", "applid=12345, category=Correspondence, "
    "sub=RPPR Unobligated Balance, extract=1, ", "efile"),
    ("eRA Commons: PRAM for Grant", "", "", "pram_grant",
     "category=PRAM: Requested, sub=PRAM for Grant, extract=1, ", "devteststage"),
    ("FRAM Requested 5R01CA258784", "", "", "fram_pram",
     "applid=12345, category=FRAM, sub=Request, extract=1, ", "efile"),
    ("PRAM Requested 5R01CA258784", "", "", "fram_pram",
     "applid=12345, category=PRAM, sub=Request, extract=1, ", "efile"),
    ("CHANGE_NOTICE_FOR Application is withdrawn request", "", "", "withdrawn",
     "applid=12345, category=eRA Notification, sub=Application Withdrawn, extract=1, ", "efile"),
    ("RPPR Reminder 5R01CA258784", "", "", "rppr_reminder",
     "applid=12345, category=RPPR, sub=Reminder, extract=1, ", "efile"),
    ("IRPPR Reminder 5R01CA258784", "", "", "irppr_reminder",
     "applid=12345, category=IRPPR, sub=Reminder, extract=1, ", "efile"),
    ("URGENT: CLOSEOUT REPORTS OVERDUE for 5R01CA258784", "", "", "closeout_overdue",
     "category=closeout, sub=Past Due Documents Reminder, applid=12345, extract=1, ", "efile"),
    ("Closeout Program Action Required: 5R21CA249649-02 - Sun", "", "", "closeout_program",
    "category=closeout, sub=F-RPPR Acceptance Past Due Reminder, "
    "applid=12345, extract=1, ", "efile"),
    ("FFR Reminder FFR Past Due 5R01CA258784", "", "", "ffr_reminder",
     "applid=12345, category=FFR, sub=Reminder, extract=1, ", "efile"),
    ("ClinicalTrials.gov Results Reporting for Grant NCT1234", "", "", "clinical_trials",
    "applid=12345, category=CT.gov, sub=Results Reporting Reminder NCT1234 , "
    "extract=1, ", "efile"),
    ("SBIR/STTR Foreign Risk Management Not Cleared", "", "1R43CA291415-01 (10921643) text",
    "sbir_sttr", "applid=10921643, category=Funding, sub=DCI-InTh Not Cleared, "
    "extract=1, ", "efile"),
]


@pytest.mark.parametrize("subject,sender,body,name,prefix,group", CASES)
def test_production_parity(subject: str, sender: str, body: str,
                           name: str, prefix: str, group: str) -> None:
    config = RouterConfig()
    result = Processor(config, FakeLookup()).handle_single_email(
        IncomingEmail(subject, body, sender), "n"
    )
    assert result.route_name == name
    assert result.matched
    outgoing = result.outgoing_messages[0]
    assert outgoing.subject == prefix + subject
    assert outgoing.body == body
    groups = {
        "efile": [config.efile_email, config.egrants_dev_email,
                  config.egrants_test_email, config.egrants_stage_email],
        "devteststage": [config.egrants_dev_email, config.egrants_test_email,
                         config.egrants_stage_email],
        "postaward": [config.nci_grants_post_award_email],
        "relinquishing": config.relinquishing_recipients.split(";"),
        "supplement": [config.supplements_email],
        "bob": [config.bob_team_email, "primary@example.org", "backup@example.org"],
    }
    assert outgoing.recipients == groups[group]


@pytest.mark.parametrize("subject,sender,body,name,prefix,group", CASES)
def test_debug_routes_only_to_safe_recipients(subject: str, sender: str, body: str,
                                               name: str, prefix: str, group: str) -> None:
    config = RouterConfig()
    result = Processor(config, FakeLookup()).handle_single_email(
        IncomingEmail(subject, body, sender), "y"
    )
    expected = ([config.egrants_dev_email] if name == "no_cost_extension"
                else [config.debug_email, config.egrants_dev_email])
    assert result.matched
    assert all(message.recipients == expected for message in result.outgoing_messages)


def test_esnap_creates_two_forwards() -> None:
    result = Processor(RouterConfig()).handle_single_email(
        IncomingEmail("eSNAP Received at NIH submitted to NIH with a Non-Compliance", "body"), "n"
    )
    assert len(result.outgoing_messages) == 2
    assert result.outgoing_messages[1].recipients == (
        RouterConfig().public_access_recipients.split(";")
    )


@pytest.mark.parametrize("subject", [
    "undeliverable: Prior Approval: 123", "re: ffr notification FFR NOTIFICATION : REJECTED",
    "re: eRA Commons: PRAM for Grant", "unrecognized",
])
def test_skips_or_ignores(subject: str) -> None:
    result = Processor(RouterConfig()).handle_single_email(IncomingEmail(subject, ""), "n")
    assert not result.matched


def test_fcoi_uses_unspaced_subject_for_db() -> None:
    lookup = FakeLookup()
    Processor(RouterConfig(), lookup).handle_single_email(IncomingEmail("award FCOI 123", ""), "n")
    assert lookup.inputs == ["awardFCOI123"]


def test_normalization_is_case_sensitive() -> None:
    assert remove_sp_characters("5R/01: CA&2") == "5R01CAand2"


class Recipients:
    def __init__(self) -> None:
        self.values: list[str] = []

    def Add(self, recipient: str) -> None:
        self.values.append(recipient)


@dataclass
class Mail:
    Subject: str = "Prior Approval: 123"
    Body: str = "body"
    SenderEmailAddress: str = "sender@example.org"
    SenderEmailType: str = "SMTP"
    Class: int = 43
    Recipients: Recipients = field(default_factory=Recipients)
    sent: bool = False
    moved: bool = False
    forwarded: list[Any] = field(default_factory=list)

    def Forward(self) -> "Mail":
        result = Mail()
        self.forwarded.append(result)
        return result

    def Send(self) -> None:
        self.sent = True

    def Move(self, archive: Any) -> None:
        self.moved = True


class Folder:
    def __init__(self, items: list[Mail]) -> None:
        self.Items = items
        self.Folders = {"Old emails": object()}


class Namespace:
    def __init__(self, folder: Folder) -> None:
        root = Folder([])
        root.Folders["Sub"] = folder
        self.Folders = {"Root": root}

    def Logon(self, *args: Any) -> None:
        pass


class App:
    def __init__(self, folder: Folder) -> None:
        self.namespace = Namespace(folder)

    def GetNamespace(self, name: str) -> Namespace:
        return self.namespace


def test_outlook_dry_run_does_not_forward_or_move() -> None:
    item = Mail()
    folder = Folder([item])
    config = RouterConfig(folder_path="Root\\Sub")
    assert process_folder(App(folder), Processor(config), config, "n", dry_run=True) == 1
    assert not item.forwarded and not item.moved


def test_outlook_send_forwards_and_archives() -> None:
    item = Mail()
    folder = Folder([item])
    config = RouterConfig(folder_path="Root\\Sub", routing_break_duration=0)
    assert process_folder(App(folder), Processor(config), config, "n",
                          dry_run=False, sleep=lambda seconds: None) == 1
    assert item.moved
    assert item.forwarded[0].sent
    assert item.forwarded[0].Recipients.values == [config.nci_grants_post_award_email]


def test_outlook_failed_send_does_not_archive() -> None:
    class BrokenMail(Mail):
        def Forward(self) -> Mail:
            raise RuntimeError("Outlook send failed")

    item = BrokenMail()
    config = RouterConfig(folder_path="Root\\Sub", routing_break_duration=0)
    process_folder(App(Folder([item])), Processor(config), config, "n", dry_run=False)
    assert not item.moved


def test_outlook_preserves_forward_not_new_message() -> None:
    item = Mail()
    result = Processor(RouterConfig()).handle_single_email(
        IncomingEmail(item.Subject, item.Body), "n"
    )
    forward_result(item, result, "Development")
    assert item.forwarded[0].Subject == "(Development) Prior Approval: 123"
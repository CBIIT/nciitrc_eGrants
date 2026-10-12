"""Ordered subject routing from Router/Processor.cs HandleSingleEmail."""

from typing import Protocol

from app.config import RouterConfig
from app.models import IncomingEmail, OutgoingEmail, RouteResult

PRIOR_APPROVAL_TOKEN = "Prior Approval: "


class GrantLookup(Protocol):
    def appl_id(self, text: str) -> str: ...

    def officer_emails(self, appl_id: str) -> tuple[str, str]: ...


class NoLookup:
    """For offline previews only; IDs will be blank."""

    def appl_id(self, text: str) -> str:
        return ""

    def officer_emails(self, appl_id: str) -> tuple[str, str]:
        return "", ""


def remove_sp_characters(text: str) -> str:
    """Match the original RemoveSpCharacters substitutions and ordering."""
    for old, new in (
        ("vbLf", "vbCrLF"), (":", " "), ("/", " "), ("\\", " "),
        ("&", "and"), (";", " "), ("<", " "), (">", " "),
        ("<<", " "), (">>", " "), ("^", " "), ("%", " "),
        ("@", " "), ("'", " "), (" ", ""),
    ):
        text = text.replace(old, new)
    return text.strip()


def _nth_word(text: str, number: int) -> str:
    # C# Split(' ') retains empty entries; do not use Python str.split().
    return text.split(" ")[number - 1]


def _recipients(text: str) -> list[str]:
    return [recipient.strip() for recipient in text.split(";") if recipient.strip()]


def route_email(
    email: IncomingEmail, config: RouterConfig, debug: str,
    lookup: GrantLookup | None = None,
) -> RouteResult:
    """Run one ordered branch; the eSNAP branch may produce two forwards."""
    db = lookup if lookup is not None else NoLookup()
    subject = email.subject
    body = email.body
    production = debug == "n"
    efile_group = [config.efile_email, config.egrants_dev_email,
                   config.egrants_test_email, config.egrants_stage_email]
    debug_group = [config.debug_email, config.egrants_dev_email]

    def result(name: str, forwards: list[tuple[str, list[str]]]) -> RouteResult:
        return RouteResult(
            matched=True, route_name=name,
            outgoing_messages=[
                OutgoingEmail(subject=title, body=body, recipients=people)
                for title, people in forwards
            ],
        )

    def forward(name: str, title: str | None = None,
                production_recipients: list[str] | None = None,
                debug_recipients: list[str] | None = None) -> RouteResult:
        people = (production_recipients or efile_group) if production else (
            debug_recipients if debug_recipients is not None else debug_group
        )
        return result(name, [(subject if title is None else title, people)])

    def enriched(name: str, prefix: str, **kwargs: list[str]) -> RouteResult:
        return forward(name, prefix + subject, **kwargs)

    def id_from(text: str) -> str:
        return db.appl_id(remove_sp_characters(text))

    if "undeliverable: " in subject.lower():
        return RouteResult(False, "undeliverable")

    if "eSNAP Received at NIH" in subject or "eRA Commons: RPPR for Grant" in subject:
        forwards: list[tuple[str, list[str]]] = []
        if "submitted to NIH with a Non-Compliance" in subject:
            forwards.append((
                f"category=eRANotification, sub=RPPR Non-Compliance, extract=1,{subject}",
                efile_group if production else debug_group,
            ))
        forwards.append((subject, _recipients(config.public_access_recipients)
                         if production else debug_group))
        return result("esnap", forwards)

    if "IC ACTION REQUIRED - Relinquishing Statement" in subject:
        return forward(
            "relinquishing", production_recipients=_recipients(config.relinquishing_recipients)
        )

    if " Supplement Requested through " in subject:
        return forward("supplement", production_recipients=[config.supplements_email])

    if " FCOI " in subject and not subject.startswith("Automatic reply:"):
        appl_id = db.appl_id(remove_sp_characters(subject))
        primary, backup = db.officer_emails(appl_id) if appl_id else ("", "")
        officers = list(dict.fromkeys(
            address for address in (primary, backup) if address.strip()
        ))
        return forward("fcoi", production_recipients=[config.bob_team_email, *officers])

    if "No Cost Extension Submitted" in subject:
        return enriched(
            "no_cost_extension", "category=eRANotification, sub=No Cost Extension, extract=1,",
            debug_recipients=[config.egrants_dev_email],
        )

    if "Change of Institution request for Grant" in subject:
        return forward(
            "change_of_institution",
            production_recipients=_recipients(config.relinquishing_recipients),
        )

    if "public" in email.sender.lower():
        middle = subject.split(": ")[-1].strip().split(" - ")[0]
        appl_id = id_from(middle)
        sub = "Compliant" if "compliant" in subject.lower() else ""
        return enriched(
            "public_access", f"category=PublicAccess, sub={sub}, applid={appl_id}, extract=1, "
        )

    if "JIT Request for Grant" in subject:
        return enriched("jit_request", "category=JIT Info, sub=Reminder, extract=1, ")

    if "JIT Documents Have Been Submitted for Grant" in subject:
        return enriched(
            "jit_submitted", "category=eRA Notification, sub=JIT Submitted, extract=1, "
        )

    if "NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant" in subject:
        title = ("category=eRANotification, sub=Late Progress Report, extract=1, " + subject
                 if " R15 " in subject else "")
        return forward("late_progress", title)

    if "Expiring Funds" in subject or "EXPIRING FUNDS-" in subject:
        return enriched("expiring_funds", "category=Closeout, extract=2, ")

    if PRIOR_APPROVAL_TOKEN in subject:
        return forward("prior_approval", production_recipients=[config.nci_grants_post_award_email])

    if "FFR NOTIFICATION : REJECTED" in subject:
        if "re: ffr notification" in subject.lower() or "fw: ffr notification" in subject.lower():
            return RouteResult(False, "ffr_reply")
        return enriched("ffr_rejected", "category=Notification, sub=FFR Rejection, extract=1, ")

    if "eRA Commons: The Final RPPR - Additional Materials for Award" in subject:
        if ("re: eRA Commons: The Final RPPR " in subject
            or "fw: eRA Commons: The Final RPPR " in subject):
            return RouteResult(False, "fram_reply")
        return enriched("final_rppr", "category=FRAM: Request, sub=The Final RPPR, extract=1, ")

    if "RPPR Unobligated Balance: Additional Information Needed" in subject:
        grant = subject.strip().split(" ")[7]
        appl_id = id_from(grant) if grant.strip() else ""
        return enriched(
            "rppr_balance", f"applid={appl_id}, category=Correspondence, "
            "sub=RPPR Unobligated Balance, extract=1, "
        )

    if "eRA Commons: PRAM for Grant" in subject:
        if ("re: eRA Commons: PRAM for Grant" in subject
            or "fw: eRA Commons: PRAM for Grant" in subject):
            return RouteResult(False, "pram_reply")
        return enriched("pram_grant", "category=PRAM: Requested, sub=PRAM for Grant, extract=1, ",
                        production_recipients=efile_group[1:])

    if "FRAM Requested" in subject or "PRAM Requested" in subject:
        category = "FRAM" if "FRAM Requested" in subject else "PRAM"
        return enriched(
            "fram_pram", f"applid={id_from(subject)}, category={category}, "
            "sub=Request, extract=1, "
        )

    if "CHANGE_NOTICE_FOR" in subject and "Application is withdrawn request" in subject:
        return enriched(
            "withdrawn", f"applid={id_from(subject)}, category=eRA Notification, "
            "sub=Application Withdrawn, extract=1, "
        )

    if subject.startswith("RPPR Reminder"):
        return enriched(
            "rppr_reminder", f"applid={id_from(subject)}, category=RPPR, sub=Reminder, extract=1, "
        )

    if "IRPPR Reminder" in subject:
        return enriched(
            "irppr_reminder",
            f"applid={id_from(subject)}, category=IRPPR, sub=Reminder, extract=1, ",
        )

    if "urgent: closeout reports overdue" in subject.lower():
        appl_id = id_from(_nth_word(subject, 6))
        return enriched(
            "closeout_overdue", f"category=closeout, sub=Past Due Documents Reminder, "
            f"applid={appl_id}, extract=1, "
        )

    if "closeout program action required" in subject.lower():
        appl_id = id_from(_nth_word(subject.strip(), 5))
        return enriched(
            "closeout_program", "category=closeout, sub=F-RPPR Acceptance Past Due Reminder, "
            f"applid={appl_id}, extract=1, "
        )

    if "FFR Reminder" in subject and "FFR Past Due" in subject:
        return enriched(
            "ffr_reminder", f"applid={id_from(subject)}, category=FFR, sub=Reminder, extract=1, "
        )

    if "ClinicalTrials.gov Results Reporting for Grant" in subject:
        last_four = subject.split(" ")[-1][-4:]
        return enriched(
            "clinical_trials", f"applid={id_from(subject)}, category=CT.gov, "
            f"sub=Results Reporting Reminder NCT{last_four} , extract=1, "
        )

    if "SBIR/STTR Foreign Risk Management" in subject:
        appl_id = body.split(" ")[1].replace("(", "").replace(")", "")
        sub = "DCI-InTh Not Cleared" if "Not Cleared" in subject else "DCI-InTh Cleared"
        return enriched("sbir_sttr", f"applid={appl_id}, category=Funding, sub={sub}, extract=1, ")

    return RouteResult(False, "unmatched")


def apply_prior_approval_rule(
    email: IncomingEmail, config: RouterConfig, debug: str
) -> RouteResult:
    """Keep the original slice API for standalone rule tests."""
    if PRIOR_APPROVAL_TOKEN not in email.subject:
        return RouteResult(False, "prior_approval")
    return route_email(email, config, debug)

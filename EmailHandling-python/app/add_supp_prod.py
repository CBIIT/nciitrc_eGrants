"""Offline AddSuppProd business rules; mailbox reads use Graph, not Outlook COM."""

import re
from dataclasses import dataclass
from typing import Mapping, Protocol

from app.graph import GraphAttachment, GraphMail

STAFF = frozenset({"driskelleb", "jonesni", "omairi", "woldezf"})


class SupplementLookup(Protocol):
    def notification_appl_id(self, notification_id: str) -> str: ...

    def appl_id(self, text: str) -> str: ...

    def pa_code(self, text: str) -> str: ...


@dataclass(frozen=True)
class SupplementPlan:
    branch: str
    appl_id: str = ""
    category: str = ""
    subcategory: str = ""
    file_type: str = "txt"
    pa_code: str = ""
    notification_id: str = ""
    attachment: GraphAttachment | None = None
    subject_for_wip: str = ""
    body_for_wip: str = ""
    admin_alert: str = ""


def notification_id(body: str) -> str:
    match = re.search(r"Notification Id=(\d+)", body, re.IGNORECASE)
    return match.group(1) if match else ""


def normalize(text: str) -> str:
    for old, new in (
        (":", " "),
        ("/", " "),
        ("\\", " "),
        ("&", "and"),
        (";", " "),
        ("<", " "),
        (">", " "),
        ("^", " "),
        ("%", " "),
        ("@", " "),
        ("'", " "),
        (" ", ""),
    ):
        text = text.replace(old, new)
    return text.strip()


def subject_parameters(subject: str) -> dict[str, str]:
    params: dict[str, str] = {}
    for part in subject.split(","):
        if "=" in part and part.index("=") > 0:
            key, value = part.split("=", 1)
            params[key.strip().lower()] = value.strip()
    return params


def file_extension(name: str) -> str:
    return (
        name.rsplit(".", 1)[-1].lower()
        if name and "." in name and not name.endswith(".")
        else "txt"
    )


def sender_id(address: str, trusted_aliases: Mapping[str, str] | None = None) -> str:
    # Outlook's EX alias is not necessarily an SMTP local part. Only explicitly
    # approved exact addresses may enter the privileged staff/system branches.
    return (trusted_aliases or {}).get(address.casefold(), address).casefold()


def plan_supplement(
    mail: GraphMail,
    attachments: list[GraphAttachment],
    db: SupplementLookup,
    *,
    trusted_aliases: Mapping[str, str] | None = None,
) -> SupplementPlan:
    """Mirror the four source branches; return a plan, never mutate DB/files/mail."""
    sender = sender_id(mail.sender, trusted_aliases)
    subject, body = mail.subject, mail.body
    if sender == "nciogaegrantsprod":
        branch = "system"
        sub = next(
            (
                label
                for token, label in (
                    ("Change in Status", "Supplement Status Change"),
                    ("Admin Supplement", "Admin Supplement"),
                    ("Response Required", "Supplement Response Required"),
                    ("Diversity Supplement", "Diversity Supplement"),
                )
                if token in subject
            ),
            "Unknown",
        )
        notif = notification_id(body)
        if not notif:
            return SupplementPlan(branch, admin_alert="Could not extract notification ID")
        appl = db.notification_appl_id(notif)
        if not appl:
            return SupplementPlan(
                branch, notification_id=notif, admin_alert="Could not find application ID"
            )
        return SupplementPlan(
            branch,
            appl,
            "Correspondence",
            sub,
            notification_id=notif,
            subject_for_wip=subject,
            body_for_wip=body,
        )
    if sender == "caeranotifications":
        branch = "era"
        appl = db.appl_id(" " + normalize(subject) + " ") or db.appl_id(" " + normalize(body) + " ")
        if not appl:
            return SupplementPlan(branch, admin_alert="Could not identify application ID")
        pa = db.pa_code(" " + normalize(subject) + " ")
        return SupplementPlan(
            branch,
            appl,
            "eRA Notification",
            "Supplement Requested" if "Supplement Requested" in subject else "Unknown",
            pa_code=pa,
            subject_for_wip=subject,
            body_for_wip=body,
        )
    if sender in STAFF:
        branch = "staff"
        params = subject_parameters(subject)
        category = params.get("category", "correspondence")
        sub = params.get("sub", "")
        is_file = category.lower() in {"application file", "applicationfile"}
        first = attachments[0] if is_file and attachments else None
        fmt = file_extension(first.name) if first else "txt"
        if is_file:
            sub = ""
        appl = params.get("applid", "")
        if not appl:
            lookup_text = params.get("grantnumber") or subject
            appl = db.appl_id(" " + normalize(lookup_text) + " ") or db.appl_id(
                " " + normalize(body) + " "
            )
        if not appl:
            return SupplementPlan(branch, admin_alert="Grant number or applid not identified")
        if category.lower() == "correspondence" and not sub.strip():
            return SupplementPlan(
                branch, appl_id=appl, admin_alert="Correspondence requires subcategory"
            )
        return SupplementPlan(branch, appl, category, sub, fmt, attachment=first)
    branch = "reply"
    notif = notification_id(body)
    if not notif:
        return SupplementPlan(branch, admin_alert="Unidentified email")
    pa = db.pa_code(" " + normalize(subject) + " ")
    appl = db.notification_appl_id(notif)
    if not appl:
        return SupplementPlan(
            branch, notification_id=notif, admin_alert="Reply application ID not found"
        )
    # Actual processing updates reply_recieved_date before the PA/appl lookup;
    # the plan records this dependency rather than touching the database.
    return SupplementPlan(
        branch,
        appl,
        "Correspondence",
        "Supplement Response",
        pa_code=pa,
        notification_id=notif,
        subject_for_wip=subject,
        body_for_wip=body,
    )

"""Offline ExchangeFixed document planning; Graph is the intended mailbox source.

Plans are not registrations: production SQL, PDF and network-share effects are
deliberately unavailable until the document-fidelity contract is verified.
"""

from dataclasses import dataclass
from typing import Protocol

from app.graph import GraphAttachment, GraphMail


class ApplicationLookup(Protocol):
    def appl_id(self, text: str) -> str: ...


@dataclass(frozen=True)
class FixedSubject:
    grant_number: str = ""
    category: str = ""
    subcategory: str = ""
    appl_id: str = ""
    extract: str = "1"
    document_date: str = ""
    document_id: str = ""


@dataclass(frozen=True)
class DocumentPlan:
    category: str
    subcategory: str
    appl_id: str
    document_date: str
    document_id: str
    format: str
    qc_required: bool
    attachment: GraphAttachment | None = None
    # 'pdf' means a faithful rendering is required, NOT that conversion exists.


def parse_subject(subject: str) -> FixedSubject:
    """Replicate the C# Contains-key and exactly-one-equals subject parser."""
    values: dict[str, str] = {}
    for part in subject.split(","):
        lower = part.strip().lower()
        for marker, field in (
            ("grantnumber", "grant_number"),
            ("category", "category"),
            ("applid", "appl_id"),
            ("documentdate", "document_date"),
            ("documentid", "document_id"),
            ("sub=", "subcategory"),
            ("extract", "extract"),
        ):
            if marker in lower:
                pair = part.split("=")
                if len(pair) == 2 and marker.replace("=", "") in pair[0].strip().lower():
                    values[field] = pair[1].strip()
                break
    values["extract"] = values.get("extract") or "1"
    return FixedSubject(**values)


def normalize_grant(text: str) -> str:
    if not text:
        return ""
    for old, new in (
        ("\n", "\r\n"),
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


def resolve_appl_id(subject: FixedSubject, mail: GraphMail, db: ApplicationLookup) -> str:
    if subject.appl_id.strip():
        return subject.appl_id
    if subject.grant_number.strip():
        return db.appl_id(" " + normalize_grant(subject.grant_number) + " ")
    return db.appl_id(" " + normalize_grant(mail.subject) + " ") or db.appl_id(
        " " + normalize_grant(mail.body) + " "
    )


def needs_pdf(subject: FixedSubject) -> bool:
    category, sub = subject.category, subject.subcategory
    return (
        category in ("PublicAccess", "JIT Info", "CT.gov")
        or category == "eRA Notification"
        and sub == "JIT Submitted"
        or category.lower() == "closeout"
        and sub.lower() in ("past due documents reminder", "f-rppr acceptance past due reminder")
        or category.lower() == "correspondence"
        and sub.lower() == "rppr unobligated balance"
        or category == "Funding"
        and "dci-inth" in sub.lower()
    )


def plan_documents(
    mail: GraphMail,
    attachments: list[GraphAttachment],
    db: ApplicationLookup,
) -> list[DocumentPlan]:
    subject = parse_subject(mail.subject)
    if subject.extract not in {"1", "2", "3"}:
        raise ValueError(f"Unsupported ExchangeFixed extract mode: {subject.extract!r}")
    appl_id = resolve_appl_id(subject, mail, db)
    date = subject.document_date or mail.received_at
    plans: list[DocumentPlan] = []
    if subject.extract in ("1", "3"):
        # extract=3 uses PDF only for PublicAccess; the C# body path for all
        # other categories is plain text even when extract=1 renders a PDF.
        fmt = (
            "pdf"
            if (subject.category == "PublicAccess" or subject.extract == "1" and needs_pdf(subject))
            else "txt"
        )
        plans.append(
            DocumentPlan(
                subject.category or "Correspondence",
                subject.subcategory,
                appl_id,
                date,
                subject.document_id,
                fmt,
                not bool(appl_id),
            )
        )
    if subject.extract == "2" or subject.extract == "3" and subject.category != "PublicAccess":
        for attachment in attachments:
            # The C# implementation skips ATT-prefixed filenames in the
            # attachment path (case insensitive).
            if attachment.name.upper().startswith("ATT"):
                continue
            suffix = attachment.name.rsplit(".", 1)[-1] if "." in attachment.name else "txt"
            fmt = suffix.lower()
            qc = fmt not in {"pdf", "txt", "doc", "xls", "docx", "xlsx", "ppt"}
            plans.append(
                DocumentPlan(
                    subject.category or "Correspondence",
                    subject.subcategory,
                    appl_id,
                    date,
                    subject.document_id,
                    fmt,
                    qc,
                    attachment,
                )
            )
    return plans

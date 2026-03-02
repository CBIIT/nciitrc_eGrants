"""Email router service -- ported from eMailRouter.vbs.

Processes eRA notification emails and routes them to appropriate mailboxes
based on subject line patterns. In the VBScript version, this read from
Outlook public folders. In this Python version, it can be called as a
scheduled task or API endpoint.
"""

import logging
import re

from sqlalchemy import text
from sqlalchemy.orm import Session

from . import email_service

logger = logging.getLogger(__name__)

# Max items per processing run (matches VBScript cap)
MAX_ITEMS_PER_RUN = 50

# Subject pattern routing rules (ported from eMailRouter.vbs)
ROUTING_RULES = [
    {
        "pattern": r"eSNAP Received at NIH|eRA Commons: RPPR for Grant",
        "category": "eRANotification",
        "sub_category": "RPPR Non-Compliance",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"submitted to NIH with a Non-Compliance",
        "category": "eRANotification",
        "sub_category": "RPPR Non-Compliance",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"IC ACTION REQUIRED - Relinquishing Statement",
        "category": None,
        "forward_to_efile": False,
        "forward_to_tiers": False,
        "notify_staff": True,
    },
    {
        "pattern": r"Supplement Requested through",
        "category": None,
        "forward_to_supplement": True,
    },
    {
        "pattern": r"FCOI",
        "exclude_pattern": r"Automatic reply",
        "category": None,
        "lookup_specialist": True,
    },
    {
        "pattern": r"No Cost Extension Submitted",
        "category": "eRANotification",
        "sub_category": "No Cost Extension",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"Change of Institution request",
        "category": None,
        "notify_staff": True,
    },
    {
        "pattern": r"JIT Request for Grant",
        "category": "JIT Info",
        "sub_category": "Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"JIT Documents Have Been Submitted",
        "category": "eRA Notification",
        "sub_category": "JIT Submitted",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"Overdue Progress Report",
        "category": "eRANotification",
        "sub_category": "Late Progress Report",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"Expiring Funds|EXPIRING FUNDS-",
        "category": "Closeout",
        "sub_category": "",
        "forward_to_efile": True,
        "forward_to_tiers": True,
        "extract_type": 2,  # attachments only
    },
    {
        "pattern": r"Prior Approval:",
        "category": None,
        "forward_to_post_award": True,
    },
    {
        "pattern": r"FFR NOTIFICATION : REJECTED",
        "exclude_pattern": r"^(RE:|FW:)",
        "category": "Notification",
        "sub_category": "FFR Rejection",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"The Final RPPR - Additional Materials",
        "exclude_pattern": r"^(RE:|FW:)",
        "category": "FRAM: Request",
        "sub_category": "The Final RPPR",
        "forward_to_tiers": True,
    },
    {
        "pattern": r"RPPR Unobligated Balance",
        "category": "Correspondence",
        "sub_category": "RPPR Unobligated Balance",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"PRAM for Grant",
        "exclude_pattern": r"^(RE:|FW:)",
        "category": "PRAM: Requested",
        "sub_category": "PRAM for Grant",
        "forward_to_tiers": True,
    },
    {
        "pattern": r"FRAM Requested",
        "category": "FRAM",
        "sub_category": "Request",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"PRAM Requested",
        "category": "PRAM",
        "sub_category": "Request",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"CHANGE_NOTICE_FOR.*Application is withdrawn",
        "category": "eRA Notification",
        "sub_category": "Application Withdrawn",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"IRPPR Reminder",
        "category": "IRPPR",
        "sub_category": "Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"RPPR Reminder",
        "category": "RPPR",
        "sub_category": "Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"closeout action required",
        "category": "closeout",
        "sub_category": "Past Due Documents Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"closeout program action required",
        "category": "closeout",
        "sub_category": "F-RPPR Acceptance Past Due Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"FFR Reminder|FFR Past Due",
        "category": "FFR",
        "sub_category": "Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"ClinicalTrials\.gov Results Reporting",
        "category": "CT.gov",
        "sub_category": "Results Reporting Reminder",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
    {
        "pattern": r"SBIR/STTR Foreign Risk Management",
        "category": "Funding",
        "sub_category": "DCI-InTh",
        "forward_to_efile": True,
        "forward_to_tiers": True,
    },
]


def classify_email(subject: str) -> dict | None:
    """Classify an email based on subject line patterns.

    Returns the matching routing rule or None if no match.
    """
    if not subject:
        return None

    # Skip undeliverable messages
    if "Undeliverable:" in subject:
        return None

    for rule in ROUTING_RULES:
        exclude = rule.get("exclude_pattern")
        if exclude and re.search(exclude, subject, re.IGNORECASE):
            continue

        if re.search(rule["pattern"], subject, re.IGNORECASE):
            return rule

    return None


def extract_appl_id(db: Session, text_str: str) -> int | None:
    """Extract application ID from text using the database function.

    Ports the VBScript call to dbo.Imm_fn_applid_match().
    """
    if not text_str:
        return None

    result = db.execute(
        text("SELECT dbo.Imm_fn_applid_match(:text_str) AS appl_id"),
        {"text_str": text_str},
    ).first()

    return result.appl_id if result and result.appl_id else None


def get_specialist_email(db: Session, appl_id: int) -> dict:
    """Look up the grant specialist email for an application.

    Ports the VBScript call to sp_getOfficersEmailForGrantNum.
    """
    row = db.execute(
        text(
            "EXEC sp_getOfficersEmailForGrantNum @1=:appl_id, @2='SPEC'"
        ),
        {"appl_id": appl_id},
    ).first()

    if not row:
        return {}

    return dict(row._mapping)


def route_email(
    db: Session,
    subject: str,
    body: str,
    sender: str,
    recipients: list[str] | None = None,
) -> dict:
    """Route an email based on subject classification.

    This is the main entry point for the email routing service,
    replacing the VBScript eMailRouter.vbs logic.
    """
    rule = classify_email(subject)

    if not rule:
        logger.info("No routing rule matched for subject: %s", subject[:100])
        return {"status": "unmatched", "subject": subject}

    category = rule.get("category", "")
    sub_category = rule.get("sub_category", "")

    # Extract application ID if needed
    appl_id = extract_appl_id(db, subject) or extract_appl_id(db, body)

    result = {
        "status": "routed",
        "category": category,
        "sub_category": sub_category,
        "appl_id": appl_id,
        "rule_pattern": rule["pattern"],
    }

    # Handle specialist lookup for FCOI
    if rule.get("lookup_specialist") and appl_id:
        specialist = get_specialist_email(db, appl_id)
        result["specialist"] = specialist

    logger.info(
        "Routed email: subject=%s, category=%s, sub=%s, appl_id=%s",
        subject[:80], category, sub_category, appl_id,
    )

    return result

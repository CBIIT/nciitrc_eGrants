"""Supplement service -- supplement processing and notifications.

Ported from add_supp_prod.vbs, Add_Supp_Emailer.vbs, AddSupp_VoteCollection.vbs.
"""

import logging

from sqlalchemy.orm import Session

from ..database import exec_sp
from . import email_service

logger = logging.getLogger(__name__)


def get_supplement_data(
    db: Session, act: str, grant_id: int, support_year: str,
    suffix_code: str, docid_str: str, former_applid: int,
    ic: str, operator: str,
) -> list[dict]:
    """Get supplement data via stored procedure."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_supplement "
        "@act=:act, @grant_id=:grant_id, @support_year=:support_year, "
        "@suffix_code=:suffix_code, @docid_str=:docid_str, "
        "@former_applid=:former_applid, @ic=:ic, @Operator=:operator",
        {
            "act": act,
            "grant_id": grant_id,
            "support_year": support_year,
            "suffix_code": suffix_code,
            "docid_str": docid_str,
            "former_applid": former_applid,
            "ic": ic,
            "operator": operator,
        },
    )


def send_pending_notifications(db: Session) -> dict:
    """Send unsent supplement notification emails.

    Ported from Add_Supp_Emailer.vbs -- queries adsup_Notification_email_status
    for unsent notifications and sends them via SMTP.
    """
    sent = 0
    failed = 0

    # Get unsent notification IDs
    notification_rows = exec_sp(
        db,
        "SELECT DISTINCT Notification_id "
        "FROM dbo.adsup_Notification_email_status "
        "WHERE email_date IS NULL "
        "ORDER BY Notification_id DESC",
    )

    for nrow in notification_rows:
        notification_id = nrow["Notification_id"]

        # Get email subject
        subj_rows = exec_sp(
            db,
            "SELECT dbo.fn_adsupp_getemail_subject(:nid) AS subject",
            {"nid": notification_id},
        )
        subject = subj_rows[0]["subject"] if subj_rows else ""

        # Get email body
        body_rows = exec_sp(
            db,
            "SELECT dbo.fn_adsupp_getemail_body(:nid) AS body",
            {"nid": notification_id},
        )
        body = body_rows[0]["body"] if body_rows else ""

        # Get recipient details
        detail_rows = exec_sp(
            db,
            "SELECT DISTINCT Notification_id, email, "
            "dbo.fn_adsupp_getemail_string(Notification_id, email) AS emailstr, "
            "email_template_id "
            "FROM dbo.adsup_Notification_email_status "
            "WHERE email_date IS NULL AND Notification_id = :nid",
            {"nid": notification_id},
        )

        for detail in detail_rows:
            email_type = detail["email"]
            recipients_str = detail["emailstr"]

            if not recipients_str:
                # No recipients -- mark as not sent
                exec_sp(
                    db,
                    "UPDATE dbo.adsup_Notification_email_status "
                    "SET email_date = GETDATE(), email_send_status = 'NtSend' "
                    "WHERE Notification_id = :nid",
                    {"nid": notification_id},
                )
                failed += 1
                continue

            recipients = [r.strip() for r in recipients_str.split(",") if r.strip()]

            if email_type == "to":
                success = email_service.send_email(
                    to=recipients, subject=subject, body=body,
                )
            else:
                success = email_service.send_email(
                    to=[], cc=recipients, subject=subject, body=body,
                )

            if success:
                exec_sp(
                    db,
                    "UPDATE dbo.adsup_Notification_email_status "
                    "SET email_date = GETDATE(), email_send_status = 'Send' "
                    "WHERE Notification_id = :nid",
                    {"nid": notification_id},
                )
                sent += 1
            else:
                failed += 1

    db.commit()

    return {"sent": sent, "failed": failed}


def record_vote_reply(
    db: Session, notification_id: int, sender_id: str,
) -> dict:
    """Record a vote reply (Accepted/Rejected) from a PD/PI.

    Ported from AddSupp_VoteCollection.vbs and the reply-handling
    section of add_supp_prod.vbs.
    """
    exec_sp(
        db,
        "UPDATE dbo.adsup_Notification_email_status "
        "SET reply_recieved_date = GETDATE() "
        "WHERE Notification_id = :nid "
        "AND email_address LIKE :sender_pattern",
        {"nid": notification_id, "sender_pattern": f"{sender_id}%"},
    )
    db.commit()

    return {"status": "recorded", "notification_id": notification_id}


def create_supplement_document(
    db: Session, appl_id: int, pa: str, received_time: str,
    category_name: str, file_type: str, subject: str,
    body: str, sub_category_name: str,
) -> dict:
    """Create a supplement document record via stored procedure.

    Ported from add_supp_prod.vbs call to getPlaceHolder_new.
    """
    rows = exec_sp(
        db,
        "EXEC getPlaceHolder_new "
        "@1=:appl_id, @2=:pa, @3=:received_time, "
        "@4=:category_name, @5=:file_type, @6=:subject, "
        "@7=:body, @8=:sub_category_name",
        {
            "appl_id": appl_id,
            "pa": pa,
            "received_time": received_time,
            "category_name": category_name,
            "file_type": file_type,
            "subject": subject,
            "body": body,
            "sub_category_name": sub_category_name,
        },
    )
    db.commit()

    row = rows[0] if rows else None
    file_name = row.get("ABC") if row else None
    return {"file_name": file_name}

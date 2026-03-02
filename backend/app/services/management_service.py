"""Management service -- QC, doc transaction reports, system reports."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def get_qc_queue(db: Session, ic: str) -> list[dict]:
    """Get the QC queue -- documents requiring quality control review."""
    return exec_sp(
        db,
        "SELECT d.document_id, d.appl_id, d.category_id, "
        "c.category_name, d.sub_category_name, d.document_date, "
        "d.document_name, d.url, d.created_by, d.created_date, "
        "d.page_count, d.qc_date, d.problem_msg, d.problem_reported_by "
        "FROM documents d "
        "JOIN categories c ON d.category_id = c.category_id "
        "WHERE d.qc_date IS NULL AND d.problem_msg IS NOT NULL "
        "ORDER BY d.created_date DESC",
    )


# ---------------------------------------------------------------------------
# QC Assignment
# ---------------------------------------------------------------------------

def get_qc_reasons(db: Session, ic: str) -> list[dict]:
    """Load distinct QC reasons from vw_quality_control."""
    return exec_sp(
        db,
        "SELECT DISTINCT qc_reason "
        "FROM vw_quality_control "
        "WHERE profile = :ic "
        "ORDER BY qc_reason",
        {"ic": ic},
    )


def get_specialists(db: Session, ic: str) -> list[dict]:
    """Load specialist list from vw_people (position_id > 1)."""
    return exec_sp(
        db,
        "SELECT person_name, person_id "
        "FROM vw_people "
        "WHERE ic = :ic "
        "AND application_type = 'egrants' "
        "AND position_id > 1 "
        "AND PATINDEX('%,%', person_name) > 0 "
        "ORDER BY person_name",
        {"ic": ic},
    )


def get_qc_persons(db: Session, ic: str) -> list[dict]:
    """Load current QC person assignments from vw_quality_control."""
    return exec_sp(
        db,
        "SELECT qc_reason, userid, person_id, person_name "
        "FROM vw_quality_control "
        "WHERE profile = :ic "
        "ORDER BY qc_reason",
        {"ic": ic},
    )


def get_qc_report(db: Session, ic: str) -> list[dict]:
    """Load QC route report -- files to QC and average days per specialist."""
    return exec_sp(
        db,
        "WITH qc AS ( "
        "  SELECT COUNT(*) AS files_to_qc, "
        "         AVG(DATEDIFF(D, qc_date, GETDATE())) AS qc_days, "
        "         qc_person_id "
        "  FROM egrants "
        "  WHERE qc_date IS NOT NULL "
        "    AND qc_person_id IS NOT NULL "
        "    AND qc_reason IS NOT NULL "
        "    AND disabled_date IS NULL "
        "    AND ic = :ic "
        "    AND parent_id IS NULL "
        "    AND grant_id IS NOT NULL "
        "  GROUP BY qc_person_id "
        ") "
        "SELECT qc.files_to_qc, qc.qc_days, "
        "       qc.qc_person_id, "
        "       COALESCE(vp.person_name, CAST(qc.qc_person_id AS VARCHAR(10))) AS qc_person_name "
        "FROM qc "
        "INNER JOIN vw_people vp ON qc.qc_person_id = vp.person_id",
        {"ic": ic},
    )


def qc_assign(
    db: Session,
    act: str,
    person_id: int,
    qc_person_id: int,
    qc_reason: str,
    percent: int,
    ic: str,
    operator: str,
) -> list[dict]:
    """Execute sp_web_management_qc_assign (to_assign / to_remove / to_route)."""
    result = exec_sp(
        db,
        "EXEC sp_web_management_qc_assign "
        "@act = :act, @person_id = :person_id, "
        "@qc_person_id = :qc_person_id, @qc_reason = :qc_reason, "
        "@percent = :percent, @ic = :ic, @operator = :operator",
        {
            "act": act,
            "person_id": person_id,
            "qc_person_id": qc_person_id,
            "qc_reason": qc_reason,
            "percent": percent,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()
    return result


# ---------------------------------------------------------------------------
# Document Transaction Report
# ---------------------------------------------------------------------------

def get_doc_transaction_report(
    db: Session,
    transaction_type: str,
    person_id: int,
    start_date: str | None,
    end_date: str | None,
    date_range: str | None,
    ic: str,
    operator: str,
) -> list[dict]:
    """Execute sp_web_management_doc_transaction_report."""
    return exec_sp(
        db,
        "EXEC sp_web_management_doc_transaction_report "
        "@transaction_type = :transaction_type, "
        "@startdate = :start_date, @enddate = :end_date, "
        "@date_range = :date_range, @person_id = :person_id, "
        "@ic = :ic, @operator = :operator",
        {
            "transaction_type": transaction_type,
            "start_date": start_date or "",
            "end_date": end_date or "",
            "date_range": date_range or "",
            "person_id": person_id,
            "ic": ic,
            "operator": operator,
        },
    )


def get_doc_transactions(
    db: Session, start_date: str, end_date: str, ic: str,
) -> list[dict]:
    """Get document transaction report data (legacy direct SQL)."""
    return exec_sp(
        db,
        "SELECT d.document_id, d.appl_id, a.full_grant_num, "
        "c.category_name, d.sub_category_name, d.document_date, "
        "d.created_by, d.created_date, d.modified_by, d.modified_date "
        "FROM documents d "
        "JOIN appls a ON d.appl_id = a.appl_id "
        "JOIN categories c ON d.category_id = c.category_id "
        "WHERE d.created_date BETWEEN :start_date AND :end_date "
        "ORDER BY d.created_date DESC",
        {"start_date": start_date, "end_date": end_date},
    )


# ---------------------------------------------------------------------------
# System Report
# ---------------------------------------------------------------------------

def get_accessions(db: Session, ic: str) -> list[dict]:
    """Load accession list for the given IC."""
    return exec_sp(
        db,
        "SELECT accession_id, accession_number "
        "FROM eim.dbo.accessions "
        "WHERE contract = 0 "
        "AND profile_id = (SELECT profile_id FROM profiles WHERE profile = :ic) "
        "ORDER BY accession_id DESC",
        {"ic": ic},
    )


def get_system_report(
    db: Session, act: str, search_number: int, ic: str, operator: str,
) -> list[dict]:
    """Execute sp_web_management_system_report (by_serialnumber / by_accessionid)."""
    return exec_sp(
        db,
        "EXEC sp_web_management_system_report "
        "@act = :act, @search_number = :search_number, "
        "@ic = :ic, @operator = :operator",
        {
            "act": act,
            "search_number": search_number,
            "ic": ic,
            "operator": operator,
        },
    )


# ---------------------------------------------------------------------------
# GPMAT (existing)
# ---------------------------------------------------------------------------

def get_gpmat_report(db: Session, userid: str) -> list[dict]:
    """Get GPMAT workload report data."""
    return exec_sp(
        db,
        "SELECT action_type, status_code "
        "FROM DB_GPMATS_ASSIGNMENT_STATUS "
        "ORDER BY action_type, status_code",
    )

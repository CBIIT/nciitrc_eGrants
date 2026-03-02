"""Dashboard service -- widget data aggregation."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def get_dashboard_data(db: Session, userid: str, ic: str, operator: str) -> dict:
    """Get all dashboard widget data for a user."""
    dashboard_rows = exec_sp(
        db,
        "EXEC sp_web_egrants_dashboard @act='show', @idstr='', @ic=:ic, @operator=:operator",
        {"ic": ic, "operator": operator},
    )

    link_rows = exec_sp(
        db,
        "SELECT category_name, category_id, Link_title, Link_url, "
        "sort_order, icon_name FROM DB_WIDGET_LINK "
        "WHERE end_date IS NULL ORDER BY sort_order",
    )

    expedited = exec_sp(
        db,
        "EXEC DB_GET_WIDGET_EXPEDITED_GRANTS @userid=:userid",
        {"userid": userid},
    )

    competing = exec_sp(
        db,
        "EXEC DB_LISTOF_GRANTS_TOGO_OFTYPE @userid=:userid, @type='competing'",
        {"userid": userid},
    )

    noncompeting = exec_sp(
        db,
        "EXEC DB_LISTOF_GRANTS_TOGO_OFTYPE @userid=:userid, @type='noncompeting'",
        {"userid": userid},
    )

    late = exec_sp(
        db,
        "EXEC DB_GET_WIDGET_LATEGRANTS @userid=:userid",
        {"userid": userid},
    )

    new_grants = exec_sp(
        db,
        "EXEC DB_LISTOF_NEW_GRANTS_OFTYPE @userid=:userid, @type='all'",
        {"userid": userid},
    )

    audit = exec_sp(db, "EXEC DB_GET_EGRANTS_AUDIT_REPORT")

    return {
        "widgets": [
            {"widget_id": 1, "widget_title": "Audit Report", "data": audit},
            {"widget_id": 2, "widget_title": "Expedited Grants", "data": expedited},
            {"widget_id": 3, "widget_title": "High Priority Competing Grants", "data": competing},
            {"widget_id": 4, "widget_title": "High Priority Non-Competing Grants", "data": noncompeting},
            {"widget_id": 5, "widget_title": "Late Grants", "data": late},
            {"widget_id": 6, "widget_title": "New Grants - 10 Days", "data": new_grants},
        ],
        "links": link_rows,
        "summary": dashboard_rows,
    }


def get_audit_report(db: Session) -> list[dict]:
    """Get the eGrants audit report."""
    return exec_sp(db, "EXEC DB_GET_EGRANTS_AUDIT_REPORT")

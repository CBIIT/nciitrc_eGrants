"""Admin service -- access control, flag maintenance, category edit."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def access_control(
    db: Session, act: str, index_id: int | None, active_id: int | None,
    user_id: str, login_id: str, first_name: str, middle_name: str,
    last_name: str, email_address: str, phone_number: str,
    coordinator_id: int | None, position_id: int | None, ic_id: str,
    egrants_tab: int, mgt_tab: int, admin_tab: int, docman_tab: int,
    cft_tab: int, dashboard_tab: int, iccoord_tab: int,
    is_coordinator: int, end_date: str | None, ic: str, operator: str,
) -> list[dict]:
    """Call sp_web_egrants_access_control for user management."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_access_control "
        "@act=:act, @index_id=:index_id, @active_id=:active_id, "
        "@user_id=:user_id, @login_id=:login_id, "
        "@first_name=:first_name, @middle_name=:middle_name, "
        "@last_name=:last_name, @email_address=:email_address, "
        "@phone_number=:phone_number, @coordinator_id=:coordinator_id, "
        "@position_id=:position_id, @ic_id=:ic_id, "
        "@egrants_tab=:egrants_tab, @mgt_tab=:mgt_tab, "
        "@admin_tab=:admin_tab, @docman_tab=:docman_tab, "
        "@cft_tab=:cft_tab, @dashboard_tab=:dashboard_tab, "
        "@iccoord_tab=:iccoord_tab, @is_coordinator=:is_coordinator, "
        "@end_date=:end_date, @ic=:ic, @operator=:operator",
        {
            "act": act,
            "index_id": index_id,
            "active_id": active_id,
            "user_id": user_id,
            "login_id": login_id,
            "first_name": first_name,
            "middle_name": middle_name,
            "last_name": last_name,
            "email_address": email_address,
            "phone_number": phone_number,
            "coordinator_id": coordinator_id,
            "position_id": position_id,
            "ic_id": ic_id,
            "egrants_tab": egrants_tab,
            "mgt_tab": mgt_tab,
            "admin_tab": admin_tab,
            "docman_tab": docman_tab,
            "cft_tab": cft_tab,
            "dashboard_tab": dashboard_tab,
            "iccoord_tab": iccoord_tab,
            "is_coordinator": is_coordinator,
            "end_date": end_date,
            "ic": ic,
            "operator": operator,
        },
    )

    if act in ("add", "update", "delete"):
        db.commit()

    return rows


def flag_maintenance(
    db: Session, act: str, flag_type: str, admin_code: str,
    serial_num: str, id_string: str, ic: str, operator: str,
) -> list[dict]:
    """Call sp_web_admin_flag_maintenance for grant flag management."""
    rows = exec_sp(
        db,
        "EXEC dbo.sp_web_admin_flag_maintenance "
        "@act=:act, @flag_type=:flag_type, @admin_code=:admin_code, "
        "@serial_num=:serial_num, @id_string=:id_string, "
        "@ic=:ic, @operator=:operator",
        {
            "act": act,
            "flag_type": flag_type,
            "admin_code": admin_code,
            "serial_num": serial_num,
            "id_string": id_string,
            "ic": ic,
            "operator": operator,
        },
    )

    if act in ("add", "update", "delete"):
        db.commit()

    return rows


def category_edit(
    db: Session, act: str, category_id: int | None,
    category_name: str, ic: str, operator: str,
) -> dict:
    """Call sp_web_admin_category_edit for category management."""
    rows = exec_sp(
        db,
        "DECLARE @return_notice VARCHAR(500); "
        "EXEC dbo.sp_web_admin_category_edit "
        "@act=:act, @category_id=:category_id, "
        "@category_name=:category_name, @ic=:ic, @operator=:operator, "
        "@return_notice=@return_notice OUTPUT; "
        "SELECT @return_notice AS return_notice;",
        {
            "act": act,
            "category_id": category_id,
            "category_name": category_name,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    row = rows[0] if rows else None
    return {"return_notice": row["return_notice"] if row else ""}


def ic_coordinator(
    db: Session, act: str, cord_id: int | None, request_user_id: str,
    first_name: str, middle_name: str, last_name: str, login_id: str,
    email_address: str, phone_number: str, division: str,
    access_type: str, start_date: str, end_date: str,
    comments: str, ic: str, operator: str,
) -> list[dict]:
    """Call sp_web_egrants_ic_coordinator for IC coordinator management."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_ic_coordinator "
        "@act=:act, @cord_id=:cord_id, "
        "@request_user_id=:request_user_id, "
        "@first_name=:first_name, @middle_name=:middle_name, "
        "@last_name=:last_name, @login_id=:login_id, "
        "@email_address=:email_address, @phone_number=:phone_number, "
        "@division=:division, @access_type=:access_type, "
        "@start_date=:start_date, @end_date=:end_date, "
        "@comments=:comments, @ic=:ic, @operator=:operator",
        {
            "act": act,
            "cord_id": cord_id,
            "request_user_id": request_user_id,
            "first_name": first_name,
            "middle_name": middle_name,
            "last_name": last_name,
            "login_id": login_id,
            "email_address": email_address,
            "phone_number": phone_number,
            "division": division,
            "access_type": access_type,
            "start_date": start_date,
            "end_date": end_date,
            "comments": comments,
            "ic": ic,
            "operator": operator,
        },
    )

    if act in ("add", "update", "delete"):
        db.commit()

    return rows


def get_positions(db: Session) -> list[dict]:
    """Get all position types for dropdowns."""
    return exec_sp(
        db,
        "SELECT position_id, position_name FROM people_positions ORDER BY position_name",
    )


def get_admin_codes(db: Session) -> list[dict]:
    """Get distinct admin codes from grants table for dropdowns."""
    return exec_sp(
        db,
        "SELECT DISTINCT admin_phs_org_code FROM grants WHERE admin_phs_org_code IS NOT NULL ORDER BY admin_phs_org_code",
    )

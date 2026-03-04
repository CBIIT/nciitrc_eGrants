"""Search service -- keyword search, filter search, pagination.

All searches call existing stored procedures in the EIM database.
The main sp_web_egrants procedure returns a result set with a 'tag' column:
  tag=1: Grant-level rows
  tag=2: Application-level rows
  tag=3: Document count rows
"""

from sqlalchemy.orm import Session

from ..database import exec_sp


def search_by_string(
    db: Session,
    search_str: str,
    package: str,
    ic: str,
    operator: str,
) -> dict:
    """Search grants by keyword string."""
    rows = exec_sp(
        db,
        "EXEC dbo.sp_web_egrants "
        "@str=:str, @grant_id=NULL, @package=:package, "
        "@appl_id=NULL, @current_page=1, @browser=:browser, "
        "@ic=:ic, @operator=:operator",
        {
            "str": search_str,
            "package": package,
            "browser": "Chrome",
            "ic": ic,
            "operator": operator,
        },
    )

    return _parse_tagged_results(rows, ic)


def search_by_grant(
    db: Session,
    grant_id: int,
    ic: str,
    operator: str,
) -> dict:
    """Load all applications and documents for a grant (legacy 'All' button).

    Calls sp_web_egrants with @grant_id and @package='All', replicating
    the old getByAll(grant_id) → by_grant(grant_id, 'All', 'All', cats_list, 'All').
    """
    rows = exec_sp(
        db,
        "EXEC dbo.sp_web_egrants "
        "@str=NULL, @grant_id=:grant_id, @package=:package, "
        "@appl_id=NULL, @current_page=1, @browser=:browser, "
        "@ic=:ic, @operator=:operator",
        {
            "grant_id": grant_id,
            "package": "All",
            "browser": "Chrome",
            "ic": ic,
            "operator": operator,
        },
    )

    return _parse_tagged_results(rows, ic)


def search_by_filters(
    db: Session,
    fy: str,
    mechanism: str,
    admin_code: str,
    serial_num: str,
    page_num: int,
    ic: str,
    operator: str,
) -> dict:
    """Search grants by filter criteria."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_search_by_filters "
        "@fy=:fy, @mechanism=:mechanism, @adminCode=:admin_code, "
        "@serialnum=:serial_num, @page_num=:page_num, "
        "@browser=:browser, @ic=:ic, @operator=:operator",
        {
            "fy": fy,
            "mechanism": mechanism,
            "admin_code": admin_code,
            "serial_num": serial_num,
            "page_num": page_num,
            "browser": "Chrome",
            "ic": ic,
            "operator": operator,
        },
    )

    return _parse_tagged_results(rows, ic)


def search_by_appl_id(
    db: Session,
    appl_id: int,
    search_type: str,
    category_list: str,
    ic: str,
    operator: str,
) -> dict:
    """Search by specific application ID."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_search_by_appl_id "
        "@appl_id=:appl_id, @search_type=:search_type, "
        "@category_list=:category_list, @ic=:ic, @operator=:operator",
        {
            "appl_id": appl_id,
            "search_type": search_type,
            "category_list": category_list,
            "ic": ic,
            "operator": operator,
        },
    )

    return _parse_tagged_results(rows, ic)


def get_stop_notice(db: Session, grant_id: int, ic: str) -> list[dict]:
    """Get stop notice details for a grant."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_stop_notice @GrantID=:grant_id, @ic=:ic",
        {"grant_id": grant_id, "ic": ic},
    )


def get_supplement(
    db: Session,
    act: str,
    grant_id: int,
    support_year: str,
    suffix_code: str,
    docid_str: str,
    former_applid: int,
    ic: str,
    operator: str,
) -> list[dict]:
    """Get supplement information."""
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


def load_data_years(
    db: Session, fy: str, mechanism: str, admin_code: str, serial_num: str
) -> list[dict]:
    """Load available data years for filter dropdowns.

    This SP uses dynamic SQL internally (exec(@sql)).  The actual data comes
    back in the *first* result set that has rows, but exec_sp() returns the
    *last* result set (which may contain the SQL text string).  We replicate
    the legacy .NET SqlDataReader behaviour by taking the first result set
    with actual rows instead.
    """
    sql = (
        "EXEC sp_web_egrants_load_data_years "
        "@fy=?, @mechanism=?, @admincode=?, @serialnum=?"
    )
    params = (fy, mechanism, admin_code, serial_num)

    raw_conn = db.connection().connection.dbapi_connection
    cursor = raw_conn.cursor()
    cursor.execute(sql, params)

    # Take the first result set that has columns and rows (matches .NET behaviour)
    rows: list[dict] = []
    while True:
        if cursor.description:
            columns = [col[0] for col in cursor.description]
            fetched = cursor.fetchall()
            if fetched:
                rows = [dict(zip(columns, row)) for row in fetched]
                # Consume remaining result sets so the connection is clean
                while cursor.nextset():
                    pass
                break
        if not cursor.nextset():
            break

    cursor.close()
    return rows


def autocomplete_fy(
    db: Session, term: str, fy: str, mechanism: str, admin_code: str, serial_num: str
) -> list[str]:
    """Autocomplete fiscal year values."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_load_data_autocomplete_fy "
        "@term=:term, @fy=:fy, @mechanism=:mechanism, "
        "@admincode=:admin_code, @serialnum=:serial_num",
        {
            "term": term,
            "fy": fy,
            "mechanism": mechanism,
            "admin_code": admin_code,
            "serial_num": serial_num,
        },
    )

    return [str(list(r.values())[0]) for r in rows]


def autocomplete_mechanism(
    db: Session, term: str, fy: str, mechanism: str, admin_code: str, serial_num: str
) -> list[str]:
    """Autocomplete mechanism values."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_load_data_autocomplete_mechanism "
        "@term=:term, @fy=:fy, @mechanism=:mechanism, "
        "@admincode=:admin_code, @serialnum=:serial_num",
        {
            "term": term,
            "fy": fy,
            "mechanism": mechanism,
            "admin_code": admin_code,
            "serial_num": serial_num,
        },
    )

    return [str(list(r.values())[0]) for r in rows]


def autocomplete_serial_num(
    db: Session, term: str, fy: str, mechanism: str, admin_code: str, serial_num: str
) -> list[str]:
    """Autocomplete serial number values."""
    rows = exec_sp(
        db,
        "EXEC sp_web_egrants_load_data_autocomplete_serialnum "
        "@term=:term, @fy=:fy, @mechanism=:mechanism, "
        "@admincode=:admin_code, @serialnum=:serial_num",
        {
            "term": term,
            "fy": fy,
            "mechanism": mechanism,
            "admin_code": admin_code,
            "serial_num": serial_num,
        },
    )

    return [str(list(r.values())[0]) for r in rows]


def get_all_appls_list(db: Session, admin_code: str, serial_num: str) -> list[dict]:
    """Get applications list for grant year dropdown (matches old GetAllApplsList)."""
    return exec_sp(
        db,
        "SELECT full_grant_num, appl_id FROM vw_appls "
        "WHERE admin_phs_org_code = :admin_code AND serial_num = :serial_num "
        "ORDER BY support_year DESC",
        {"admin_code": admin_code, "serial_num": serial_num},
    )


def create_grant_year(
    db: Session,
    grant_id: int,
    appl_type_code: int,
    activity_code: str,
    admin_code: str,
    serial_num: str,
    support_year: str,
    suffix_code: str,
) -> dict:
    """Create a new grant year (appl record) for grants not found in IMPAC.

    Builds full_grant_num and inserts directly into appls table,
    matching the old system's 'Create Grant Year' forced-entry feature.
    """
    full_grant_num = f"{appl_type_code}{activity_code}{admin_code}{serial_num}-{support_year}{suffix_code}"

    raw_conn = db.connection().connection.dbapi_connection
    cursor = raw_conn.cursor()
    cursor.execute(
        "INSERT INTO [EIM].[dbo].[appls] "
        "(grant_id, appl_type_code, full_grant_num, support_year, suffix_code) "
        "VALUES (?, ?, ?, ?, ?)",
        (grant_id, appl_type_code, full_grant_num, support_year, suffix_code),
    )
    # Retrieve the identity value for the new row
    cursor.execute("SELECT SCOPE_IDENTITY() AS appl_id")
    row = cursor.fetchone()
    appl_id = int(row[0]) if row and row[0] else None
    cursor.close()
    raw_conn.commit()

    return {"appl_id": appl_id, "full_grant_num": full_grant_num}


def set_grant_year_label(db: Session, appl_id: int, label: str) -> None:
    """Update the label (request name) for an application."""
    raw_conn = db.connection().connection.dbapi_connection
    cursor = raw_conn.cursor()
    cursor.execute(
        "UPDATE [EIM].[dbo].[appls] SET label=? WHERE appl_id=?",
        (label, appl_id),
    )
    cursor.close()
    raw_conn.commit()


def _parse_tagged_results(rows: list[dict], ic: str = "") -> dict:
    """Parse the tagged result set from sp_web_egrants.

    The stored procedure returns rows with a 'tag' column:
      tag=1 -> grant-level data
      tag=2 -> application-level data
      tag=3 -> document count data
    """
    grants = []
    applications = []
    doc_counts = []
    total_count = 0

    for row_dict in rows:
        tag = row_dict.get("tag")

        if tag == 1:
            grants.append(row_dict)
        elif tag == 2:
            applications.append(row_dict)
        elif tag == 3:
            doc_counts.append(row_dict)

        if "total_count" in row_dict and row_dict["total_count"]:
            total_count = row_dict["total_count"]

    # Enrich application rows with can_rename_label permission
    ic_lower = ic.lower()
    for appl in applications:
        support_year = str(appl.get("support_year", "")).lower()
        appl["can_rename_label"] = "y" if (
            ic_lower in ("ca", "nci")
            and str(appl.get("appl_type_code", "")) == "3"
            and any(c in support_year for c in ("s", "w"))
        ) else "n"

    return {
        "grants": grants,
        "applications": applications,
        "doc_counts": doc_counts,
        "total_count": total_count,
    }

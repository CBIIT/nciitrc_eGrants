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

    return _parse_tagged_results(rows)


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

    return _parse_tagged_results(rows)


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

    return _parse_tagged_results(rows)


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
    """Load available data years for filter dropdowns."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_load_data_years "
        "@fy=:fy, @mechanism=:mechanism, "
        "@admincode=:admin_code, @serialnum=:serial_num",
        {
            "fy": fy,
            "mechanism": mechanism,
            "admin_code": admin_code,
            "serial_num": serial_num,
        },
    )


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


def _parse_tagged_results(rows: list[dict]) -> dict:
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

    return {
        "grants": grants,
        "applications": applications,
        "doc_counts": doc_counts,
        "total_count": total_count,
    }

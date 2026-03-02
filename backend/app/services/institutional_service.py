"""Institutional files service -- organization document management."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def show_orgs(db: Session, index_id: int) -> list[dict]:
    """Get organizations for the institutional files view."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_inst_files_show_orgs @index_id=:index_id",
        {"index_id": index_id},
    )


def search_orgs(db: Session, search_str: str) -> list[dict]:
    """Search organizations by name."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_inst_files_search_orgs @str=:str",
        {"str": search_str},
    )


def find_org(db: Session, org_id: int, org_name: str) -> list[dict]:
    """Find a specific organization."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_institutional_file_find_org "
        "@org_id=:org_id, @org_name=:org_name",
        {"org_id": org_id, "org_name": org_name},
    )


def show_docs(db: Session, org_id: int) -> list[dict]:
    """Get documents for an organization."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_inst_files_show_docs @org_id=:org_id",
        {"org_id": org_id},
    )


def disable_doc(db: Session, doc_id: int, user_id: str) -> dict:
    """Disable an institutional document."""
    exec_sp(
        db,
        "EXEC sp_web_egrants_inst_files_disable_doc "
        "@doc_id=:doc_id, @user_id=:user_id",
        {"doc_id": doc_id, "user_id": user_id},
    )
    db.commit()

    return {"status": "disabled"}


def create_institutional_file(
    db: Session, org_id: int, category_id: int, file_type: str,
    start_date: str, end_date: str, ic: str, operator: str, comments: str,
) -> dict:
    """Create a new institutional file via stored procedure."""
    rows = exec_sp(
        db,
        "DECLARE @document_id INT; "
        "EXEC sp_web_egrants_institutional_file_create "
        "@org_id=:org_id, @category_id=:category_id, "
        "@file_type=:file_type, @start_date=:start_date, "
        "@end_date=:end_date, @ic=:ic, @operator=:operator, "
        "@document_id=@document_id OUTPUT, @comments=:comments; "
        "SELECT @document_id AS document_id;",
        {
            "org_id": org_id,
            "category_id": category_id,
            "file_type": file_type,
            "start_date": start_date,
            "end_date": end_date,
            "ic": ic,
            "operator": operator,
            "comments": comments,
        },
    )
    db.commit()

    row = rows[0] if rows else None
    return {"document_id": row["document_id"] if row else None}


def update_institutional_file(
    db: Session, category_id: int, start_date: str, end_date: str,
    ic: str, operator: str, document_id: int, comments: str,
) -> dict:
    """Update an existing institutional file."""
    exec_sp(
        db,
        "EXEC sp_web_egrants_institutional_file_update "
        "@category_id=:category_id, @start_date=:start_date, "
        "@end_date=:end_date, @ic=:ic, @operator=:operator, "
        "@document_id=:document_id, @comments=:comments",
        {
            "category_id": category_id,
            "start_date": start_date,
            "end_date": end_date,
            "ic": ic,
            "operator": operator,
            "document_id": document_id,
            "comments": comments,
        },
    )
    db.commit()

    return {"document_id": document_id, "status": "updated"}


def get_org_categories(db: Session) -> list[dict]:
    """Get organization document categories."""
    return exec_sp(
        db,
        "SELECT doctype_id, doctype_name, tobe_flagged, "
        "Flag_period, comments_required, active "
        "FROM Org_Categories WHERE active = 'Y' "
        "ORDER BY doctype_name",
    )

"""Funding service -- funding document management."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def get_funding_docs(
    db: Session, act: str, serial_num: str, fy: str, ic: str, operator: str,
) -> list[dict]:
    """Get funding documents for a grant."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_funding_docs "
        "@act=:act, @serial_num=:serial_num, @fy=:fy, "
        "@ic=:ic, @Operator=:operator",
        {
            "act": act,
            "serial_num": serial_num,
            "fy": fy,
            "ic": ic,
            "operator": operator,
        },
    )


def create_funding_doc(
    db: Session, appl_id: int, category_id: int, doc_date: str,
    sub_category: str, file_type: str, ic: str, operator: str,
) -> dict:
    """Create a new funding document via stored procedure."""
    rows = exec_sp(
        db,
        "DECLARE @DocumentID INT; "
        "EXEC sp_web_egrants_funding_doc_create "
        "@ApplID=:appl_id, @CategoryID=:category_id, "
        "@DocDate=:doc_date, @SubCategory=:sub_category, "
        "@FileType=:file_type, @ic=:ic, @operator=:operator, "
        "@DocumentID=@DocumentID OUTPUT; "
        "SELECT @DocumentID AS document_id;",
        {
            "appl_id": appl_id,
            "category_id": category_id,
            "doc_date": doc_date,
            "sub_category": sub_category,
            "file_type": file_type,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    row = rows[0] if rows else None
    return {"document_id": row["document_id"] if row else None}


def edit_funding_doc(
    db: Session, act: str, appl_id: int, document_id: int, ic: str, operator: str,
) -> dict:
    """Edit or delete a funding document."""
    exec_sp(
        db,
        "EXEC sp_web_egrants_funding_doc_edit "
        "@act=:act, @appl_id=:appl_id, @document_id=:document_id, "
        "@ic=:ic, @Operator=:operator",
        {
            "act": act,
            "appl_id": appl_id,
            "document_id": document_id,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    return {"status": "ok"}


def edit_funding_appl(
    db: Session, act: str, appl_id: int, document_id: int, ic: str, operator: str,
) -> dict:
    """Edit funding application assignment."""
    exec_sp(
        db,
        "EXEC sp_web_egrants_funding_appl_edit "
        "@act=:act, @appl_id=:appl_id, @document_id=:document_id, "
        "@ic=:ic, @Operator=:operator",
        {
            "act": act,
            "appl_id": appl_id,
            "document_id": document_id,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    return {"status": "ok"}

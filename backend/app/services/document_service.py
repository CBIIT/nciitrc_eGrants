"""Document service -- document CRUD, grid display, QC, file operations."""

import os

from sqlalchemy.orm import Session

from ..config import settings
from ..database import exec_sp


def get_document_grid(
    db: Session, appl_id: int, search_type: str, category_list: str,
    ic: str, operator: str,
) -> dict:
    """Get the document grid for an application."""
    documents = exec_sp(
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

    return {"documents": documents}


def create_document(
    db: Session, appl_id: int, category_id: int, sub_category: str,
    doc_date: str, file_type: str, ic: str, operator: str,
) -> dict:
    """Create a new document record via stored procedure.

    Returns the document_id from the OUTPUT parameter.
    Exact SP param names from old EgrantsDoc.GetDocID():
      @ApplID, @CategoryID, @SubCategory, @DocDate (PascalCase)
      @file_type, @ic, @operator (snake_case/lowercase)
      @DocumentID OUTPUT (PascalCase)
    """
    rows = exec_sp(
        db,
        "DECLARE @DocumentID INT; "
        "EXEC sp_web_egrants_doc_create "
        "@ApplID=:appl_id, @CategoryID=:category_id, "
        "@SubCategory=:sub_category, @DocDate=:doc_date, "
        "@file_type=:file_type, @ic=:ic, @operator=:operator, "
        "@DocumentID=@DocumentID OUTPUT; "
        "SELECT @DocumentID AS document_id;",
        {
            "appl_id": appl_id,
            "category_id": category_id,
            "sub_category": sub_category,
            "doc_date": doc_date,
            "file_type": file_type,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    row = rows[0] if rows else None
    document_id = row["document_id"] if row else None
    return {"document_id": document_id}


def modify_document(
    db: Session, act: str, appl_id: int, category_id: int,
    sub_category: str, doc_date: str, docid_str: str,
    file_type: str, ic: str, operator: str,
) -> dict:
    """Modify an existing document via stored procedure.

    Exact SP param names from old EgrantsDoc.doc_modify() (all snake_case):
      @act, @appl_id, @category_id, @sub_category, @doc_date,
      @docid_str (VARCHAR - document ID(s) as string), @file_type, @ic, @operator
    """
    exec_sp(
        db,
        "EXEC sp_web_egrants_doc_modify "
        "@act=:act, @appl_id=:appl_id, @category_id=:category_id, "
        "@sub_category=:sub_category, @doc_date=:doc_date, "
        "@docid_str=:docid_str, @file_type=:file_type, "
        "@ic=:ic, @operator=:operator",
        {
            "act": act,
            "appl_id": appl_id,
            "category_id": category_id,
            "sub_category": sub_category,
            "doc_date": doc_date,
            "docid_str": docid_str,
            "file_type": file_type,
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()

    return {"docid_str": docid_str, "status": "modified"}


def qc_action(db: Session, act: str, docids: str, ic: str, operator: str) -> dict:
    """Store, delete, or restore documents via stored procedure.

    Matches old system's EgrantsDoc.doc_modify(act, 0, 0, '', '', docids, '', ic, operator).
    """
    exec_sp(
        db,
        "EXEC sp_web_egrants_doc_modify "
        "@act=:act, @appl_id=0, @category_id=0, "
        "@sub_category=:sub_category, @doc_date=:doc_date, "
        "@docid_str=:docids, @file_type=:file_type, "
        "@ic=:ic, @operator=:operator",
        {
            "act": act,
            "sub_category": "",
            "doc_date": "",
            "docids": docids,
            "file_type": "",
            "ic": ic,
            "operator": operator,
        },
    )
    db.commit()
    return {"ok": True}


def get_impac_docs(db: Session, act: str, appl_id: int) -> list[dict]:
    """Get IMPAC document references."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_impac_docs @act=:act, @appl_id=:appl_id",
        {"act": act, "appl_id": appl_id},
    )


def save_uploaded_file(
    document_id: int, file_bytes: bytes, file_ext: str, path_type: str = "new",
) -> str:
    """Save an uploaded file to the file server.

    Returns the relative URL path to the saved file.
    """
    if path_type == "new":
        base_dir = os.path.join(settings.web_grant_url, "funded2", "nci", "main")
    else:
        base_dir = os.path.join(settings.web_grant_url, "funded", "nci", "modify")

    os.makedirs(base_dir, exist_ok=True)

    filename = f"{document_id}{file_ext}"
    filepath = os.path.join(base_dir, filename)

    with open(filepath, "wb") as f:
        f.write(file_bytes)

    return filename


def get_download_url(document_id: int, url: str) -> str:
    """Build the full download URL for a document."""
    if url and url.startswith("http"):
        return url
    return f"{settings.image_server_url}/{url}" if url else ""


def get_categories_for_grant(db: Session, grant_id: int, years: str) -> list[dict]:
    """Load the category list for a grant across selected years."""
    return exec_sp(
        db,
        "EXEC sp_web_egrants_load_category_list "
        "@grant_id=:grant_id, @years=:years",
        {"grant_id": grant_id, "years": years},
    )

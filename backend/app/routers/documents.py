from fastapi import APIRouter, Depends, File, HTTPException, UploadFile
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..schemas.document import DocumentCreateRequest, DocumentModifyRequest, DocumentQcRequest
from ..services import document_service

router = APIRouter(prefix="/api/documents", tags=["documents"])


@router.get("/grid/{appl_id}")
def get_document_grid(
    appl_id: int,
    search_type: str = "",
    category_list: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.get_document_grid(
        db, appl_id, search_type, category_list, user.ic, user.userid,
    )


@router.post("/create")
def create_document(
    data: DocumentCreateRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.create_document(
        db, data.appl_id, data.category_id, data.sub_category,
        data.document_date, data.file_type, user.ic, user.userid,
    )


@router.post("/modify")
def modify_document(
    data: DocumentModifyRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.modify_document(
        db, "modify", data.appl_id, data.category_id,
        data.sub_category, data.document_date, str(data.document_id),
        "", user.ic, user.userid,
    )


@router.post("/qc-action")
def qc_action(
    data: DocumentQcRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.qc_action(db, data.act, data.docids, user.ic, user.userid)


@router.post("/upload/{document_id}")
async def upload_file(
    document_id: int,
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    """Save uploaded file to disk.

    Old system flow for NEW documents (doc_create_by_file):
      sp_web_egrants_doc_create → save file to funded2/nci/main/ → done.
      The create SP already sets the url column. No modify call.

    Old system flow for EXISTING docs (doc_upload_by_file):
      sp_web_egrants_doc_modify("to_upload") → save file to funded/nci/modify/.
      The modify SP updates the url column to the modify path.
    """
    file_bytes = await file.read()
    ext = ""
    if file.filename and "." in file.filename:
        ext = "." + file.filename.rsplit(".", 1)[1].lower()

    # New documents already have their url set by sp_web_egrants_doc_create;
    # just save the file to the "main" directory (funded2/nci/main/).
    filename = document_service.save_uploaded_file(document_id, file_bytes, ext)

    return {"document_id": document_id, "filename": filename}


@router.post("/upload-as-pdf/{document_id}")
async def upload_file_as_pdf(
    document_id: int,
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    """Convert uploaded file to PDF and save to disk.

    Matches old system's convert_to_pdf_by_ddrop: converts the file
    server-side then saves as {document_id}.pdf.
    """
    file_bytes = await file.read()
    ext = ""
    if file.filename and "." in file.filename:
        ext = "." + file.filename.rsplit(".", 1)[1].lower()

    try:
        pdf_bytes = document_service.convert_to_pdf(file_bytes, ext)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))

    filename = document_service.save_uploaded_file(document_id, pdf_bytes, ".pdf")

    return {"document_id": document_id, "filename": filename}


@router.get("/impac/{appl_id}")
def get_impac_docs(
    appl_id: int,
    act: str = "show",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.get_impac_docs(db, act, appl_id)


@router.get("/categories/{grant_id}")
def get_categories_for_grant(
    grant_id: int,
    years: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return document_service.get_categories_for_grant(db, grant_id, years)

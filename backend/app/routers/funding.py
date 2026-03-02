from fastapi import APIRouter, Depends, File, UploadFile
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..schemas.funding import FundingDocCreateRequest
from ..services import document_service, funding_service

router = APIRouter(prefix="/api/funding", tags=["funding"])


@router.get("")
def get_funding_docs(
    serial_num: str = "",
    fy: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return funding_service.get_funding_docs(db, "show", serial_num, fy, user.ic, user.userid)


@router.post("/create")
def create_funding_doc(
    data: FundingDocCreateRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return funding_service.create_funding_doc(
        db, data.appl_id, data.category_id, data.doc_date,
        data.sub_category, data.file_type, user.ic, user.userid,
    )


@router.post("/upload/{document_id}")
async def upload_funding_file(
    document_id: int,
    file: UploadFile = File(...),
    user: UserInfo = Depends(require_authorized),
):
    file_bytes = await file.read()
    ext = ""
    if file.filename and "." in file.filename:
        ext = "." + file.filename.rsplit(".", 1)[1].lower()

    filename = document_service.save_uploaded_file(document_id, file_bytes, ext)
    return {"document_id": document_id, "filename": filename}


@router.post("/edit")
def edit_funding_doc(
    act: str,
    appl_id: int,
    document_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return funding_service.edit_funding_doc(db, act, appl_id, document_id, user.ic, user.userid)


@router.post("/appl-edit")
def edit_funding_appl(
    act: str,
    appl_id: int,
    document_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return funding_service.edit_funding_appl(db, act, appl_id, document_id, user.ic, user.userid)

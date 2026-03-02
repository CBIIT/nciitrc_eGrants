from fastapi import APIRouter, Depends, File, UploadFile, Query
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..schemas.institutional import InstitutionalFileCreateRequest, InstitutionalFileUpdateRequest
from ..services import document_service, institutional_service

router = APIRouter(prefix="/api/institutional", tags=["institutional"])


@router.get("/orgs")
def show_orgs(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.show_orgs(db, user.person_id or 0)


@router.get("/orgs/search")
def search_orgs(
    q: str = Query(..., min_length=1),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.search_orgs(db, q)


@router.get("/orgs/{org_id}")
def find_org(
    org_id: int,
    org_name: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.find_org(db, org_id, org_name)


@router.get("/docs/{org_id}")
def show_docs(
    org_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.show_docs(db, org_id)


@router.post("/docs/disable/{doc_id}")
def disable_doc(
    doc_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.disable_doc(db, doc_id, user.userid)


@router.post("/docs/create")
def create_institutional_file(
    data: InstitutionalFileCreateRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.create_institutional_file(
        db, data.org_id, data.category_id, data.file_type,
        data.start_date, data.end_date, user.ic, user.userid, data.comments,
    )


@router.post("/docs/update")
def update_institutional_file(
    data: InstitutionalFileUpdateRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.update_institutional_file(
        db, data.category_id, data.start_date, data.end_date,
        user.ic, user.userid, data.document_id, data.comments,
    )


@router.post("/docs/upload/{document_id}")
async def upload_institutional_file(
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


@router.get("/categories")
def get_org_categories(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return institutional_service.get_org_categories(db)

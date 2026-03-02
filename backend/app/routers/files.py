import os

from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import FileResponse
from sqlalchemy import text
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..config import settings
from ..database import get_db
from ..schemas.auth import UserInfo

router = APIRouter(prefix="/api/files", tags=["files"])


@router.get("/download/{document_id}")
def download_file(
    document_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    """Download a document file by document_id."""
    row = db.execute(
        text("SELECT url FROM documents WHERE document_id = :did"),
        {"did": document_id},
    ).first()

    if not row or not row.url:
        raise HTTPException(status_code=404, detail="Document not found")

    filepath = os.path.join(settings.web_grant_url, row.url)

    if not os.path.isfile(filepath):
        raise HTTPException(status_code=404, detail="File not found on disk")

    return FileResponse(filepath)

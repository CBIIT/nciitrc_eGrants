import os

from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import FileResponse, RedirectResponse
from sqlalchemy import text
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..config import settings
from ..database import get_db
from ..schemas.auth import UserInfo

router = APIRouter(prefix="/api/files", tags=["files"])


def _resolve_file_path(url: str) -> str:
    """Translate a documents.url value to a local filesystem path.

    The DB stores relative paths like 'data/funded2/nci/main/12345.pdf'.
    The 'data/' prefix is a legacy IIS virtual-directory mapping that does
    not exist on the local filesystem.  Strip it before joining with
    web_grant_url.
    """
    rel = url
    if rel.startswith("data/"):
        rel = rel[5:]  # strip 'data/'
    elif rel.startswith("/data/"):
        rel = rel[6:]  # strip '/data/'
    return os.path.join(settings.web_grant_url, rel)


@router.get("/download/{document_id}")
def download_file(
    document_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    """Download a document file by document_id."""
    row = db.execute(
        text("SELECT url, file_type FROM documents WHERE document_id = :did"),
        {"did": document_id},
    ).first()

    if not row or not row.url:
        raise HTTPException(status_code=404, detail="Document not found")

    url = row.url

    # External URLs (IMPAC / eRA docs) — redirect to the original source
    if url.startswith("http://") or url.startswith("https://"):
        return RedirectResponse(url)

    filepath = _resolve_file_path(url)

    if not os.path.isfile(filepath):
        raise HTTPException(status_code=404, detail="File not found on disk")

    return FileResponse(filepath)

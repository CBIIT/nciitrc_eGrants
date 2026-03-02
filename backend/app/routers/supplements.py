from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..services import supplement_service

router = APIRouter(prefix="/api/supplements", tags=["supplements"])


@router.get("")
def get_supplement_data(
    grant_id: int,
    support_year: str = "",
    suffix_code: str = "",
    docid_str: str = "",
    former_applid: int = 0,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return supplement_service.get_supplement_data(
        db, "show", grant_id, support_year, suffix_code,
        docid_str, former_applid, user.ic, user.userid,
    )


@router.post("/send-notifications")
def send_notifications(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return supplement_service.send_pending_notifications(db)

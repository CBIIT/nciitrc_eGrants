from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..services import dashboard_service

router = APIRouter(prefix="/api/dashboard", tags=["dashboard"])


@router.get("")
def get_dashboard(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return dashboard_service.get_dashboard_data(db, user.userid, user.ic, user.userid)


@router.get("/audit-report")
def get_audit_report(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return dashboard_service.get_audit_report(db)

from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..services import lookup_service

router = APIRouter(prefix="/api/lookups", tags=["lookups"])


@router.get("/categories")
def get_categories(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_categories(db, user.ic)


@router.get("/categories/{category_id}/sub-categories")
def get_sub_categories(
    category_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_sub_categories(db, category_id)


@router.get("/profiles")
def get_profiles(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_profiles(db)


@router.get("/funding-categories")
def get_funding_categories(
    fy: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_funding_categories(db, fy)


@router.get("/flag-types")
def get_flag_types(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_flag_types(db)


@router.get("/positions")
def get_positions(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return lookup_service.get_positions(db)

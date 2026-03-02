from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from ..auth.dependencies import require_admin, require_authorized
from ..database import get_db
from ..schemas.admin import (
    AccessControlRequest,
    CategoryEditRequest,
    FlagMaintenanceRequest,
    ICCoordinatorRequest,
)
from ..schemas.auth import UserInfo
from ..services import admin_service

router = APIRouter(prefix="/api/admin", tags=["admin"])


@router.get("/access")
def get_access_control(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.access_control(
        db, "show", None, None, "", "", "", "", "",
        "", "", None, None, user.ic, 0, 0, 0, 0, 0, 0, 0, 0, None, user.ic, user.userid,
    )


@router.post("/access")
def update_access_control(
    data: AccessControlRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.access_control(
        db, data.act, data.person_id, data.person_id, data.user_id,
        data.login_id, data.first_name, data.middle_name, data.last_name,
        data.email_address, data.phone_number, data.coordinator_id,
        data.position_id, data.ic_id, data.egrants_tab, data.mgt_tab,
        data.admin_tab, data.docman_tab, data.cft_tab, data.dashboard_tab,
        data.iccoord_tab, data.is_coordinator, data.end_date, user.ic, user.userid,
    )


@router.get("/flags")
def get_flags(
    flag_type: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.flag_maintenance(
        db, "show", flag_type, "", "", "", user.ic, user.userid,
    )


@router.post("/flags")
def update_flags(
    data: FlagMaintenanceRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.flag_maintenance(
        db, data.act, data.flag_type, data.admin_code,
        data.serial_num, data.id_string, user.ic, user.userid,
    )


@router.get("/categories")
def get_categories(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.category_edit(db, "show", None, "", user.ic, user.userid)


@router.post("/categories")
def update_category(
    data: CategoryEditRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_admin),
):
    return admin_service.category_edit(
        db, data.act, data.category_id, data.category_name, user.ic, user.userid,
    )


@router.get("/ic-coordinators")
def get_ic_coordinators(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return admin_service.ic_coordinator(
        db, "show", None, "", "", "", "", "", "", "", "", "", "", "", user.ic, user.userid,
    )


@router.post("/ic-coordinators")
def update_ic_coordinator(
    data: ICCoordinatorRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return admin_service.ic_coordinator(
        db, data.act, data.cord_id, data.request_user_id,
        data.first_name, data.middle_name, data.last_name,
        data.login_id, data.email_address, data.phone_number,
        data.division, data.access_type, data.start_date,
        data.end_date, data.comments, user.ic, user.userid,
    )


@router.get("/positions")
def get_positions(db: Session = Depends(get_db), user: UserInfo = Depends(require_admin)):
    return admin_service.get_positions(db)


@router.get("/admin-codes")
def get_admin_codes(db: Session = Depends(get_db), user: UserInfo = Depends(require_authorized)):
    return admin_service.get_admin_codes(db)

from fastapi import APIRouter, Depends, Query
from sqlalchemy.orm import Session

from ..auth.dependencies import get_current_user, require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..services import search_service

router = APIRouter(prefix="/api/search", tags=["search"])


@router.get("/by-string")
def search_by_string(
    q: str = Query(..., min_length=1),
    package: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.search_by_string(db, q, package, user.ic, user.userid)


@router.get("/by-grant/{grant_id}")
def search_by_grant(
    grant_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.search_by_grant(db, grant_id, user.ic, user.userid)


@router.get("/by-filters")
def search_by_filters(
    fy: str = "",
    mechanism: str = "",
    admin_code: str = "",
    serial_num: str = "",
    page_num: int = 1,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.search_by_filters(
        db, fy, mechanism, admin_code, serial_num, page_num, user.ic, user.userid,
    )


@router.get("/by-appl/{appl_id}")
def search_by_appl_id(
    appl_id: int,
    search_type: str = "",
    category_list: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.search_by_appl_id(
        db, appl_id, search_type, category_list, user.ic, user.userid,
    )


@router.get("/stop-notice/{grant_id}")
def get_stop_notice(
    grant_id: int,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.get_stop_notice(db, grant_id, user.ic)


@router.get("/supplement")
def get_supplement(
    grant_id: int,
    act: str = "to_view",
    support_year: str = "",
    suffix_code: str = "",
    docid_str: str = "",
    former_applid: int = 0,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.get_supplement(
        db, act, grant_id, support_year, suffix_code,
        docid_str, former_applid, user.ic, user.userid,
    )


@router.get("/data-years")
def get_data_years(
    fy: str = "",
    mechanism: str = "",
    admin_code: str = "",
    serial_num: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.load_data_years(db, fy, mechanism, admin_code, serial_num)


@router.get("/autocomplete/fy")
def autocomplete_fy(
    term: str = Query(..., min_length=1),
    fy: str = "",
    mechanism: str = "",
    admin_code: str = "",
    serial_num: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.autocomplete_fy(db, term, fy, mechanism, admin_code, serial_num)


@router.get("/autocomplete/mechanism")
def autocomplete_mechanism(
    term: str = Query(..., min_length=1),
    fy: str = "",
    mechanism: str = "",
    admin_code: str = "",
    serial_num: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.autocomplete_mechanism(db, term, fy, mechanism, admin_code, serial_num)


@router.get("/autocomplete/serial-num")
def autocomplete_serial_num(
    term: str = Query(..., min_length=1),
    fy: str = "",
    mechanism: str = "",
    admin_code: str = "",
    serial_num: str = "",
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return search_service.autocomplete_serial_num(db, term, fy, mechanism, admin_code, serial_num)

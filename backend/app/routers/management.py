from fastapi import APIRouter, Depends, Query
from pydantic import BaseModel
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo
from ..services import management_service

router = APIRouter(prefix="/api/management", tags=["management"])


# ---------------------------------------------------------------------------
# QC Assignment
# ---------------------------------------------------------------------------

@router.get("/qc")
def get_qc_queue(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_qc_queue(db, user.ic)


@router.get("/qc-reasons")
def get_qc_reasons(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_qc_reasons(db, user.ic)


@router.get("/specialists")
def get_specialists(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_specialists(db, user.ic)


@router.get("/qc-persons")
def get_qc_persons(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_qc_persons(db, user.ic)


@router.get("/qc-report")
def get_qc_report(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_qc_report(db, user.ic)


class QcAssignRequest(BaseModel):
    act: str  # to_assign | to_remove | to_route
    person_id: int = 0
    qc_person_id: int = 0
    qc_reason: str = ""
    percent: int = 0


@router.post("/qc-assign")
def qc_assign(
    body: QcAssignRequest,
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    management_service.qc_assign(
        db,
        act=body.act,
        person_id=body.person_id,
        qc_person_id=body.qc_person_id,
        qc_reason=body.qc_reason,
        percent=body.percent,
        ic=user.ic,
        operator=user.userid,
    )
    return {"ok": True}


# ---------------------------------------------------------------------------
# Document Transaction Report
# ---------------------------------------------------------------------------

@router.get("/doc-transactions")
def get_doc_transactions(
    start_date: str = Query(...),
    end_date: str = Query(...),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_doc_transactions(db, start_date, end_date, user.ic)


@router.get("/doc-transaction-report")
def get_doc_transaction_report(
    transaction_type: str = Query(...),
    person_id: int = Query(...),
    start_date: str | None = Query(None),
    end_date: str | None = Query(None),
    date_range: str | None = Query(None),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_doc_transaction_report(
        db,
        transaction_type=transaction_type,
        person_id=person_id,
        start_date=start_date,
        end_date=end_date,
        date_range=date_range,
        ic=user.ic,
        operator=user.userid,
    )


# ---------------------------------------------------------------------------
# System Report
# ---------------------------------------------------------------------------

@router.get("/accessions")
def get_accessions(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_accessions(db, user.ic)


@router.get("/system-report")
def get_system_report(
    act: str = Query(...),
    search_number: int = Query(...),
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_system_report(
        db,
        act=act,
        search_number=search_number,
        ic=user.ic,
        operator=user.userid,
    )


# ---------------------------------------------------------------------------
# GPMAT (existing)
# ---------------------------------------------------------------------------

@router.get("/gpmat-report")
def get_gpmat_report(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    return management_service.get_gpmat_report(db, user.userid)

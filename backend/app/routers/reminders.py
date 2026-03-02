from fastapi import APIRouter, Depends
from sqlalchemy import text
from sqlalchemy.orm import Session

from ..auth.dependencies import require_authorized
from ..database import get_db
from ..schemas.auth import UserInfo

router = APIRouter(prefix="/api/reminders", tags=["reminders"])


@router.get("/deactivation")
def get_deactivation_reminders(
    db: Session = Depends(get_db),
    user: UserInfo = Depends(require_authorized),
):
    """Get users approaching deactivation due to inactivity."""
    rows = db.execute(
        text(
            "SELECT p.person_id, p.userid, p.first_name, p.last_name, "
            "p.email, p.end_date "
            "FROM people p "
            "WHERE p.active = 1 "
            "AND p.end_date IS NOT NULL "
            "AND p.end_date > GETDATE() "
            "AND DATEDIFF(day, GETDATE(), p.end_date) <= 30 "
            "ORDER BY p.end_date"
        )
    ).fetchall()

    return [dict(r._mapping) for r in rows]

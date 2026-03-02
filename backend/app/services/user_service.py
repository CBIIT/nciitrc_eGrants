"""User service -- user profile and validation queries."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def validate_user(db: Session, userid: str) -> dict | None:
    """Look up a user in the people table and return profile info."""
    rows = exec_sp(
        db,
        "SELECT p.person_id, p.userid, p.first_name, p.last_name, "
        "p.middle_name, p.email, p.phone_number, p.position_id, "
        "p.active, p.ic, p.is_coordinator, p.coordinator_id, "
        "pp.position_name "
        "FROM people p "
        "LEFT JOIN people_positions pp ON p.position_id = pp.position_id "
        "WHERE p.userid = :userid AND p.active = 1",
        {"userid": userid},
    )

    return rows[0] if rows else None


def get_menu_items(db: Session, person_id: int, ic: str) -> list[dict]:
    """Get the menu items visible to this user based on their permissions."""
    return exec_sp(
        db,
        "SELECT ci.character_index, ci.index_seq "
        "FROM character_index ci "
        "WHERE ci.index_id = :person_id "
        "ORDER BY ci.index_seq",
        {"person_id": person_id},
    )

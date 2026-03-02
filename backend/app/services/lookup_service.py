"""Lookup service -- reference data for dropdowns and lists."""

from sqlalchemy.orm import Session

from ..database import exec_sp


def get_categories(db: Session, ic: str = "NCI") -> list[dict]:
    """Get document categories visible to the user's IC."""
    return exec_sp(
        db,
        "SELECT c.category_id, c.category_name, c.package, "
        "c.input_type, c.input_constraint "
        "FROM categories c "
        "LEFT JOIN categories_ic ci ON c.category_id = ci.category_id AND ci.ic = :ic "
        "WHERE ci.removed_date IS NULL "
        "ORDER BY c.category_name",
        {"ic": ic},
    )


def get_sub_categories(db: Session, category_id: int) -> list[dict]:
    """Get sub-categories for a given category."""
    return exec_sp(
        db,
        "SELECT parent_category_id, sub_category_name "
        "FROM sub_categories "
        "WHERE parent_category_id = :category_id "
        "ORDER BY sub_category_name",
        {"category_id": category_id},
    )


def get_profiles(db: Session) -> list[dict]:
    """Get all profiles."""
    return exec_sp(
        db,
        "SELECT profile_id, profile_name, admin_phs_org_code FROM profiles ORDER BY profile_name",
    )


def get_funding_categories(db: Session, fy: str = "") -> list[dict]:
    """Get funding category tree."""
    return exec_sp(
        db,
        "SELECT category_id, category_name, level_id, parent_id, category_fy "
        "FROM funding_categories "
        "WHERE (:fy = '' OR category_fy = :fy) "
        "ORDER BY level_id, category_name",
        {"fy": fy},
    )


def get_flag_types(db: Session) -> list[dict]:
    """Get grant flag types."""
    return exec_sp(
        db,
        "SELECT flag_type_code, flag_application_code, end_date "
        "FROM Grants_Flag_Master "
        "WHERE end_date IS NULL "
        "ORDER BY flag_type_code",
    )


def get_positions(db: Session) -> list[dict]:
    """Get people positions."""
    return exec_sp(
        db,
        "SELECT position_id, position_name FROM people_positions ORDER BY position_name",
    )

from pydantic import BaseModel


class UserInfo(BaseModel):
    person_id: int | None = None
    userid: str = ""
    first_name: str = ""
    last_name: str = ""
    full_name: str = ""
    email: str = ""
    ic: str = "NCI"
    position_id: int | None = None
    position_name: str = ""
    is_coordinator: bool = False
    coordinator_id: int | None = None

    # Raw menulist from sp_web_egrants_user_profile (legacy Session["Menus"])
    menulist: str = ""

    # Permission flags (parsed from menulist)
    can_egrants: bool = False
    can_mgt: bool = False
    can_admin: bool = False
    can_docman: bool = False
    can_cft: bool = False
    can_dashboard: bool = False
    can_iccoord: bool = False

    authorized: bool = False
    environment: str = ""
    version: str = ""
    build: str = ""

from datetime import datetime

from pydantic import BaseModel


class PersonOut(BaseModel):
    person_id: int
    userid: str | None = None
    first_name: str | None = None
    last_name: str | None = None
    middle_name: str | None = None
    email: str | None = None
    phone_number: str | None = None
    position_id: int | None = None
    position_name: str | None = None
    active: int | None = None
    ic: str | None = None
    is_coordinator: int | None = None
    coordinator_id: int | None = None
    start_date: datetime | None = None
    end_date: datetime | None = None
    can_egrants: bool = False
    can_mgt: bool = False
    can_admin: bool = False
    can_docman: bool = False
    can_cft: bool = False
    can_dashboard: bool = False
    can_iccoord: bool = False

    model_config = {"from_attributes": True}


class AccessControlRequest(BaseModel):
    act: str  # "add", "update", "delete", "show"
    person_id: int | None = None
    user_id: str = ""
    login_id: str = ""
    first_name: str = ""
    middle_name: str = ""
    last_name: str = ""
    email_address: str = ""
    phone_number: str = ""
    coordinator_id: int | None = None
    position_id: int | None = None
    ic_id: str = "NCI"
    egrants_tab: int = 0
    mgt_tab: int = 0
    admin_tab: int = 0
    docman_tab: int = 0
    cft_tab: int = 0
    dashboard_tab: int = 0
    iccoord_tab: int = 0
    is_coordinator: int = 0
    end_date: str | None = None


class CategoryEditRequest(BaseModel):
    act: str  # "add", "update", "delete"
    category_id: int | None = None
    category_name: str = ""


class FlagMaintenanceRequest(BaseModel):
    act: str
    flag_type: str = ""
    admin_code: str = ""
    serial_num: str = ""
    id_string: str = ""


class ICCoordinatorRequest(BaseModel):
    act: str
    cord_id: int | None = None
    request_user_id: str = ""
    first_name: str = ""
    middle_name: str = ""
    last_name: str = ""
    login_id: str = ""
    email_address: str = ""
    phone_number: str = ""
    division: str = ""
    access_type: str = ""
    start_date: str = ""
    end_date: str = ""
    comments: str = ""

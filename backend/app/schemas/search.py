from pydantic import BaseModel


class SearchByStringRequest(BaseModel):
    search_str: str
    package: str = ""
    ic: str = "NCI"


class SearchByFiltersRequest(BaseModel):
    fy: str = ""
    mechanism: str = ""
    admin_code: str = ""
    serial_num: str = ""
    page_num: int = 1
    ic: str = "NCI"


class GrantResult(BaseModel):
    grant_id: int
    serial_num: str | None = None
    admin_phs_org_code: str | None = None
    current_pi_name: str | None = None
    current_pi_email_address: str | None = None

    model_config = {"from_attributes": True}


class ApplicationResult(BaseModel):
    appl_id: int
    grant_id: int | None = None
    full_grant_num: str | None = None
    support_year: str | None = None
    project_title: str | None = None
    first_name: str | None = None
    last_name: str | None = None
    org_name: str | None = None
    label: str | None = None
    appl_type_code: int | None = None
    deleted_by_impac: str | None = None

    model_config = {"from_attributes": True}


class SearchResult(BaseModel):
    grants: list[GrantResult] = []
    applications: list[ApplicationResult] = []
    doc_counts: list[dict] = []
    total_count: int = 0
    page_num: int = 1
    message: str = ""

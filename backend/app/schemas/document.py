from datetime import datetime

from pydantic import BaseModel


class DocumentOut(BaseModel):
    document_id: int
    appl_id: int | None = None
    category_id: int | None = None
    category_name: str | None = None
    sub_category_name: str | None = None
    document_date: datetime | None = None
    document_name: str | None = None
    url: str | None = None
    created_by: str | None = None
    created_date: datetime | None = None
    modified_by: str | None = None
    modified_date: datetime | None = None
    file_modified_by: str | None = None
    file_modified_date: datetime | None = None
    page_count: int | None = None
    qc_date: datetime | None = None
    problem_msg: str | None = None
    problem_reported_by: str | None = None

    model_config = {"from_attributes": True}


class DocumentCreateRequest(BaseModel):
    appl_id: int
    category_id: int
    sub_category: str = ""
    document_date: str = ""
    file_type: str = ""


class DocumentModifyRequest(BaseModel):
    appl_id: int
    document_id: int
    category_id: int
    sub_category: str = ""
    document_date: str = ""


class DocumentQcRequest(BaseModel):
    act: str  # "to store", "to delete", "to restore", "to store all", etc.
    docids: str  # comma-separated document IDs


class DocumentGridResponse(BaseModel):
    documents: list[DocumentOut] = []
    categories: list[dict] = []
    sub_categories: list[dict] = []
    flags: list[dict] = []
    grant_info: dict = {}
    appl_info: dict = {}
    years: list[dict] = []
    message: str = ""

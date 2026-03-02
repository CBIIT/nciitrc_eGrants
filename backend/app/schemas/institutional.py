from datetime import datetime

from pydantic import BaseModel


class OrgOut(BaseModel):
    org_id: int | None = None
    org_name: str | None = None
    doc_count: int = 0


class InstitutionalDocOut(BaseModel):
    document_id: int
    org_id: int | None = None
    category_id: int | None = None
    category_name: str | None = None
    file_type: str | None = None
    start_date: datetime | None = None
    end_date: datetime | None = None
    comments: str | None = None
    disabled: bool = False


class InstitutionalFileCreateRequest(BaseModel):
    org_id: int
    category_id: int
    file_type: str = ""
    start_date: str = ""
    end_date: str = ""
    comments: str = ""


class InstitutionalFileUpdateRequest(BaseModel):
    document_id: int
    category_id: int
    start_date: str = ""
    end_date: str = ""
    comments: str = ""

from datetime import datetime

from pydantic import BaseModel


class FundingDocResponse(BaseModel):
    documents: list[dict] = []
    categories: list[dict] = []
    message: str = ""


class FundingDocCreateRequest(BaseModel):
    appl_id: int
    category_id: int
    doc_date: str = ""
    sub_category: str = ""
    file_type: str = ""


class FundingCategoryOut(BaseModel):
    category_id: int
    category_name: str | None = None
    level_id: int | None = None
    parent_id: int | None = None
    category_fy: str | None = None
    child_count: int = 0
    doc_count: int = 0

    model_config = {"from_attributes": True}

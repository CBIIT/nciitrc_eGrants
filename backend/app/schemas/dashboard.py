from pydantic import BaseModel


class WidgetData(BaseModel):
    widget_id: int
    widget_title: str = ""
    data: list[dict] = []


class DashboardResponse(BaseModel):
    widgets: list[WidgetData] = []
    links: list[dict] = []
    message: str = ""

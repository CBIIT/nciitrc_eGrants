from sqlalchemy import Column, DateTime, Integer, String

from ..database import Base


class WidgetMaster(Base):
    __tablename__ = "DB_Widget_Master"

    widget_id = Column(Integer, primary_key=True)
    widget_title = Column(String(200))
    template_name = Column(String(200))
    end_date = Column(DateTime)


class WidgetAssignment(Base):
    __tablename__ = "DB_WIDGET_ASSIGNMENT"

    widget_id = Column(Integer, primary_key=True)
    userid = Column(String(50), primary_key=True)
    end_date = Column(DateTime)


class WidgetLink(Base):
    __tablename__ = "DB_WIDGET_LINK"

    category_name = Column(String(200), primary_key=True)
    category_id = Column(Integer)
    link_title = Column("Link_title", String(200))
    link_url = Column("Link_url", String(500))
    sort_order = Column(Integer)
    icon_name = Column(String(100))
    end_date = Column(DateTime)


class GpmatsAssignmentStatus(Base):
    __tablename__ = "DB_GPMATS_ASSIGNMENT_STATUS"

    action_type = Column(String(50), primary_key=True)
    status_code = Column(String(50), primary_key=True)

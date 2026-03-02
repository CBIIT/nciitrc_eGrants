from sqlalchemy import Column, DateTime, ForeignKey, Integer, String

from ..database import Base


class FundingCategory(Base):
    __tablename__ = "funding_categories"

    category_id = Column(Integer, primary_key=True)
    category_name = Column(String(200))
    level_id = Column(Integer)
    parent_id = Column(Integer)
    category_fy = Column(String(10))


class FundingAppl(Base):
    __tablename__ = "funding_appls"

    appl_id = Column(Integer, ForeignKey("appls.appl_id"), primary_key=True)
    document_id = Column(Integer, ForeignKey("documents.document_id"), primary_key=True)
    disabled_date = Column(DateTime)

from sqlalchemy import Column, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import relationship

from ..database import Base


class Document(Base):
    __tablename__ = "documents"

    document_id = Column(Integer, primary_key=True)
    appl_id = Column(Integer, ForeignKey("appls.appl_id"))
    category_id = Column(Integer, ForeignKey("categories.category_id"))
    sub_category_name = Column(String(200))
    document_date = Column(DateTime)
    document_name = Column(String(500))
    url = Column(String(500))
    created_by = Column(String(100))
    created_date = Column(DateTime)
    modified_by = Column(String(100))
    modified_date = Column(DateTime)
    file_modified_by = Column(String(100))
    file_modified_date = Column(DateTime)
    page_count = Column(Integer)
    qc_date = Column(DateTime)
    problem_msg = Column(String(500))
    problem_reported_by = Column(String(100))

    category = relationship("Category", foreign_keys=[category_id], lazy="joined")

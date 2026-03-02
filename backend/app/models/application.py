from sqlalchemy import Column, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import relationship

from ..database import Base


class Application(Base):
    __tablename__ = "appls"

    appl_id = Column(Integer, primary_key=True)
    grant_id = Column(Integer, ForeignKey("grants.grant_id"))
    appl_type_code = Column(Integer)
    full_grant_num = Column(String(50))
    support_year = Column(String(10))
    suffix_code = Column(String(10))
    project_title = Column(String(500))
    first_name = Column(String(100))
    last_name = Column(String(100))
    org_name = Column(String(200))
    label = Column(String(100))
    deleted_by_impac = Column(String(1))

    grant = relationship("Grant", foreign_keys=[grant_id], lazy="joined")

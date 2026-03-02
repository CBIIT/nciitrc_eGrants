from sqlalchemy import Column, Integer, String

from ..database import Base


class Grant(Base):
    __tablename__ = "grants"

    grant_id = Column(Integer, primary_key=True)
    admin_phs_org_code = Column(String(10))
    serial_num = Column(String(20))
    current_pi_name = Column(String(200))
    current_pi_email_address = Column(String(200))
    current_pd_name = Column(String(200))
    current_pd_email_address = Column(String(200))
    current_spec_name = Column(String(200))
    current_spec_email_address = Column(String(200))
    current_bo_email_address = Column(String(200))

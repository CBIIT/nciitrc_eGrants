from sqlalchemy import Column, DateTime, Integer, String

from ..database import Base


class GrantsFlagMaster(Base):
    __tablename__ = "Grants_Flag_Master"

    flag_type_code = Column(String(50), primary_key=True)
    flag_application_code = Column(String(50))
    end_date = Column(DateTime)


class GrantsFlag(Base):
    __tablename__ = "grants_flags"

    gf_id = Column(Integer, primary_key=True)
    serial_num = Column(String(20))
    grant_id = Column(Integer)
    appl_id = Column(Integer)
    grant_num = Column(String(50))
    full_grant_num = Column(String(50))
    flag = Column(String(50))
    flag_type = Column(String(50))
    flag_application = Column(String(50))
    flag_icon_namepath = Column(String(200))

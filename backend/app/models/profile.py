from sqlalchemy import Column, Integer, String

from ..database import Base


class Profile(Base):
    __tablename__ = "profiles"

    profile_id = Column(Integer, primary_key=True)
    profile_name = Column(String(200))
    admin_phs_org_code = Column(String(10))



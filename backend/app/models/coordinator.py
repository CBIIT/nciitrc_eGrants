from sqlalchemy import Column, DateTime, Integer, String

from ..database import Base


class CordManager(Base):
    __tablename__ = "cord_manager"

    id = Column(Integer, primary_key=True)
    user_login = Column(String(50))
    user_fname = Column(String(100))
    user_lname = Column(String(100))
    user_mi = Column(String(10))
    email = Column(String(200))
    phone_number = Column(String(50))
    cord_id = Column(Integer)
    access_type = Column(String(50))
    start_date = Column(DateTime)
    end_date = Column(DateTime)

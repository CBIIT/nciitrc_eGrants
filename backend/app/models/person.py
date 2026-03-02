from sqlalchemy import Column, DateTime, ForeignKey, Integer, String

from ..database import Base


class Person(Base):
    __tablename__ = "people"

    person_id = Column(Integer, primary_key=True)
    userid = Column(String(50))
    first_name = Column(String(100))
    last_name = Column(String(100))
    middle_name = Column(String(100))
    email = Column(String(200))
    phone_number = Column(String(50))
    position_id = Column(Integer, ForeignKey("people_positions.position_id"))
    active = Column(Integer)
    ic = Column(String(10))
    application_type = Column(String(50))
    is_coordinator = Column(Integer)
    coordinator_id = Column(Integer)
    start_date = Column(DateTime)
    end_date = Column(DateTime)


class PeoplePosition(Base):
    __tablename__ = "people_positions"

    position_id = Column(Integer, primary_key=True)
    position_name = Column(String(100))

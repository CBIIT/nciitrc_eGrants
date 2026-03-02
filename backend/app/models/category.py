from sqlalchemy import Column, Integer, String

from ..database import Base


class Category(Base):
    __tablename__ = "categories"

    category_id = Column(Integer, primary_key=True)
    category_name = Column(String(200))
    package = Column(String(100))
    input_type = Column(String(50))
    input_constraint = Column(String(200))


class SubCategory(Base):
    __tablename__ = "sub_categories"

    parent_category_id = Column(Integer, primary_key=True)
    sub_category_name = Column(String(200), primary_key=True)


class CategoryIc(Base):
    __tablename__ = "categories_ic"

    category_id = Column(Integer, primary_key=True)
    ic = Column(String(10), primary_key=True)
    removed_date = Column(String(50))


class OrgCategory(Base):
    __tablename__ = "Org_Categories"

    doctype_id = Column("doctype_id", Integer, primary_key=True)
    doctype_name = Column("doctype_name", String(200))
    tobe_flagged = Column(String(1))
    flag_period = Column("Flag_period", Integer)
    comments_required = Column(String(1))
    active = Column(String(1))

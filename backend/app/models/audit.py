from sqlalchemy import Column, DateTime, Integer, String

from ..database import Base


class EgrantsAuditReport(Base):
    __tablename__ = "egrants_audit_report"

    id = Column(Integer, primary_key=True, autoincrement=True)
    report_name = Column("Report_name", String(200))
    file_name = Column("File_name", String(200))
    run_date = Column("Run_date", DateTime)
    url = Column(String(500))

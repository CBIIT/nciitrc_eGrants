from sqlalchemy import Column, DateTime, Integer, String

from ..database import Base


class SuppNotificationEmailStatus(Base):
    __tablename__ = "adsup_Notification_email_status"

    notification_id = Column("Notification_id", Integer, primary_key=True)
    email = Column(String(200), primary_key=True)
    email_template_id = Column(Integer)
    email_date = Column(DateTime)
    email_send_status = Column(String(20))
    reply_recieved_date = Column(DateTime)
    email_address = Column(String(200))


class SuppNotification(Base):
    __tablename__ = "adsup_notification"

    id = Column(Integer, primary_key=True)
    appl_id = Column(Integer)

from urllib.parse import quote_plus

from sqlalchemy import create_engine
from sqlalchemy.orm import DeclarativeBase, sessionmaker

from .config import settings

engine = create_engine(
    "mssql+pyodbc:///?odbc_connect=" + quote_plus(settings.database_odbc),
    pool_pre_ping=True,
)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


class Base(DeclarativeBase):
    pass


def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def exec_sp(db, sql: str, params: dict | None = None) -> list[dict]:
    """Execute a stored procedure and return the last result set with data.

    SQL Server SPs often emit multiple result sets (row counts from INSERT/UPDATE
    when SET NOCOUNT is OFF). This helper skips empty result sets and returns
    the last one that has actual columns/rows, as a list of dicts.
    """
    raw_conn = db.connection().connection.dbapi_connection
    cursor = raw_conn.cursor()

    # Convert :named params to ? positional for pyodbc
    if params:
        for key, val in params.items():
            sql = sql.replace(f":{key}", "?")
        cursor.execute(sql, tuple(params.values()))
    else:
        cursor.execute(sql)

    # Iterate result sets — keep the last one that has columns
    rows = []
    columns = []
    while True:
        if cursor.description:
            columns = [col[0] for col in cursor.description]
            rows = cursor.fetchall()
        if not cursor.nextset():
            break

    cursor.close()

    return [dict(zip(columns, row)) for row in rows]

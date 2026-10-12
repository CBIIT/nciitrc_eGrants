"""SQL Server lookups used by the original Router; imported only in live mode."""

import os
from types import TracebackType
from typing import Any


class SqlGrantLookup:
    def __init__(self, connection_string: str) -> None:
        if not connection_string:
            raise ValueError("EIM connection string is required")
        import pyodbc  # type: ignore[import-not-found]

        parts = dict(part.split("=", 1) for part in connection_string.split(";") if "=" in part)
        aliases = {"Data Source": "SERVER", "Initial Catalog": "DATABASE",
                   "User ID": "UID", "Password": "PWD"}
        allowed = {"Data Source", "Initial Catalog", "User ID", "Password"}
        odbc = {aliases[key]: value for key, value in parts.items() if key in allowed}
        if not all(key in odbc for key in ("SERVER", "DATABASE", "UID", "PWD")):
            raise ValueError("Connection string must specify server, database, user and password")
        odbc["DRIVER"] = os.getenv("EGRANTS_ODBC_DRIVER", "ODBC Driver 18 for SQL Server")
        odbc["Encrypt"] = os.getenv("EGRANTS_ODBC_ENCRYPT", "yes")
        odbc["TrustServerCertificate"] = os.getenv("EGRANTS_ODBC_TRUST_CERT", "no")
        self._connection: Any = pyodbc.connect(
            ";".join(f"{key}={value}" for key, value in odbc.items()), autocommit=True
        )

    def appl_id(self, text: str) -> str:
        cursor = self._connection.cursor()
        try:
            row = cursor.execute("SELECT dbo.Imm_fn_applid_match(?) AS applid", text).fetchone()
            return str(row[0]) if row and row[0] is not None else ""
        finally:
            cursor.close()

    def officer_emails(self, appl_id: str) -> tuple[str, str]:
        cursor = self._connection.cursor()
        primary, backup = "", ""
        try:
            for row in cursor.execute(
                "EXEC sp_getOfficersEmailForGrantNum @APPLID=?, @OffCode=?", int(appl_id), "SPEC"
            ):
                primary = str(row.Email_address_p or "")
                backup = str(row.Email_address_b or "")
        finally:
            cursor.close()
        return primary, backup

    def close(self) -> None:
        self._connection.close()

    def __enter__(self) -> "SqlGrantLookup":
        return self

    def __exit__(self, exc_type: type[BaseException] | None,
                 exc: BaseException | None, tb: TracebackType | None) -> None:
        self.close()
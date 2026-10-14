"""SQL Server lookups used by the original Router; imported only in live mode."""

import os
from types import TracebackType
from typing import Any


class OdbcDriverError(RuntimeError):
    """Safe driver-availability diagnostic without connection details."""


class SqlConnectionError(ConnectionError):
    """Safe SQL connection diagnostic without raw ODBC error text."""


def _odbc_driver(available: list[str]) -> str:
    for name in ("ODBC Driver 18 for SQL Server", "ODBC Driver 17 for SQL Server"):
        if name in available:
            return name
    raise OdbcDriverError(
        "No supported SQL Server ODBC driver is installed. "
        "Install ODBC Driver 18 for SQL Server (or the approved Driver 17)."
    )


class SqlGrantLookup:
    def __init__(self, connection_string: str) -> None:
        if not connection_string:
            raise ValueError("EIM connection string is required")
        import pyodbc  # type: ignore[import-not-found]

        parts = dict(
            part.strip().split("=", 1) for part in connection_string.split(";") if "=" in part
        )
        aliases = {
            "data source": "SERVER",
            "initial catalog": "DATABASE",
            "user id": "UID",
            "password": "PWD",
        }
        odbc = {
            aliases[key.casefold()]: value
            for key, value in parts.items()
            if key.casefold() in aliases
        }
        if not all(key in odbc for key in ("SERVER", "DATABASE", "UID", "PWD")):
            raise ValueError("Connection string must specify server, database, user and password")
        odbc["DRIVER"] = os.getenv("EGRANTS_ODBC_DRIVER") or _odbc_driver(pyodbc.drivers())
        odbc["Encrypt"] = os.getenv("EGRANTS_ODBC_ENCRYPT", "yes")
        odbc["TrustServerCertificate"] = os.getenv("EGRANTS_ODBC_TRUST_CERT", "no")
        try:
            self._connection: Any = pyodbc.connect(
                ";".join(f"{key}={value}" for key, value in odbc.items()), autocommit=True
            )
        except pyodbc.Error as error:
            details = str(error).casefold()
            if (
                "certificate chain was issued by an authority that is not trusted" in details
                or "certificate verify failed" in details
                or "self-signed certificate" in details
            ):
                raise SqlConnectionError(
                    "SQL Server TLS certificate chain is not trusted. Install the approved "
                    "organizational CA certificate(s) or have the SQL administrator fix the "
                    "server chain. Keep Encrypt=yes and TrustServerCertificate=no."
                ) from None
            raise SqlConnectionError(
                "Unable to connect to EIM SQL Server; check ODBC, network, TLS and account access."
            ) from None

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
                "EXEC sp_getOfficersEmailForGrantNum @APPLID=?, @OffCode=?", appl_id, "SPEC"
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

    def __exit__(
        self,
        exc_type: type[BaseException] | None,
        exc: BaseException | None,
        tb: TracebackType | None,
    ) -> None:
        self.close()

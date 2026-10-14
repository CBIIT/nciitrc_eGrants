"""SQL contract tests using synthetic cursors only; never open a connection."""

import sys
from types import SimpleNamespace
from typing import Any

import pytest

from app.database import SqlGrantLookup, _odbc_driver


def test_parameterized_lookup_and_last_officer_row(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    calls: list[tuple[Any, ...]] = []

    class Cursor:
        def execute(self, *args: Any) -> "Cursor":
            calls.append(args)
            return self

        def fetchone(self) -> tuple[int]:
            return (12345,)

        def __iter__(self) -> Any:
            return iter(
                [
                    SimpleNamespace(Email_address_p="first@example.org", Email_address_b=""),
                    SimpleNamespace(
                        Email_address_p="last@example.org", Email_address_b="backup@example.org"
                    ),
                ]
            )

        def close(self) -> None:
            pass

    class Connection:
        def cursor(self) -> Cursor:
            return Cursor()

        def close(self) -> None:
            pass

    def connect(value: str, *, autocommit: bool) -> Connection:
        assert "SERVER=synthetic" in value
        assert "DRIVER=ODBC Driver 17 for SQL Server" in value
        assert "Encrypt=yes" in value
        assert "TrustServerCertificate=no" in value
        assert autocommit
        return Connection()

    monkeypatch.delenv("EGRANTS_ODBC_DRIVER", raising=False)
    monkeypatch.setitem(
        sys.modules,
        "pyodbc",
        SimpleNamespace(connect=connect, drivers=lambda: ["ODBC Driver 17 for SQL Server"]),
    )
    connection_string = "data source=synthetic;Initial Catalog=test;User ID=user;Password=test"
    with SqlGrantLookup(connection_string) as db:
        assert db.appl_id("5R01'example") == "12345"
        assert db.officer_emails("12345") == ("last@example.org", "backup@example.org")
    assert calls == [
        ("SELECT dbo.Imm_fn_applid_match(?) AS applid", "5R01'example"),
        ("EXEC sp_getOfficersEmailForGrantNum @APPLID=?, @OffCode=?", "12345", "SPEC"),
    ]


def test_prefer_driver_18_and_require_a_modern_driver() -> None:
    assert (
        _odbc_driver(
            ["SQL Server", "ODBC Driver 17 for SQL Server", "ODBC Driver 18 for SQL Server"]
        )
        == "ODBC Driver 18 for SQL Server"
    )
    with pytest.raises(RuntimeError, match="No supported SQL Server ODBC driver"):
        _odbc_driver(["SQL Server"])


def test_untrusted_certificate_reports_actionable_message_without_secrets(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    class FakeOdbcError(Exception):
        pass

    def fail_connect(value: str, *, autocommit: bool) -> None:
        raise FakeOdbcError(
            "08001",
            "SSL Provider: The certificate chain was issued by an authority "
            "that is not trusted; Password=synthetic-secret",
        )

    monkeypatch.delenv("EGRANTS_ODBC_DRIVER", raising=False)
    monkeypatch.setitem(
        sys.modules,
        "pyodbc",
        SimpleNamespace(
            connect=fail_connect,
            drivers=lambda: ["ODBC Driver 17 for SQL Server"],
            Error=FakeOdbcError,
        ),
    )
    with pytest.raises(ConnectionError, match="organizational CA") as caught:
        SqlGrantLookup(
            "Data Source=synthetic;Initial Catalog=test;User ID=user;Password=synthetic-secret"
        )
    assert "synthetic-secret" not in str(caught.value)
    assert caught.value.__cause__ is None

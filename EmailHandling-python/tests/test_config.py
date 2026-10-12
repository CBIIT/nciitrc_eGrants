import json
from pathlib import Path

import pytest

from app.config import SOURCE_ROUTER_CONFIG, load_config


def test_overlays_and_env_take_precedence(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    (tmp_path / "appsettings.json").write_text(json.dumps({
        "EmailRecipients": {"DebugEmail": "base@example.org"},
        "AppSettings": {"dBug": "n"},
        "FolderPaths": {"dirpathRouter": "Root\\Mail"},
        "ConnectionStrings": {"EIM": "User ID=%DB_USER%;Password=%DB_PASSWORD%"},
    }), encoding="utf-8")
    (tmp_path / "appsettings.Development.json").write_text(json.dumps({
        "EmailRecipients": {"DebugEmail": "dev@example.org"},
    }), encoding="utf-8")
    monkeypatch.setenv("DOTNET_ENVIRONMENT", "Development")
    monkeypatch.setenv("DB_USER", "test-user")
    monkeypatch.setenv("DB_PASSWORD", "test-password")
    monkeypatch.setenv("EmailRecipients__DebugEmail", "env@example.org")
    config = load_config(tmp_path)
    assert config.debug_email == "env@example.org"
    assert config.folder_path == "Root\\Mail"
    assert config.debug == "n"
    assert config.connection_string == "User ID=test-user;Password=test-password"


def test_required_database_credential_is_validated(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch,
) -> None:
    (tmp_path / "appsettings.json").write_text(json.dumps({
        "ConnectionStrings": {"EIM": "Password=%DB_PASSWORD%"},
    }), encoding="utf-8")
    monkeypatch.delenv("DB_PASSWORD", raising=False)
    with pytest.raises(ValueError, match="DB_PASSWORD"):
        load_config(tmp_path)


def test_upgraded_router_development_settings(monkeypatch: pytest.MonkeyPatch) -> None:
    if not SOURCE_ROUTER_CONFIG.exists():
        pytest.skip("External upgraded Router source is not available")
    monkeypatch.delenv("DOTNET_ENVIRONMENT", raising=False)
    monkeypatch.setenv("DB_USER", "fixture")
    monkeypatch.setenv("DB_PASSWORD", "fixture")
    config = load_config(SOURCE_ROUTER_CONFIG)
    assert config.environment == "Development"
    assert config.folder_path == "INBOX"
    assert config.debug == "y"
    assert config.graph_mailbox
    assert config.graph_tenant_id and config.graph_client_id and config.graph_client_secret
    assert config.smtp_port == 25


def test_upgraded_router_production_overlay(monkeypatch: pytest.MonkeyPatch) -> None:
    if not SOURCE_ROUTER_CONFIG.exists():
        pytest.skip("External upgraded Router source is not available")
    monkeypatch.setenv("DOTNET_ENVIRONMENT", "Production")
    monkeypatch.setenv("DB_USER", "fixture")
    monkeypatch.setenv("DB_PASSWORD", "fixture")
    config = load_config(SOURCE_ROUTER_CONFIG)
    assert config.environment == "Production"
    assert config.debug == "n"
    assert config.folder_path.endswith("\\Inbox\\")
    assert config.nci_grants_post_award_email


def test_missing_base_config_is_error(tmp_path: Path) -> None:
    with pytest.raises(FileNotFoundError, match="Required Router settings"):
        load_config(tmp_path)
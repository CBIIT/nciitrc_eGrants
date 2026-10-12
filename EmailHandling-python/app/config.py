import json
import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Any

SOURCE_ROUTER_CONFIG = Path(r"C:\Development\eGrants-EmailHandlingUpgrade\EmailHandling\Router")


@dataclass(slots=True)
class RouterConfig:
    nci_grants_post_award_email: str = "NCIGrantsPostAward@nih.gov"
    debug_email: str = "leul.ayana@nih.gov"
    egrants_dev_email: str = "eGrantsDev@mail.nih.gov"
    egrants_test_email: str = "eGrantsTest1@mail.nih.gov"
    egrants_stage_email: str = "eGrantsStage@mail.nih.gov"
    efile_email: str = "efile@mail.nih.gov"
    public_access_recipients: str = (
        "jonesni@mail.nih.gov;bakerb@mail.nih.gov;edward.mikulich@nih.gov"
    )
    relinquishing_recipients: str = (
        "emily.driskell@nih.gov;dvellaj@mail.nih.gov;edward.mikulich@nih.gov"
    )
    supplements_email: str = "NCIOGASupplements@mail.nih.gov"
    bob_team_email: str = "nciogabobteam1@mail.nih.gov"
    error_recipients: str = "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov"
    legacy_error_recipient: str = "leul.ayana@nih.gov"
    folder_path: str = ""
    connection_string: str = ""
    verbose: str = "n"
    debug: str = "y"
    routing_break_duration: int = 1000
    environment: str = "Development"
    graph_mailbox: str = ""
    graph_tenant_id: str = ""
    graph_client_id: str = ""
    graph_client_secret: str = ""
    smtp_host: str = ""
    smtp_port: int = 25
    smtp_from_address: str = ""
    smtp_from_name: str = "eGrants Router"



_RECIPIENT_KEYS = {
    "nci_grants_post_award_email": "NCIGrantsPostAwardEmail",
    "debug_email": "DebugEmail",
    "egrants_dev_email": "EGrantsDevEmail",
    "egrants_test_email": "EGrantsTestEmail",
    "egrants_stage_email": "EGrantsStageEmail",
    "efile_email": "EFileEmail",
    "public_access_recipients": "PublicAccessComplianceRecipients",
    "relinquishing_recipients": "RelinquishingStatementRecipients",
    "supplements_email": "NCIOGASupplementsEmail",
    "bob_team_email": "NCIOGABOBTeamEmail",
    "error_recipients": "ErrorNotificationRecipients",
    "legacy_error_recipient": "LegacyErrorRecipient",
}


def _read_json(path: Path, *, required: bool = False) -> dict[str, Any]:
    if not path.exists():
        if required:
            raise FileNotFoundError(f"Required Router settings not found: {path}")
        return {}
    with path.open(encoding="utf-8") as stream:
        data: dict[str, Any] = json.load(stream)
    return data


def load_config(config_dir: Path | None = None) -> RouterConfig:
    """Read .NET appsettings overlays and __ environment overrides without copying secrets."""
    environment = os.getenv("DOTNET_ENVIRONMENT") or "Development"
    base = _read_json(config_dir / "appsettings.json", required=True) if config_dir else {}
    overlay = _read_json(config_dir / f"appsettings.{environment}.json") if config_dir else {}
    sections: dict[str, dict[str, Any]] = {}
    for source in (base, overlay):
        for section, values in source.items():
            if isinstance(values, dict):
                sections.setdefault(section, {}).update(values)

    def get(section: str, key: str, default: str) -> str:
        return str(os.getenv(f"{section}__{key}", sections.get(section, {}).get(key, default)))

    defaults = RouterConfig()
    recipients = {
        field: get("EmailRecipients", key, str(getattr(defaults, field)))
        for field, key in _RECIPIENT_KEYS.items()
    }
    connection = get("ConnectionStrings", "EIM", "")

    def expand(match: re.Match[str]) -> str:
        name = match.group(1)
        value = os.getenv(name)
        required = {"DB_USER", "DB_PASSWORD", "EGRANTS_DB_USER", "EGRANTS_DB_PASSWORD"}
        if not value and name in required:
            raise ValueError(f"Required environment variable {name} is not set")
        return value or ""

    # Resolve credentials only if a live connection is requested; the CLI calls this
    # with the source config only in live mode.
    connection = re.sub(r"%([^%]+)%", expand, connection)
    graph_secret = get("Graph", "ClientSecret", "")
    graph_secret = re.sub(
        r"%([A-Za-z0-9_]+)%", lambda match: os.getenv(match.group(1), ""), graph_secret
    )
    if not graph_secret:
        graph_secret = os.getenv("GRAPH_CLIENT_SECRET") or os.getenv("Graph__ClientSecret") or ""
    duration = get("AppSettings", "RoutingBreakDuration", "1000")
    port = get("SMTP", "Port", "25")
    return RouterConfig(
        **recipients,
        folder_path=get("FolderPaths", "dirpathRouter", ""),
        connection_string=connection,
        verbose=get("AppSettings", "Verbose", "n"),
        debug=get("AppSettings", "dBug", "n"),
        routing_break_duration=int(duration) if duration.isdecimal() else 1000,
        environment=environment,
        graph_mailbox=get("Graph", "Mailbox", ""),
        graph_tenant_id=get("Graph", "TenantId", ""),
        graph_client_id=get("Graph", "ClientId", ""),
        graph_client_secret=graph_secret,
        smtp_host=get("SMTP", "Host", ""),
        smtp_port=int(port),
        smtp_from_address=get("SMTP", "FromAddress", ""),
        smtp_from_name=get("SMTP", "FromName", "eGrants Router"),
    )

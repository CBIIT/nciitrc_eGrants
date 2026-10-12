# EmailHandling Python Router Migration

This folder contains an offline CLI and an experimental port of the .NET Router's
ordered routing rules, EIM SQL lookups and Microsoft Graph mailbox processing.
The Graph/SQL integrations have **not been tested against production**.

## Slice implemented

- Slice 001: Prior Approval; additional source branches are implemented in the
  routing engine with mocked parity tests. Do not replace the .NET service until
  Graph, SQL and configuration parity are verified in a test environment.

## VS Code setup

1. Open folder `EmailHandling-python` in VS Code.
2. Run task `Create venv`.
3. Run task `Install dev deps`.
4. Select interpreter: `.venv\\Scripts\\python.exe`.

## Run tests

- Run task `Run tests`, or:
- `python -m pytest -q`

## Run slice manually

Use launch profile `Run Slice001 CLI`, or run:

`python -m app.main --subject "Prior Approval: Carryover needed" --body "Please review" --debug n`

Expected behavior:
- Simulated send printed to console with recipient `NCIGrantsPostAward@nih.gov`.

## Test the full router safely (Windows)

Use Python 3.11+ and install the optional `live` dependency in the virtual
environment, plus ODBC Driver 18 for SQL Server. A Graph app registration with
mailbox permissions and an EIM account are required; **Outlook is not needed**.
`--live` defaults to reading the settings files from the upgraded Router in
`C:\Development\eGrants-EmailHandlingUpgrade\EmailHandling\Router`, without
copying them into this repository. Override with `--config-dir` on another
machine. The base settings and `appsettings.{DOTNET_ENVIRONMENT}.json` overlay
are merged, then `Section__Key` environment variables take precedence. As in
that Router, an unset `DOTNET_ENVIRONMENT` selects Development. Set `DB_USER`
and `DB_PASSWORD` in the environment; do not commit credentials. Configure
`Graph__ClientSecret` or `GRAPH_CLIENT_SECRET` in the environment to override
the source Graph secret; rotate any credential committed in source control.

Run `python -m app.main --live` for a **no-send dry run**: it authenticates to
Graph, reads mailbox folders and messages, queries SQL as needed, and logs
planned recipients (up to 50 messages). It does **not** send or move mail.
Review the log, recipients, configuration, mailbox access and database
lookups in a nonproduction mailbox first. Only then add `--send` to forward
mail and move it into `Old emails`; this option causes real side effects.
For extra safety set `AppSettings__dBug=y` while testing; `--debug n` selects
real recipients and should be used only when approved for production.

The upgraded .NET process is still the production reference. Differences to
validate include Graph forward formatting and attachments, the SQL stored
procedure and UDF, admin notifications, scheduling, logging, and recovery
following a partial send. The Python loop deliberately
does not archive a message when routing or forwarding fails, so a partial
forward may be duplicated on a retry. Do not run concurrently with the .NET
router on the same folder.

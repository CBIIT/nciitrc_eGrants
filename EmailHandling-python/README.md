# EmailHandling Python Router Migration

This folder contains an offline CLI and a source-reviewed, mock-tested port of the
upgraded .NET Router's ordered rules, EIM SQL lookups and Microsoft Graph mailbox
processing. The Graph/SQL/SMTP integrations have **not been tested live**.
See [migration/router-contract.md](migration/router-contract.md) for the ordered
branch/side-effect contract and the deliberate operational safety deviations.

## Slice implemented

- All 25 ordered C# subject/sender conditions (26 tested route variants, plus
  suppression/unmatched) have offline routing fixtures; SQL, Graph and SMTP
  behavior is mock-tested.
  This is **not production readiness**. Do not replace the .NET job until an
  isolated test mailbox, EIM database and SMTP relay have been validated.

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
For a Graph-only local check without SQL or email side effects, run
`python -m app.main --graph-check --test-recipient daryl.dehuff@nih.gov`.
It checks Graph authentication, the configured source and `Old emails` folders,
and counts visible messages; it neither sends nor moves any message. For a
full read-only routing preview use
`python -m app.main --live --test-recipient daryl.dehuff@nih.gov`.
The test-recipient option forces debug routing and overrides **all** Graph
forwards, Graph admin notifications and SMTP global alerts to that single
address if sending is later explicitly enabled. Do not add `--send` when
checking a shared mailbox: it really forwards and archives messages.
On this machine, the Graph-only check succeeded with one visible message.
The SQL adapter now selects an installed SQL Server ODBC driver (18 preferred,
17 if 18 is absent), unless `EGRANTS_ODBC_DRIVER` explicitly selects one.
The full Graph + EIM dry run still cannot start routing: the installed Driver
17 reports that the SQL Server TLS certificate chain is **not trusted**.
Installing Driver 18 by itself will not establish trust. Have the SQL/IT team
install the approved organizational issuing/root CA certificates (or fix the
server's certificate chain), then retry the no-send dry run. A user-scoped
trusted root is possible only if organization policy approves the exact CA
certificate; system-wide installation requires IT/admin action. Do **not**
disable TLS or set `TrustServerCertificate=yes` as a shortcut.
Review the log, recipients, configuration, mailbox access and database
lookups in a nonproduction mailbox first. Only then add `--send` to forward
mail and move it into `Old emails`; this option causes real side effects.
For extra safety set `AppSettings__dBug=y` while testing; `--debug n` selects
real recipients and should be used only when approved for production.

The upgraded .NET process remains the production reference. Validate Graph
forward formatting/attachments, the SQL stored procedure and UDF, SMTP
notifications, scheduling, logging, environment overlays and partial-send
recovery in isolation. The Python loop deliberately does not archive failed
items and stops at 50; .NET archives failed items and continues processing
after sending repeated warnings. A partial forward can be duplicated on retry.
Never run concurrently with the .NET router on the same mailbox. Rollback:
disable the Python schedule, reconcile pending/partially sent messages, and
resume the existing .NET job; never run both jobs simultaneously.

## Whole-Router agent workflow

Choose `router-orchestrator` in the VS Code chat agent picker and ask it to
complete the upgraded Router migration. It **starts only when invoked**;
agent files do not schedule or automatically launch one another. The
orchestrator calls its specialists in gated order: inventory → contract →
implement one component → parity tests for that component (repeat) → final
quality review. The original `migration/agents/*.prompt.md` files other than
the orchestrator pointer are historical Slice 001 prompts, not a whole-project
automation system. See `.github/agents/router-orchestrator.agent.md` for the
handoff contract and live-integration safety requirements.

## Next Graph mailbox projects

The ExchangeFixed and AddSuppProd Outlook-COM → Graph migration has an
**offline planning stage**, not a production replacement. Graph attachment
retrieval and both projects' business-rule plans are under `app/`; synthetic
tests cover them. The read-only `preview_project()` API requires a separately
mapped Graph mailbox folder; it never files, sends or moves email. Source
Outlook public-folder paths cannot simply be reused as Graph mailbox paths.
See `migration/graph-mailbox-projects.md` for missing Graph configuration,
trusted sender mappings, PDF output, database writes and cutover gates. Do
not point these projects at production or enable live effects yet.

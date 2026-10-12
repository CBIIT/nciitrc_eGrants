# Router migration handoff (2026-10-12)

## Context and requested source correction

The requested source is the upgraded Router under
`C:\Development\eGrants-EmailHandlingUpgrade\EmailHandling\Router`. Unlike the
earlier eGrants-1360 Router, it uses Microsoft Graph and SMTP, not Outlook COM.
The Python live entry point now loads the upgraded settings in place and uses
Graph by default; the Outlook adapter is legacy code and is not used by the CLI.
No credentials from source settings are copied into this workspace.

## Earlier context

The original `EmailHandling/Router` is a Windows .NET Outlook COM application.
Its `Processor.cs` reads a public folder, applies an ordered subject/sender rule
chain, queries EIM SQL Server, forwards messages, and moves them to `Old emails`.
The existing prompts in `migration/agents/` are **instructions for Slice 001**,
not executable agents, and are still limited to the Prior Approval rule.

## Decisions

- Inventory: inspect upgraded source `Program.cs`, `Processor.cs`,
  `GraphEmailService.cs` and shared `AppConfig.cs`.
- Contract: use C# branch order and exact subject/recipient strings as the
  offline route contract; keep source-specific quirks (e.g. debug No Cost
  Extension recipients, and eSNAP's two forwards).
- Logic port: isolate routing, config, SQL, and Graph integration in separate modules.
- Test parity: fake SQL lookups, Graph HTTP calls, SMTP and legacy COM objects;
  never send during tests.
- Quality: default to offline CLI; live mode is a read-only dry run unless
  explicitly supplied `--send`.

## Artifacts

- `app/routing_rules.py`: full ordered branch chain.
- `app/config.py`: reads upgraded Router settings in place, with .NET overlays,
  recipient, SMTP and Graph options and environment-variable overrides.
- `app/database.py`: EIM UDF and SPEC stored procedure adapters.
- `app/graph.py`: upgraded Router's Graph OAuth, folder paging, forwarding and archive.
- `app/outlook.py`: unused earlier-tree Outlook adapter (not the requested runtime).
- `app/main.py`: offline preview or guarded Graph live entry point.
- `tests/test_router_parity.py`, `tests/test_config.py`, `tests/test_graph.py`,
  `tests/test_notifications.py`: mocked contracts and source overlay checks.

## Validation

- 76 unit tests pass; `ruff check .` and `mypy app` pass.
- Offline Prior Approval CLI preview prints the expected simulated recipient.
- `ruff format --check .` currently reports 11 files needing formatting; not
  auto-applied because formatting must go through workspace edit tools.
- No real SQL Server, Graph, SMTP, or mailbox integration test has been run.
  Production parity is **not established**.

## Open risks

- Graph forwarded subject/body and attachments require live test-mailbox
  verification. The offline preview uses original subject/body rather than
  the Graph draft's `FW:` subject.
- A Graph failure after the first of multiple sends may duplicate the sent
  forward on retry; the item is intentionally left in its source folder.
- Source C# archives even unmatched/undeliverable messages, which Python also
  does in send mode; inspect folder and routing in dry run first.
- Positional word extraction can fail for malformed input as in the source;
  failed messages remain in the folder and administrators are notified.
- Test Graph mailbox access, SQL connectivity, ODBC 18 TLS, overlays, alert
  delivery, scheduling, and rollback in a nonproduction environment.
- The source Production folder name contains a `/` inside parentheses;
  both the .NET and Python Graph folder-path splitters treat `/` as a separator.
  Confirm a resolvable folder path before any production run.

## Next agent input

Validate against the original Router with representative **sanitized** real
emails and an isolated Graph mailbox + test EIM database. Do not enable
`--send` or retire the .NET job until operations approves the results.
Rollback: stop Python execution and resume the existing .NET job; avoid
running both on the same mailbox at once.
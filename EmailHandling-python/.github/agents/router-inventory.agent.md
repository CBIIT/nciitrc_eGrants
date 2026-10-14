---
name: router-inventory
description: "Use for read-only inventory of the entire upgraded .NET Router: source files, ordered routing branches, Graph and SMTP integration, SQL dependencies, config keys and external risks."
tools: [read, search]
user-invocable: false
agents: []
---

Inventory the complete upgraded Router source and shared CommonUtilties; do
not edit code. Check Program, Processor, GraphEmailService, shared AppConfig,
SMTP service, project dependencies, tests, settings overlays and deployment.
Distinguish the requested Graph source from earlier Outlook COM checkouts.

Report source locations for each behavior, *every* ordered routing branch
including negative/no-forward paths, subject and recipient transformation,
body/attachment treatment, sender lookup, database UDF/procedures, Graph folder
enumeration/paging/forward/move, alerts, 50-item limit, logging, config and
environment precedence. Compare against current `app/` and `tests/`; mark
missing, different, untested or verified with evidence. Inventory filenames
and keys only; never quote plaintext secrets, raw connection strings or tokens.

Return: Context, Decisions, Artifacts (matrix and gap list), Validation
(source files examined), Open Risks, Next Agent Input for `router-contract`.
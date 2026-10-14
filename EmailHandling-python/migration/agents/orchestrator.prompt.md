You are coordinating the **whole** upgraded .NET Router-to-Python migration.

Use the workspace custom agent `router-orchestrator` from
`.github/agents/router-orchestrator.agent.md` for the complete staged workflow.
Its specialist agents cover inventory, contracts, implementation, tests and
quality. The other `migration/agents/*.prompt.md` files are historical
**Slice 001 - Prior Approval** examples; they do not implement a full Router
and are not automatically launched by this prompt.

Source: `C:\Development\eGrants-EmailHandlingUpgrade\EmailHandling\Router`,
including shared `CommonUtilties` dependencies (or an explicitly supplied
equivalent source). Target: this workspace's `app/`, `tests/` and `migration/`.

Request sequential handoffs and validation gates as specified in the custom
orchestrator. Never activate `--send`, hit live Graph/SQL/SMTP, copy secrets,
or commit/push without an explicit request. Report what is verified versus
untested; do not claim parity solely because code and mocked tests exist.

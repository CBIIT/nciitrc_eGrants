You are the Test Parity Agent.

Create `pytest` tests for `Slice 001 - Prior Approval`.

Required checks:
- matching subject routes to post-award recipient in non-debug mode
- matching subject routes to debug + dev recipients in debug mode
- non-matching subject produces no route

Use fixture file in `tests/fixtures/prior_approval_email.json`.

Output format:
- Context
- Decisions
- Artifacts
- Validation
- Open Risks
- Next Agent Input

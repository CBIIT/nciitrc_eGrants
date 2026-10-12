# Slice 001 - Prior Approval routing rule

- Source project: `Router`
- Source files:
  - `Router/Processor.cs`
  - `Router/GraphEmailService.cs`
- Target Python package: `app`

## Behavior contract

- Match condition:
  - `subject` contains `"Prior Approval: "`
- Non-debug path (`debug == "n"`):
  - route to `EmailRecipients:NCIGrantsPostAwardEmail`
- Debug path (`debug != "n"`):
  - route to `EmailRecipients:DebugEmail`
  - route to `EmailRecipients:EGrantsDevEmail`
- Output:
  - 1 outgoing email preserving subject/body from inbound message

## Acceptance criteria

- Matching subject routes to post-award recipient in non-debug mode.
- Matching subject routes to debug+dev recipients in debug mode.
- Non-matching subject does not route.
- `pytest` suite passes for this slice.

# ExchangeFixed and AddSuppProd → Graph migration (offline stage)

## Scope completed

- Reused the Router's Microsoft Graph OAuth, folder listing and message paging.
  Added message metadata and paginated file-attachment decoding in
  `app/graph.py`. Reference and item attachments fail closed; their content
  is **not** silently dropped.
- Added offline ExchangeFixed subject parsing, grant lookup priority,
  extract=1/2/3 document *plans*, QC flags, attachment skipping and PDF
  category detection in `app/exchange_fixed.py`.
- Added offline AddSuppProd sender branches, subject/notification parsing,
  recipient/alias precautions, and WIP document *plans* in
  `app/add_supp_prod.py`.
- Added an injected read-only `preview_project()` entry point in
  `app/graph_project_preview.py`: maximum 30 messages, explicit Graph folder
  required; no send, move, document registration, file write or reply update.
  This is a Python API for tests/integration planning, **not a production CLI**.

## Cutover blockers (do not turn on sending/archiving yet)

1. The original projects' folder paths are **Outlook public folders** and the
   settings provide **no Graph mailbox, OAuth application, or Graph folder ID**.
   Graph `/users/{mailbox}/mailFolders` cannot be assumed to expose those
   public folders. Obtain mailbox migration/access confirmation and a reviewed
   folder mapping for each project. The source base settings for these two
   projects also contain invalid JSON (a missing section comma); don't copy or
   silently patch those files as if deployment values were already valid.
2. AddSuppProd previously resolves trusted Exchange aliases through Outlook.
   Graph only supplies SMTP addresses. Supply a verified **exact address →
   legacy alias allowlist** for system/eRA/staff; matching the local part of
   arbitrary SMTP addresses would grant unauthorized routing privileges.
3. ExchangeFixed requires faithful Outlook/Word/Acrobat PDF rendering and
   merging (including PublicAccess attachments); no equivalent conversion
   engine or approved degradation policy exists yet. Registering a document
   as PDF without a matching file must be forbidden.
4. AddSuppProd `getPlaceHolder_new`/reply status updates, ExchangeFixed
   `SP_CREATE_EGRANTS_DOCUMENT_NEW`/JIT cleanup, and network-share document
   delivery are **not yet wired to Python**. Define transaction/duplicate
   recovery after a DB registration, file save, forward or Graph move fails.
5. Confirm Graph file-attachment payload limits and item/reference handling,
   mailbox-scoped permission and delegated-vs-application access, mapping of
   Outlook sender/received/To metadata, and correct `old` folder (not Router's
   `Old emails`).

## Decisions from project owner (2026-10-13)

- Plan to read from **new folders underneath an Exchange Online inbox** where
   incoming messages are pulled; the mailbox address, exact folder names/IDs,
   provisioning and access permissions are still to be confirmed. The preview
   requires an explicit reviewed Graph folder path (for example, a configured
   `Inbox/…` child), not the existing Outlook public-folder paths.
- ExchangeFixed PDF output requires **exact or explicitly approved rendering**.
   No degraded text/PDF substitution is accepted by default.
- Keep this migration **fail closed**: no live SQL/file/email writes, sends or
   moves before isolated integration tests and explicit approval.

## Sequence after prerequisites

Inventory/test-source parity → provision an isolated Graph mailbox and
explicit folder map → validate attachment/identity metadata → implement and
fake-test parameterized SQL and share writes → choose and validate PDF
conversion for every category → dry-run with synthetic test data → isolated
Graph/SQL/SMTP test → reconcile archive/duplicate recovery → approve cutover.
Do not run both Python and legacy jobs on the same mailbox. Rollback by
disabling the Python scheduler, reconciling filed/forwarded document IDs, and
re-enabling the .NET job only after mail state is checked.
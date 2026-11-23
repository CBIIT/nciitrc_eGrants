# Cross-IC access SQL rollout

## Scripts
1. `2026-10-15-cross-ic-procedure-wrappers.sql`
   - Wraps core stored procedures with cross-IC-aware parameter contract.
   - Preserves legacy logic in `*_legacy` procedures.
2. `2026-10-15-fn_get_doc_url-cross-ic.sql`
   - Fixes URL authority derivation for shared/local docs.
3. `2026-10-15-cross-ic-procedure-wrappers-rollback.sql`
   - Restores pre-wrapper procedure names/behavior.

## Execution order
1. Run `2026-10-15-cross-ic-all-objects-change.sql`.
2. Smoke-test:
   - FOIA read-only access (cross-IC view/open)
   - non-NCI PD read-only access (cross-IC view/open)
   - non-NCI specialist upload/update/read (cross-IC)

## Notes
- Wrappers are backward-compatible for existing app callers.
- App-layer changes already pass authority IC for mapped cross-IC users.
- If rollback is needed, run `2026-10-15-cross-ic-all-objects-rollback.sql`.

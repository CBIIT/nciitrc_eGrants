-- =============================================================================
-- Script:  eGrants-1361_insert_profiles_additional_ics_BACKOUT.sql
-- Purpose: Backout / rollback for
--          eGrants-1361_insert_profiles_additional_ics.sql
--
--          Removes the additional IC profiles that were inserted by the forward
--          script. Rows are matched on both [profile] and [admin_phs_org_code]
--          to avoid accidentally deleting any pre-existing or unrelated rows.
--
-- Notes:   - The delete is idempotent and can be re-run safely.
--          - Only the 18 ICs added by the forward script are removed.
--          - IMPORTANT: If any of these profiles are now referenced by other
--            records (e.g. users/egrants_users.profile_id), deletion may fail
--            due to foreign key constraints. In that case, reassign or remove
--            the dependent records first, or coordinate with a DBA.
-- =============================================================================

SET NOCOUNT ON;

DELETE p
FROM [dbo].[profiles] p
INNER JOIN (VALUES
    ('CIT',    'CT'),
    ('CLC',    'CL'),
    ('FIC',    'TW'),
    ('NCATS',  'TR'),
    ('NCCIH',  'AT'),
    ('NEI',    'EY'),
    ('NHGRI',  'HG'),
    ('NIA',    'AG'),
    ('NIAAA',  'AA'),
    ('NIAMS',  'AR'),
    ('NIDA',   'DA'),
    ('NIDCD',  'DC'),
    ('NIDCR',  'DE'),
    ('NIH',    'NH'),
    ('NIMHD',  'MD'),
    ('NINDS',  'NS'),
    ('NINR',   'NR')
) AS src ([profile], [admin_phs_org_code])
    ON p.[profile] = src.[profile]
   AND p.[admin_phs_org_code] = src.[admin_phs_org_code];

PRINT CONVERT(VARCHAR(10), @@ROWCOUNT) + ' profile row(s) deleted.';

-- =============================================================================
-- Script:  eGrants-1361_insert_profiles_additional_ics.sql
-- Purpose: Insert additional IC profiles into the [profiles] table.
--
--          Adds the ICs listed below that are not already present in the table.
--          The insert is idempotent: each row is only inserted when a matching
--          profile does not already exist, so the script can be re-run safely.
--
-- Notes:   - profile_id is NOT specified; it is assumed to be an IDENTITY column.
--            If profile_id is not an identity column, assign explicit values.
--          - Matching is done on the [profile] (IC abbreviation) value.
--          - All QC types in the source list are 0.
-- =============================================================================

SET NOCOUNT ON;

INSERT INTO [dbo].[profiles] ([profile], [admin_phs_org_code], [qc_type])
SELECT src.[profile], src.[admin_phs_org_code], src.[qc_type]
FROM (VALUES
    ('CIT',    'CT', 0),
    ('CLC',    'CL', 0),
    ('FIC',    'TW', 0),
    ('NCATS',  'TR', 0),
    ('NCCIH',  'AT', 0),
    ('NEI',    'EY', 0),
    ('NHGRI',  'HG', 0),
    ('NIA',    'AG', 0),
    ('NIAAA',  'AA', 0),
    ('NIAMS',  'AR', 0),
    ('NIDA',   'DA', 0),
    ('NIDCD',  'DC', 0),
    ('NIDCR',  'DE', 0),
    ('NIH',    'NH', 0),
    ('NIMHD',  'MD', 0),
    ('NINDS',  'NS', 0),
    ('NINR',   'NR', 0)
) AS src ([profile], [admin_phs_org_code], [qc_type])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[profiles] p
    WHERE p.[profile] = src.[profile]
);

PRINT CONVERT(VARCHAR(10), @@ROWCOUNT) + ' profile row(s) inserted.';

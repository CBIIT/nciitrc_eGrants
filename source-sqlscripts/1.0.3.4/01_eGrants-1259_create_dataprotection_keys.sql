/* ==========================================================================
   eGrants-1259 : durable Data Protection key ring

   Database : EIM
   Purpose  : Gives ASP.NET Core Data Protection a permanent home for the keys
              that encrypt and sign the eGrants authentication cookie. Without
              this table the keys are held per-process under IIS, so every
              application pool recycle discards them and every user holding an
              older cookie is forced to sign in to Entra ID again.

   IMPORTANT: Run this BEFORE deploying the matching application build. If the
              table or the grants are missing, the application cannot create a
              key and sign-in fails outright.

   Run once per environment (dev, test, stage, prod). Each environment keeps its
   own key ring; keys are not shared between environments.

   The application populates and rotates the rows automatically. Do not insert,
   edit or delete rows by hand - deleting a key permanently invalidates every
   cookie that was issued under it.
   ========================================================================== */

IF NOT EXISTS (SELECT 1
               FROM sys.tables
               WHERE name = 'DataProtectionKeys'
                 AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.DataProtectionKeys
    (
        Id           INT IDENTITY(1,1) NOT NULL
                     CONSTRAINT PK_DataProtectionKeys PRIMARY KEY,
        FriendlyName NVARCHAR(MAX) NULL,
        Xml          NVARCHAR(MAX) NULL
    );

    PRINT 'Created table dbo.DataProtectionKeys.';
END
ELSE
BEGIN
    PRINT 'Table dbo.DataProtectionKeys already exists - no change made.';
END
GO

/* The application reads the key ring, creates new keys as they roll, and marks
   keys revoked. All four permissions are required. */
GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.DataProtectionKeys TO egrantsuser;
GO

/* ------------------------------------------------------------------------
   Verification (expect 0 rows before the app starts, 1 row after first use)
   ------------------------------------------------------------------------
   SELECT Id, FriendlyName, LEN(Xml) AS XmlLength FROM dbo.DataProtectionKeys;
   ------------------------------------------------------------------------ */

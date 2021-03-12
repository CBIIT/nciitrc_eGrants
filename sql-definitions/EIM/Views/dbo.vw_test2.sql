SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_test2
AS
SELECT dbo.vw_appls.appl_id, dbo.vw_appls.full_grant_num, C.APPL_ID AS Expr1, C.FULL_GRANT_NUM AS Expr2
FROM  dbo.vw_appls INNER JOIN
               OPENQUERY(CIIP, 'select * from x_pv_grant_pi') C ON dbo.vw_appls.serial_num = C.SERIAL_NUM AND 
               dbo.vw_appls.admin_phs_org_code = C.ADMIN_PHS_ORG_CODE COLLATE SQL_Latin1_General_Pref_CP1_CI_AS AND 
               dbo.vw_appls.appl_type_code = C.APPL_TYPE_CODE COLLATE SQL_Latin1_General_Pref_CP1_CI_AS AND 
               dbo.vw_appls.activity_code = C.ACTIVITY_CODE COLLATE SQL_Latin1_General_Pref_CP1_CI_AS AND 
               dbo.vw_appls.support_year = C.SUPPORT_YEAR
WHERE (dbo.vw_appls.appl_id < 0) AND (dbo.vw_appls.suffix_code IS NULL) AND (C.SUFFIX_CODE IS NULL)

GO


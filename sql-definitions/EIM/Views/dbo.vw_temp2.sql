SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_temp2
AS
SELECT     dbo.vw_appls.appl_id, dbo.vw_appls.grant_id AS old_grant_id, dbo.grants.grant_id AS new_grant_id
FROM         dbo.temp2 INNER JOIN
                      dbo.grants INNER JOIN
                      dbo.impp_mech_codes ON 
                      dbo.grants.mechanism_code COLLATE SQL_Latin1_General_CP1_CI_AS = dbo.impp_mech_codes.MECHANISM_CODE INNER JOIN
                      dbo.vw_appls ON dbo.impp_mech_codes.APPL_ID = dbo.vw_appls.appl_id AND 
                      dbo.grants.admin_phs_org_code = dbo.vw_appls.admin_phs_org_code AND dbo.grants.serial_num = dbo.vw_appls.serial_num ON 
                      dbo.temp2.appl_id = dbo.vw_appls.appl_id

GO


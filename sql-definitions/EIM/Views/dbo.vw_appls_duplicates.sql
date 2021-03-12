SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE VIEW [dbo].[vw_appls_duplicates]
AS
select   	dbo.vw_appls.appl_id, dbo.vw_appls.full_grant_num, 
	vw_appls.appl_type_code, vw_appls.activity_code,vw_appls.grant_id, vw_appls.support_year, vw_appls.suffix_code,
	isnumeric(dbo.vw_all_appls_used.appl_id) AS is_used, isnumeric(impp.APPL_ID) AS in_impac
from       dbo.vw_appls 
	INNER JOIN dbo.vw_appls_dups ON dbo.vw_appls.grant_id = dbo.vw_appls_dups.grant_id AND 
	dbo.vw_appls.appl_type_code = dbo.vw_appls_dups.appl_type_code AND 
	dbo.vw_appls.activity_code = dbo.vw_appls_dups.activity_code AND 
     	dbo.vw_appls.support_year = dbo.vw_appls_dups.support_year 
	LEFT OUTER JOIN OPENQUERY(IRDB, 'select appl_id from appls_t') impp ON dbo.vw_appls.appl_id = impp.APPL_ID 
	LEFT OUTER JOIN dbo.vw_all_appls_used ON dbo.vw_appls.appl_id = dbo.vw_all_appls_used.appl_id
where    (dbo.vw_appls.suffix_code IS NULL) AND (dbo.vw_appls_dups.suffix_code IS NULL) OR
	(dbo.vw_appls.suffix_code = dbo.vw_appls_dups.suffix_code)

GO


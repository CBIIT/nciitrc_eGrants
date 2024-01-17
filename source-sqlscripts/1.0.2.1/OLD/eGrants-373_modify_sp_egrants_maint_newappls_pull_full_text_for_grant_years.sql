ALTER PROCEDURE [dbo].[sp_egrants_maint_newappls]

AS

INSERT ncieim_b..appls_txt(appl_id,keywords)
	SELECT dbo.vw_appls_used.appl_id,dbo.fn_appl_keywords(dbo.vw_appls_used.appl_id)
	FROM  dbo.vw_appls_used LEFT OUTER JOIN
				   ncieim_b.dbo.appls_txt ON dbo.vw_appls_used.appl_id = ncieim_b.dbo.appls_txt.appl_id
	WHERE (ncieim_b.dbo.appls_txt.appl_id IS NULL)
UNION
	SELECT dbo.vw_appls.appl_id,dbo.fn_appl_keywords(dbo.vw_appls.appl_id)
	FROM  dbo.vw_appls 
		WHERE ((loaded_date>convert(varchar,getdate(),101) and dbo.vw_appls.appl_id < 1) OR
			--(loaded_date>'2023-09-30 1:1:01.01' and dbo.vw_appls.appl_id<1 )		OR
			(appl_type_code =3 and admin_phs_org_code ='CA' and doc_count = 0 and loaded_date>'2023-09-30 1:1:01.01'))
		AND -- its not already in there
			NOT exists (select * from ncieim_b..appls_txt where appl_id = dbo.vw_appls.appl_id)


EXEC ncieim_b..sp_fulltext_catalog 'appl_kw', 'start_incremental'
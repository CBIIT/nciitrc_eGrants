SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE VIEW [dbo].[vw_Grants_Flag_Construct]
AS
SELECT		gf_id,f.grant_id,f.appl_id,admin_phs_org_code,serial_num,g.grant_num,dbo.fn_appl_full_grant_num(appl_id) as full_grant_num, 
			flag_type,flag_icon_namepath,flag_application,start_dt,end_dt,created_by,created_dt,last_updated_by,last_updated_date,flag_persist,flag_gen_type
FROM		dbo.Grants_Flag_Construct f inner join vw_grants g ON f.grant_id=g.grant_id inner join dbo.Grants_Flag_Master m on f.flag_type=m.flag_type_code
WHERE		f.end_dt IS NULL and m.end_date is null



GO


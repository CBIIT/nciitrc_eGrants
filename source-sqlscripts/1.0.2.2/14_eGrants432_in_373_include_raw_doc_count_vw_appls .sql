USE [EIM]
GO

/****** Object:  View [dbo].[vw_appls]    Script Date: 1/4/2024 9:48:23 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER VIEW [dbo].[vw_appls] AS

SELECT     dbo.grants.grant_id, dbo.grants.admin_phs_org_code, dbo.grants.serial_num, dbo.appls.appl_id, 
dbo.grants.is_tobacco, 
--dbo.fn_tobacco_flag (grants.grant_id) is_tobacco, --newly added, is_tobacco_new
dbo.grants.to_be_destroyed, 
dbo.grants.grant_close_date, dbo.grants.mechanism_code, dbo.grants.former_admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, 
dbo.grants.former_serial_num), 6) AS former_grant_num, dbo.grants.future_admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, dbo.grants.future_serial_num), 
6) AS future_grant_num, dbo.grants.former_serial_num, dbo.grants.former_admin_phs_org_code, dbo.grants.future_admin_phs_org_code, 
dbo.grants.future_serial_num, REPLICATE('0', 4 - LEN(CONVERT(varchar, dbo.grants.former_serial_num))) + CONVERT(varchar, dbo.grants.former_serial_num) 
AS former_serial_num_4, REPLICATE('0', 5 - LEN(CONVERT(varchar, dbo.grants.former_serial_num))) + CONVERT(varchar, dbo.grants.former_serial_num) 
AS former_serial_num_5, RIGHT('00000' + CONVERT(varchar, dbo.grants.former_serial_num), 6) AS former_serial_num_6, REPLICATE('0', 4 - LEN(CONVERT(varchar, 
dbo.grants.future_serial_num))) + CONVERT(varchar, dbo.grants.future_serial_num) AS future_serial_num_4, REPLICATE('0', 5 - LEN(CONVERT(varchar, 
dbo.grants.future_serial_num))) + CONVERT(varchar, dbo.grants.future_serial_num) AS future_serial_num_5, RIGHT('00000' + CONVERT(varchar, 
dbo.grants.future_serial_num), 6) AS future_serial_num_6, REPLICATE('0', 4 - LEN(CONVERT(varchar, dbo.grants.serial_num))) + CONVERT(varchar, 
dbo.grants.serial_num) AS serial_num_4, REPLICATE('0', 5 - LEN(CONVERT(varchar, dbo.grants.serial_num))) + CONVERT(varchar, dbo.grants.serial_num) 
AS serial_num_5, RIGHT('00000' + CONVERT(varchar, dbo.grants.serial_num), 6) AS serial_num_6, dbo.appls.appl_type_code, dbo.appls.activity_code, 
dbo.appls.support_year, dbo.appls.suffix_code, dbo.appls.project_title, dbo.appls.former_num, dbo.appls.rfa_pa_number, dbo.appls.council_meeting_date, 
dbo.appls.external_org_id, dbo.appls.org_name, dbo.appls.person_id, dbo.appls.last_name, dbo.appls.first_name, dbo.appls.mi_name, dbo.appls.prog_class_code, 
dbo.appls.irg_code, dbo.appls.appl_status_group_descrip, dbo.appls.fy, dbo.appls.appl_received_date, dbo.appls.created_date, dbo.appls.last_upd_date, 
dbo.appls.receipt_date, dbo.appls.active_grant_flag, dbo.appls.new_appl_id, ISNULL(dbo.appls.frc_destroyed, 0) AS frc_destroyed, ISNULL(dbo.appls.first_name + ' ', 
'') + ISNULL(dbo.appls.mi_name + ' ', '') + dbo.appls.last_name AS pi_name, dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, 
dbo.grants.serial_num), 6) AS grant_num, dbo.appls.appl_type_code + dbo.appls.activity_code + dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar,
dbo.grants.serial_num), 6) + '-' + RIGHT('0' + CONVERT(varchar, dbo.appls.support_year), 2) + ISNULL(dbo.appls.suffix_code, '') AS full_grant_num, 
dbo.appls.activity_code + dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, dbo.grants.serial_num), 6) + '-' + RIGHT('0' + CONVERT(varchar, 
dbo.appls.support_year), 2) + ISNULL(dbo.appls.suffix_code, '') AS project_num, RIGHT('00' + CONVERT(varchar, dbo.appls.support_year), 2) AS support_year_2, 
dbo.appls.irg_flex_code, RIGHT('00' + CONVERT(varchar, dbo.appls.support_year), 2) + ISNULL(dbo.appls.suffix_code, '') AS support_year_suffix, 
dbo.appls.ss_upd_date, dbo.appls.loaded_date, dbo.appls.fis_date, dbo.appls.fpr_date, dbo.appls.summary_statement_flag, dbo.appls.fsr_date, 
CASE WHEN appl_type_code IN ('1', '2', '6', '9') THEN 'yes' ELSE 'no' END AS competing, CASE WHEN grant_close_date IS NOT NULL 
THEN 'Yes' ELSE 'No' END AS closed_out, CASE WHEN MONTH(getdate()) >= 10 AND fy = YEAR(getdate()) + 1 THEN 'Yes' WHEN MONTH(getdate()) < 10 AND 
fy = YEAR(getdate()) THEN 'Yes' ELSE 'No' END AS is_current_fy,
dbo.fn_doc_count(appls.appl_id) as doc_count,
dbo.fn_raw_doc_count(appls.appl_id) as raw_doc_count,
dbo.fn_all_docs_disabled(appls.appl_id) as all_docs_disabled,
--ISNULL(dbo.appls.fda_flag, 'n') AS fda_flag,
--dbo.fn_appl_fda_flag (appls.appl_id) fda_flag, --appl level  --newly added ; fda_flag_new

closeout_flag,
fsrs.fsr_count,--- comment out by leon 5/9/2015
CloseOut_Not.CloseOut_NotCount, --added by leon 5/20/2015
CASE WHEN deleted_by_impac.deleted_count=0 or deleted_by_impac.deleted_count is null THEN 'n' ELSE 'y' END AS deleted_by_impac,				

CASE WHEN OD_flag.ODFlag_cnt=0 or OD_flag.ODFlag_cnt is null THEN 'n' ELSE 'y' END AS OD_flag,						
CASE WHEN MS_flag.MSFlag_cnt=0 or MS_flag.MSFlag_cnt is null THEN 'n' ELSE 'y' END AS MS_flag,
CASE WHEN FDA_flag.FDAFlag_cnt=0 or FDA_flag.FDAFlag_cnt is null THEN 'n' ELSE 'y' END AS FDA_flag,
CASE WHEN DS_flag.DSFlag_cnt=0 or DS_flag.DSFlag_cnt is null THEN 'n' ELSE 'y' END AS DS_flag


--dbo.fn_appl_fsr_count (appls.appl_id) fsr_count,  --- added by leon 5/4/2015
--dbo.fn_appl_deleted_by_impac (appls.appl_id) deleted_by_impac--- added by leon 5/4/2015

FROM dbo.grants INNER JOIN dbo.appls ON dbo.grants.grant_id = dbo.appls.grant_id 

--LEFT OUTER JOIN(SELECT appl_id, COUNT(*) AS fsr_count FROM dbo.impp_fsrs_all
--GROUP BY appl_id) AS fsrs ON dbo.appls.appl_id = fsrs.appl_id--- comment out by leon 5/9/2015 

LEFT OUTER JOIN(SELECT appl_id, COUNT(*) AS fsr_count FROM dbo.impp_fsrs_all
GROUP BY appl_id) AS fsrs ON dbo.appls.appl_id = fsrs.appl_id--- comment in by leon 5/9/2015 

LEFT OUTER JOIN(SELECT appl_id,COUNT(*) AS CloseOut_NotCount FROM dbo.IMPP_CloseOut_Notification_All 
GROUP BY appl_id) as CloseOut_Not ON dbo.appls.appl_id = CloseOut_Not.appl_id--added by leon 5/20/2015

LEFT OUTER JOIN(SELECT appl_id, COUNT(*) AS deleted_count FROM dbo.appls_deleted_in_impac WHERE exception_date is null---added by leon 7/13/2017
GROUP BY appl_id) as deleted_by_impac ON dbo.appls.appl_id =deleted_by_impac.appl_id--added by leon 5/20/2015

LEFT OUTER JOIN(SELECT appl_id,count(*) AS ODFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='OD' and flag_application='A'
GROUP BY appl_id) as OD_flag ON dbo.appls.appl_id =OD_flag.appl_id and dbo.grants.admin_phs_org_code='CA'

LEFT OUTER JOIN(SELECT appl_id,count(*) AS MSFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='MS' and flag_application='A'
GROUP BY appl_id) as MS_flag ON dbo.appls.appl_id =MS_flag.appl_id and dbo.grants.admin_phs_org_code='CA'

LEFT OUTER JOIN(SELECT appl_id,count(*) AS FDAFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='FDA' and flag_application='B'
GROUP BY appl_id) as FDA_flag ON dbo.appls.appl_id =FDA_flag.appl_id and dbo.grants.admin_phs_org_code='CA'

LEFT OUTER JOIN(SELECT appl_id,count(*) AS DSFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='DS' and flag_application='A'
GROUP BY appl_id) as DS_flag ON dbo.appls.appl_id =DS_flag.appl_id and dbo.grants.admin_phs_org_code='CA'



GO



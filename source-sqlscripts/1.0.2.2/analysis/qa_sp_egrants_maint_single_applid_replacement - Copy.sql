USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_single_applid_replacement]    Script Date: 12/20/2023 3:28:24 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER PROCEDURE [dbo].[sp_egrants_maint_single_applid_replacement] 

@appl_id_old	 int

/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:  sp_egrants_maint_single_applid_replacement				***/
/***	Description:	replace appl_id	from appls table by impac appl_id		***/
/***	Created:	03/19/2007	Leon											***/
/***	Modified:	04/25/2007	Leon											***/
/***																			***/
/************************************************************************************************************/

AS

SET NOCOUNT ON

declare 
@sql			varchar(4000),
@appl_id_new		int,
@full_grant_num	varchar(50)


---find new appl_id
SET @appl_id_new=(
SELECT s.appl_id
FROM appls_ciip s, vw_appls a
WHERE
a.appl_id= @appl_id_old and 
s.serial_num=a.serial_num and
s.appl_type_code=a.appl_type_code and
s.activity_code=a.activity_code and
s.support_year=a.support_year and
ISNULL(s.suffix_code,'')=ISNULL(a.suffix_code,'')
)

--- if @appl_id_new is null
select @full_grant_num from 
vw_appls a
where appl_id  = @appl_id_old

---insert new appl_id from impac
IF @appl_id_new is null or @appl_id_new=''
BEGIN
SET @full_grant_num=(select full_grant_num from vw_appls where appl_id=@appl_id_old)
SET @sql=
'insert appls_ciip
(appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, external_org_id, org_name, 
person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag,active_grant_flag )'

SET @sql=@sql+
'select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date,external_org_id, org_name,
person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code,summary_statement_flag, active_grant_flag from '


--commented by hareeshj on 5/04/16 5:10pm
--SET @sql= @sql + 'openquery(IRDB,' + char(39) + '
--select appl_id, appl_type_code,
--activity_code, admin_phs_org_code,
--serial_num, support_year, suffix_code,
--project_title, former_num, rfa_pa_number,
--council_meeting_date, external_org_id, org_name,
--person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
--APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag, active_grant_flag from 
--dm_pv_grant_pi where full_grant_num=' + char(39) + char(39) +@full_grant_num + char(39) + char(39) + char(39) + ')'


--added by hareeshj on 5/04/16 5:10pm
SET @sql= @sql + 'openquery(IRDB,' + char(39) + '
select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, external_org_id, org_name,
pi_role_person_id as person_id, pi_last_name as last_name, pi_first_name as first_name , pi_mi_name as mi_name , prog_class_code as prog_class_code , irg_code as irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, ss_exists_code as summary_statement_flag, active_grant_flag from 
pva_grant_pi_mv where full_grant_num=' + char(39) + char(39) +@full_grant_num + char(39) + char(39) + char(39) + ')'


EXEC (@sql)

END

   print  @appl_id_new

IF @appl_id_new != null
BEGIN
--insert replacement info
INSERT appls_replacement (appl_id_old, appl_id_new)  SELECT @appl_id_old, @appl_id_new

--update appl_id in appls table
	IF (select count(*) from appls  where appl_id=@appl_id_new)=0 --@new_appl_id is not in appls
	BEGIN
	UPDATE appls
	SET appl_id=@appl_id_new
	WHERE appl_id=@appl_id_old 
	END
	ELSE	

	---@new_appl_id is in appls
	--update documents tables ,  folder_appls, funding_appls table	
	BEGIN	
	UPDATE documents  SET appl_id=@appl_id_new WHERE appl_id=@appl_id_old
	UPDATE folder_appls SET appl_id=@appl_id_new WHERE appl_id=@appl_id_old 
	UPDATE funding_appls SET appl_id=@appl_id_new WHERE appl_id=@appl_id_old 

	--delete old appl_id from appls table
	DELETE FROM appls WHERE  appl_id=@appl_id_old		
END
END

--update @appl_id with new data
IF (select count(*) from appls  where appl_id=@appl_id_new) is not null
BEGIN
UPDATE appls
SET
appl_type_code=s.appl_type_code,
activity_code=s.activity_code,
support_year=s.support_year,
suffix_code=s.suffix_code,
last_name=s.last_name,
first_name=s.first_name,
mi_name=s.mi_name,
project_title=s.project_title,
former_num=s.former_num,
org_name=s.org_name,
rfa_pa_number=s.rfa_pa_number,
council_meeting_date=s.council_meeting_date,
prog_class_code=s.prog_class_code,
irg_code=s.irg_code,
appl_status_group_descrip=s.appl_status_group_descrip,
fy=s.fy,
appl_received_date=s.appl_received_date,
last_upd_date=s.last_upd_date,
irg_flex_code=s.irg_flex_code,
summary_statement_flag=s.summary_statement_flag,
loaded_date=getdate()

FROM appls a, appls_ciip s 
WHERE a.appl_id=s.appl_id and a.appl_id=@appl_id_new
END




-------------------------------
/*
GRANT EXECUTE ON  [dbo].[sp_econtracts_noise_words_deleted] TO [egrants]
GO


GRANT ALTER ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_admin_flag_maint] TO [egrantsuser]
GO



GRANT ALTER ON  [dbo].[sp_y2013_contract_edit] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_edit] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_edit] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_edit] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_edit] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_edit] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_edit] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_edit] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_edit] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_edit] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_contract_efinal] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_efinal] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_efinal] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_efinal] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_efinal] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_efinal] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_efinal] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_efinal] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_efinal] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_efinal] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_efinal_view] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_contract_head] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_head] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_head] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_head] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_head] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_contract_head] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_head] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_head] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_head] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_head] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_head] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_head] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_head] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_head] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_head] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_contract_modify] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_modify] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_modify] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_modify] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_modify] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_modify] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_modify] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_modify] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_modify] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_modify] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_contract_tracking] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_appl_add] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_appl_add] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_appl_add] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_appl_add] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_appl_add] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_appl_add] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_category_edit] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_category_edit] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_category_edit] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_category_edit] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_category_edit] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_category_edit] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_error] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_error] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_error] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_error] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_error] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_error] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_modify] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_modify] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_modify] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_modify] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_modify] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_modify] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_transaction_report] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_upload] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_upload] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_upload] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_upload] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_upload] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_upload] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url_nci] TO [egrantsuser]
GO




GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_doc_url_restore] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_appl_edit] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_doc_edit] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_master] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_master] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_master] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_master] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_master] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_funding_master] TO [egrantsuser]
GO

GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
GO


GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_qc_assignment] TO [egrantsuser]
GO



GRANT ALTER ON  [dbo].[sp_y2013_egrants_system_report] TO [ChienL]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_system_report] TO [ChienL]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_system_report] TO [ChienL]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_system_report] TO [ChienL]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_system_report] TO [ChienL]
GRANT ALTER ON  [dbo].[sp_y2013_egrants_system_report] TO [egrants]
GRANT CONTROL ON  [dbo].[sp_y2013_egrants_system_report] TO [egrants]
GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_system_report] TO [egrants]
GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_system_report] TO [egrants]
GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_system_report] TO [egrants]
--GRANT ALTER ON  [dbo].[sp_y2013_egrants_system_report] TO [egrantsuser]
--GRANT CONTROL ON  [dbo].[sp_y2013_egrants_system_report] TO [egrantsuser]
--GRANT TAKE OWNERSHIP ON  [dbo].[sp_y2013_egrants_system_report] TO [egrantsuser]
--GRANT VIEW DEFINITION ON  [dbo].[sp_y2013_egrants_system_report] TO [egrantsuser]
--GRANT EXECUTE ON  [dbo].[sp_y2013_egrants_system_report] TO [egrantsuser]
GO
*/
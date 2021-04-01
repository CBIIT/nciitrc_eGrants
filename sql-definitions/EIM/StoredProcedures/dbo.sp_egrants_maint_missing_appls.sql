SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_maint_missing_appls]

@admin_phs_org_code 	varchar(2),
@serial_number 		int

/****************************************************************************************************************/
/***									 																	***/
/***	Procedure Name: sp_egrants_maint_missing_appls														***/
/***	Description:	find some appls in IMPAC II but not in eGrants										***/
/***	Created:	07/16/2004	leon																		***/
/***	Modified:																							***/
/***																										***/
/****************************************************************************************************************/

AS

SET NOCOUNT ON

DECLARE @sql 		varchar(4000)
DECLARE @D  		smalldatetime
DECLARE @UpdDate 		varchar(12)
DECLARE @linkCounter 	tinyint
DECLARE @linkServer		varchar(10)
--------------------------------
-- Joel 06/06/2013 for IRDB
--------------------------------
DECLARE @DD			varchar(10)

--SET @admin_phs_org_code='DK'
--SET @serial_number=69929

SET @linkCounter=1
--SET @linkServer='IMPAC'
--SELECT @linkServer='IMPAC' + CASE @linkCounter WHEN 0 THEN '' ELSE convert(varchar,@linkCounter) END
SET @linkServer='IRDB'
SELECT @D=DATEADD(d,-30, max(last_upd_date)) FROM appls WHERE last_upd_date is not null
SELECT @UpdDate= convert(varchar,DAY(@D)) + '-' + LEFT(DATENAME(month,@D),3) + '-' + right(convert(varchar,YEAR(@D)),2)

--------------------------------
-- Joel 06/06/2013 for IRDB
--------------------------------
Set @DD=convert(varchar,YEAR(@D))+ '-' + right('00' + convert(varchar,MONTH(@D)),2) + '-' + right('00' + convert(varchar,DAY(@D)),2) 

IF @serial_number is not null 

BEGIN

/*--commented by hareeshj on 4/21/16 12:55pm--*/
--SET @sql= 'select * from openquery(' + @linkServer +  ','  + char(39) + '
--select appl_id, appl_type_code,
--activity_code, admin_phs_org_code,
--serial_num, support_year, suffix_code,
--project_title, former_num, rfa_pa_number,
--council_meeting_date, external_org_id, org_name,
--person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
--APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag, active_grant_flag from 
--DM_PV_GRANT_PI where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+convert(varchar,@serial_number)+char(39)+char(39)+
--'and to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'
--------------------------------
-- Joel 06/06/2013 for IRDB
--------------------------------
--PV_GRANT_PI where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+convert(varchar,@serial_number)+char(39)+char(39)+' and last_upd_date>=' +char(39)+char(39)+@UpdDate +char(39)+char(39)+char(39)+' )'

/*--added by hareeshj on 5/4/16 5:10pm--*/
SET @sql= 'select * from openquery(' + @linkServer +  ','  + char(39) + '
select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, external_org_id, org_name,
pi_role_person_id, pi_last_name, pi_first_name, pi_mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, ss_exists_code, active_grant_flag 
from PVA_GRANT_PI_MV 
where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+convert(varchar,@serial_number)+char(39)+char(39)+
'and to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'


END

ELSE

BEGIN

/*--commented by hareeshj on 4/21/16 12:55pm--*/
--SET @sql= 'select * from openquery(' + @linkServer +  ','  + char(39) + '
--select appl_id, appl_type_code,
--activity_code, admin_phs_org_code,
--serial_num, support_year, suffix_code,
--project_title, former_num, rfa_pa_number,
--council_meeting_date, external_org_id, org_name,
--person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
--APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag, active_grant_flag from 
--PV_GRANT_PI where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+
--'and to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') order by appl_id '')'
--------------------------------
-- Joel 06/06/2013 for IRDB
--------------------------------
--PV_GRANT_PI where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+' and last_upd_date>=' +char(39)+char(39)+@DD +char(39)+char(39)+ ' order by appl_id '+char(39)+' )'

/*--added by hareeshj on 5/4/16 5:10pm--*/
SET @sql= 'select * from openquery(' + @linkServer +  ','  + char(39) + '
select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, external_org_id, org_name,
pi_role_person_id, pi_last_name, pi_first_name, pi_mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, ss_exists_code, active_grant_flag 
from PVA_GRANT_PI_MV
where admin_phs_org_code='+char(39)+char(39)+@admin_phs_org_code+char(39)+char(39)+' and serial_num='+char(39)+char(39)+
'and to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') order by appl_id '')'


END


EXEC (@sql)

GO


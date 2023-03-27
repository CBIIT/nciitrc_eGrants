USE [EIM]
GO

/****** Object:  StoredProcedure [dbo].[sp_egrants_maint]    Script Date: 3/24/2023 9:57:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO






ALTER   PROCEDURE [dbo].[sp_egrants_maint] AS

DECLARE @sql 		varchar(4000)
DECLARE @D  		smalldatetime
DECLARE @UpdDate 		varchar(12)
DECLARE @linkCounter 	tinyint
DECLARE @linkServer		varchar(10)

DECLARE @DD			varchar(10)

-- Joel 06/06/2013 for IRDB
/*
SET @linkCounter=1
SELECT @linkServer='IMPAC' + CASE @linkCounter WHEN 0 THEN '' ELSE convert(varchar,@linkCounter) END
*/

SELECT @linkServer='IRDB'
--SELECT @linkServer='IMPAC'

--commented out by hareesh on 6/5/13 at 4:48am
--SELECT @linkServer='IRDB' 

--SELECT @linkServer='IMPAC'
--print '0'

print '-----------LOG Begin----------at--' + cast(getdate() as varchar)

---find the latest upload date (one month ago) from appl table
--SELECT @D='01/01/1970'    --comment this out for automated mode

--SELECT @D=DATEADD(d,-30, max(last_upd_date)) FROM appls WHERE last_upd_date is not null
--To enhance performance changed it to 7 days

--commented by hareeshj on 5/4/16 10:58pm
--SELECT @D=DATEADD(d,-8, max(last_upd_date)) FROM appls WHERE last_upd_date is not NULL

--commented by hareeshj on 11292016 6:30pm, since 11/21/16 has 291k records and dilay ~300k+ records are bring downloaded
--SELECT @D=DATEADD(d,-8, max(last_upd_date)) FROM appls WHERE last_upd_date is not null
--commented by Madhu on 08/19/2022 to get only appls from 8/15 
SELECT @D=DATEADD(d,-1, max(last_upd_date)) FROM appls WHERE last_upd_date is not null
print '@D = '   + CAST(@D AS VARCHAR)


SELECT @UpdDate= convert(varchar,DAY(@D)) + '-' + LEFT(DATENAME(month,@D),3) + '-' + right(convert(varchar,YEAR(@D)),2)
--convert(varchar,DAY(@D)) + '-' + LEFT(DATENAME(month,@D),3) + '-' + right(convert(varchar,YEAR(@D)),2)
Set @DD=convert(varchar,YEAR(@D))+ '-' + right('00' + convert(varchar,MONTH(@D)),2) + '-' + right('00' + convert(varchar,DAY(@D)),2) 
print '@DD ' + CAST(@DD AS VARCHAR)

--DELETE appls_ciip
TRUNCATE TABLE appls_ciip
print 'Truncate appls_ciip @ ' + cast(getdate() as varchar)

-- Joel 06/06/2013 for IRDB
---insert all latest appl info to appls_ciip, from impac pv_grant_pi table
SET @sql=
'insert appls_ciip
(appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, external_org_id, org_name,
person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag,active_grant_flag,
GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,PI_EMAIL_ADDR)'

SET @sql=@sql+
'select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date,external_org_id, org_name,
pi_role_person_id, pi_last_name, pi_first_name, pi_mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, ss_exists_code, active_grant_flag ,
GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,PI_EMAIL_ADDR from '

/*--commented by hareeshj on 5/4/16 5:10pm--*/
--SET @sql= @sql + 'openquery(' + @linkServer +  ','  + char(39) + '
--select appl_id, appl_type_code,
--activity_code, admin_phs_org_code,
--serial_num, support_year, suffix_code,
--project_title, former_num, rfa_pa_number,
--council_meeting_date, external_org_id, org_name,
--person_id, last_name, first_name, mi_name, prog_class_code, irg_code,
--APPL_STATUS_GROUP_DESCRIP,fy,LAST_UPD_DATE, irg_flex_code, summary_statement_flag, active_grant_flag 
--from DM_PV_GRANT_PI 
--where to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'


/*--added by hareeshj on 5/4/16 5:10pm--*/
SET @sql= @sql + 'openquery(' + @linkServer +  ','  + char(39) + '
select p.appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title, former_num, rfa_pa_number,
council_meeting_date, p.external_org_id, org_name,
pi_role_person_id, pi_last_name, pi_first_name, pi_mi_name, prog_class_code, irg_code,
APPL_STATUS_GROUP_DESCRIP,fy,p.LAST_UPD_DATE, irg_flex_code, ss_exists_code, active_grant_flag,
GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,
c.email_addr PI_EMAIL_ADDR
from PVA_GRANT_PI_MV p
left outer join person_involvements_mv b on b.appl_id = p.appl_id
and b.role_type_code = ''''PI''''
and b.version_code <> ''''W''''
left outer join person_addresses_mv c on c.person_id = b.person_id
and c.addr_type_code = ''''HOM''''
and c.preferred_addr_code = ''''Y''''
WHERE to_date(to_char(p.last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'

--pv_grant_pi where last_upd_date>=' + char(39) + char(39) +@UpdDate + char(39) + char(39) + char(39) + ')'

--pv_grant_pi where to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'
--DM_PV_GRANT_PI where to_date(to_char(last_upd_date,''''yyyy-mm-dd''''),''''yyyy-mm-dd'''')>=to_date(''''' + @DD + ''''',''''yyyy-mm-dd'''') '')'

 print @sql

EXEC (@sql)

print 'Downloaded from PVA_GRANT_PI_MV ' + cast(@@ROWCOUNT as varchar)

--UPDATE appls_ciip
--set former_num = '2L30CA136372-2A1'
--WHERE former_num = 'L30CA136372--2A1'

--select * from appls_ciip where former_num like '%--%'

delete from appls_ciip where former_num like '%--%'

---------------------------------------------------
---insert early grant year from appls_t  for NIDDK
-- Joel 06/06/2013 for IRDB
insert appls_ciip
(
appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title,former_num,rfa_pa_number,council_meeting_date, external_org_id,
prog_class_code, irg_code,fy,LAST_UPD_DATE, irg_flex_code
)

--select c.* from openquery(IMPAC,
select c.* from openquery(IRDB,
'select appl_id, appl_type_code,
activity_code, admin_phs_org_code,
serial_num, support_year, suffix_code,
project_title,former_num,rfa_pa_number,council_meeting_date, external_org_id,
prog_class_code, irg_code,fy,LAST_UPD_DATE, irg_flex_code
from appls_t where ADMIN_PHS_ORG_CODE=''DK''') c
LEFT OUTER JOIN vw_appls a  ON 
(
c.serial_num=a.serial_num and
c.support_year=a.support_year and
ISNULL(c.suffix_code,'~')=ISNULL(a.suffix_code,'~') COLLATE SQL_Latin1_General_Pref_CP1_CI_AS and
c.appl_type_code=a.appl_type_code COLLATE SQL_Latin1_General_Pref_CP1_CI_AS and
c.admin_phs_org_code=a.admin_phs_org_code COLLATE SQL_Latin1_General_Pref_CP1_CI_AS
)
WHERE a.appl_id IS NULL and c.appl_id NOT IN (select appl_id from appls) and
c.appl_id NOT IN (select appl_id from appls_ciip)

print 'Early Grant Year for NIDDK Added ' + cast(@@ROWCOUNT as varchar)
------------------------------------------------------------
print 'Completed sp_egrants_maint'

EXEC sp_egrants_maint_new

print 'Completed sp_egrants_maint_new'

-- Code Added by Madhu on 12/28 for EGRANTS-223
-- The following proc was added to update the latest PI info for the appl_ids
--EXEC sp_egrants_maint_new_UpdateApplsEmail @DD  , @DD

--print 'Completed sp_egrants_maint_new_UpdateApplsEmail'



GO


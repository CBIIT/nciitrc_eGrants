USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants]    Script Date: 9/26/2023 2:28:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER           PROCEDURE [dbo].[sp_web_egrants]

@str 			nvarchar(400),
@grant_id 		int,
@package 		varchar(50),
@appl_id 		int,
@current_page	int,
@browser		varchar(50),
@ic  			varchar(10),
@operator 		varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants											***/
/***	Description:searching for egrants										***/
/***	Created:	03/07/2013	Leon											***/
/***	Modified:	11/07/2013	Leon											***/
/***	Modified:   08/08/2016	Frances											***/
/***	Modified:   10/04/2016	Leon simplify and pagination					***/
/***	Modified:   01/25/2019	Leon added filters search						***/
/***	Modified:   03/08/2019	Leon added email address for PI, PD and SP		***/
/***	Modified:   03/19/2019	Leon added flag information for appl and grant	***/
/***    Modified:   05/08/2023  Robin modified grant level to return current year PI Info ***/
/************************************************************************************************************/
SET NOCOUNT ON

--for sql
declare @type				varchar(50)
declare @sql				varchar(1000)
declare @count				int

--for user info
declare @profile_id			int
declare @profile			varchar(10)
declare @person_id			int
declare @position_id		int

--for search info
declare @search_str			varchar(800)
declare @separate		    int
declare @search_id			int

--for filter searching
declare @filter_type		int
declare @fy					int
declare @activity_code		varchar(3)
declare	@admin_phs_org_code	varchar(2)

--for pagination
declare @total_grants		int
declare @per_page			int
declare @per_tab			int
declare @start				int
declare @end				int

print('made it to here')

--find unser info
SET @operator=LOWER(@operator)
SELECT @profile_id=profile_id  FROM  profiles  WHERE  profile=@IC
SELECT @person_id= person_id FROM vw_people WHERE userid=@operator AND profile_id=@profile_id
SELECT @position_id=position_id FROM vw_people WHERE person_id=@person_id

SET @str=RTRIM(LTRIM(@str))
IF (@str='' or @str is null) and (@grant_id='' or @grant_id is null) and (@appl_id='' or @appl_id is null) Return

create table #a(appl_id int primary key)

--search by appl_id:123456 
IF (@str<>'' and @str is not null and (@str<>'qc' and @package<>'by_filters') )
SELECT @separate=PATINDEX('appl_id:%',@str)
IF @separate=1
BEGIN
SET @separate=PATINDEX('%:%',@str)
SET @count = ISNUMERIC(SUBSTRING(@str,@separate+1,LEN(@str)))
IF	@count = 1 SET @appl_id=convert(int,SUBSTRING(@str,@separate+1,LEN(@str))) 
END

--search by appl_id=123456
IF @separate<>1
SELECT @separate=PATINDEX('appl_id=%',@str)
IF @separate=1
BEGIN
SET @separate=PATINDEX('%=%',@str)
SET @count = ISNUMERIC(SUBSTRING(@str,@separate+1,LEN(@str)))
IF	@count = 1 SET @appl_id=convert(int,SUBSTRING(@str,@separate+1,LEN(@str))) 
END

---search by full_grant_num
IF @appl_id is null and (@str<>'' and @str is not null and @str<>'qc' and @package<>'by_filters') and PATINDEX('%-%',@str)>1 ---and len(@str)<=19
BEGIN
---IF RIGHT(@str,1)='+' SET @str=substring(@str,1,LEN(@str)-1)--remove last '+' for chrome
SET @appl_id=(select appl_id FROM vw_appls WHERE full_grant_num=@str)
IF @appl_id is not null SET @str=null  ----and @appl_id<>0 
END

--create appl_id or grant_id by full text searching
--IF (@str<>'' and @str is not null and @str<>'qc' and @package<>'by_filters') 
--BEGIN
--SET @sql='insert #a (appl_id) select distinct [key] from containstable(ncieim_b..appls_txt, keywords,' + char(39) + @str + char(39) + ')'
--EXEC(@sql)
--SET @count=(select count(*) from #a)
--IF @count=1 SET @appl_id=(select appl_id from #a)
--IF @count>1 SET @grant_id=(select distinct grant_id from vw_appls where appl_id in(select appl_id from #a))
--END

---find search_type and create search string
IF @appl_id is not null and @appl_id<>0 
BEGIN
SET @str=null
SET @type='egrants_appl' 
SET @search_str='appl_id:'+ convert(varchar,@appl_id)
END

IF @grant_id is not null and @grant_id>0 
BEGIN
SET @str=null
--SET @package='all'
SET @type='egrants_grant' 
SET @search_str='grant_id:'+ convert(varchar,@grant_id)+' package:'+@package
END

IF @str<>'' and @str is not null and (@str<>'qc' and @package<>'by_filters') 
BEGIN
SET @type='egrants_str' 
SET @search_str=@str
END

IF @str<>'' and @str is not null and @str='qc' 
BEGIN
SET @type='egrants_qc' 
SET @search_str='filter:qc  user:'+ @operator 
END

IF @str<>'' and @str is not null and @package='by_filters'
BEGIN
SET @type='egrants_filters' 
END

---create table to save all grants
CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, grant_id int,serial_num int)

---create table to save search data**/
DECLARE  @g table(grant_id int primary key)
DECLARE  @a table(appl_id int primary key)
DECLARE  @d table(document_id int primary key, appl_id int)
--DECLARE  @d table(document_id int primary key)

--set page number and per_page for pagenation with egrants_str or egrants_qc or egrants_filters
IF @type='egrants_str' or @type='egrants_qc' or @type='egrants_filters' 
BEGIN
IF (@current_page is null or @current_page='' or @current_page=0) SET @current_page=1
IF @str='qc'SET @per_page=5 
IF @str<>'qc' SET @per_page=20
SET @end = @current_page * @per_page
SET @start = @end - @per_page + 1
END

IF @type<>'egrants_filters'
BEGIN
SET @search_id=(SELECT search_id FROM searches WHERE search_string=@search_str)
IF @search_id IS NULL
BEGIN
INSERT searches(search_string) SELECT @search_str
SET @search_id=@@IDENTITY
END
INSERT queries(search_id,execution_time,ic,searched_by,page,browser_type)
SELECT @search_id,null,UPPER(@IC),@operator,ISNULL(@current_page, null),@browser
END

print(CONCAT('goin to type: ', @type))	-- egrants_str

--go to load data
if @type='egrants_qc' GOTO egrants_qc
if @type='egrants_str' GOTO egrants_str
if @type='egrants_appl' GOTO egrants_appl
if @type='egrants_grant' GOTO egrants_grant
if @type='egrants_filters' GOTO egrants_filters

-------------------
egrants_filters:

SET @sql='INSERT #t(grant_id, serial_num) '+ @str
--print(CONCAT('#t insert: ', @sql))
EXEC(@sql)

--for pagination
SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end --ORDER BY id
END
ELSE 
BEGIN
INSERT @g(grant_id)SELECT grant_id FROM #t --ORDER BY id
END

----INSERT @g(grant_id) SELECT top 10 grant_id FROM #t ORDER BY id

----insert appl_id with @g
INSERT @a 
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id 
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1 --display new created appl_id

----clean up search if there is no appls return
SET @count=(select count(*) from @a)
IF @count=0 
BEGIN
delete from @g
END

GOTO OUTPUT
--------------------
egrants_str:
print('in output for egrants_str')
--print(CONCAT('goin to type: ', @type))	-- egrants_str

SET @str=LTRIM(RTRIM(dbo.fn_str_decode(@str)))
SET @str=dbo.fn_str(@str)

print(CONCAT('contains table query: ', 'select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.doc_count>0 and a.appl_id=c.[key] order by serial_num'))

SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.doc_count>0 and a.appl_id=c.[key] order by serial_num'
---SET @sql='insert @t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.appl_id=c.[key] and a.admin_phs_org_code='+char(39)+'CA'+char(39)+' and a.closed_out='+char(39)+'no'+char(39)+ 'order by serial_num'
EXEC(@sql)

print('passed exec1')

--print('#t count:')
--select count(*) from #t	--it's 0 here

--for pagination
SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end ORDER BY serial_num
END
ELSE 
BEGIN
INSERT @g(grant_id)SELECT grant_id FROM #t ORDER BY serial_num
END

print('@g count')
--select count(*) from @g	--it's 0 here
--print(CONCAT('@g count', select count(*) from @a))

print('passed insert')

--insert appl_id with @g
INSERT @a 
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id 
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1 --display new created appl_id

print('passed insert appl_id with @g')

--clean up search if there is no appls return
SET @count=(select count(*) from @a)
IF @count=0 
BEGIN
delete from @g
return
END
ELSE 

print('going to output')

GOTO OUTPUT
--------------------
egrants_grant:

INSERT @g(grant_id) SELECT @grant_id

---insert appls by grant_id
INSERT @a 
SELECT DISTINCT appl_id FROM vw_appls_used_bygrant vg WHERE vg.grant_id=@grant_id
UNION
SELECT DISTINCT appl_id FROM vw_appls a WHERE a.grant_id=@grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

IF @package is null or @package ='' SET @package ='All'

---insert all documents by grant_id
IF @package ='All' or @package ='all'		
BEGIN
INSERT @d(document_id, appl_id) 
SELECT document_id, appl_id FROM egrants WHERE grant_id=@grant_id
--UNION
--SELECT distinct document_id, appl_id FROM vw_funding WHERE grant_id=@grant_id
--add doc from vw_funding for the appl without any documents in egrants by Leon 05/11/2019
INSERT @d(document_id, appl_id) SELECT distinct document_id, appl_id FROM vw_funding WHERE grant_id=@grant_id and appl_id not in(select appl_id from @d)
END

---insert documents by flag type
IF @package<>'All' and @package<>'all'
BEGIN
INSERT #a(appl_id) 
select distinct appl_id from Grants_Flag_Construct where flag_type=@package and grant_id=@grant_id
INSERT @d(document_id, appl_id) 
SELECT document_id, appl_id FROM egrants WHERE appl_id in(select appl_id from #a)

END
 
GOTO OUTPUT
--------------------
egrants_appl:

IF (@appl_id<>'' or @appl_id is not null) ---GOTO foot
BEGIN

INSERT @d(document_id, appl_id) 
SELECT document_id, appl_id FROM egrants WHERE appl_id=@appl_id
UNION
SELECT document_id, appl_id FROM vw_funding WHERE appl_id=@appl_id  --add in by leon 12/21/2016

INSERT @g(grant_id) SELECT grant_id FROM vw_appls WHERE appl_id = @appl_id

INSERT @a(appl_id)
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

---IF (SELECT COUNT(*) FROM @d)=0 AND (SELECT COUNT(*) FROM @a)=0 GOTO foot

END

GOTO OUTPUT
--------------------
egrants_qc:

SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num FROM egrants WHERE grant_id is not null and qc_date is not null and appl_id is not null and parent_id is null and qc_person_id='+convert(varchar,@person_id) + ' order by serial_num desc'
EXEC(@sql)

SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end ---ORDER BY serial_num DESC
END 
ELSE 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t ---ORDER BY serial_num DESC
END

INSERT @d(document_id, appl_id) SELECT document_id, appl_id FROM egrants e, @g g  ---DISTINCT 
WHERE e.grant_id=g.grant_id and qc_person_id=@person_id and qc_date is not null ---and appl_id is not null and parent_id is null

INSERT @a(appl_id)
SELECT appl_id FROM vw_appls_used_bygrant AS vg, @g AS g WHERE vg.grant_id=g.grant_id ---DISTINCT
UNION 
SELECT appl_id FROM vw_appls AS a, @g AS g WHERE a.grant_id=g.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

GOTO OUTPUT
------------------
OUTPUT:

print('reached final output here')

SELECT
1		as tag, 
0		as parent,
g.grant_id, 
null as label,
RIGHT('00000' + CONVERT(varchar,g.serial_num), 6) as serial_num,
g.admin_phs_org_code,
g.former_grant_num,
dbo.fn_get_latest_full_grant_num(g.grant_id) as latest_full_grant_num,
dbo.fn_get_all_activity_code(g.grant_id) as all_activity_code,
CASE 
	WHEN len(g.project_title)<=60 
	THEN UPPER(g.project_title)
	ELSE UPPER(substring(g.project_title,0,60))+'...' 
END as project_title, 
g.org_name,			--dbo.fn_clean_characters(org_name) as org_name,                                       
g.pi_name,			--dbo.fn_clean_characters(pi_name) as pi_name, 
UPPER(current_pi_name) as current_pi_name,
current_pi_email_address,
UPPER(current_pd_name) as current_pd_name,
current_pd_email_address,
UPPER(current_spec_name) as current_spec_name,
current_spec_email_address,
current_bo_email_address,
g.prog_class_code,
org_sv_url as sv_url,		---dbo.fn_get_org_flag_url(org_name) as sv_url,
g.ARRA_flag as arra_flag,
g.FDA_flag as fda_flag,
STP_flag as stop_flag,
g.MS_flag	as ms_flag,
g.OD_flag as od_flag,
g.DS_flag as ds_flag,
adm_supp,
g.institutional_flag1,
g.institutional_flag2,
g.inst_flag1_url,
--applsLayer              
null			as appl_id,
null			as full_grant_num,
null			as support_year,
null			as project_title,
null			as appl_type_code,
null			as deleted_by_impac,
null			as doc_count,       ---pi_name,org_name,
null			as closeout_notcount,
null			as competing,
null            as fsr_count, 
null			as frc_destroyed, 
null			as appl_fda_flag,
null			as appl_ms_flag,
null			as appl_od_flag,
null            as appl_ds_flag,
null			as closeout_flag,
null			as irppr_id,
null            as can_add_doc,
null			as can_add_funding, 
---docsLayer
null			as docs_count,

--- --dbo.appls.pi_email_addr as current_pi_email_address,dbo.fn_get_pi_name_by_applid(7188870) )
CASE 
	WHEN (UPPER(pi_name) = COALESCE(dbo.fn_get_pi_name_by_applid_specific_year(@appl_id), '')) 
	THEN 1
	ELSE 0
END as is_current_pi,
COALESCE(dbo.fn_get_pi_name_by_applid_specific_year(@appl_id), '') as specific_year_pi_name,
COALESCE(dbo.fn_get_pi_email_by_applid_specific_year(@appl_id), '') as specific_year_pi_email_address,
COALESCE(dbo.fn_get_project_name_by_applid_specific_year(@appl_id), '') as specific_year_project_name,
COALESCE(dbo.fn_get_org_name_by_applid_specific_year(@appl_id), '') as specific_year_org_name,
COALESCE(dbo.fn_get_full_grant_num_by_applid_specific_year(@appl_id), '') as specific_year_full_grant_num,
CASE
	WHEN (COALESCE(dbo.fn_get_institution1_by_applid_specific_year(@appl_id), '') = 1)
	THEN 1
	ELSE 0
END as specific_year_institution1,
CASE
	WHEN (COALESCE(dbo.fn_get_institution2_by_applid_specific_year(@appl_id), '') = 1)
	THEN 1
	ELSE 0
END as specific_year_institution2
FROM @g AS t inner join vw_grants g on t.grant_id=g.grant_id 


UNION ALL

SELECT
2, 
1,
grant_id,
dbo.fn_appl_get_year_label(t.appl_id) as label,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
CAST(0 as bit),
CAST(0 as bit),
null,
--applsLayer
a.appl_id,
full_grant_num,
support_year_suffix,
dbo.fn_clean_characters(UPPER(project_title)),
appl_type_code,
deleted_by_impac,
dbo.fn_doc_count(a.appl_id),					----dbo.fn_clean_characters(pi_name),
dbo.fn_appl_CloseOut_NotCount(a.appl_id),
--CASE
--WHEN (a.appl_id=@appl_id) or (grant_id=@grant_id and @package='all') or ( grant_id=@grant_id and (dbo.fn_applid_package(a.appl_id, @package))>= 1) or ( @str='qc' and dbo.fn_appl_with_qc(a.appl_id, @person_id)>=1) THEN 1   ---or (dbo.fn_applid_package(@appl_id, @package) = 1)
--ELSE 0
--END,
competing,
dbo.fn_appl_fsr_count(a.appl_id),
frc_destroyed,
FDA_flag,
MS_flag,		---dbo.fn_flag_A(a.appl_id,'ms'),
OD_flag,
DS_flag,
dbo.fn_show_closeout_flag(a.appl_id),
dbo.fn_get_irppr_id(a.appl_id),
CASE
WHEN @position_id >= 2 and frc_destroyed=0 and deleted_by_impac='n' 
THEN 'y'
ELSE 'n'
END,
CASE
WHEN @IC='NCI' and @position_id>=5 and admin_phs_org_code='CA'  
THEN 'y'
ELSE 'n'
END,
--docsLayer
null,
null,
null,
null,
null,
null,
null,
null,
null
FROM @a AS t,vw_appls a WHERE t.appl_id=a.appl_id 
--order by a.support_year desc

UNION ALL	--add grant documents

SELECT
3,
2,
null,			---grant_id, 
null as label,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
CAST(0 as bit),
CAST(0 as bit),
null,
--applsLayer
t.appl_id,
null,			---dbo.fn_appl_full_grant_num(t.appl_id),
null,
null,
null,
null,
null,
null,
null,
null,
CASE 
WHEN dbo.fn_appl_frc_destroyed(t.appl_id)=1 THEN 1
ELSE 0
END,
null,
null,
null,
null,
null,
null,
null,
null,
--docsLayer
count(t.document_id),
null,
null,
null,
null,
null,
null,
null,
null
--FROM @d AS t, egrants d WHERE t.document_id=d.document_id and t.appl_id IS NOT NULL group by t.appl_id
--ORDER BY tag,grant_id,support_year desc  ----,appl_id  
FROM @d AS t, vw_appls a WHERE t.appl_id = a.appl_id group by t.appl_id
ORDER BY tag,grant_id, support_year desc  ----added by Leon 5/11/2019 

SET ANSI_NULLS OFF







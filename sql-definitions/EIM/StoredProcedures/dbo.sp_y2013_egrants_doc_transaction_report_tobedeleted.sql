SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_y2013_egrants_doc_transaction_report_tobedeleted]

@type			varchar(20),
@doc_id			int,
@startdate		smalldatetime,
@enddate		smalldatetime,
@timerange		varchar(20),
@person_id		smallint,
@inst			varchar(10),
@operator		varchar(50)

 AS
/*********************************************************************************************************************/
/***									 														***/
/***	Procedure Name:sp_y2013_egrants_doc_transaction_report									***/
/***	Description:Find documents procession info by operator name, time rank  or style.		***/
/***	Created:	05/16/2002	Leon															***/
/***	Modified:	03/29/2007	Leon,Joel														***/
/***	Simplified:	11/13/2014	Leon															***/
/*********************************************************************************************************************/

SET NOCOUNT ON

DECLARE
@profile_id			smallint,
@person_name		varchar(50),
@total				int,
@day 				int,
@days 				int,
@month				int,
@year 				int,
@today  			smalldatetime,
@target_person_name	varchar(50),
@xmlout				varchar(max),
@X					Xml

SET @profile_id=(select profile_id from profiles  where profile=@inst)

---return all users data
SET @X = (
SELECT person_id,
upper(ISNULL(person_name,userid)) as person_name
FROM vw_people as operator
WHERE profile_id=@profile_id and application_type='egrants' and position_id >=2 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

----find the searching person id
IF @type is null or @type = '' GOTO header

IF @type is not null or @type <>''
BEGIN

--find searching person_id
IF @person_id=0 SET @target_person_name=@inst
ELSE SET @target_person_name=(SELECT ISNULL(person_name, userid) FROM people WHERE person_id=@person_id)

----find searching date range

/**set time range**/
IF (@StartDate is not null or @StartDate<>'') and (@EndDate is not null or @EndDate<>'' )
BEGIN
SET @StartDate =convert(varchar,@StartDate,101)	
SET @EndDate =convert(varchar,@EndDate,101)	
END

/**set time range **/
IF @TimeRange<>'' and @TimeRange is not null SET @today =convert(varchar,getdate(),101)	

IF @TimeRange='Today'
BEGIN
SET @StartDate=@today 
SET @EndDate=(SELECT DATEADD(day,1,@StartDate ))
END

IF @TimeRange='This_Week'
BEGIN
SET DATEFIRST 1
SET @day=(SELECT DATEPART(dw, @today) )
SET @StartDate = (SELECT @today - @day+1)
SET @EndDate=(SELECT @StartDate+5+2)
END

IF @TimeRange='Last_Week'
BEGIN
SET DATEFIRST 1
SET @day=(SELECT DATEPART(dw, @today) )
SET @StartDate = (SELECT @today - @day-6)
SET @EndDate=(SELECT @StartDate+5+2)
END

IF @TimeRange='This_Month'
BEGIN
SET @day =DAY(@today)
SET @StartDate = (SELECT @today - @day+1)
SET @month=MONTH (@StartDate )
IF @month=1 or @month=3 or @month=5 or @month=7 or @month=8 or @month=10 or @month=12 SET @days =31
IF @month=4 or @month=6 or @month=9 or @month=11 SET @days =30
IF @month=2 SET @days =28
SET @EndDate=(SELECT @StartDate+@days)
END

IF @TimeRange='Last_Month'
BEGIN
SET @day =DAY(@today)
SET @month=MONTH (@today)

--	New last month logic Joel Friedman 03/29/2013
set @StartDate =DATEADD(month, -1, @today)
set @StartDate = dateadd(day, -(day( @StartDate)-1), @StartDate)
set @endDate = dateadd(month,1, @StartDate)-1
--select @StartDate, @endDate 

END

-------run action
IF @type='all' GOTO search_all
IF @type='index modified' GOTO index_modified
IF @type='created' GOTO created
IF @type='deleted' GOTO deleted
IF @type='image modified' GOTO image_modified
IF @type='stored' GOTO stored
IF @type='to_restore' GOTO to_restore

END
-------------------------------------
created:

IF @person_id=0  	---for ic only
BEGIN	

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,dbo.fn_clean_characters(url) as url,convert(varchar, d.created_date,101) as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (d.created_by_person_id is not null and d.created_by_person_id=p.person_id) and  (d.created_date >@StartDate and  d.created_date<@EndDate )

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and created_by_person_id is not null and (created_date>@StartDate  and created_date<@EndDate))

END
ELSE
BEGIN		---for personal

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,url,convert(varchar, d.created_date,101)as [date]	
FROM documents d, vw_appls a, categories c, people p
WHERE d.appl_id=a.appl_id and d.category_id=c.category_id and d.profile_id=@profile_id and  d.created_by_person_id=@person_id and d.created_by_person_id=p.person_id and (d.created_date>@StartDate and d.created_date<@EndDate)

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and created_by_person_id=@person_id and (created_date>@StartDate and created_date<@EndDate))

END

GOTO header

RETURN
-------------------------------------
index_modified:

IF @person_id=0  --for ic
BEGIN

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,url,convert(varchar, d.modified_date,101)as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE  d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (index_modified_by_person_id is not null and index_modified_by_person_id=p.person_id) and (d.modified_date>@StartDate and d.modified_date<@EndDate)

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and index_modified_by_person_id is not null and (modified_date>@StartDate and modified_date<@EndDate))

END
ELSE  ---for personal
BEGIN

SELECT document_id, full_grant_num, category_name,ISNULL(person_name, userid)as person_name,url,convert(varchar, d.modified_date,101)as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE  d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and d.index_modified_by_person_id=@person_id and d.index_modified_by_person_id=p.person_id and (d.modified_date>@StartDate and d.modified_date<@EndDate)

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and index_modified_by_person_id=@person_id and (modified_date>@StartDate and modified_date<@EndDate))

END

GOTO header

RETURN
-------------------------------------
deleted:

IF @person_id=0 --for ic
BEGIN

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,url,convert(varchar, d.disabled_date,101)as [date]	
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and (d.appl_id is not null and d.appl_id=a.appl_id) and (d.category_id is not null and d.category_id=c.category_id) and 
(disabled_by_person_id is not null and disabled_by_person_id=p.person_id) and (disabled_date>@StartDate and disabled_date<@EndDate)

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and appl_id is not null and category_id is not null and disabled_by_person_id is not null and (disabled_date>@StartDate and disabled_date<@EndDate))

END
ELSE  --for personal
BEGIN

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,url,convert(varchar, d.disabled_date,101)as [date]	
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and disabled_by_person_id=@person_id and disabled_by_person_id=p.person_id and (disabled_date>@StartDate and disabled_date<@EndDate)

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and appl_id is not null and category_id is not null and disabled_by_person_id=@person_id and (disabled_date>@StartDate and disabled_date<@EndDate))

END

GOTO header

RETURN
-------------------------------------
image_modified:

IF @person_id=0 ---for ic
BEGIN	

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid) as person_name,url,convert(varchar, file_modified_date,101) as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (file_modified_by_person_id is not null and file_modified_by_person_id=p.person_id) and ( file_modified_date>@StartDate and file_modified_date<@EndDate) 

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and file_modified_by_person_id is not null and (file_modified_date>@StartDate and file_modified_date<@EndDate))

END
ELSE	---for personal
BEGIN		

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid)as person_name,url,convert(varchar, file_modified_date,101) as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and file_modified_by_person_id=@person_id and  file_modified_by_person_id=p.person_id and (file_modified_date>@StartDate and file_modified_date<@EndDate) 

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and file_modified_by_person_id=@person_id and (file_modified_date>@StartDate and file_modified_date<@EndDate))

END

GOTO header

RETURN
-------------------------------------
stored:

IF @person_id=0----for ic
BEGIN	

SELECT document_id,full_grant_num,category_name,ISNULL(person_name, userid)as person_name,url,convert(varchar, stored_date,101)as [date]
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (stored_by_person_id is not null and stored_by_person_id=p.person_id) and (stored_date>@StartDate and stored_date<@EndDate) 

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and stored_by_person_id is not null and (stored_date>@StartDate and stored_date<@EndDate))

END
ELSE	----for personal
BEGIN   

SELECT document_id,full_grant_num,category_name	,ISNULL(person_name, userid)as person_name,url,convert(varchar, stored_date,101)as [date]	
FROM documents d, vw_appls a, categories c, people p
WHERE  d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (stored_by_person_id=@person_id and stored_by_person_id=p.person_id) and  (stored_date>@StartDate and stored_date<@EndDate) 

SET @total=(SELECT count(document_id) AS total FROM documents WHERE profile_id=@profile_id and stored_by_person_id=@person_id  and (stored_date>@StartDate and stored_date<@EndDate))

END

GOTO header

RETURN
-------------------------------------
to_restore:

--restore deleted documen
UPDATE documents SET  disabled_date=null, disabled_by_person_id=null WHERE document_id=@doc_id

GOTO header

RETURN
--------------------------
search_all:

CREATE TABLE #all(operator varchar(50), created_total int, created_pages int, uploaded_total int, uploaded_pages int, modified_total int, stored_total int, stored_pages int, deleted_total int, deleted_pages int )
INSERT  #all(operator) SELECT upper (@target_person_name)

IF @person_id=0 or  @person_id is null
BEGIN

/**for ic**/
UPDATE #all SET created_total=ISNULL((SELECT count(document_id)  FROM documents WHERE profile_id=@profile_id and created_by_person_id is not null and (created_date>@StartDate and created_date<@EndDate)),0)
UPDATE #all SET created_pages=ISNULL((SELECT sum(page_count)  FROM documents WHERE page_count is not null and profile_id=@profile_id and created_by_person_id is not null and (created_date>@StartDate and created_date<@EndDate)),0)

UPDATE #all SET uploaded_total=ISNULL((SELECT count(document_id) FROM documents WHERE profile_id=@profile_id and file_modified_by_person_id is not null and (file_modified_date>@StartDate and file_modified_date<@EndDate)),0)
UPDATE #all SET uploaded_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE page_count is not null and profile_id=@profile_id and file_modified_by_person_id is not null and (file_modified_date>@StartDate and file_modified_date<@EndDate)),0)

UPDATE #all SET modified_total=ISNULL((SELECT count(document_id) FROM documents WHERE profile_id=@profile_id and index_modified_by_person_id is not null and (modified_date>@StartDate and modified_date<@EndDate)),0)

UPDATE #all SET stored_total=ISNULL((SELECT count(document_id) FROM documents WHERE profile_id=@profile_id and stored_by_person_id is not null and (stored_date>@StartDate and stored_date<@EndDate)),0)
UPDATE #all SET stored_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE page_count is not null and profile_id=@profile_id and stored_by_person_id is not null and (stored_date>@StartDate and stored_date<@EndDate)),0)

UPDATE #all SET deleted_total=ISNULL((SELECT count(document_id) FROM documents WHERE profile_id=@profile_id and appl_id is not null and disabled_by_person_id is not null and (disabled_date>@StartDate and disabled_date<@EndDate)),0)
UPDATE #all SET deleted_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE page_count is not null and profile_id=@profile_id and appl_id is not null and disabled_by_person_id is not null and (disabled_date>@StartDate and disabled_date<@EndDate)),0)

END
ELSE	----for personal
BEGIN
UPDATE #all SET created_total=ISNULL((SELECT count(document_id)  FROM documents WHERE  created_by_person_id=@person_id and (created_date>@StartDate and created_date<@EndDate)),0)
UPDATE #all SET created_pages=ISNULL((SELECT sum(page_count)  FROM documents WHERE  page_count is not null and created_by_person_id=@person_id and (created_date>@StartDate and created_date<@EndDate)),0)

UPDATE #all SET uploaded_total=ISNULL((SELECT count(document_id) FROM documents WHERE file_modified_by_person_id=@person_id and (file_modified_date>@StartDate and file_modified_date<@EndDate)),0)
UPDATE #all SET uploaded_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and file_modified_by_person_id=@person_id and (file_modified_date>@StartDate and file_modified_date<@EndDate)),0)

UPDATE #all SET modified_total=ISNULL((SELECT count(document_id) FROM documents WHERE  index_modified_by_person_id=@person_id and (modified_date>@StartDate and modified_date<@EndDate)),0)

UPDATE #all SET stored_total=ISNULL((SELECT count(document_id) FROM documents WHERE  stored_by_person_id=@person_id and (stored_date>@StartDate and stored_date<@EndDate)),0)
UPDATE #all SET stored_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and stored_by_person_id=@person_id and (stored_date>@StartDate and stored_date<@EndDate)),0)

UPDATE #all SET deleted_total=ISNULL((SELECT count(document_id) FROM documents WHERE disabled_by_person_id=@person_id and (disabled_date>@StartDate and disabled_date<@EndDate)),0)
UPDATE #all SET deleted_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and disabled_by_person_id=@person_id and (disabled_date>@StartDate and disabled_date<@EndDate)),0)
END

SET @total=(SELECT count(*) FROM #all)

SET @X = (
SELECT * FROM  #all AS search_all
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout 

GOTO header

RETURN
-----------------------------------------------------
header:
/** find user info***/
declare @first_name		varchar(50)
declare @separate		int
--declare @qc_date		smalldatetime	--commented by hareesh on 12/22/2014 6:00pm
--declare @daydiff 		int				--commented by hareesh on 12/22/2014 6:00pm

set @person_name=(select person_name from vw_people where userid=@operator)

IF @person_name<>'' and @person_name is not null 
BEGIN
select  @separate=PATINDEX('%,%', @person_name)
select  @first_name=LEFT(@person_name,@separate-1 )
END 

/*--commented by hareesh on 12/22/2014 6:00pm
SELECT @qc_date=MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null
IF @qc_date IS NOT NULL
BEGIN
SELECT @daydiff=DATEDIFF(day, getdate(), @qc_date)-1
END
*/

--find user info
declare @count		int
SET @count=(select COUNT(*) from vw_people where userid=LOWER(@operator) and application_type='egrants' and can_egrants_upload='y' )

IF @count=1
BEGIN
SET @X = (
SELECT *, @enddate as end_date,@total as total,convert(varchar,getdate(),101) as today,@target_person_name as target_person_name
FROM vw_people as user_info
WHERE userid=LOWER(@operator)
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
END
ELSE
BEGIN
SELECT null
END

--added by hareesh on 12/22/2014 6:00pm
--return user's info
--SELECT person_id, person_id, person_name,ISNULL(@first_name, NULL) as first_name,userid,profile_id,[profile] as ic,
--admin_phs_org_code,position_id,convert(varchar,getdate(),101) as today,can_egrants,can_egrants_upload,can_cft,can_mgt,can_docman,can_admin
--FROM vw_people 
--WHERE userid=@operator and application_type='egrants' and can_egrants_upload='y'


/*--commented by hareesh on 12/22/2014 6:00pm
SET @X = (
SELECT person_id, person_name,ISNULL(@first_name, NULL) as first_name, userid,profile_id,[profile] as ic, 
@StartDate as startdate,@EndDate as enddate,@daydiff as qc_days,application_type AS user_type,position_id,
@total as total, UPPER(@person_name)AS person,UPPER(@target_person_name) AS target_person_name,
convert(varchar,@StartDate,101) AS [start_date], convert(varchar,@EndDate,101) AS end_date,
can_egrants,can_cft,can_mgt,can_docman,can_admin
FROM vw_people AS user_info 
WHERE userid=@operator
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

RETURN
*/
-----------------------------------------------------
GO


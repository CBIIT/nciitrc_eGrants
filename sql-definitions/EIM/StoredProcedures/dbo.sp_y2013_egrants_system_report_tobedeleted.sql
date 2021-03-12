SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_egrants_system_report_tobedeleted]

@act				varchar(10),
@serialnum			int,
@AccessionId		int,
--@TimeRange		varchar(10),
--@StartDate 		smalldatetime,
--@EndDate  		smalldatetime,
@inst				varchar(10),
@operator			varchar(50)

AS
/************************************************************************************************************/
/***									 								***/
/***	Procedure Name:dbo. sp_y2013_egrants_system_report				***/
/***	Description: Find the egrants system data						***/
/***	Created:	02/28/2012	 leon									***/
/***	Modified:	03/28/2012	 leon									***/
/***	Modified:	10/04/2014	 leon	simplify						***/
/***	Modified:	11/14/2014	 leon	simplify						***/
/************************************************************************************************************/

SET NOCOUNT ON

DECLARE
@profile_id			smallint,
@person_id			int,
@xmlout				varchar(max),
@X					Xml

set @profile_id=( select profile_id from eim.dbo.profiles where profile=@inst)
set @person_id=(select person_id from eim.dbo.people where userid=@operator)

/**display accession mumbers**/
SET @X = (
SELECT accession_id,accession_number,accession_year,accession_counter 
FROM eim.dbo.accessions as accession
WHERE profile_id=@profile_id and contract=0 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

IF (@act=''or @act is null) GOTO head
---IF @act ='report' GOTO report
IF @act ='accession' GOTO accession
IF @act ='serialnumsearch' GOTO serialnum
------------------------------
serialnum:

SET @X = (
SELECT folder_id, grant_num,[status],latest_access_date 
FROM vw_folders AS folder
WHERE @serialnum in (serial_num,former_serial_num,future_serial_num) and profile_id=@profile_id
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

GOTO head

RETURN
----------------
report:
/**
DECLARE
@pis_delinquent		int,
@check_out_folers	int,
@destroyed_folders 	int,
@day 				int,
@days 				int,
@month				int,
@year 				int,
@today  			smalldatetime

--set basic date as today
IF (@StartDate='' or @StartDate is null) and (@EndDate='' or @EndDate is null) and (@TimeRange='' or  @TimeRange is null )
BEGIN
SET @StartDate=convert(varchar,getdate(),101)	
SET @EndDate=(SELECT DATEADD(day,1,@StartDate ))
END

--set time range
IF (@StartDate is not null or @StartDate<>'') and  (@EndDate is not null or @EndDate<>'' )
BEGIN
SET @StartDate =convert(varchar,@StartDate,101)	
SET @EndDate =convert(varchar,@EndDate,101)	
END

--today 
SET @today =convert(varchar,getdate(),101)	

--today 
IF @TimeRange='Today'
BEGIN
SET @StartDate=@today 
SET @EndDate=(SELECT DATEADD(day,1,@StartDate ))
END

--this week 
IF @TimeRange='This_Week'
BEGIN
SET DATEFIRST 1
SET @day=(SELECT DATEPART(dw, @today) )
SET @StartDate = (SELECT @today - @day+1)
SET @EndDate=(SELECT @StartDate+5+2)
END

--last tweek
IF @TimeRange='Last_Week'
BEGIN
SET DATEFIRST 1
SET @day=(SELECT DATEPART(dw, @today) )
SET @StartDate = (SELECT @today - @day-6)
SET @EndDate=(SELECT @StartDate+5+2)
END

--this month
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

--last month
IF @TimeRange='Last_Month'
BEGIN
SET @day =DAY(@today)
SET @month=MONTH (@today)

set @StartDate =DATEADD(month, -1, @today)
set @StartDate =dateadd(day, -(day( @StartDate)-1), @StartDate)
set @endDate =dateadd(month,1, @StartDate)-1

END

/**find created page count**/
SET @X = (
SELECT UPPER(created_by) as 'created_by',SUM(page_count) as 'page_count'
FROM eim.dbo.egrants as records
WHERE profile_id=@profile_id and page_count is not null and created_date between @StartDate and @EndDate and (created_by<>'impac' and created_by<>'efile') 
GROUP BY created_by
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/**find QC and stored page count**/ 
SET @X = (
SELECT UPPER(stored_by) as 'stored_by',SUM(page_count)as 'page_count'
FROM eim.dbo.egrants as records
WHERE profile_id=@profile_id and page_count is not null and stored_date between @StartDate  and @EndDate
GROUP BY stored_by
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

**/

GOTO head

RETURN
----------------
accession:

/**find data by accession id**/
SET @X = (
SELECT
--accession.accession_id,
folder_id,
bar_code,
admin_phs_org_code,
serial_num,
former_grant_num,
id_string,
[status],
convert(varchar,latest_move_date,101) as latest_move_date,
closed_out
FROM eim.dbo.vw_boxes as accession, eim.dbo.vw_folders as folder
WHERE accession.box_id=folder.box_id and accession.accession_id=@AccessionId
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

GOTO head

RETURN
--------------
head:

---find user info***/
DECLARE @count int
SET @count=(select COUNT(*) from vw_people where userid=LOWER(@operator) and application_type='egrants' and can_egrants_upload='y')

IF @count=1
BEGIN
SET @X = (
SELECT *,convert(varchar,getdate(),101) as today
FROM vw_people as user_info
where userid=LOWER(@operator)
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
END
ELSE
BEGIN
SELECT null
END

/** find user info***/
--declare @first_name		varchar(50)
--declare @separate		int
--declare @person_name	varchar(50)
--declare @qc_date		smalldatetime	--commented on 12/22/2014 5:30pm by hareeshj
--declare @daydiff 		int				--commented on 12/22/2014 5:30pm by hareeshj

/*--commented on 12/22/2014 5:30pm by hareeshj
SELECT @qc_date=MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null
IF @qc_date IS NOT NULL
BEGIN
SELECT @daydiff=DATEDIFF(day, getdate(), @qc_date)-1
END
*/

--set @person_name=(select person_name from people where person_id=@person_id)

--IF @person_name<>'' and @person_name is not null 
--BEGIN
--select  @separate=PATINDEX('%,%', @person_name)
--select  @first_name=LEFT(@person_name,@separate-1 )
--END 


----return user's info
--SELECT person_id, person_id, person_name,ISNULL(@first_name, NULL) as first_name,userid,profile_id,[profile] as ic,
--admin_phs_org_code,position_id,convert(varchar,getdate(),101) as today,can_egrants,can_egrants_upload,can_cft,can_mgt,can_docman,can_admin
--FROM vw_people 
--WHERE userid=@operator and application_type='egrants' and can_egrants_upload='y'  


/*--commented on 12/22/2014 5:30pm by hareeshj

SET @X = (
SELECT person_id, person_name,ISNULL(@first_name, NULL) as first_name,userid, profile_id,[profile] as ic, 
@daydiff as qc_days,application_type AS user_type,position_id,
---convert(varchar,@StartDate,101) as [start_date],convert(varchar,@EndDate,101) as end_date,
can_egrants,can_cft,can_mgt,can_admin
FROM eim.dbo.vw_people AS user_info 
WHERE userid=@operator 
FOR XML AUTO, TYPE, ELEMENTS
)

select @xmlout = cast(@X as varchar(max))
select @xmlout
*/


GO


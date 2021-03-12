SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_y2013_egrants_dashboard_tobedeleted]

@act		varchar(20),
@idstr		varchar(20),
@ic			varchar(10),
@operator	varchar(50)

AS
/************************************************************************************************************/
/***									 							***/
/***	Procedure Name:  sp_y2013_egrants_dashboard					***/
/***	Description:egrants dashboard								***/
/***	Created:	08/12/2016		Leon							***/
/***	Modified:	09/15/2016		Leon							***/
/***	Modified:	09/29/2016		Leon	add new grants			***/
/***	Modified:	12/08/2016		Leon	simplified				***/
/************************************************************************************************************/

SET NOCOUNT ON

DECLARE @person_id 			int
DECLARE @profile_id			int
DECLARE @separate			int
DECLARE @person_name		varchar(50)
DECLARE @first_name			varchar(20)
DECLARE @sql				varchar(800)
DECLARE @count				int
DECLARE @total_selections	int
DECLARE @xmlout				varchar(max)
DECLARE @X					Xml
DECLARE @USERID				varchar(50)
SET @USERID=@operator
--SET @USERID='OMAIRI'
--SET @USERID='BROWNELD'
--BROWNELD
--FISHERB
--GASTLEYK
--BIRKENJG
--BOUDJEDAJ

declare @StartDate smalldatetime
declare @EndDate	smalldatetime

--set start date as six month ago
set @StartDate = Convert(date, (DATEADD(month, -6, getdate()))) 
set @EndDate = DATEADD(month, 6, @StartDate)

/**find profile_id **/
SET @profile_id=(select profile_id FROM profiles WHERE profile=@ic)
SET @person_id = (SELECT person_id FROM vw_people WHERE userid=@operator)
SET @person_name=(SELECT person_name from vw_people where person_id=@person_id)


--check permission
SET @count=(select COUNT(*) from vw_people where userid=LOWER(@operator) and application_type='egrants' and can_dashboard='y')

--return user info
IF @count=1
BEGIN
SET @X = (
SELECT *
FROM vw_people AS user_info 
WHERE userid=LOWER(@operator)
FOR XML AUTO, TYPE, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout
END
ELSE
BEGIN
SELECT null
END

---find user personal selections
--SET @total_selections=(select COUNT(*) from dbo.DB_Widget_Master where end_date is null)
--SET @count = (select COUNT(*) from dbo.DB_WIDGET_ASSIGNMENT where userid=@operator)
--IF @count=0 
--BEGIN
--DECLARE @i int
--SET @i=1
--WHILE @i<@total_selections+1
--BEGIN
--   insert dbo.DB_WIDGET_ASSIGNMENT(person_id,userid,widget_id,sortorder,[start_date])
--   select @person_id,@operator,@i,@i,GETDATE()
--   set @i=@i+1
--END
--END

IF @act='' or @act is null GOTO header
IF @act='set_assignment' GOTO set_assignment

--RETURN
--------------------
set_assignment:

CREATE TABLE #assignment(widget_id int)
SET @sql='INSERT #assignment SELECT WIDGET_ID FROM dbo.DB_Widget_Master WHERE WIDGET_ID in (' + @idstr + ')'
EXEC (@sql)

--DISABLE ALL PAST ASSIGNMENT
UPDATE dbo.DB_WIDGET_ASSIGNMENT 
SET end_date = GETDATE()
WHERE userid=@operator AND person_id=@person_id AND end_date IS NULL

--INSERT NEW WIDGET SELECTION TO ASSIGNMENT TABLE
INSERT dbo.DB_WIDGET_ASSIGNMENT(person_id,userid,widget_id,sortorder,start_date)
SELECT @person_id,@operator,w.widget_id,w.widget_id,GETDATE()
FROM #assignment a, DB_Widget_Master w
WHERE w.widget_id=a.widget_id

GOTO header

RETURN
-----------------------------------
header:

---return total widgets 
SET @X = 
(
SELECT max(widget_id) as total_widget 
FROM dbo.DB_Widget_Master as total_widgets
WHERE end_date is null 
FOR XML AUTO, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

---return widget master with personal selections
SET @X = 
(
SELECT widget_id,widget_title,dbo.fn_get_personal_assigment(@person_id,widget_id) as selected
FROM dbo.DB_Widget_Master as widget
WHERE end_date is null 
FOR XML AUTO, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

---return assignment by personal selected
SET @X = 
(
SELECT ROW_NUMBER()OVER(ORDER BY widget.widget_id) AS order_id, widget.widget_id,widget_title,template_name
FROM dbo.DB_Widget_Master as widget, dbo.DB_WIDGET_ASSIGNMENT a
WHERE widget.widget_id=a.widget_id and widget.end_date is null and a.person_id=@person_id and a.end_date is null
FOR XML AUTO, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

--return EXPEDITED_GRANTS 
SELECT appl_id,fgn,ncab_date
FROM dbo.DB_EARLY_CONCUR_FLAG as appl
WHERE USERID=@USERID
ORDER BY ncab_date ASC
FOR XML AUTO,TYPE,ELEMENTS

--return late grants
SELECT APPL_ID as appl_id,FGN as fgn,STATUS_CODE as status_code,
	CASE APPL_TYPE_CODE
		WHEN 5 THEN '['+CAST( DAY_COUNT_NUM as varchar)+'/45]'
	END AS days_late
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS as appl
WHERE USERID=@USERID AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED')
AND APPL_TYPE_CODE IN (5) AND (ACTIVITY_CODE NOT like 'P%' AND ACTIVITY_CODE NOT like 'U%') AND DAY_COUNT_NUM > 45
UNION
SELECT APPL_ID as appl_id,FGN as fgn,STATUS_CODE as status_code,
	CASE APPL_TYPE_CODE
		WHEN 5 THEN '['+CAST( DAY_COUNT_NUM as varchar)+'/60]'
	END AS days_late
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS as appl
WHERE USERID=@USERID AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED')
AND APPL_TYPE_CODE IN (5) AND (ACTIVITY_CODE like 'P%' OR ACTIVITY_CODE like 'U%') AND DAY_COUNT_NUM > 60
UNION
SELECT APPL_ID as appl_id,FGN as fgn,STATUS_CODE as status_code,
	CASE APPL_TYPE_CODE
		WHEN 1 THEN '['+CAST( DAY_COUNT_NUM as varchar)+'/60]'
		WHEN 2 THEN '['+CAST( DAY_COUNT_NUM as varchar)+'/60]'
	END AS days_late
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS as appl
WHERE USERID=@USERID AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED') AND APPL_TYPE_CODE IN (1,2) AND DAY_COUNT_NUM > 60
FOR XML AUTO,TYPE,ELEMENTS

---return new grants 
DECLARE @CUTOFFPERIOD	int
DECLARE @TYPE			varchar(20)
SET @TYPE=''
SET @CUTOFFPERIOD = 10

SELECT appl_id, FGN as fgn,convert(varchar(10),GRANT_ASSIGN_DATE,101) AS assigned_date
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS as appl 
WHERE USERID=@USERID and STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED') AND DAY_COUNT_NUM <= @CUTOFFPERIOD
FOR XML AUTO,TYPE,ELEMENTS

---return grants to go cc
SELECT APPL_ID as appl_id,FGN as fgn,convert(varchar(10),GRANT_ASSIGN_DATE,101) + ' ['+CAST( DAY_COUNT_NUM as varchar)+'/60]' AS assigned_date
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  as appl	
WHERE USERID=@USERID AND APPL_TYPE_CODE IN (1,2) AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED') and DAY_COUNT_NUM BETWEEN 50 AND 60
ORDER BY GRANT_ASSIGN_DATE
FOR XML AUTO,TYPE,ELEMENTS

---return grants to go nc
SELECT APPL_ID as appl_id,FGN as fgn,convert(varchar(10),GRANT_ASSIGN_DATE,101) + ' ['+CAST( DAY_COUNT_NUM as varchar)+'/45]' AS assigned_date
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS as appl
WHERE USERID=@USERID AND APPL_TYPE_CODE IN (5) AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED') AND ACTIVITY_CODE NOT like '%P%' AND ACTIVITY_CODE NOT like '%U%' and DAY_COUNT_NUM BETWEEN 35 AND 45
UNION	
SELECT APPL_ID as appl_id,FGN as fgn,convert(varchar(10),GRANT_ASSIGN_DATE,101) + ' ['+CAST( DAY_COUNT_NUM as varchar)+'/60]' AS assigned_date
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  as appl
WHERE USERID=@USERID AND APPL_TYPE_CODE IN (5) AND STATUS_CODE NOT IN ('CLOSED','NEW','CANCELLED') AND (ACTIVITY_CODE like 'P%' OR ACTIVITY_CODE like 'U%') and DAY_COUNT_NUM BETWEEN 50 AND 60	
ORDER BY ASSIGNED_DATE 	
FOR XML AUTO,TYPE,ELEMENTS	
	
--return average processing time data
DECLARE @type1_avg		int
DECLARE @type5_avg		int
DECLARE @other_avg		int
DECLARE @type1_count	int
DECLARE @type5_count	int
DECLARE @other_count	int

SET @type1_avg=(SELECT AVG(a.RELEASED_IN_DAYS) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID  AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE in(1 ,2)
)a)

SET @type5_avg=(SELECT AVG(a.RELEASED_IN_DAYS) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE=5
)a)

SET @other_avg=(SELECT AVG(a.RELEASED_IN_DAYS) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS 
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE not in(1,2,5)
)a)

SET @type1_count=(SELECT COUNT(*) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS 
FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE in(1,2)
)a)

SET @type5_count=( SELECT COUNT(*) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE=5
)a)

SET @other_count=(SELECT COUNT(*) FROM( 
SELECT DATEDIFF(day,GRANT_ASSIGN_DATE,RELEASE_DATE) as RELEASED_IN_DAYS FROM dbo.DB_GPMATS_ASSIGNMENT_STATUS  
WHERE USERID=@USERID AND action_type='AWARD' AND GRANT_ASSIGN_DATE IS NOT NULL  AND STATUS_CODE='CLOSED' and APPL_TYPE_CODE not in(1,2,5)
)a)

SET @X=
(
SELECT @type1_avg as avg_daystaken, @type1_count as grants_count FROM performance as type_1
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

SET @X=
(
SELECT @type5_avg as avg_daystaken, @type5_count as grants_count FROM performance as type_5
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

SET @X=
(
SELECT @other_avg as avg_daystaken, @other_count as grants_count FROM performance as others
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

--return grants assigment status
SET @X=
(
SELECT 
action_type as [action],
(
SELECT status_code, COUNT(*) AS grants_count
FROM DB_GPMATS_ASSIGNMENT_STATUS as [status]
WHERE [status].ACTION_TYPE=actions.ACTION_TYPE
GROUP BY ACTION_TYPE,status_code
FOR XML AUTO, TYPE, ELEMENTS
)
FROM DB_GPMATS_ASSIGNMENT_STATUS as actions
GROUP BY ACTION_TYPE
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

---return link list
SELECT category_name, category_id,
(SELECT Link_title as link_title,Link_url as url,sort_order,
CASE
	WHEN icon_name is null THEN ''
	WHEN icon_name is not null THEN icon_name 
END as icon_name 
---dbo.fn_clean_characters(Link_url) as Link_url
FROM dbo.DB_WIDGET_LINK as link
WHERE link.Category_name=category.Category_name and end_date is null and id<>3
FOR XML AUTO, TYPE, ELEMENTS
)
FROM dbo.DB_WIDGET_LINK as category
WHERE end_date is null and id<>3  and id<>3 and Category_name<>''
GROUP BY Category_name,category.Category_id 
ORDER BY category.Category_name desc
FOR XML AUTO, TYPE, ELEMENTS

---return audit report
DECLARE @audit_report_url	varchar(800)
SET @audit_report_url='data/funded/egrantsadmin/auditreport'

SELECT [File_name] as report_name,@audit_report_url + '/'+ [File_name] as report_url, convert(varchar(10),Run_date,101) as run_date
FROM dbo.egrants_audit_report as report
WHERE Run_date between @StartDate and @EndDate
ORDER BY run_date desc
FOR XML AUTO, TYPE, ELEMENTS


RETURN

GO


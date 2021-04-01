SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_egrants_pagination]

@str 				nvarchar(400),
@package 			varchar(50)=null,
@ic  				varchar(10),
@operator 			varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_grants_search_pagination							***/
/***	Description:set pagination by searching	string							***/
/***	Created:	09/30/2016	Frances											***/
/***	Modified:	2/15/2019	Leon                          					***/
/************************************************************************************************************/

SET NOCOUNT ON

declare @sql			varchar(800)
declare @person_id		int
declare @profile_id		int
declare @separate		int

--for filter searching
declare @filter_type		int
declare @activity_code		varchar(3)
declare	@admin_phs_org_code	varchar(2)

/** find and return user info***/
SET @profile_id=(select profile_id from dbo.profiles where profile=@ic)
SET @person_id=(select person_id from dbo.vw_people where userid=@operator )

/**search by nothing and set by default**/
CREATE TABLE #t (id int IDENTITY (1, 1) NOT NULL, grant_id int, serial_num int)

/**search by  or piid**/
IF (@str = 'qc') GOTO by_qc 
--ELSE IF (SUBSTRING(@str,1,7)='filters') GOTO by_filters
ELSE IF @package='by_filters' GOTO by_filters
ELSE GOTO by_str
--------------------------------------------------
by_filters:

SET @sql='INSERT #t(grant_id, serial_num) '+ @str
EXEC(@sql)

GOTO pagination

RETURN
--------------------------------------------------
by_qc: 

-----find if need pagination
SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num FROM egrants 
WHERE grant_id is not null and appl_id is not null and qc_date is not null and parent_id is null and stored_date is null and qc_person_id='+convert(varchar,@person_id)---distinct 
EXEC(@sql)

GOTO pagination

RETURN
--------------------------------------------------
by_str:

SET @str=LTRIM(RTRIM(dbo.fn_str_decode(@str)))
SET @str=dbo.fn_str(@str)

--SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.appl_id=c.[key] and a.admin_phs_org_code='+char(39)+'CA'+char(39)+' and a.closed_out='+char(39)+'no'+char(39)+ 'order by serial_num'
SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.appl_id=c.[key] order by serial_num'

EXEC(@sql)

GOTO pagination

RETURN
--------
pagination:

CREATE TABLE #tabs (tab_number int)
CREATE TABLE #pages (tab_number int, page_number int)

declare @per_page			int
declare @per_tab			int
declare @start			    int
declare @end				int
declare @total_grants   	int
declare @total_pages		int
declare @total_tabs			int

/**set @current_page to defult of 1 if not entered**/
SET @total_pages=1
SET @total_tabs=1 
--SET @per_page=20
IF @str='qc' SET @per_page=5 ELSE SET @per_page=20
SET @per_tab=20

/**find total number from #t **/
SELECT @total_grants = count(*) from #t 
SELECT @total_pages = ceiling(convert(real,@total_grants)/@per_page)
SELECT @total_tabs = ceiling(convert(real,@total_grants)/@per_page/@per_tab)

IF @total_tabs>=1
BEGIN
DECLARE @i INT
SET @i=1
WHILE @i<=@total_pages
BEGIN
	INSERT #pages(page_number) SELECT @i
	SET @i=@i+1
END
UPDATE #pages SET tab_number=ceiling(convert(real,page_number)/@per_tab)

--insert all tabs 
INSERT #tabs(tab_number) SELECT distinct tab_number FROM #pages 
END

--display all tabs and pages
SELECT distinct
1					as tag,
null				as parent,
@total_grants   	as total_grants,
@total_tabs			as total_tabs,
@total_pages		as total_pages,
null				as tab_number,
null				as page_number

UNION

SELECT distinct
2                  as tag,
1                  as parent,
null,
null,
null,
tab_number,
null	
FROM #tabs 

UNION

SELECT 
3,
2,
null,
null,
null,
#tabs.tab_number,
page_number 
FROM #pages, #tabs 
WHERE #pages.tab_number = #tabs.tab_number 
ORDER BY tab_number

--SELECT tab_number,
--(select page_number from #pages as page where tab_number=tab.tab_number)
--from #tabs as tab order by tab_number 


GO


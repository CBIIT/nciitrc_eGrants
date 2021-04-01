SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[OGA_OVERALL_WORKLOAD]
--@ACTIONFY VARCHAR(4)
AS

BEGIN
--oct through august sum .. prev yr.
--sept the prev yr
----0 as "Oct Rem"
DECLARE @SQL varchar(4000)

SET @SQL= 'select * from OPENQUERY(CIIP,''SELECT RESP_SPEC_FULL_NAME_CODE,
OCT_CNT AS "Oct Cnt", OCT_REL as "Oct Rel", OCT_CNT - OCT_REL as "Oct Rem", 0 as "Oct %",  
Nov_CNT AS "Nov Cnt", NOV_REL as "Nov Rel", Nov_CNT - NOV_REL as "Nov Rem", 0 as "Nov %",
Dec_CNT AS "Dec Cnt", DEC_REL as "Dec Rel", Dec_CNT - Dec_REL as "Dec Rem", 0 as "Dec %",
JAN_CNT AS "Jan Cnt", JAN_REL as "Jan Rel", JAN_CNT - JAN_REL as "Jan Rem", 0 as "Jan %",
FEB_CNT AS "Feb Cnt", FEB_REL as "Feb Rel", FEB_CNT - FEB_REL as "Feb Rem", 0 as "Feb %",
MAR_CNT AS "Mar Cnt", MAR_REL as "Mar Rel", MAR_CNT - MAR_REL as "Mar Rem", 0 as "Mar %",
APR_CNT AS "Apr Cnt", APR_REL as "Apr Rel", APR_CNT - APR_REL as "Apr Rem", 0 as "Apr %",
MAY_CNT AS "May Cnt", MAY_REL as "May Rel", MAY_CNT - MAY_REL as "May Rem", 0 as "May %",
JUN_CNT AS "Jun Cnt", JUN_REL as "Jun Rel", JUN_CNT - JUN_REL as "Jun Rem", 0 as "Jun %",
JUL_CNT AS "Jul Cnt", JUL_REL as "Jul Rel", JUL_CNT - JUL_REL as "Jul Rem", 0 as "Jul %",
AUG_CNT AS "Aug Cnt", AUG_REL as "Aug Rel", AUG_CNT - NOV_REL as "Aug Rem", 0 as "Aug %",
SEP_CNT AS "Sep Cnt", SEP_REL as "Sep Rel", SEP_CNT - SEP_REL as "Sep Rem", 0 as "Sep %",
TOTAL_CNT AS "Total Count", TOTAL_REL as "Total Released", TOTAL_CNT - TOTAL_REL as "Total Not Released", 
0 as "Pct Released", 0 as "Pct Not Released", TOTL_WRKLD as "Total Workload" '

SET @SQL=@SQL + 'FROM OGA_OVERALL_WORKLOAD WHERE ACTION_FY=2014 order by RESP_SPEC_FULL_NAME_CODE'')'

PRINT @SQL


EXEC (@SQL)

END

GO


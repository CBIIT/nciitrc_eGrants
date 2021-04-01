SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



--Large_Application_W_Notes

CREATE PROCEDURE [dbo].[Large_Application_W_Notes]
--@ProjectedStartDate datetime
AS

/************************************************************************************************************/
/***									 				***/
/***	Procedure Name: Large_Application_W_Notes		***/
/***	Description:	Large_Application_W_Notes		***/
/***	Created:	09/19/2011	Hareesh					***/
/***	Modification: 09/30/2011 Imran, Added Release_date as a data item												***/
/************************************************************************************************************/

BEGIN

--DECLARE @ProjectedStartDate DATETIME
--Set @ProjectedStartDate = '09/19/2011'

DECLARE @SQL varchar(4000)
--DECLARE @ProjStrtDt DATETIME
--DECLARE @ModProjStrtDt varchar(10)
--SET @ProjStrtDt = @ProjectedStartDate
--SET @ModProjStrtDt = CONVERT(varchar(10),@ProjStrtDt,101)
--SELECT @ProjectedStartDate, @ProjStrtDt, @ModProjStrtDt


SET @SQL = 'SELECT * FROM OPENQUERY(CIIP, ''SELECT FULL_GRANT_NUM, CURRENT_ACTION_COMMENTS, ORG_NAME, RESP_SPEC_FULL_NAME_CODE, PI_LAST_NAME, 
PD_LAST_NAME, TO_CHAR(PROJECTED_START_DATE,''''YYYY-MM-DD'''') AS PROJECTED_START_DATE, TO_CHAR(TRUNC(RELEASE_DATE),''''YYYY-MM-DD'''')  AS "RELEASED DATE", TO_CHAR(APPLICATION_RECEIPT_DATE,''''YYYY-MM-DD'''') AS APPLICATION_RECEIPT_DATE, 
CURRENT_ACTION_STATUS_DESC, GM_NOTES '
SET @SQL = @SQL + ' FROM gm_action_queue_vw WHERE INTERNAL_CODE_CODE=''''EARLY'''' AND REVISION_NUM IS NULL ' 
--SET @SQL = @SQL + ' AND trunc(projected_start_date) <='
--SET @SQL = @SQL + ' ADD_MONTHS(TRUNC(TO_DATE(''''' + @ModProjStrtDt + ''''',''''MM/DD/YYYY''''),''''MM''''),1)'
SET @SQL = @SQL + ' ORDER BY SERIAL_NUM'')'

--PRINT @SQL

EXEC (@SQL)

END



GO


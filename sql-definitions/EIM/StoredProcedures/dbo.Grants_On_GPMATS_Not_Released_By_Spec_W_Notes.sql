SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


--Grants_On_GPMATS_Not_Released_By_Spec_W_Notes '9/22/2011'

CREATE PROCEDURE [dbo].[Grants_On_GPMATS_Not_Released_By_Spec_W_Notes]

--@ProjectedStartDate datetime

AS

/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: Grants_On_GPMATS_Not_Released_By_Spec_W_Notes		***/
/***	Description:	Grants_On_GPMATS_Not_Released_By_Spec_W_Notes		***/
/***	Created:	09/19/2011	Hareesh										***/
/***																		***/
/************************************************************************************************************/

BEGIN

--DECLARE @ProjectedStartDate DATETIME
--Set @ProjectedStartDate = '09/19/2011'

DECLARE @SQL varchar(4000)
DECLARE @ProjStrtDt DATETIME
DECLARE @ModProjStrtDt varchar(10)
--SET @ProjStrtDt = @ProjectedStartDate
--SET @ModProjStrtDt = CONVERT(varchar(10),@ProjStrtDt,101)

--SELECT @ProjectedStartDate, @ProjStrtDt, @ModProjStrtDt

--SET @SQL = 'SELECT * FROM OPENQUERY(CIIP, ''select RESP_SPEC_FULL_NAME_CODE, IMPACII_FULL_GRANT_NUM, CURRENT_ACTION_COMMENTS,
--ORG_NAME, PI_LAST_NAME, PD_LAST_NAME, PROJECTED_START_DATE, TRUNC(APPLICATION_RECEIPT_DATE) APPLICATION_RECEIPT_DATE,
--CURRENT_ACTION_STATUS_DESC, GM_NOTES '
--SET @SQL = @SQL + 'from gm_action_queue_vw where fy = 2011 and revision_num is null and RELEASE_DATE is null and GM_NOTES_AVAILABLE_FLAG = ''''Y'''' and trunc(projected_start_date) <=  '
--SET @SQL = @SQL + 'ADD_MONTHS(TRUNC(TO_DATE(''''' + @ModProjStrtDt + ''''',''''MM/DD/YYYY''''),''''MM''''),1) order by RESP_SPEC_FULL_NAME_CODE, SERIAL_NUM'')'
--PRINT @SQL

SET @SQL = 'SELECT * FROM OPENQUERY(CIIP, ''select RESP_SPEC_FULL_NAME_CODE, IMPACII_FULL_GRANT_NUM, CURRENT_ACTION_COMMENTS,
ORG_NAME, PI_LAST_NAME, PD_LAST_NAME, PROJECTED_START_DATE, TRUNC(APPLICATION_RECEIPT_DATE) APPLICATION_RECEIPT_DATE,
CURRENT_ACTION_STATUS_DESC, GM_NOTES '
SET @SQL = @SQL + 'from gm_action_queue_vw where APPL_ID IN (SELECT DISTINCT APPL_ID FROM NCIGAB.OGA_WORKLOAD_POST) and revision_num is null and RELEASE_DATE is null and GM_NOTES_AVAILABLE_FLAG = ''''Y'''' and trunc(projected_start_date) <=  '
SET @SQL = @SQL + 'ADD_MONTHS(TRUNC(SYSDATE,''''MM''''),1) order by RESP_SPEC_FULL_NAME_CODE, SERIAL_NUM'')'

--PRINT @SQL

--SELECT * FROM OPENQUERY(CIIP, 'select RESP_SPEC_FULL_NAME_CODE, IMPACII_FULL_GRANT_NUM, CURRENT_ACTION_COMMENTS,
--ORG_NAME, PI_LAST_NAME, PD_LAST_NAME, PROJECTED_START_DATE, TRUNC(APPLICATION_RECEIPT_DATE) APPLICATION_RECEIPT_DATE,
--CURRENT_ACTION_STATUS_DESC, GM_NOTES from gm_action_queue_vw 
--where APPL_ID IN (SELECT DISTINCT APPL_ID FROM NCIGAB.OGA_WORKLOAD_POST)
--and revision_num is null and RELEASE_DATE is null and GM_NOTES_AVAILABLE_FLAG = ''Y'' 
--and trunc(projected_start_date) <=  ADD_MONTHS(TRUNC(SYSDATE,''MM''),1)
--order by RESP_SPEC_FULL_NAME_CODE, SERIAL_NUM')


EXEC (@SQL)

END


GO


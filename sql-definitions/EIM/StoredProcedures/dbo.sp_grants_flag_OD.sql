SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF



/*************************************************************************************************
*  Author :   Imran Omair
*  Date   :   3/16/2019
*
*************************************************************************************************/
CREATE PROCEDURE [dbo].[sp_grants_flag_OD] 
AS

BEGIN

DECLARE 
	@SYSUSR INT,
	@FLAGAPPLICATION CHAR(1),
	@FLAGTYPE VARCHAR(5)

SET @FLAGTYPE='OD'  --HARD CODE :  BECAUSE THIS PROC IS FOR LAG

SELECT @SYSUSR=PERSON_ID FROM People WHERE person_name='SYSTEM'

SELECT @FLAGAPPLICATION=flag_application_code 
FROM dbo.Grants_Flag_Master 
WHERE flag_type_code=@FLAGTYPE 
AND end_date IS NULL 
AND flag_gen_type='automatic'


--select A.APPL_ID,A.grant_id,@FLAGTYPE,@FLAGAPPLICATION from vw_appls A, (SELECT DISTINCT APPL_ID FROM OPENQUERY(IRDB,'SELECT APPL_ID FROM AWD_FUNDINGS_MV WHERE PHS_ORG_CODE=''CA'' AND CAN IN (
--SELECT CAN FROM CANS_MV WHERE PHS_ORG_CODE=''OD'')')) B  WHERE A.APPL_ID=B.APPL_ID


INSERT DBO.Grants_Flag_Construct (APPL_ID,grant_id,flag_type,flag_application,start_dt,created_by,created_dt)
select A.APPL_ID,A.grant_id,@FLAGTYPE,@FLAGAPPLICATION, GETDATE(),1899, GETDATE() from vw_appls A, (SELECT DISTINCT APPL_ID FROM OPENQUERY(IRDB,'SELECT APPL_ID FROM AWD_FUNDINGS_MV WHERE PHS_ORG_CODE=''CA'' AND CAN IN (
SELECT CAN FROM CANS_MV WHERE PHS_ORG_CODE=''OD'')')) B  WHERE A.APPL_ID=B.APPL_ID
AND A.grant_id not in 
(
	SELECT B.grant_id FROM dbo.Grants_Flag_Construct B WHERE  b.grant_id=a.grant_id and b.flag_type=@FLAGTYPE and b.appl_id=a.appl_id
)
and A.appl_id not in 
(
	SELECT B.appl_id FROM dbo.Grants_Flag_Construct B WHERE b.grant_id=a.grant_id and b.flag_type=@FLAGTYPE and b.appl_id=a.appl_id
)

PRINT 'NEW OD FLAGS ADDED = ' + CAST(@@ROWCOUNT AS VARCHAR) 
PRINT '==>> PROC [sp_grants_flag_OD] FINISHED AT = ' + CAST(GETDATE() AS VARCHAR)

END

GO


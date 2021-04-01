SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF



/*************************************************************************************************
*  Author :   Imran Omair
*  Date   :   3/16/2019
*  Date	:	5/17/2019: NEW QUERY HAS BEEN SENT BY lISA THEREFORE COMMENTING OUT OLD ONE BELLOW AND ADDING NEWONE UNDERNEATH
*************************************************************************************************/
CREATE PROCEDURE [dbo].[sp_grants_flag_MS] 
AS

BEGIN

DECLARE 
	@SYSUSR INT,
	@FLAGAPPLICATION CHAR(1),
	@FLAGTYPE VARCHAR(5)

SET @FLAGTYPE='MS'  --HARD CODE :  BECAUSE THIS PROC IS FOR LAG

SELECT @SYSUSR=PERSON_ID FROM People WHERE person_name='SYSTEM'

SELECT @FLAGAPPLICATION=flag_application_code 
FROM dbo.Grants_Flag_Master 
WHERE flag_type_code=@FLAGTYPE 
AND end_date IS NULL 
AND flag_gen_type='automatic'

PRINT '==>>  PROC [sp_grants_flag_MS] STARTED AT = ' + cast(GETDATE() as varchar)

TRUNCATE TABLE dbo.grants_flag_MS_WIP

--5/9/2019: iMRAN COMMENTED OUT THE FOLLOWING BECAUSE OF A NEW REQUIREMENT CHANGE
--INSERT dbo.grants_flag_MS_WIP([APPL_ID],[support_year],[suffix_code],[nci_budget_subclass_code] )
--SELECT APPL_ID,SUPPORT_YEAR,SUFFIX_CODE,NCI_BUDGET_SUBCLASS_CODE 
--FROM OPENQUERY(CIIP_DM,'select APPL_ID,SUPPORT_YEAR,SUFFIX_CODE,NCI_BUDGET_SUBCLASS_CODE from RDM.GRANTS_VW
--where  VER_ID = 1  AND RST_ID = 1  AND NCI_BUDGET_SUBCLASS_CODE = ''MS'' ')

/*5/9/2019:Imran:commenting to get a better list of MS*/
--INSERT dbo.grants_flag_MS_WIP([APPL_ID],[support_year],[suffix_code],[nci_budget_subclass_code] )
--SELECT APPL_ID,SUPPORT_YEAR,SUFFIX_CODE,'MS'
--FROM OPENQUERY(IRDB,'SELECT DISTINCT B.APPL_ID,B.SERIAL_NUM,B.ADMIN_PHS_ORG_CODE,B.SUPPORT_YEAR,B.SUFFIX_CODE FROM AWD_FUNDINGS_MV A, APPLS_T B
--WHERE A.APPL_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA''
--AND A.CAN IN (SELECT DISTINCT CAN FROM IRDB.CANS_MV WHERE CFDA_CODE = ''353'')  ')

--5/16/2019: NEW QUERY HAS BEEN SENT BY lISA THEREFORE COMMENTING OUT OLD ONE BELLOW AND ADDING NEWONE UNDERNEATH
--INSERT dbo.grants_flag_MS_WIP([APPL_ID],[support_year],[suffix_code],[nci_budget_subclass_code] )
--SELECT APPL_ID,SUPPORT_YEAR,SUFFIX_CODE,'MS'
--FROM OPENQUERY(IRDB,' SELECT DISTINCT B.APPL_ID,B.SERIAL_NUM,B.ADMIN_PHS_ORG_CODE,B.SUPPORT_YEAR,B.SUFFIX_CODE FROM AWD_FUNDINGS_MV A, APPLS_T B
--WHERE A.APPL_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA''
--AND A.CAN IN (SELECT DISTINCT CAN FROM IRDB.CANS_MV WHERE CFDA_CODE = ''353'')  
--AND B.APPL_STATUS_CODE IN (''05'',''06'') ')

INSERT dbo.grants_flag_MS_WIP([APPL_ID],[support_year],[suffix_code],[nci_budget_subclass_code] )
SELECT APPL_ID,SUPPORT_YEAR,SUFFIX_CODE,'MS'
FROM OPENQUERY(IRDB,'SELECT DISTINCT B.APPL_ID,B.SERIAL_NUM,B.ADMIN_PHS_ORG_CODE,B.SUPPORT_YEAR,B.SUFFIX_CODE  
from awd_fundings_mv A,appls_mv B,cans_mv C where C.CAN = A.CAN and C.CFDA_CODE = ''353''
and B.APPL_ID = A.APPL_ID and B.APPL_STATUS_CODE in (''05'',''06'') and A.PERIOD_TYPE_CODE = ''BUD'' ')



--DELETING THE ONE NOT IN APPLS (MOSTLY FOR DEV TIERS ONLY)
delete grants_flag_MS_WIP where appl_id not in (select distinct appl_id from appls)
PRINT 'APPLS NOT FOUND IN APPL TABLE (IN THIS TIER) = ' + CAST(@@ROWCOUNT AS VARCHAR) 

--5/9/2019: iMRAN COMMENTED OUT THE FOLLOWING BECAUSE OF A NEW REQUIREMENT CHANGE
--UPDATE grants_flag_MS_WIP
--SET grants_flag_MS_WIP.GRANT_ID=(SELECT DISTINCT GRANT_ID FROM vw_appls WHERE APPL_ID=grants_flag_MS_WIP.APPL_ID),
--grants_flag_MS_WIP.ADMIN_PHS_ORG_CODE=(SELECT DISTINCT ADMIN_PHS_ORG_CODE FROM vw_appls WHERE APPL_ID=grants_flag_MS_WIP.APPL_ID),
--grants_flag_MS_WIP.SERIAL_NUM=(SELECT DISTINCT serial_num FROM vw_appls WHERE APPL_ID=grants_flag_MS_WIP.APPL_ID)

UPDATE grants_flag_MS_WIP
SET grants_flag_MS_WIP.GRANT_ID=(SELECT DISTINCT GRANT_ID FROM vw_appls WHERE APPL_ID=grants_flag_MS_WIP.APPL_ID)

/**  INSERT ALL NEW FLAGS CAME IN TODAY    **/

INSERT DBO.Grants_Flag_Construct (grant_id,APPL_ID,flag_type,flag_application,start_dt,created_by,created_dt)
SELECT A.GRANT_ID,A.APPL_ID, @FLAGTYPE, @FLAGAPPLICATION, GETDATE(),1899, GETDATE() 
FROM dbo.grants_flag_MS_WIP A  
WHERE A.grant_id not in 
(
	SELECT B.grant_id FROM dbo.Grants_Flag_Construct B WHERE flag_type='MS' and b.grant_id=a.grant_id and b.flag_type='MS' and b.appl_id=a.appl_id
)

and A.appl_id not in 
(
	SELECT B.appl_id FROM dbo.Grants_Flag_Construct B WHERE flag_type='MS' and b.grant_id=a.grant_id and b.flag_type='MS' and b.appl_id=a.appl_id
)


PRINT 'NEW MOONSHOT FLAGS ADDED = ' + CAST(@@ROWCOUNT AS VARCHAR) 

PRINT '==>> PROC [sp_grants_flag_MS] FINISHED AT = ' + CAST(GETDATE() AS VARCHAR)

END

GO


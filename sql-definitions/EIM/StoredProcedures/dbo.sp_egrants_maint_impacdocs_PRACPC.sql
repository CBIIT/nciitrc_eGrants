SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_egrants_maint_impacdocs_PRACPC] 
AS

DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'    
DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_impacdocs_PRACPC'
SET @RUN_DATETIME = GETDATE()

BEGIN
print 'BEGIN sp_egrants_maint_impacdocs_PRACPC = @ '+ cast(getdate() as varchar)

TRUNCATE TABLE dbo.impp_PiChange

INSERT dbo.impp_PiChange(APPL_ID,DOCUMENT_DATE,keyId)
SELECT APPL_ID, DOCUMENT_DATE,keyId FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,d.doc_key_id as keyId from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
WHERE pa.request_type_id =3 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRACPC'' and d.doc_key_id = pa.pa_request_id ')  
SELECT @IMPAC_DOWNLOAD_CNT = @@ROWCOUNT

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRACPC'

INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.keyId,'PI'
from impp_PiChange zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.keyId not in 
--4/24/2019:Imran:If ImpacII has not created any PRACPC there will be no nga_rpt_seq_num therefore create one, If created and got anotated then do not add another
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null and x.category_id=@CATEGORYID AND x.profile_id=1)
--4/24/2019 : Imran : Requirement change CR-1937. Commenting out folowing code and adding code above
SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT
print 'NEW Post-Award Change PI INSERTED =' + cast(@EGRANTS_UPLOAD_CNT as varchar)
EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'PRACPC',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,null
print 'END sp_egrants_maint_impacdocs_PRACPC = @ '+ cast(getdate() as varchar)
END

GO


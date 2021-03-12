SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_egrants_maint_impacdocs_PRACOV] 
AS

DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'    
DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_impacdocs_PRACOV'
SET @RUN_DATETIME = GETDATE()

BEGIN
print 'BEGIN sp_egrants_maint_impacdocs_PRACOV = @ '+ cast(getdate() as varchar)

TRUNCATE TABLE dbo.impp_Carryover

INSERT dbo.impp_Carryover(APPL_ID,DOCUMENT_DATE,keyId,Request_id)
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRACOV'' and d.doc_key_id = pa.pa_request_id ')  
SELECT @IMPAC_DOWNLOAD_CNT = @@ROWCOUNT

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRACOV'  --382

INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.Request_id,'Request'
from impp_Carryover zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.Request_id not in 
--4/24/2019:Imran:If ImpacII has not created any PRACOV there will be no nga_rpt_seq_num therefore create one, If created and got anotated then do not add another
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null and x.category_id=@CATEGORYID)
--4/24/2019 : Imran : Requirement change CR-1937. Commenting out folowing code and adding code above
----(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
----and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  ---AND x.profile_id=1 
SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT

print 'NEW Prior-approval carry Over INSERTED =' + cast(@@ROWCOUNT as varchar)
EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'PRACOV',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,null
print 'END sp_egrants_maint_impacdocs_PRACOV = @ '+ cast(getdate() as varchar)
END

GO


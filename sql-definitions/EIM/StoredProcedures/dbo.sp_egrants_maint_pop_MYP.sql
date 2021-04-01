SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_egrants_maint_pop_MYP]

AS
DECLARE @impacID int
DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_pop_MYP'
SET @RUN_DATETIME = GETDATE()

PRINT 'BEGIN PROC: ['+@PROC_NAME+'] @ '+ cast(getdate() as varchar)

TRUNCATE TABLE IMPP_RPPR_MYP_NEW

/**
Bring all PRAM from ImpacII
**/

select @CATEGORYID=category_id from categories where impac_doc_type_code='MYP' and category_name='Progress Report'
SELECT @impacID=person_id FROM people WHERE userid='impac'

INSERT IMPP_RPPR_MYP_NEW(APPL_ID, DOCUMENT_DATE, RPPR_DOC_ID)
SELECT APPL_ID, RECEIVED_DATE, RPPR_DOC_ID
FROM openquery(IRDB, 'select c.appl_id, D.RECEIVED_DATE,B.RPPR_DOC_ID
from IRDB.RPPRS_MV a, IRDB.RPPR_DOCS_MV b, appls_t c, IRDB.EAPPL_ROUTINGS_MV D
where C.APPL_ID=A.APPL_ID
AND c.admin_phs_org_code=''CA''
AND A.RPPR_ID=B.RPPR_ID
AND B.RPPR_ID=D.RPPR_ID
AND A.rppr_status_code = ''FIN''
AND D.REVIEWER_USER_ID in (''Agency'',''NIH'')
AND B.DOC_TYPE_CODE = ''MYP''
and D.PRAM_FLAG=''N'' ')

SELECT @IMPAC_DOWNLOAD_CNT = @@ROWCOUNT
PRINT 'Total MYP brought today =' + cast(@IMPAC_DOWNLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)

/*
Insert new PRAM in documents table
*/

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_MYP_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null and created_by_person_id= @impacID
and category_id =@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

print 'Total MYP Added in eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
PRINT 'END PROC: ['+@PROC_NAME+'] @ '+ cast(getdate() as varchar)
GO


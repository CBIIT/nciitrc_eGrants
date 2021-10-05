SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF


CREATE   PROCEDURE [dbo].[sp_egrants_maint_pop_FRM]

AS

DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_pop_FRM'
SET @RUN_DATETIME = GETDATE()
DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'

PRINT 'BEGIN PROC: ['+@PROC_NAME+'] @ '+ cast(getdate() as varchar)


--3/12/2015: Imran : Add FRM into eGrants
TRUNCATE TABLE IMPP_FRM
INSERT dbo.IMPP_FRM(appl_id,document_date,FRM_DOC_ID,NEW_DOC_TYPE)
SELECT appl_id, document_date, FRM_DOC_ID, NEW_DOC_TYPE
FROM OPENQUERY(IRDB,'select b.appl_id, B.CREATED_DATE as document_date, null as FRM_DOC_ID, ''FRM'' as NEW_DOC_TYPE 
from appls_t a, IRDB.CLOSEOUT_DOCS_MV b
where a.appl_id=B.APPL_ID
and A.ADMIN_PHS_ORG_CODE=''CA''
and B.DOC_TYPE_CODE=''FRM'' 

UNION

select r.appl_id, rd.created_date as document_date, rd.ram_doc_id as FRM_DOC_ID, ''FRM_NEW'' as NEW_DOC_TYPE
from RAMS_MV r, RAM_DOCS_MV rd, appls_mv a
where r.ram_id= rd.ram_id
and r.ram_type_code in (''COMS_RO'')
and rd.DOC_TYPE_CODE in (''CORAM'')
and r.appl_id = a.appl_id
and a.admin_phs_org_Code = ''CA''

')


INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, c.category_id, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101),@impacID,
'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.FRM_DOC_ID
from IMPP_FRM gi inner join categories c on gi.NEW_DOC_TYPE = c.impac_doc_type_code 
inner join egrants e on e.appl_id = gi.appl_id 
where gi.appl_id not in (select distinct appl_id from egrants e  where e.impac_doc_type in ('FRM','FRM_NEW'))

print 'FRM Added into eGrants=' + cast(@@ROWCOUNT as varchar)
----------------------------------------------------------

PRINT 'END PROC: ['+@PROC_NAME+'] @ '+ cast(getdate() as varchar)

GO


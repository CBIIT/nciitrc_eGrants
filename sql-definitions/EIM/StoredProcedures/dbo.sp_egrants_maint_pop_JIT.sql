SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF



CREATE   PROCEDURE [dbo].[sp_egrants_maint_pop_JIT]
AS

DECLARE @impacID int
DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_pop_JIT'
SET @RUN_DATETIME = GETDATE()

PRINT 'BEGIN PROC: [sp_egrants_maint_pop_JIT] @ '+ cast(getdate() as varchar)

CREATE TABLE #IMPP_JIT(
	[serial_num] [int] NULL,
	[appl_id] [int] NULL,
	[doc_type_code] [varchar](10) NULL,
	[submitted_file_date] [datetime] NULL,
	[doc_version] [int] NULL,
	[url] [varchar](200) NULL
) ON [PRIMARY]


/**
Bring all JIT from ImpacII
**/
--INSERT IMPP_JIT(SERIAL_NUM,APPL_ID,DOC_TYPE_CODE,SUBMITTED_FILE_DATE,DOC_VERSION)
-- values(109250,6810657,'JIT',getdate(),11)
SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='JIT' and category_name='JIT Info'
SELECT @impacID=person_id FROM people WHERE userid='impac'

INSERT #IMPP_JIT(SERIAL_NUM,APPL_ID,DOC_TYPE_CODE,SUBMITTED_FILE_DATE,DOC_VERSION,[URL])
SELECT DISTINCT serial_num,appl_id,doc_type_code,submitted_file_date,doc_version,'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'+CAST(DOC_KEY_ID as varchar) +'/' + DOC_TYPE_CODE as [url]
FROM OPENQUERY(IRDB, 'select 

a.SERIAL_NUM,
a.APPL_ID,
rdt.DOC_TYPE_CODE,
max(e.SUBMITTED_DATE) as SUBMITTED_FILE_DATE,
max(e.JIT_SEQ_NUM) as DOC_VERSION,
max(rdt.DOC_KEY_ID) as DOC_KEY_ID

from appls_t a, rams_t r, jit_eappls_t e, ram_docs_t rdt
where a.appl_id  = r.appl_id
and a.admin_phs_org_code=''CA''
and e.appl_id= a.appl_id
and rdt.ram_id = r.ram_id
and r.ram_type_code in (''JBU_RAM'',
''JGS_RAM'',
''JHS_RAM'',
''JOD_RAM'',
''JOU_RAM'',
''JHSD_RAM'',
''JAS_RAM''
 ) 
 and rdt.doc_type_code = ''CORAM''
 group by 
 a.SERIAL_NUM,
a.APPL_ID,
rdt.DOC_TYPE_CODE
 ' )


SELECT @IMPAC_DOWNLOAD_CNT = @@ROWCOUNT
PRINT 'Total JIT brought today =' + cast(@IMPAC_DOWNLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)

/*
Update Documents_date & sub_category_name if a New JIT has arrived (with new file_submitted_date) and the 
Existing JIT has not been Annotated and replaced file_modified_by_person_id is null or (url is null) Yet
4/23/2019: Added the following condition: and sub_category_name <> 'Reminder'
4/30/2019 Removed sub_category_name <> 'Reminder' because now checking on ImpacII created document
*/

 Update d SET d.DOCUMENT_DATE=A.submitted_file_date, d.sub_category_name='Revised('+cast(a.doc_version as varchar)+')' , url = a.url
--Select *
from #Impp_jit a inner join documents d
on a.appl_id = d.appl_id
where d.category_id=@CATEGORYID and d.created_by_person_id=@impacID 
and (cast(CONVERT(NVARCHAR(100),a.submitted_file_date,100) as datetime) > cast(CONVERT(NVARCHAR(100),d.document_date,100) as datetime))
and (d.disabled_by_person_id is null) and (d.disabled_date is null) and (d.qc_reason is null)

SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT
PRINT 'Recent JIT Date changed =' + cast(@EGRANTS_UPLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)
EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'JIT',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,'Recent JIT Date changed'

/*
Insert New JIT that is not in egrants and the old ImpacII JIT has been replaced by an annotated version.
Update Documents_date if a New JIT has arrived (with new file_submitted_date) and the 
Existing JIT has been Annotated and replaced (url is null) Previously
SOP : User should annotate this new one and replace this one not the previous one. User might 
want to delete the old one if the need is to keep only one latest one.
4/23/2019: Added the following condition: and sub_category_name <> 'Reminder'
4/30/2019: Removing condition added on 4/23 because now we check only impac created JIT
*/
INSERT documents(appl_id, category_id, created_by_person_id, file_type, created_date, document_date, profile_id,sub_category_name,[url])
select distinct a.appl_id,@CATEGORYID, @impacID, 'pdf',getdate(),a.submitted_file_date,dbo.fn_grant_profile_id_from_Appl(a.appl_id),'Revised('+cast(a.doc_version as varchar)+')',a.url 
--select distinct a.appl_id,60, 530, 'pdf',getdate(),a.submitted_file_date,dbo.fn_grant_profile_id_from_Appl(a.appl_id),'Revised('+cast(a.doc_version as varchar)+')' 
from #IMPP_JIT a, documents 
where a.appl_id=documents.appl_id
and a.appl_id not in (select distinct b.appl_id from documents b where b.category_id=@CATEGORYID and b.appl_id is not null and b.disabled_date is null and b.created_by_person_id=@impacID and b.file_modified_by_person_id is null)
and cast(CONVERT(NVARCHAR(100),a.submitted_file_date,100) as datetime) > (select max(cast(CONVERT(NVARCHAR(100),x.document_date,100) as datetime)) from documents x where x.appl_id=a.appl_id and x.category_id=@CATEGORYID and x.disabled_date is null and x.disabled_by_person_id is null and x.created_by_person_id=@impacID and x.file_modified_by_person_id is not null)

SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT
PRINT 'New Version JIT Added =' + cast(@EGRANTS_UPLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)
EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'JIT',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,'New Version JIT Added'

/*
Insert Brand New JIT
4/23/2019: Added the following condition: and sub_category_name <> 'Reminder'
*/
--Insert impacII created jit that we do not have (we might have non impacII creted one)
--4/30/2019 : Imran : added constraint "created_by_person_id=@impacID"
--This will add duplicates because so many of them have JIT created by User

INSERT documents(appl_id, category_id, created_by_person_id, file_type, created_date, document_date, profile_id,sub_category_name, [url])
select a.appl_id,@CATEGORYID, @impacID, 'pdf',getdate(),a.submitted_file_date,dbo.fn_grant_profile_id_from_Appl(a.appl_id),'Revised('+cast(a.doc_version as varchar)+')', a.url 
from #Impp_jit a  
where a.appl_id in (select distinct appl_id from appls)
and a.appl_id not in (select distinct b.appl_id from documents b where b.category_id=@CATEGORYID and appl_id is not null and disabled_date is null and created_by_person_id=@impacID)

SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT
PRINT 'Brand New JIT Added =' + cast(@EGRANTS_UPLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)
EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'JIT',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,'Brand New JIT Added'
PRINT 'END PROC:==> [sp_egrants_maint_pop_JIT] @ '+ cast(getdate() as varchar)



GO


USE [EIM]
GO

/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_pop_FRM]    Script Date: 11/6/2026 ******/
/******                                                                                         ******/
/****** eGrants-1365: Import a separate eRA file link under Final Report Additional Materials   ******/
/******               for EACH distinct FRAM submission for the same grant year (including the  ******/
/******               second and subsequent submissions).                                       ******/
/******                                                                                          ******/
/****** PROBLEM:                                                                                 ******/
/******   The previous dedup clause excluded an appl_id entirely once ANY FRM/FRM_NEW link       ******/
/******   existed:                                                                               ******/
/******       where gi.appl_id not in                                                            ******/
/******         (select distinct appl_id from egrants e                                          ******/
/******          where e.impac_doc_type in ('FRM','FRM_NEW'))                                    ******/
/******   As a result the first submission was imported, but links for subsequent submissions   ******/
/******   (which carry distinct FRM_DOC_ID values) were never added automatically.              ******/
/******                                                                                          ******/
/****** FIX:                                                                                     ******/
/******   Deduplicate by the distinct submission id (FRM_DOC_ID -> nga_rpt_seq_num) for          ******/
/******   new-style FRAM rows, matching the pattern used by sp_egrants_maint_pop_PRAM. This      ******/
/******   adds second/subsequent submissions, preserves existing links, and prevents duplicate   ******/
/******   links when the same submission is reprocessed. Legacy FRM rows (no submission id)      ******/
/******   retain the original one-link-per-grant behavior.                                        ******/
/******                                                                                           ******/
/****** A backup of the prior version is stored in sp_egrants_maint_pop_FRM_BACKUP.sql           ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER OFF
GO



ALTER   PROCEDURE [dbo].[sp_egrants_maint_pop_FRM]

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
--2/16/2024: Micah : Added conversion bc the added .000000 was overflowing the VARCHAR(10)

TRUNCATE TABLE IMPP_FRM

INSERT dbo.IMPP_FRM(appl_id,document_date,FRM_DOC_ID,NEW_DOC_TYPE)
SELECT appl_id, document_date, CAST(CAST(FRM_DOC_ID AS INT) AS VARCHAR(10)), NEW_DOC_TYPE
FROM OPENQUERY(IRDB,'select b.appl_id, B.CREATED_DATE as document_date, null as FRM_DOC_ID, ''FRM'' as NEW_DOC_TYPE 
from appls_t a, CLOSEOUT_DOCS_T b
where a.appl_id=B.APPL_ID
and A.ADMIN_PHS_ORG_CODE=''CA''
and B.DOC_TYPE_CODE=''FRM'' 

UNION

select r.appl_id, rd.created_date as document_date, rd.ram_doc_id as FRM_DOC_ID, ''FRM_NEW'' as NEW_DOC_TYPE
from RAMS_T r, RAM_DOCS_T rd, appls_mv a
where r.ram_id= rd.ram_id
and r.ram_type_code in (''COMS_RO'')
and rd.DOC_TYPE_CODE in (''CORAM'')
and r.appl_id = a.appl_id
and a.admin_phs_org_Code = ''CA''

')

--eGrants-1365: Deduplicate by the distinct submission id so that each FRAM submission for the
--              same grant year adds its own eRA file link (second and subsequent submissions
--              included), existing links are preserved, and reprocessing the same submission
--              does not create a duplicate link.
INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, c.category_id, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101),@impacID,
'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.FRM_DOC_ID
from IMPP_FRM gi inner join categories c on gi.NEW_DOC_TYPE = c.impac_doc_type_code 
inner join egrants e on e.appl_id = gi.appl_id 
where
(
    -- New-style FRAM submissions: add the link when this specific submission
    -- (identified by FRM_DOC_ID -> nga_rpt_seq_num) has not already been imported.
    (gi.FRM_DOC_ID is not null
     and not exists (select 1 from egrants ex
                     where ex.appl_id = gi.appl_id
                       and ex.impac_doc_type in ('FRM','FRM_NEW')
                       and ex.nga_rpt_seq_num = gi.FRM_DOC_ID))
    OR
    -- Legacy FRAM rows have no submission id; keep the original one-link-per-grant behavior.
    (gi.FRM_DOC_ID is null
     and gi.appl_id not in (select distinct appl_id from egrants ex2
                            where ex2.impac_doc_type in ('FRM','FRM_NEW')))
)

print 'FRM Added into eGrants=' + cast(@@ROWCOUNT as varchar)
----------------------------------------------------------

PRINT 'END PROC: ['+@PROC_NAME+'] @ '+ cast(getdate() as varchar)
GO

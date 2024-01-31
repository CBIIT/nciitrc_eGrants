USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_impacdocs_evening]    Script Date: 12/8/2023 3:50:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


ALTER   PROCEDURE [dbo].[sp_egrants_maint_impacdocs_evening] 
AS

DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'
DECLARE @CATEGORYID smallint
DECLARE @DOCTYPE VARCHAR(5)
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_impacdocs'
SET @RUN_DATETIME = GETDATE()

print '====>>Proc sp_egrants_maint_impacdocs_EVENING Started @' + cast(getdate() as varchar)
-------------------------------------------
--: Greensheet ARC (ARC)
--Imran:12/11/2018
--eGrants Category Map : Award Review Certification
--Sub category Text = automatic from gm_actions_queue_vw  (Ask Lisa?)
-------------------------------------------
TRUNCATE TABLE dbo.Greensheet_data_ARC

INSERT dbo.Greensheet_data_ARC(APPL_ID,agt_id,SUBMITTED_DATE)
SELECT APPL_ID,agt_id,SUBMITTED_DATE FROM OPENQUERY(CIIP, '
SELECT G.appl_id,B.AGT_ID,A.submitted_date FROM FORMS_T A, APPL_FORMS_T B, APPL_GM_ACTIONS_T G
WHERE A.ID=B.FRM_ID AND G.ID=B.AGT_ID AND A.FORM_ROLE_CODE=''ARC'' and A.FORM_STATUS=''FROZEN'' ')  

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='ARC'

INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num)
select zz.appl_id,@CATEGORYID,zz.SUBMITTED_DATE,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.AGT_ID 
from Greensheet_data_ARC zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.AGT_ID not in 
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  ---AND x.profile_id=1 
print 'NEW Greensheet ARC INSERTED =' + cast(@@ROWCOUNT as varchar)

/*
5/5/2021 - BSHELL - MOVED SPEC REV to Separate SP

*/
EXEC sp_egrants_maint_SPEC_REV


/*----------------------------------------------------------
4/9/2021 - BSHELL - INSERT New PGM Revision Population Here per eGrants-52
*/

EXEC sp_egrants_maint_PGM_REV

-------------------------------------------
--3/30/2017: Prior approval carry Over (PRACOV)
--Imran:6/13/2017
--eGrants Category Map : CarryOver
--Sub category Text = None
--12/12/2017: Imran: Change : There must not be multiple PRACOV for a given appl. If user annotate and upload(by replacing the impacII doc) 
--there should not be any new download form ImpacII Unless ImpacII has a new document with a new Key ID
--5/8/2019 : iMRAN : Requirement change. Latest version deployment using stored procedure
-------------------------------------------
EXEC sp_egrants_maint_impacdocs_PRACOV


EXEC sp_egrants_maint_impacdocs_PRACPC

EXEC sp_egrants_maint_impacdocs_PRANCE

EXEC sp_egrants_maint_impacdocs_PRAGEN

-- Run Supplements  BSHELL 08-05-2021
EXEC sp_egrants_maint_impacdocs_IMPP_Supplements


------------------------------------------------------------------------------------------------------
--set arra_flag at the grant level
update grants set arra_flag='n' where arra_flag='y'
update grants SET arra_flag='y'
WHERE grant_id IN(select distinct grant_id from vw_appls_arra where admin_phs_org_code='CA')
print 'ARRA FLAG SET count=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------


-------------------------------------------------------------------------------------------------------
--Add new PRAM Added by Imran on 2/4/2014
--ENHANCEMENT : IMRAN 9/25/2014
--Enhancement : Imran 5/11/2015 : There are two types of PRAM one with Flag=y (non-compl by system) and other flag=R (add matarial requested by specialist)
--Hence adding this clause D.PRAM_FLAG in (''Y'',''R'')
--6/20/2018: mOVED PRAM CODE TO FOLLOWING SP
EXEC sp_egrants_maint_pop_PRAM


EXEC sp_egrants_maint_pop_FRM

-- Added new IRAM documents BSHELL 08-05-2021
exec sp_egrants_maint_pop_IRAM
-------------------------------------------
--3/9/2015: Imran : Add FPA into eGrants

TRUNCATE TABLE IMPP_FPA

INSERT dbo.IMPP_FPA(appl_id,document_date)
SELECT appl_id,document_date
FROM OPENQUERY(IRDB,'select a.appl_id,b.SUBMITTED_FILE_DATE as document_date from appls_t a, IRDB.DOC_AVAILABLE_MV b
where a.appl_id=B.DOC_KEY_ID
and A.ADMIN_PHS_ORG_CODE=''CA''
and B.DOC_TYPE_CODE=''FPA'' ')


SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='FPA'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id)
from IMPP_FPA gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.appl_id not in (select distinct appl_id from egrants where category_id = @CATEGORYID)

print 'FPA Added into eGrants=' + cast(@@ROWCOUNT as varchar)

-----------------------------------------------------------------------------------------------------
---Add new AWS (Award Worksheet)
---
---
---
------------------------------------------------------------------------------------------------------
TRUNCATE TABLE dbo.impp_AWS

INSERT dbo.impp_AWS(doc_id,appl_id,doc_type_code,created_date,rpt_seq_num)
select doc_id, impp.appl_id, doc_type_code, impp.SUBMITTED_FILE_DATE, rpt_seq_num 
from openquery(IRDB,'select d.doc_key_id as doc_id, a.appl_id, doc_type_code, d.SUBMITTED_FILE_DATE, r.rpt_seq_num 
from DOC_AVAILABLE_MV d, RPT_JOBS_MV r, appls_t a 
where d.doc_key_id = r.RPT_SEQ_NUM 
and r.ID = a.APPL_ID 
and r.EVENT_STATUS_CODE =''C'' 
and d.DOC_TYPE_CODE = ''AWS'' 
and A.ADMIN_PHS_ORG_CODE=''CA'' ') impp --130726

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='AWS'

INSERT documents(appl_id, category_id, created_by_person_id, file_type, document_date, profile_id, nga_rpt_seq_num)
SELECT appl_id, @CATEGORYID, @impacID , 'pdf' , created_date, 1, RPT_SEQ_NUM
FROM impp_AWS  where rpt_seq_num not in 
(select distinct x.nga_rpt_seq_num from Documents x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null AND x.profile_id=1 and x.created_by_person_id=@impacID) --7046
SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT

print 'New AWS Added =' + cast(@EGRANTS_UPLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)



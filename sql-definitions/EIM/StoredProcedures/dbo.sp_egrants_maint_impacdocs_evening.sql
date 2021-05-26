SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE     PROCEDURE [dbo].[sp_egrants_maint_impacdocs_evening] 
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

TRUNCATE TABLE dbo.IMPP_Admin_Supplements

INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE)
select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE
from OPENQUERY(IRDB,'SELECT A.APPL_ID, A.APPL_TYPE_CODE||A.GRANT_NUM AS FULL_GRANT_NUM, A.FORMER_NUM, B.ACTION_DATE, B.ADMIN_SUPP_ACTION_CODE,
A.SERIAL_NUM, A.APPL_TYPE_CODE, A.ACTIVITY_CODE, A.ADMIN_PHS_ORG_CODE, A.SUPPORT_YEAR, A.SUFFIX_CODE 
FROM APPLS_MV A, ADMIN_SUPP_ROUTINGS_T B
WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
ORDER BY APPL_ID DESC,ACTION_DATE DESC

Update IMPP_Admin_Supplements
SET IMPP_Admin_Supplements.Former_appl_id=B.appl_id
from IMPP_Admin_Supplements a, vw_appls b
where a.Former_Num=b.full_grant_num

INSERT INTO dbo.IMPP_Admin_Supplements_WIP(serial_num,Supp_appl_id,Full_grant_num,Former_Num,Former_appl_id,Submitted_date,file_type,category_id,doc_url)
SELECT A.serial_num,A.Supp_appl_id,A.Full_grant_num,A.Former_Num,A.Former_appl_id,A.Action_date,'PDF',38,
--'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='+convert(varchar,A.Supp_appl_id)+'&docType=IGI'
'https://s2s.era.nih.gov/docservice/dataservices/document/once/applId/'+convert(varchar,A.Supp_appl_id)+'/' + 'IGI'
FROM dbo.IMPP_Admin_Supplements A
where A.admin_supp_action_code='STA'
and A.Supp_appl_id NOT in (select distinct Supp_appl_id  from IMPP_Admin_Supplements_WIP WHERE Supp_appl_id IS NOT NULL)
--and A.serial_num NOT in (select distinct serial_num  from IMPP_Admin_Supplements_WIP WHERE serial_num IS NOT NULL)
AND Action_date > '10/1/2015'
order by a.serial_num
print 'ADMINISTRATIVE SUPPLEMENT ACTIONS DOWNLOADED =' + cast(@@ROWCOUNT as varchar)

--Insert all accepted supplement action into placeholder to send email
insert dbo.adsup_accepted(supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code)
select supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code 
from dbo.IMPP_Admin_Supplements where admin_supp_action_code='ACC'
and Supp_appl_id not in (select distinct Supp_appl_id from adsup_accepted)
print 'ADMIN SUPPLEMENT NEWLY ACCEPTED COUNT=' + cast(@@ROWCOUNT as varchar)

--Create email entry to go on next run from oga stage machine dts
Exec dbo.adsupp_create_notification_accgrant
print 'ADMIN SUPPLEMENT EMAIL CREATED=' + cast(@@ROWCOUNT as varchar)



-------------------------------------------------------------------------------------------------------
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

-------------------------------------------
--3/12/2015: Imran : Add FRM into eGrants
TRUNCATE TABLE IMPP_FRM
INSERT dbo.IMPP_FRM(appl_id,document_date)
SELECT appl_id,document_date
FROM OPENQUERY(IRDB,'select b.appl_id,B.CREATED_DATE as document_date from appls_t a, IRDB.CLOSEOUT_DOCS_MV b
where a.appl_id=B.APPL_ID
and A.ADMIN_PHS_ORG_CODE=''CA''
and B.DOC_TYPE_CODE=''FRM'' ')

select @CATEGORYID=category_id from categories where impac_doc_type_code='FRM'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id)
from IMPP_FRM gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.appl_id not in (select distinct appl_id from egrants where category_id = @CATEGORYID)

print 'FRM Added into eGrants=' + cast(@@ROWCOUNT as varchar)
----------------------------------------------------------
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



GO


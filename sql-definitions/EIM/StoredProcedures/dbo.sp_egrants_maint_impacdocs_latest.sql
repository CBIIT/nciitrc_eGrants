SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE   PROCEDURE [dbo].[sp_egrants_maint_impacdocs_latest] 
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

print '====>>Proc sp_egrants_maint_impacdocs_latest Started @' + cast(getdate() as varchar)

-------------------------------------------
--Imran: 1/18/2019 : Update documents profileid for a transfer grant

--update egrants set profile_id=1 where admin_phs_org_code='CA' and (profile_id <> 1) 

Update documents set documents.profile_id=1
where (documents.profile_id<>1 or documents.profile_id is null)
and documents.appl_id in (select distinct appl_id from vw_appls where admin_phs_org_code <> former_admin_phs_org_code and admin_phs_org_code='CA')

print 'Fixed Profileid for transfered Grants =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------
exec dbo.sp_egrants_maint_pop_FSR
-------------------------------------------
--Add CloseOut Flag
--(Imran 10/17/2014)
-------------------------------------------
TRUNCATE TABLE IRDB_CLOSEOUT_FLAG_NEW
INSERT dbo.IRDB_CLOSEOUT_FLAG_NEW(appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE,action_date)
SELECT appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM OPENQUERY(IRDB,'SELECT g.appl_id,a.fy,g.GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM GC_ACTION_HISTORY_MV g , appls_t a 
WHERE g.appl_id = a.appl_id
AND a.admin_phs_org_code=''CA''
AND g.GC_OBJECT_NAME=''GRANT_CLOSEOUT'' AND g.GC_OBJECT_STATUS_CODE=''O'' AND a.fy >= 2014 ORDER BY g.action_date desc')

print 'Closeout Flag Update =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

Update appls 
Set closeout_flag='y'
From appls a, IRDB_CLOSEOUT_FLAG_NEW b
Where a.appl_id=b.appl_id

print 'Count of Closeout_Flag in appls =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

--*********************************************************
---Add new MYP
--*********************************************************

Exec dbo.sp_egrants_maint_pop_MYP

-------------------------------------------
--7/17/2017: ADD JIT
-------------------------------------------
Exec sp_egrants_maint_pop_JIT


-------------------------------------------------------------------------------------------------------
--Add New RPPRs  @impacID
-- Note: We will have to remove the following in future (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158)
--Update 10/28/2014 : Imran
--------------------------------------------------------------------------------------------------------
TRUNCATE TABLE impp_rpprs_t 

INSERT impp_rpprs_t(appl_id, created_date, last_upd_date,correct_rcvd_date,correct_rcvd_date1)
SELECT appl_id, created_date, last_upd_date,RECEIVED_DATE,APPL_RECEIVED_DATE
FROM openquery(IRDB, 'select A.appl_id, R.created_date,ER.LAST_UPD_DATE ,ER.RECEIVED_DATE,A.APPL_RECEIVED_DATE
from APPLS_T A, RPPRs_T R, EAPPL_ROUTINGS_MV ER 
WHERE A.APPL_ID=R.APPL_ID
AND (R.RPPR_ID=ER.RPPR_ID OR R.APPL_ID=ER.APPL_ID)
AND R.RPPR_STATUS_CODE=''FIN''
AND ER.EAPPL_ACTION_CODE =''SUBMIT''
AND ER.COMPLIANT_PUBLICATIONS_FLAG IS NULL
AND ER.PRAM_FLAG=''N'' ')


print 'Total RPPRs in IRDB=' + cast(@@ROWCOUNT as varchar)

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT gi.appl_id, 38, gi.correct_rcvd_date1, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_rpprs_t gi, vw_appls a
WHERE a.appl_id=gi.appl_id and gi.appl_id not in 
(
select distinct a.appl_id from egrants a, impp_rpprs_t b where a.appl_id=b.appl_id and a.category_id=38
and a.created_by_person_id in (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158) 
)

print 'Total RPPRs Added today =' + cast(@@ROWCOUNT as varchar)


-------------------------------------------
--11/20/2015: Bring All WORKBOOK DOCUMENTS
--Production Date : 12/21/2015 : Imran
--Change Major 6/2/2017 : LAST_UPD_DATE on irdb is main src
--Mod: 7/12/2017: should be runing from any one jobs not jultiples. taking out from impacdocs
-------------------------------------------
TRUNCATE TABLE dbo.impp_award_book

INSERT dbo.impp_award_book(APPL_ID,DOCUMENT_DATE,keyId)
SELECT APPL_ID, DOCUMENT_DATE,keyId FROM OPENQUERY(IRDB, 'SELECT RPT.ID AS APPL_ID, RPT.LAST_UPD_DATE AS DOCUMENT_DATE,RPT.RPT_SEQ_NUM as keyId FROM RPT_JOBS_T RPT, DOC_AVAILABLE_MV DOCS 
WHERE RPT.RPT_SEQ_NUM=DOCS.doc_KEY_ID AND DOCS.DOC_TYPE_CODE=''WBR'' AND RPT.ID_TYPE_CODE=''APPL_ID'' ')  --17605

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='WBR'

INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num)
select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'xlsm',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.keyId
from impp_award_book zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.keyId not in 
(select distinct x.nga_rpt_seq_num from Documents x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null AND x.profile_id=1 and x.created_by_person_id=@impacID)

print 'NEW WORK BOOK INSERTED =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------------------
-- Removed separate implementation of Greensheet insert and used new 5/25/2021 bshell
-------------------------------------------------------------------------------------------------------
exec sp_egrants_maint_impacdocs_GREENSHEETS

-------------------------------------------------------------------------------------------------------

--ADD NEW MULTI-YEAR PROGRESS REPORT
TRUNCATE TABLE IMPP_RPPR_MPR_NEW
INSERT IMPP_RPPR_MPR_NEW(APPL_ID, DOCUMENT_DATE, RPPR_DOC_ID)
SELECT appl_id, RECEIVED_DATE,  rppr_doc_id
FROM openquery(IRDB, 'select C.APPL_ID,D.RECEIVED_DATE,B.RPPR_DOC_ID
from IRDB.RPPRS_MV a, IRDB.RPPR_DOCS_MV b, appls_t c, IRDB.EAPPL_ROUTINGS_MV D
where C.APPL_ID=A.APPL_ID
AND c.admin_phs_org_code=''CA''
AND A.RPPR_ID=B.RPPR_ID
AND B.RPPR_ID=D.RPPR_ID
AND A.rppr_status_code = ''FIN''
AND D.REVIEWER_USER_ID in (''Agency'',''NIH'')
AND D.PRAM_FLAG = ''Y''
AND B.DOC_TYPE_CODE = ''MPR''  ')

print 'Total MPR count in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

select @CATEGORYID=category_id from categories where impac_doc_type_code='MPR'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_MPR_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null 
and category_id=@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

print 'MULTI YEAR MPR documents Added to eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

/***********************************************************
**    DASHBOARD RELATED QUERIES
**    STARTED BUILDING ON 7/21/2016
************************************************************/
---------------------------
--DOWNLOAD EARLY_CONCUR_FLAG FOR DASHBOARD
--EXTRACT(YEAR FROM SYSDATE)
---------------------------
TRUNCATE TABLE DB_EARLY_CONCUR_FLAG

INSERT dbo.DB_EARLY_CONCUR_FLAG(APPL_ID,FGN,USERID,BUDGET_START_DATE,NCAB_DATE)
SELECT * FROM OPENQUERY(CIIP,'
SELECT DISTINCT B.APPL_ID,B.FULL_GRANT_NUM AS FGN,
(SELECT A.USERID FROM nci_people_t A WHERE A.ID=B.RESP_SPEC_NPN_ID )  AS USERID, B.BUDGET_START_DATE, B.FORMATTED_COUNCIL_DATE
FROM GM_ACTION_QUEUE_VW B
WHERE B.ACTION_FY=2017 AND B.EARLY_CONCUR_FLAG=''Y'' AND B.CURRENT_ACTION_STATUS_CODE NOT IN (''CLOSED'',''NEW'',''CANCELLED'')
ORDER BY USERID,BUDGET_START_DATE DESC')

print 'EARLY_CONCUR_FLAG DOWNLOADED for DASHBOARD=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


---------------------------
--DOWNLOAD GRANTS ASSIGNMENT FROM GPMATS FOR EGRANTS DASHBOARD
---------------------------
TRUNCATE TABLE DB_GPMATS_ASSIGNMENT_STATUS

INSERT dbo.DB_GPMATS_ASSIGNMENT_STATUS(APPL_ID,FGN,REVISON_NUM,USERID,GRANT_ASSIGN_DATE,RELEASE_DATE,BUDGET_START_DATE,APPL_TYPE_CODE,STATUS_CODE,ACTION_TYPE,ACTIVITY_CODE,DAY_COUNT_NUM)
SELECT * FROM OPENQUERY(CIIP,'
SELECT DISTINCT B.APPL_ID,B.FULL_GRANT_NUM AS FGN,B.REVISION_NUM,
(SELECT A.USERID FROM nci_people_t A WHERE A.ID=B.RESP_SPEC_NPN_ID)  AS USERID,
B.AWAITGMR_DATE AS GRANT_ASSIGN_DATE,B.RELEASE_DATE,B.BUDGET_START_DATE,B.APPL_TYPE_CODE,B.CURRENT_ACTION_STATUS_CODE,B.ACTION_TYPE,B.ACTIVITY_CODE,B.DAY_COUNT_NUM
FROM GM_ACTION_QUEUE_VW B WHERE B.ACTION_FY=2017 ORDER BY USERID,B.BUDGET_START_DATE DESC')

print 'GPMATS_ASSIGNMENT DOWNLOADED for dashboard =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


GO


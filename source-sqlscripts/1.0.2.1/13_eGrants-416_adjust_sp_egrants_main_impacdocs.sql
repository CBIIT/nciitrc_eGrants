USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_impacdocs]    Script Date: 11/7/2023 9:33:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER        PROCEDURE [dbo].[sp_egrants_maint_impacdocs] 
AS

DECLARE @impacID int
DECLARE @efileID int
DECLARE @CATEGORYID smallint
DECLARE @DOCTYPE VARCHAR(5)
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_impacdocs'
SET @RUN_DATETIME = GETDATE()

SELECT @impacID=person_id FROM people WHERE userid='impac'
SELECT @efileID=person_id FROM people WHERE userid='efile'
---------------------------------------------
--Imran: 5/3/2019: GRAB GRANTS CONTACTS FROM GPMATS.
---------------------------------------------

TRUNCATE TABLE Grant_contacts_PD

INSERT INTO [dbo].[Grant_contacts_PD](PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS)  
SELECT PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS
from openquery(CIIP,'SELECT ROLE_USAGE_CODE||CAY_CODE AS PROG_CLASS_CODE, PD_NAME AS PD_FULL_NAME, EMAIL_ADDRESS AS PD_EMAIL_ADDRESS FROM PD_ORG_VW ')
SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT
--WHERE PD_END_DATE IS NULL
print 'PD DETAIL DOWNLOADED =' + cast(@EGRANTS_UPLOAD_CNT as varchar)


--- Moved to separate Stored Procedure
EXEC sp_egrants_maint_impacdocs_IMPP_Supplements


----print 'Closeout Flag Updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
TRUNCATE TABLE IRDB_CLOSEOUT_FLAG_NEW
INSERT dbo.IRDB_CLOSEOUT_FLAG_NEW(appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE,action_date)
SELECT appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM OPENQUERY(IRDB,'SELECT g.appl_id,a.fy,g.GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM GC_ACTION_HISTORY_MV g , appls_t a 
WHERE g.appl_id = a.appl_id
AND a.admin_phs_org_code=''CA''
AND g.GC_OBJECT_NAME=''GRANT_CLOSEOUT'' AND g.GC_OBJECT_STATUS_CODE=''O'' AND a.fy >= 2014 ORDER BY g.action_date desc')

print 'Closeout Flag from IRDB =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

Update appls 
Set closeout_flag='y'
From appls a, IRDB_CLOSEOUT_FLAG_NEW b
Where a.appl_id=b.appl_id

print 'Count of Closeout_Flag in appls =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------

/** Following code is to capture new user of egrants into egrants_people table and egrants_access table. **/
-------------------------------------------------------------------------------------------------------
--Add new Summary Statement
TRUNCATE TABLE impp_ss

	-------------------------------------
	--IMRAN : CHANGED FROM IMPACII TO IRDB
	-------------------------------------
	---insert new appl info to impp_ss from impac SUMMARY_STMNTS_MV
	--INSERT impp_ss(appl_id, ss_upd_date, ss_page_count_num)
	--SELECT appl_id, ss_upd_date, ss_page_count_num
	--FROM 
	--openquery(IMPAC,'select appl_id, ss_upd_date, ss_page_count_num from summary_stmnts_t
	--where last_upd_date>=''01-JUN-09'' and release_flag=''Y'' ')

INSERT impp_ss(appl_id, ss_upd_date, ss_page_count_num)
SELECT appl_id, ss_upd_date, ss_page_count_num
FROM 
openquery(IRDB,'select appl_id, ss_upd_date, ss_page_count_num from SUMMARY_STMNTS_MV
where to_date(to_char(last_upd_date,''yyyy-mm-dd''),''yyyy-mm-dd'')>=to_date(''2009-06-01'',''yyyy-mm-dd'')
and release_flag=''Y'' ')

--insert new doc info to document from impp_ss
INSERT documents(appl_id, category_id, document_date, page_count, created_by_person_id, file_type, profile_id)
SELECT i.appl_id, 33, i.ss_upd_date, i.ss_page_count_num, @impacID, 'pdf', dbo.fn_grant_profile_id(a.grant_id)
FROM impp_ss i, appls a
WHERE i.appl_id=a.appl_id and i.appl_id not in (SELECT appl_id FROM egrants WHERE created_by_person_id=@impacID and category_id=33)

print 'Summary Statement Added=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------
----Add new Application File  (Grant Images) once every 24 Hr.  **
--
--Imran:  10/24/2014 I have noticed that document_date in the insert statement bellow is wrongly populated
--My practical observance is document_date in eGrants should be appls_t.appl_recieved_date, so I am changing this for temporarily 
--and see what happens. Please do not change in this para before asking me.
-------------------------------------
TRUNCATE TABLE impp_grant_images_NEW_t  


--added by hareeshj on 1/20/2016 at 3:35pm 
--(also modified dbo.impp_grant_images_NEW_t table columns accession_num and total_pages_num to accept NULL's)
--(also commented out total_pages_num column in select, since it is not available in doc_available_mv)
INSERT impp_grant_images_NEW_t(appl_id, accession_num, appl_received_date)--, total_pages_num
SELECT appl_id, 
       (case when TRY_CONVERT(int, accession_num) is not null then cast(accession_num as int) else null end) as accession_num,
       (case when isdate(appl_received_date) = 1 THEN convert(datetime, appl_received_date, 121) END) appl_received_dateview
FROM openquery(IRDB, 'select appl_id, a.accession_num, CAST(a.appl_received_date AS varchar2(30)) appl_received_date
from DOC_AVAILABLE_MV g, appls_t a 
where doc_type_code = ''GI''
and g.doc_key_id = a.appl_id
and a.serial_num is not null')

print 'Application File Found in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
select @CATEGORYID=category_id from categories where impac_doc_type_code='IGI'

INSERT documents(appl_id, category_id, document_date, page_count, created_by_person_id, file_type,url,profile_id)
SELECT gi.appl_id, @categoryid, gi.appl_received_date, total_pages_num, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_grant_images_NEW_t gi, vw_appls a
WHERE a.appl_id=gi.appl_id and gi.appl_id not in (select appl_id from egrants where created_by_person_id=@impacID and category_id=@categoryid)

print 'New Application File Added=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------
----Add new DMS plan File once every 24 Hr.  **
--
--Micah:  9/6/2023 
-------------------------------------	
TRUNCATE TABLE impp_dms_plans_NEW_t  

INSERT impp_dms_plans_NEW_t(appl_id, accession_num, appl_received_date)--, total_pages_num
SELECT appl_id, 
       (case when TRY_CONVERT(int, accession_num) is not null then cast(accession_num as int) else null end) as accession_num,
       (case when isdate(appl_received_date) = 1 THEN convert(datetime, appl_received_date, 121) END) appl_received_dateview
FROM openquery(IRDB, 'select appl_id, a.accession_num, CAST(a.appl_received_date AS varchar2(30)) appl_received_date
from DOC_AVAILABLE_MV g, appls_t a 
where doc_type_code = ''DMS''
AND a.admin_phs_org_code=''CA''
AND a.appl_status_code = ''35''
AND a.APPL_TYPE_CODE <> ''3''
and g.doc_key_id = a.appl_id
and a.serial_num is not null')

print 'Application File Found in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
select @CATEGORYID=category_id from categories where impac_doc_type_code='DMS'	-- 683

INSERT documents(appl_id, category_id, document_date, page_count, created_by_person_id, file_type,url,profile_id)
SELECT dms.appl_id, @categoryid, dms.appl_received_date, total_pages_num, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_dms_plans_NEW_t dms, vw_appls a
WHERE a.appl_id=dms.appl_id and dms.appl_id not in (select appl_id from egrants where created_by_person_id=@impacID and category_id=@categoryid)

print 'New Application File Added=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

--------------------------------------------------------------------------------------------------------
--Add New RPPRs  @impacID
--These are doctype_code= IGI mapped to GMM as e-Applications
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


print 'Total RPPRs in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT gi.appl_id, 38, gi.correct_rcvd_date1, @impacID, 'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_rpprs_t gi, vw_appls a
WHERE a.appl_id=gi.appl_id and gi.appl_id not in 
(
select distinct a.appl_id from egrants a, impp_rpprs_t b where a.appl_id=b.appl_id and a.category_id=38
and a.created_by_person_id in (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158) 
)

print 'Total RPPRs Added today =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------------------
--Add new PRAM Added by Imran on 2/4/2014
--docType = PRM (Progress Report Additional Material)
--ENHANCEMENT : IMRAN 9/25/2014
--Enhancement : Imran 5/11/2015 : There are two types of PRAM one with Flag=y (non-compl by system) and other flag=R (add matarial requested by specialist)
--Hence adding this clause D.PRAM_FLAG in (''Y'',''R'')

TRUNCATE TABLE IMPP_RPPR_PRAM_NEW 

INSERT IMPP_RPPR_PRAM_NEW(APPL_ID, DOCUMENT_DATE,RPPR_DOC_ID )
SELECT APPL_ID,RECEIVED_DATE,RPPR_DOC_ID FROM
OPENQUERY(IRDB,'SELECT A.APPL_ID,D.RECEIVED_DATE,C.RPPR_DOC_ID
FROM APPLS_T A, RPPRS_MV B, RPPR_DOCS_MV C, EAPPL_ROUTINGS_MV D
WHERE A.APPL_ID=B.APPL_ID
AND B.RPPR_ID=C.RPPR_ID
AND C.RPPR_ID=D.RPPR_ID
AND A.ADMIN_PHS_ORG_CODE=''CA''
AND B.RPPR_STATUS_CODE=''FIN''
AND C.DOC_TYPE_CODE =''PRM''
AND D.REVIEWER_USER_ID in (''Agency'',''NIH'')
and D.PRAM_FLAG in (''Y'',''R'')
union
select  r.appl_id, rh.action_date as RECEIVED_DATE, rd.ram_doc_id as RPPR_DOC_ID
from RAMS_MV r, RAM_DOCS_MV rd, RAM_HISTORY_MV rh
where r.ram_id = rd.ram_id
and r.ram_id = rh.ram_id
and rh.RAM_ACTION_CODE = ''SUB''
and r.ram_type_code = ''IC_PRAM''
and r.RAM_STATUS_CODE = ''S''
and r.ram_id = rd.ram_id
and rd.DOC_TYPE_CODE = ''CORAM''
and rownum = 1
  ')

print 'Total PRAM count in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRM'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id, nga_rpt_seq_num)

SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_PRAM_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null 
and category_id=@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

print 'Total PRAM Added in eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


-------------------------------------------------------------------------------------------------------
--ADD NEW MULTI-YEAR PROGRESS REPORT
--docType = MPR (multi-year Progress Report Additional Material) 

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

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='MPR'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
--SELECT gi.appl_id, @CATEGORYID, gi.DOCUMENT_DATE, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id),gi.rppr_doc_id
--FROM IMPP_RPPR_MPR_NEW gi, vw_appls a
--WHERE a.appl_id=gi.appl_id 
--and gi.appl_id not in (select distinct x.APPL_ID from dbo.IMPP_RPPR_MPR_NEW x, egrants y
--where y.created_by_person_id= @impacID and x.APPL_ID=y.appl_id and y.category_id=@CATEGORYID and x.rppr_doc_id=y.nga_rpt_seq_num)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_MPR_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null 
and category_id=@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

print 'MULTI YEAR MPR documents Added to eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

------------------------------------------------------------------------------------------------------------
---Add eSNAPs 
			--IMRAN : 
			--insert new doc scanned by NIHScan T5 to document from impp
			--INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type, profile_id, qc_date, qc_reason)
			--SELECT a.appl_id, 38, c.submitted_date, @impacID, 'pdf', dbo.fn_grant_profile_id(a.grant_id),  
			----CASE WHEN submitted_date IS NULL and admin_phs_org_code='CA' THEN getdate() ELSE NULL END as qc_date,
			----CASE WHEN submitted_date IS NULL and admin_phs_org_code='CA' THEN 'NIHScan T5' ELSE NULL END as qc_reason
			--CASE WHEN (submitted_date IS NULL or a.appl_type_code=5) and admin_phs_org_code='CA' THEN getdate() ELSE NULL END as qc_date,
			--CASE WHEN (submitted_date IS NULL or a.appl_type_code=5) and admin_phs_org_code='CA' THEN 'NIHScan T5' ELSE NULL END as qc_reason
			--FROM  openquery(IMPAC, 'select appl_id,submitted_date from EAPPLS_T') C INNER JOIN dbo.vw_appls a ON c.appl_id = a.appl_id 
			--	LEFT OUTER JOIN vw_egrants_impac_docs ed ON c.appl_id = ed.appl_id  and ed.category_name='Application File'
			--WHERE ed.appl_id IS NULL
			------------------------------------------------------------------------------------------------------------
--insert new doc scanned by NIHScan T5 to document from impp
INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type, profile_id)
SELECT a.appl_id, 38, c.submitted_date, @impacID, 'pdf', dbo.fn_grant_profile_id(a.grant_id)
FROM  openquery(IRDB, 'select appl_id,submitted_date from EAPPLS_T') C INNER JOIN dbo.vw_appls a ON c.appl_id = a.appl_id 
	LEFT OUTER JOIN vw_egrants_impac_docs ed ON c.appl_id = ed.appl_id  and ed.category_name='Application File'
WHERE ed.appl_id IS NULL

print 'eSNAP Added=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------
---Add new NGAs
-------------------------------------------------------------------------------------------
DECLARE @maxNgaID int
SELECT @maxNGAid=max(nga_id) from documents where nga_id is not null	

INSERT impp_nga (appl_id, nga_create_date,rpt_seq_num, nga_id)
SELECT nga_new.appl_id, nga_new.nga_create_date,nga_new.rpt_seq_num,nga_new.nga_id
FROM  dbo.impp_nga RIGHT OUTER JOIN (SELECT * FROM  openquery(IRDB, 'select a.id as appl_id, a.last_upd_date as nga_create_date, a.rpt_seq_num as rpt_seq_num, b.history_doc_seq as nga_id 
from rpt_jobs_t a, history_docs_mv b
	where a.id_type_code=''APPL_ID'' and a.event_code=''ENGA'' and a.rpt_seq_num=b.rpt_seq_num and a.last_upd_date is not null') Rowset_1) nga_new ON dbo.impp_nga.nga_id = nga_new.nga_id
WHERE (dbo.impp_nga.nga_id IS NULL)

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='ENG' and category_name='NOA'

INSERT documents(appl_id,category_id,document_date, created_by_person_id,file_type,nga_rpt_seq_num, nga_id,profile_id)
SELECT i.appl_id, @CATEGORYID, i.nga_create_date, @impacID, 'pdf',i.rpt_seq_num, i.nga_id, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_nga i, appls a
WHERE i.appl_id=a.appl_id and nga_id>@maxNGAid

print 'New NOA Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------
---Add new Final Invention Statement 
-------------------------------------------------------------------------------------------------------
UPDATE appls
SET fis_date=C.submitted_date 
FROM appls a, openquery(IRDB, 'select appl_id, submitted_date 
from DOC_AVAILABLE_MV d, closeout_docs_MV c where d.doc_key_id=c.closeout_doc_id and d.doc_type_code=''FIS'' ') c
WHERE a.appl_id=c.appl_id--75614

INSERT documents(appl_id, category_id, created_by_person_id, file_type, document_date, profile_id)
SELECT appls.appl_id, 52, @impacID,'pdf', appls.fis_date, dbo.fn_grant_profile_id(appls.grant_id)
FROM  egrants RIGHT OUTER JOIN appls ON egrants.appl_id = appls.appl_id and egrants.category_id=52 and created_by='impac'
WHERE appls.fis_date IS NOT NULL and egrants.category_name IS NULL

print 'Final Invention statement Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------
---Add new Progress Final "FPR"
--- 2/9/2017 -- change to RFPPR Because from 1/1/2017 either RFPPR or FPA
--- Remove all FPA from 1/1/2017 and bring all RFPPR
-------------------------------------

Truncate Table dbo.IMPP_FRPPR

INSERT dbo.IMPP_FRPPR(APPL_ID,SUBMITTED_DATE)
SELECT DOC_KEY_ID,SUBMITTED_FILE_DATE from openquery(IRDB,'select A.DOC_KEY_ID, A.SUBMITTED_FILE_DATE from DOC_AVAILABLE_MV A, 
APPLS_T B where A.doc_type_code=''FRPPR'' AND A.DOC_KEY_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA'' ')

--SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='FRPPR'...Imran:5/26/17  651
SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='FRPPR' and category_name='FRPPR'

UPDATE a
SET a.fpr_date=b.submitted_date
FROM appls a, dbo.IMPP_FRPPR b
WHERE a.appl_id=b.appl_id
and a.fpr_date is null

INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id)
SELECT A.appl_id, @CATEGORYID, @impacID, 'pdf', A.SUBMITTED_DATE,dbo.fn_grant_profile_id(B.grant_id)	
from dbo.IMPP_FRPPR A, appls B where A.appl_id=B.appl_id
AND B.appl_id not in (
	SELECT distinct appl_id from documents where category_id = @CATEGORYID 
	and created_by_person_id=@impacID and disabled_by_person_id is null
	and created_date>='1/1/2017'
)


print 'New FRPPR Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------
---9/28/2017 : Imran: Add new IPA
-------------------------------------

Truncate Table dbo.IMPP_IPA

INSERT dbo.IMPP_IPA(APPL_ID,SUBMITTED_DATE)
SELECT DOC_KEY_ID,SUBMITTED_FILE_DATE from openquery(IRDB,'select A.DOC_KEY_ID, A.SUBMITTED_FILE_DATE from DOC_AVAILABLE_MV A, 
APPLS_T B where A.doc_type_code=''IPA'' AND A.DOC_KEY_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA'' ')

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='IPA'  --662

INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id)
SELECT A.appl_id, @CATEGORYID, @impacID, 'pdf', A.SUBMITTED_DATE,dbo.fn_grant_profile_id(B.grant_id)	
from dbo.IMPP_IPA A, appls B where A.appl_id=B.appl_id
AND B.appl_id not in (
	SELECT distinct appl_id from documents where category_id = @CATEGORYID 
	and created_by_person_id=@impacID and disabled_by_person_id is null
)
print 'New IPA Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------------------
---7/18/2017 : Imran: Add new IRPPR
-------------------------------------

Truncate Table dbo.IMPP_IRPPR

INSERT dbo.IMPP_IRPPR(APPL_ID,SUBMITTED_DATE,IRPPR_ID)
SELECT DOC_KEY_ID,SUBMITTED_FILE_DATE,RPPR_ID from openquery(IRDB,'select A.DOC_KEY_ID, A.SUBMITTED_FILE_DATE, c.rppr_id from DOC_AVAILABLE_MV A, 
APPLS_T B, rpprs_MV C where A.doc_type_code=''IRPPR'' AND A.DOC_KEY_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA'' AND B.APPL_ID=C.APPL_ID AND C.RPPR_TYPE_CODE=''I'' ')

--SELECT DOC_KEY_ID,SUBMITTED_FILE_DATE from openquery(IRDB,'select A.DOC_KEY_ID, A.SUBMITTED_FILE_DATE from DOC_AVAILABLE_MV A, 
--APPLS_T B where A.doc_type_code=''IRPPR'' AND A.DOC_KEY_ID=B.APPL_ID AND B.ADMIN_PHS_ORG_CODE=''CA'' ')



SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='IRPPR'  --662

INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id)
SELECT A.appl_id, @CATEGORYID, @impacID, 'pdf', A.SUBMITTED_DATE,dbo.fn_grant_profile_id(B.grant_id)	
from dbo.IMPP_IRPPR A, appls B where A.appl_id=B.appl_id
AND B.appl_id not in (
	SELECT distinct appl_id from documents where category_id = @CATEGORYID 
	and created_by_person_id=@impacID and disabled_by_person_id is null
	--and created_date>='6/30/2017'
)

print 'New IRPPR Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------------------------------------------------------------------
/****** BUILDING GREEN SHEET DATA FROM NCI/CIIP SCHEMA**/

exec sp_egrants_maint_impacdocs_GREENSHEETS

-------------------------------------------------------------------------------------------------------

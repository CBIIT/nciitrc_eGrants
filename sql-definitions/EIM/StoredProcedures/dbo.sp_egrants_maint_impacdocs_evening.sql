SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF



CREATE PROCEDURE [dbo].[sp_egrants_maint_impacdocs_evening] 
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

-------------------------------------------
--6/16/2017: Greensheet Revision (REV)
--Imran:6/16/2017
--eGrants Category Map : Greensheet SPEC Rev
--Sub category Text = automatic from gm_actions_queue_vw
-------------------------------------------
TRUNCATE TABLE dbo.CIIP_Greensheet_Rev

--INSERT dbo.CIIP_Greensheet_Rev(APPL_ID,agt_id,SUBMITTED_DATE)
--SELECT APPL_ID,agt_id,SUBMITTED_DATE FROM OPENQUERY(CIIP, '
--SELECT b.appl_id,B.AGT_ID,A.submitted_date FROM FORMS_T A, APPL_FORMS_T B
--WHERE A.ID=B.FRM_ID AND A.FORM_ROLE_CODE=''REV'' and A.FORM_STATUS=''FROZEN'' ')  

--Imran : 9/24/2018 : Change the new query from David Chang --3736 are new entries
INSERT dbo.CIIP_Greensheet_Rev(APPL_ID,agt_id,SUBMITTED_DATE)
SELECT APPL_ID,agt_id,SUBMITTED_DATE FROM OPENQUERY(CIIP, ' 
SELECT c.appl_id,B.AGT_ID,A.submitted_date FROM FORMS_T A, APPL_FORMS_T B, APPL_GM_ACTIONS_T C
WHERE A.ID=B.FRM_ID AND A.FORM_ROLE_CODE=''REV'' AND b.agt_id = c.id and A.FORM_STATUS=''FROZEN'' ')  


UPDATE X SET X.Revision_type_description=Z.REVISION_TYPE_DESCRIP
FROM dbo.CIIP_Greensheet_Rev X,OPENQUERY(CIIP, 'SELECT C.ID,C.REVISION_TYPE_DESCRIP FROM GM_ACTION_QUEUE_VW C') Z
WHERE X.agt_id=Z.ID 

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='REV'

INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
select zz.appl_id,@CATEGORYID,zz.SUBMITTED_DATE,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.AGT_ID,ZZ.Revision_type_description
from CIIP_Greensheet_Rev zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.AGT_ID not in 
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  ---AND x.profile_id=1 

print 'NEW Greensheet SPEC REV INSERTED =' + cast(@@ROWCOUNT as varchar)
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
------TRUNCATE TABLE dbo.impp_Carryover

------INSERT dbo.impp_Carryover(APPL_ID,DOCUMENT_DATE,keyId,Request_id)
------SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
------select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
------WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRACOV'' and d.doc_key_id = pa.pa_request_id ')  

------SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRACOV'

------INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
------select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.Request_id,'Request'
------from impp_Carryover zz, vw_appls ee 
------where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
------and zz.Request_id not in 
------(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
------and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  ---AND x.profile_id=1 
--------and x.category_id=@CATEGORYID AND x.profile_id=1 )
------print 'NEW Prior approval carry Over INSERTED =' + cast(@@ROWCOUNT as varchar)

-------------------------------------------
--5/18/2017: PI Change Request (PRACPC)
--Imran:5/18/2017
--eGrants Category Map : Post-Award Change
--Sub category Text = PI
--Mod: 7/12/2017: should be runing from any one jobs not jultiples. taking out from impacdocs
--12/12/2017: Imran: Change : There must not be multiple PRACOV for a given appl. If user annotate and upload(by replacing the impacII doc) 
--there should not be any new download form ImpacII Unless ImpacII has a new document with a new Key ID
--5/8/2019 : iMRAN : Requirement change. Latest version deployment using stored procedure
-------------------------------------------
EXEC sp_egrants_maint_impacdocs_PRACPC

--------TRUNCATE TABLE dbo.impp_PiChange
----------Imran:11/10/2018 Implemented source change
----------INSERT dbo.impp_PiChange(APPL_ID,DOCUMENT_DATE,keyId)
----------SELECT APPL_ID, DOCUMENT_DATE,keyId FROM OPENQUERY(IRDB, '
----------select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
----------WHERE pa.request_type_id =3 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRACPC'' and d.doc_key_id = pa.pa_request_id ')  
--------INSERT dbo.impp_PiChange(APPL_ID,DOCUMENT_DATE,keyId)
--------SELECT APPL_ID, DOCUMENT_DATE,keyId FROM OPENQUERY(IRDB, '
--------select pa.appl_id, history.action_date as DOCUMENT_DATE,d.doc_key_id as keyId from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
--------WHERE pa.request_type_id =3 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRACPC'' and d.doc_key_id = pa.pa_request_id ')  
--------SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRACPC'
--------INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
--------select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.keyId,'PI'
--------from impp_PiChange zz, vw_appls ee 
--------where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
--------and zz.keyId not in 
--------(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
--------and x.category_id=@CATEGORYID and x.url is null AND x.profile_id=1 and x.created_by_person_id=@impacID)
----------and x.category_id=@CATEGORYID AND x.profile_id=1 )
--------print 'NEW PI Change Request INSERTED =' + cast(@@ROWCOUNT as varchar)

-------------------------------------------
--5/18/2017: No Cost Extension (PRANCE)
--Imran:5/18/2017
--eGrants Category Map : No Cost Extension
--Sub category Text = Request
--12/12/2017: Imran: Change : There must not be multiple PRACOV for a given appl. If user annotate and upload(by replacing the impacII doc) 
--there should not be any new download form ImpacII Unless ImpacII has a new document with a new Key ID
--5/8/2019 : iMRAN : Requirement change. Latest version deployment using stored procedure
-------------------------------------------
EXEC sp_egrants_maint_impacdocs_PRANCE

--------TRUNCATE TABLE dbo.impp_NoCostExt
--------INSERT dbo.impp_NoCostExt(APPL_ID,DOCUMENT_DATE,keyId,Request_Id)
--------SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_Id FROM OPENQUERY(IRDB, '
--------select pa.appl_id, history.action_date as DOCUMENT_DATE,d.doc_key_id as keyId, history.request_id as Request_Id from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
--------WHERE pa.request_type_id =7 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRANCE'' and d.doc_key_id = pa.pa_request_id ')  
--------SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRANCE'
--------INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num, sub_category_name)
--------select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.Request_Id,'Request'
--------from impp_NoCostExt zz, vw_appls ee 
--------where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
--------and zz.Request_Id not in 
--------(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
--------and x.category_id=@CATEGORYID and x.url is null AND x.profile_id=1 and x.created_by_person_id=@impacID)
--------print 'NEW No Cost Extension INSERTED =' + cast(@@ROWCOUNT as varchar)

/***
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

print 'NEW WORK BOOK INSERTED =' + cast(@@ROWCOUNT as varchar)
***/

-------------------------------------------
--11/25/2015: Bring All Administrative Supplements with any action_code
-------------------------------------------
TRUNCATE TABLE dbo.IMPP_Admin_Supplements

/*--commented by hareeshj on 5/10/2016 at 5:10pm--*/
--INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE)
--select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE
--from OPENQUERY(IRDB,'SELECT A.APPL_ID,A.FULL_GRANT_NUM,A.FORMER_NUM,B.ACTION_DATE,B.ADMIN_SUPP_ACTION_CODE,
--A.SERIAL_NUM,A.APPL_TYPE_CODE,A.ACTIVITY_CODE,A.ADMIN_PHS_ORG_CODE,A.SUPPORT_YEAR,A.SUFFIX_CODE 
--FROM DM_PV_GRANT_PI A, ADMIN_SUPP_ROUTINGS_T B
--WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
--ORDER BY APPL_ID DESC,ACTION_DATE DESC

/*--added by hareeshj on 5/10/2016 at 5:10pm--*/
--6/6/2016 : iMRAN oMAIR : COMMING THE FOLLOWING CODE BECAUSE NOW WE ARE CHANIGMG THIS TO APPLS_MV
----INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE)
----select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE
----from OPENQUERY(IRDB,'SELECT A.APPL_ID,A.FULL_GRANT_NUM,A.FORMER_NUM,B.ACTION_DATE,B.ADMIN_SUPP_ACTION_CODE,
----A.SERIAL_NUM,A.APPL_TYPE_CODE,A.ACTIVITY_CODE,A.ADMIN_PHS_ORG_CODE,A.SUPPORT_YEAR,A.SUFFIX_CODE 
----FROM PVA_GRANT_PI_MV A, ADMIN_SUPP_ROUTINGS_T B
----WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
----ORDER BY APPL_ID DESC,ACTION_DATE DESC

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

-------------------------------------------
--5/18/2015: Bring All GCC Notification from ImpacII  **
-------------------------------------------
/*
TRUNCATE TABLE dbo.IMPP_CloseOut_Notification_All
INSERT dbo.IMPP_CloseOut_Notification_All(appl_id,Notification_Name,Created_date)
select appl_id,Notification_name,created_date from openquery(IMPAC,'
select a.event_key_id as appl_id, a.Notification_name, a.created_date  
from IMPACII8.EVENT_LOGS_T a
where a.event_key_id in (select b.appl_id from appls_t b where b.admin_phs_org_code=''CA'')
and a.Notification_name in (''FRAM_SUBMITTED_INTERNAL'',''GCM_CLOSEOUT_COMPLETE_LETTER'',''FPR_SUBMITTED_INTERNAL'',''FPR_SUBMITTED_EXTERNAL'',''GCM_GCC_LTR1'',''GCM_GCC_LTR2'',''GCM_GCC_LTR3'') ')

--09-10-2020 bshell - This has been replaced by webservice
declare @imppQuery nvarchar(500) = '
select e.EVENT_KEY_ID, e.NOTIFICATION_NAME, e.CREATED_DATE 
from IRDB.EVENT_LOGS_T e
	join APPLS_MV a on e.EVENT_KEY_ID=a.APPL_ID
where 
	a.ADMIN_PHS_ORG_CODE=''''CA''''
	and e.NOTIFICATION_NAME in (''''FRAM_SUBMITTED_INTERNAL'''',''''GCM_CLOSEOUT_COMPLETE_LETTER'''',''''FPR_SUBMITTED_INTERNAL'''',''''FPR_SUBMITTED_EXTERNAL'''',''''GCM_GCC_LTR1'''',''''GCM_GCC_LTR2'''',''''GCM_GCC_LTR3'''')' 

declare @imppLastDate datetime = (select max(Created_Date) from IMPP_CloseOut_Notification_All)
if @imppLastDate is not null 
begin
	set @imppQuery = @imppQuery + ' and e.CREATED_DATE > TO_DATE(''''' + convert(nvarchar(25), @imppLastDate, 120) + ''''', ''''YYYY-MM-DD HH24:MI:SS'''')'
end

INSERT IMPP_CloseOut_Notification_All (appl_id, Notification_Name, Created_date)
exec('select event_key_id, notification_name, created_date from openquery(IRDB, ''' + @imppQuery + ''')')
print 'GCC Notification in ImpacII =' + cast(@@ROWCOUNT as varchar)
**/
-------------------------------------------
--7/22/2015: Bring FFR REJECTION Notification from ImpacII
-------------------------------------------
/*
TRUNCATE TABLE dbo.IMPP_FFR_Notification_All
INSERT dbo.IMPP_FFR_Notification_All(appl_id,Notification_Name,Created_date)
select appl_id,Notification_name,created_date from openquery(IMPAC,'
select a.event_key_id as appl_id, a.Notification_name, a.created_date  
from IMPACII8.EVENT_LOGS_T a
where a.event_key_id in (select b.appl_id from appls_t b where b.admin_phs_org_code=''CA'')
and a.Notification_name in (''FFR_REJECTION'') ')

--09/09/2020 bshell - Replaced with the web service
declare @imppFfrQuery nvarchar(500) = '
select e.EVENT_KEY_ID, e.NOTIFICATION_NAME, e.CREATED_DATE 
from IRDB.EVENT_LOGS_T e
	join APPLS_MV a on e.EVENT_KEY_ID=a.APPL_ID
where 
	a.ADMIN_PHS_ORG_CODE=''''CA''''
	and e.NOTIFICATION_NAME in (''''FFR_REJECTION'''')' 

declare @imppLastFfrDate datetime = (select max(Created_Date) from IMPP_FFR_Notification_All)
if @imppLastFfrDate is not null 
begin
	set @imppFfrQuery = @imppFfrQuery + ' and e.CREATED_DATE > TO_DATE(''''' + convert(nvarchar(25), @imppLastFfrDate, 120) + ''''', ''''YYYY-MM-DD HH24:MI:SS'''')'
end

INSERT dbo.IMPP_FFR_Notification_All (appl_id, Notification_Name, Created_date)
exec('select event_key_id, notification_name, created_date from openquery(IRDB, ''' + @imppFfrQuery + ''')')
print 'FFR REJECTION Notification in ImpacII =' + cast(@@ROWCOUNT as varchar)
**/
/***
-------------------------------------------
----JIT add new JIT file only
-------------------------------------------
Declare @submittedfiledt Datetime
--UPDATE appls 
--SET jit_date=@submittedfiledt
--SELECT @submittedfiledt=submitted_file_date FROM appls a, 
--openquery(IMPAC1, 'select key_id, submitted_file_date from docs where submitted_file_date>(SYSDATE - 3) and doc_type_code=''JIT''') c
--WHERE a.appl_id=c.key_id
--and jit_date is null

--2/20/2017 : Failed because docs_t has been depricated: Imran
----UPDATE appls 
----SET jit_date=@submittedfiledt
----SELECT @submittedfiledt=submitted_file_date FROM appls a, 
----openquery(IRDB, 'select key_id, submitted_file_date from docs where submitted_file_date>(SYSDATE - 3) and doc_type_code=''JIT''') c
----WHERE a.appl_id=c.key_id
----and jit_date is null

UPDATE appls 
SET jit_date=@submittedfiledt
SELECT @submittedfiledt=submitted_file_date FROM appls a, 
openquery(IRDB, 'select DOC_KEY_ID, submitted_file_date from doc_available_mv where submitted_file_date>(SYSDATE - 3) and doc_type_code=''JIT''') c
WHERE a.appl_id=c.DOC_KEY_ID
and jit_date is null


INSERT documents(appl_id, category_id, created_date, created_by_person_id, file_type, document_date,profile_id)
SELECT appls.appl_id, 60, appls.jit_date, @impacID, 'pdf', appls.jit_date, dbo.fn_grant_profile_id(appls.grant_id)
FROM  egrants RIGHT OUTER JOIN appls ON egrants.appl_id = appls.appl_id and egrants.category_name = 'JIT Info' and created_by='impac'
WHERE appls.jit_date IS NOT NULL and egrants.category_name IS NULL
print 'Inserted JIT =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/

-------------------------------------------------------------------------------------------------------
--set arra_flag at the grant level
update grants set arra_flag='n' where arra_flag='y'
update grants SET arra_flag='y'
WHERE grant_id IN(select distinct grant_id from vw_appls_arra where admin_phs_org_code='CA')
print 'ARRA FLAG SET count=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------
/***
---Add new Greensheet PGM 
INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id)	
SELECT appl_id,73,@impacID,'pdf',1
FROM vw_ciip_docs
WHERE appl_id NOT IN (select appl_id from documents where category_id=73)

print 'Inserted GreenSheet PGM=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------
---Add new Greensheet SPEC
INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id)
SELECT appl_id,74,@impacID,'pdf',1
FROM vw_ciip_docs 
WHERE appl_id NOT IN (select appl_id from documents where category_id=74)

print 'Inserted GreenSheet SPEC=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/

/****
-------------------------------------------------------------------------------------------------------
--Add New RPPRs  @impacID
-- Note: We will have to remove the following in future (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158)
--Update 10/28/2014 : Imran
--------------------------------------------------------------------------------------------------------
TRUNCATE TABLE impp_rpprs_t 

--INSERT impp_rpprs_t(appl_id, created_date, last_upd_date,correct_rcvd_date)
--SELECT appl_id, created_date, last_upd_date,RECEIVED_DATE
--FROM openquery(IRDB, 'select R.appl_id, R.created_date,ER.LAST_UPD_DATE ,ER.RECEIVED_DATE
--from RPPRs_T R,EAPPL_ROUTINGS_MV ER 
--WHERE R.RPPR_ID=ER.RPPR_ID
--AND ER.REVIEWER_USER_ID=''NIH''
--AND R.RPPR_STATUS_CODE=''FIN''
--AND ER.PRAM_FLAG=''N'' ')

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
***/

/*
-------------------------------------------------------------------------------------------------------
---Add eSNAPs 
--insert new doc scanned by NIHScan T5 to document from impp
INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type, profile_id, qc_date, qc_reason)
SELECT a.appl_id, 38, c.submitted_date, @impacID, 'pdf', dbo.fn_grant_profile_id(a.grant_id),  
--CASE WHEN submitted_date IS NULL and admin_phs_org_code='CA' THEN getdate() ELSE NULL END as qc_date,
--CASE WHEN submitted_date IS NULL and admin_phs_org_code='CA' THEN 'NIHScan T5' ELSE NULL END as qc_reason
CASE WHEN (submitted_date IS NULL or a.appl_type_code=5) and admin_phs_org_code='CA' THEN getdate() ELSE NULL END as qc_date,
CASE WHEN (submitted_date IS NULL or a.appl_type_code=5) and admin_phs_org_code='CA' THEN 'NIHScan T5' ELSE NULL END as qc_reason
FROM  openquery(IRDB, 'select appl_id,submitted_date from EAPPLS_T') C INNER JOIN dbo.vw_appls a ON c.appl_id = a.appl_id 
	LEFT OUTER JOIN vw_egrants_impac_docs ed ON c.appl_id = ed.appl_id  and ed.category_name='Application File'
WHERE ed.appl_id IS NULL

print 'eSNAP Added=' + cast(@@ROWCOUNT as varchar)

-----------------------------------------------------------------------------------------------------------
*/

-------------------------------------------------------------------------------------------------------
--Add new PRAM Added by Imran on 2/4/2014
--ENHANCEMENT : IMRAN 9/25/2014
--Enhancement : Imran 5/11/2015 : There are two types of PRAM one with Flag=y (non-compl by system) and other flag=R (add matarial requested by specialist)
--Hence adding this clause D.PRAM_FLAG in (''Y'',''R'')
--6/20/2018: mOVED PRAM CODE TO FOLLOWING SP
EXEC sp_egrants_maint_pop_PRAM

/***  COMMENTING AND REPLASING WITH PROC WRITTEN ABOVE
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
and D.compliant_publications_flag is null
AND D.REVIEWER_USER_ID in (''Agency'',''NIH'')
and D.PRAM_FLAG in (''Y'',''R'')   ')

print 'Total PRAM count in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
select @CATEGORYID=category_id from categories where impac_doc_type_code='PRM'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id, nga_rpt_seq_num)
--SELECT gi.appl_id, @CATEGORYID, gi.DOCUMENT_DATE, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id), gi.RPPR_DOC_ID
--FROM IMPP_RPPR_PRAM_NEW gi, vw_appls a
--WHERE a.appl_id=gi.appl_id 
--and gi.appl_id not in (select distinct x.APPL_ID from dbo.IMPP_RPPR_PRAM_NEW x, egrants y
--where y.created_by_person_id= 530 and x.APPL_ID=y.appl_id and y.category_id=@CATEGORYID and x.rppr_doc_id=y.nga_rpt_seq_num )
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_PRAM_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null 
and category_id=@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

print 'Total PRAM Added in eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/

/***
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
***/
/****
-------------------------------------------------------------------------------------------------------
--ADD NEW MYP 4/24/2014
--ENHANCEMENT  10/03/2014   DOCUMENT DATE CORRECTION
--Add new MYP

TRUNCATE TABLE IMPP_RPPR_MYP_NEW
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

print 'Total MYP count in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

select @CATEGORYID=category_id from categories where impac_doc_type_code='MYP'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_MYP_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null and created_by_person_id= @impacID
and category_id in (select category_id from categories where impac_doc_type_code='MYP') and url is null and egrants.appl_id=gi.appl_id)

--SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
--from IMPP_RPPR_MYP_NEW gi,egrants e
--where  gi.APPL_ID=e.appl_id
--and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null 
--and category_id=@CATEGORYID and url is null and egrants.appl_id=gi.appl_id)

--SELECT gi.appl_id, @CATEGORYID, gi.DOCUMENT_DATE, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id),gi.rppr_doc_id
--FROM IMPP_RPPR_MYP_NEW gi, vw_appls a
--WHERE a.appl_id=gi.appl_id 
--and gi.appl_id not in (select distinct x.APPL_ID from dbo.IMPP_RPPR_MYP_NEW x, egrants y
--where y.created_by_person_id= @impacID and x.APPL_ID=y.appl_id and y.category_id=@CATEGORYID and x.rppr_doc_id=y.nga_rpt_seq_num)

print 'MYP documents Added to eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/
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

--commented by hareeshj on 1/15/16 2:19pm
--INSERT dbo.IMPP_FPA(appl_id,document_date,doc_file_name)
--SELECT appl_id,document_date,doc_file_name 
--FROM OPENQUERY(IRDB,'select a.appl_id,b.SUBMITTED_FILE_DATE as document_date,b.doc_file_name from appls_t a, IRDB.DOCS_MV b
--where a.appl_id=B.KEY_ID
--and A.ADMIN_PHS_ORG_CODE=''CA''
--and B.DOC_TYPE_CODE=''FPA'' ')

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

/***
-------------------------------------------
--Add CloseOut Flag
--(Imran 10/17/2014)
-------------------------------------------
----TRUNCATE TABLE IRDB_CLOSEOUT_FLAG

----INSERT dbo.IRDB_CloseOut_Flag(appl_id, appl_type_code, full_grant_num, fy, 
----project_period_end_date, closeout_status_code, grant_close_date, ltr1_date, ltr2_date, ltr3_date, progress_rpt_accepted_date, final_report_date)

------commented on 5/4/16 5:41pm
----SELECT appl_id,appl_type_code,full_grant_num,fy,project_period_end_date,closeout_status_code,grant_close_date,ltr1_date
----,ltr2_date,ltr3_date,progress_rpt_accepted_date,final_report_date 
----FROM OPENQUERY(IRDB,'SELECT 
----g.appl_id 
----,a.appl_type_code 
----,a.grant_num as full_grant_num
----,a.fy
----,a.project_period_end_date
----,g.closeout_status_code 
----,g.grant_close_date
----,g.ltr1_date
----,g.ltr2_date
----,g.ltr3_date
----,g.progress_rpt_accepted_date
----,g.final_report_date
----FROM gm_closeouts_t g , appls_t a 
----WHERE g.appl_id = a.appl_id
----AND a.admin_phs_org_code=''CA''
----AND g.closeout_status_code = ''O'' 
----AND a.fy >= 2014 
----ORDER BY a.project_period_end_date DESC')

----print 'CloseOut Flag in IRDB=' + cast(@@ROWCOUNT as varchar)

----Update appls 
----Set closeout_flag='y'
----From appls a, IRDB_CLOSEOUT_FLAG b
----Where a.appl_id=b.appl_id

----print 'Closeout Flag Updated =' + cast(@@ROWCOUNT as varchar)
TRUNCATE TABLE IRDB_CLOSEOUT_FLAG_NEW
INSERT dbo.IRDB_CLOSEOUT_FLAG_NEW(appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE,action_date)
SELECT appl_id,fy,GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM OPENQUERY(IRDB,'SELECT g.appl_id,a.fy,g.GC_OBJECT_NAME,GC_ACTION_TYPE_CODE,GC_OBJECT_STATUS_CODE, action_date
FROM GC_ACTION_HISTORY_MV g , appls_t a 
WHERE g.appl_id = a.appl_id
AND a.admin_phs_org_code=''CA''
AND g.GC_OBJECT_NAME=''GRANT_CLOSEOUT'' AND g.GC_OBJECT_STATUS_CODE=''O'' AND a.fy >= 2014 ORDER BY g.action_date desc')

print 'Closeout Flag Update =' + cast(@@ROWCOUNT as varchar)

Update appls 
Set closeout_flag='y'
From appls a, IRDB_CLOSEOUT_FLAG_NEW b
Where a.appl_id=b.appl_id

print 'Count of Closeout_Flag in appls =' + cast(@@ROWCOUNT as varchar)
***/

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
--EXEC DBO.Record_egrants_docload_log  @PROC_NAME,@RUN_DATETIME,@CATEGORYID,'AWS',@IMPAC_DOWNLOAD_CNT,@EGRANTS_UPLOAD_CNT,null

/******begin : old code from hareesh*****  to be deleted********
/* old code, commented on 2/15/2017, 1pm by hareeshj
INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,impp_doc_id,profile_id,nga_rpt_seq_num)
SELECT i.appl_id,432,530,'pdf',i.created_date,doc_id,dbo.fn_grant_profile_id(a.grant_id),i.RPT_SEQ_NUM
FROM vw_impp_docs i, appls a
WHERE i.appl_id=a.appl_id
*/
----Adding the following code to eliminate dependencies on vw_impp_aws

--truncate impp_aws
--SELECT COUNT(*) FROM impp_AWS ----964,058 on 2/15/2017 1:00pm

TRUNCATE TABLE dbo.impp_AWS
--SELECT COUNT(*) FROM impp_AWS--0

--load impp_aws
INSERT dbo.impp_AWS(doc_id,appl_id,doc_type_code,created_date,rpt_seq_num)
select doc_id, impp.appl_id,doc_type_code,impp.created_date,rpt_seq_num 
from openquery(IRDB,'select d.doc_key_id as doc_id, a.appl_id, doc_type_code, a.created_date, r.rpt_seq_num 
from DOC_AVAILABLE_MV d, RPT_JOBS_MV r, appls_t a 
where d.doc_key_id = r.RPT_SEQ_NUM 
and r.ID = a.APPL_ID 
and r.EVENT_STATUS_CODE =''C'' 
and d.DOC_TYPE_CODE = ''AWS'' ') impp 
INNER JOIN dbo.appls ON impp.APPL_ID = dbo.appls.appl_id 
LEFT OUTER JOIN dbo.documents ON impp.DOC_ID = dbo.documents.impp_doc_id  
WHERE dbo.documents.document_id IS NULL  --1035952, 7:09 mins 2/15/2017 2pm


--insert aws into documents from impp_aws
INSERT documents(appl_id, category_id, created_by_person_id, file_type, document_date, impp_doc_id, profile_id, nga_rpt_seq_num)
SELECT i.appl_id, 432 AS category_id, 530 AS created_by_person_id, 'pdf' AS file_type, i.created_date, doc_id, dbo.fn_grant_profile_id(a.grant_id) AS profile_id, i.RPT_SEQ_NUM
FROM impp_AWS i INNER join appls a ON i.appl_id=a.appl_id
LEFT OUTER JOIN dbo.documents d ON a.appl_id = d.appl_id
WHERE d.document_id IS NULL  ----101 17 secs on 2/15 at 2pm ;10:52 secs, 0 on 2/14 --delete this dev execution time after observing performance for a week (on 2/21/2017 4:45pm)

print 'New AWS Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
*********end: old code of Hareesh***********/


/*** moved to altest
--added by hareeshj on 12032016 3:35am
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

print 'DOWNLOADED =' + cast(@@ROWCOUNT as varchar)+' EARLY_CONCUR_FLAG  ON '+ cast(getdate() as varchar)

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
print 'GPMATS_ASSIGNMENT DOWNLOADED =' + cast(@@ROWCOUNT as varchar)

--*********************************************************
***/

GO


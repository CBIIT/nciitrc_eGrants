SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE   PROCEDURE [dbo].[sp_egrants_maint_impacdocs] 
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

-------------------------------------------
--11/21/2015: DownLoad All WORKBOOK DOCUMENTS
--Production Date : 12/21/2015 : Imran
-------------------------------------------
--TRUNCATE TABLE dbo.impp_award_book

--commented by hareeshj on 1/15/16 2:20pm
--INSERT dbo.impp_award_book(APPL_ID,DOCUMENT_DATE,EGRANTS_CREATED_DATE)
--SELECT APPL_ID, DOCUMENT_DATE, GETDATE() FROM OPENQUERY(IRDB, 'SELECT RPT.ID AS APPL_ID, RPT.CREATED_DATE AS DOCUMENT_DATE FROM RPT_JOBS_T RPT, DOCS_MV DOCS 
--WHERE RPT.RPT_SEQ_NUM=DOCS.KEY_ID AND DOCS.DOC_TYPE_CODE=''WBR'' AND RPT.ID_TYPE_CODE=''APPL_ID'' ')

------Imran 3/10/2017
----INSERT dbo.impp_award_book(APPL_ID,DOCUMENT_DATE, keyid)
----SELECT APPL_ID, DOCUMENT_DATE, keyid FROM OPENQUERY(IRDB, 'SELECT RPT.ID AS APPL_ID, RPT.CREATED_DATE AS DOCUMENT_DATE, RPT.RPT_SEQ_NUM as keyid
----FROM RPT_JOBS_T RPT, DOC_AVAILABLE_MV DOCS 
----WHERE RPT.RPT_SEQ_NUM=DOCS.DOC_KEY_ID AND DOCS.DOC_TYPE_CODE=''WBR'' AND RPT.ID_TYPE_CODE=''APPL_ID'' ')

----select @CATEGORYID=category_id from categories where impac_doc_type_code='WBR'
----INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
----select zz.appl_id,@CATEGORYID,zz.document_date,@impacID,'xlsm',NULL,dbo.fn_grant_profile_id(ee.grant_id)  
----from impp_award_book zz, vw_appls ee where zz.appl_id=ee.appl_id
----and zz.appl_id not in (
----select distinct a.appl_id from documents a, impp_award_book b where a.appl_id=b.appl_id
----and datediff(ss,a.document_date,b.document_date)=0
----and a.category_id=@CATEGORYID
----)

/*--Mod: 7/12/2017: should be runing from any one jobs not jultiples. taking out from impacdocs

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
*/

-------------------------------------------
--11/25/2015: Bring All Administrative Supplements with any action_code
-------------------------------------------
TRUNCATE TABLE dbo.IMPP_Admin_Supplements

--commented by hareeshj on 5/10/2016 at 5:10pm
--INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE)
--select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE
--from OPENQUERY(IRDB,'SELECT A.APPL_ID,A.FULL_GRANT_NUM,A.FORMER_NUM,B.ACTION_DATE,B.ADMIN_SUPP_ACTION_CODE,
--A.SERIAL_NUM,A.APPL_TYPE_CODE,A.ACTIVITY_CODE,A.ADMIN_PHS_ORG_CODE,A.SUPPORT_YEAR,A.SUFFIX_CODE FROM DM_PV_GRANT_PI A, ADMIN_SUPP_ROUTINGS_T B
--WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
--ORDER BY APPL_ID DESC,ACTION_DATE DESC

--added by hareeshj on 5/10/2016 at 5:10pm
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
--'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='+convert(varchar,A.Supp_appl_id)+'&docType=IGI' commented on 10/17/18
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
print 'ADMIN SUPPLEMENT EMAIL CREATED=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

-------------------------------------------
--5/18/2015: Bring All GCC Notification from ImpacII
-------------------------------------------
--uncommented by hareesh on 5/25/15 bcos IMPAC linked server account in locked

--9/14/2015: iMRAN: DONOT UNCOMMENT EVER. THIS CODE IS HERE FOR FUTURE USE THIS CODE HAS BEEN REPLACED BY THE FOLLOWING PARA.

--------TRUNCATE TABLE dbo.IMPP_CloseOut_Notification_All

--------INSERT dbo.IMPP_CloseOut_Notification_All(appl_id,Notification_Name,Created_date)
--------select appl_id,Notification_name,created_date from openquery(IMPAC,'
--------select a.event_key_id as appl_id, a.Notification_name, a.created_date  
--------from IMPACII8.EVENT_LOGS_T a
--------where a.event_key_id in (select b.appl_id from appls_t b where b.admin_phs_org_code=''CA'')
--------and a.Notification_name in (''FRAM_SUBMITTED_INTERNAL'',''GCM_CLOSEOUT_COMPLETE_LETTER'',''FPR_SUBMITTED_INTERNAL'',''FPR_SUBMITTED_EXTERNAL'',''GCM_GCC_LTR1'',''GCM_GCC_LTR2'',''GCM_GCC_LTR3'',''FFR_REJECTION'') ')

--print 'GCC Notification in ImpacII =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------


-------------------------------------------
--5/18/2015: Bring All GCC Notification from ImpacII
-------------------------------------------
/*
TRUNCATE TABLE dbo.IMPP_CloseOut_Notification_All

INSERT dbo.IMPP_CloseOut_Notification_All(appl_id,Notification_Name,Created_date)
select appl_id,Notification_name,created_date from openquery(IMPAC,'
select a.event_key_id as appl_id, a.Notification_name, a.created_date  
from IMPACII8.EVENT_LOGS_T a
where a.event_key_id in (select b.appl_id from appls_t b where b.admin_phs_org_code=''CA'')
and a.Notification_name in (''FRAM_SUBMITTED_INTERNAL'',''GCM_CLOSEOUT_COMPLETE_LETTER'',''FPR_SUBMITTED_INTERNAL'',''FPR_SUBMITTED_EXTERNAL'',''GCM_GCC_LTR1'',''GCM_GCC_LTR2'',''GCM_GCC_LTR3'') ')

print 'GCC Notification in ImpacII =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
*/
-------------------------------------------
--7/22/2015: Bring FFR REJECTION Notification from ImpacII
-------------------------------------------
/**
TRUNCATE TABLE dbo.IMPP_FFR_Notification_All

INSERT dbo.IMPP_FFR_Notification_All(appl_id,Notification_Name,Created_date)
select appl_id,Notification_name,created_date from openquery(IMPAC,'
select a.event_key_id as appl_id, a.Notification_name, a.created_date  
from IMPACII8.EVENT_LOGS_T a
where a.event_key_id in (select b.appl_id from appls_t b where b.admin_phs_org_code=''CA'')
and a.Notification_name in (''FFR_REJECTION'') ')

print 'FFR REJECTION Notification in ImpacII =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
**/
-------------------------------------
--3/12/2015: Imran : Add FRM into eGrants
/*
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

print 'FRM Added into eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
*/

/***
-------------------------------------------
--3/10/2015: Imran :Add FPA into eGrants
TRUNCATE TABLE IMPP_FPA

--commented by hareeshj on 1/15/16 2:22pm
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

select @CATEGORYID=category_id from categories where impac_doc_type_code='FPA'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id)
from IMPP_FPA gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.appl_id not in (select distinct appl_id from egrants where category_id = @CATEGORYID)

print 'FPA Added into eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/


-------------------------------------------
--Add CloseOut Flag
--(Imran 10/17/2014)
-------------------------------------------
----TRUNCATE TABLE IRDB_CLOSEOUT_FLAG

----INSERT dbo.IRDB_CloseOut_Flag(appl_id,appl_type_code,full_grant_num,fy,project_period_end_date,closeout_status_code,grant_close_date,ltr1_date
----,ltr2_date,ltr3_date,progress_rpt_accepted_date,final_report_date)

------commented on 5/4/16 5:45pm by hareeshj
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

----print 'CloseOut Flag in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

----Update appls 
----Set closeout_flag='y'
----From appls a, IRDB_CLOSEOUT_FLAG b
----Where a.appl_id=b.appl_id

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
/*--commented out by hareeshj to stop addition on new users overnight --1/27/2014
declare @userid varchar(50)
declare @profileid int
declare @personid int
declare @role int

declare c_egpeopleRole cursor for 
SELECT DISTINCT q.searched_by AS userid, pr.profile_id, 1 as position_id
FROM  dbo.profiles pr
INNER JOIN dbo.queries q ON pr.profile = q.ic 
LEFT OUTER JOIN dbo.egrants_people egp ON q.searched_by = egp.userid
WHERE (egp.person_id IS NULL )
and (q.searched_by IS NOT NULL and q.searched_by <>'' and q.searched_by not like '%,%')

print '-----------LOG Begin----------at--' + cast(getdate() as varchar)

	open  c_egpeopleRole 
	fetch next from c_egpeopleRole into @userid, @profileid, @role
    while @@fetch_status = 0 
    begin

		INSERT INTO eim.dbo.egrants_people (userid,profile_id)
		VALUES(@userid,@profileid)
		select @personid = @@IDENTITY

		INSERT INTO eim.dbo.egrants_access (person_id,application_type,position_id)
		VALUES(@personid,'egrants',@role)

		INSERT eim.dbo.people (userid,profile_id,position_id,application_type,egrants )
		VALUES(@userid, @profileid,1,'egrants',1)

	print 'Added userid=' + cast(@userid as varchar) + ' personid=' +cast(@personid as varchar)+'role='+cast(@role as varchar)

	fetch next from c_egpeopleRole into @userid, @profileid, @role
    end
	close c_egpeopleRole 
	deallocate c_egpeopleRole

*/
	


/*** Add new people into people table until econtracts does not go to production After this the following will be commented
INSERT eim.dbo.people ( userid, profile_id,position_id )
SELECT DISTINCT q.searched_by AS userid, pr.profile_id, 1
FROM  dbo.profiles pr
INNER JOIN dbo.queries q ON pr.profile = q.ic 
LEFT OUTER JOIN dbo.people p ON q.searched_by = p.userid
WHERE (p.person_id IS NULL) and (q.searched_by IS NOT NULL and q.searched_by <>'' and q.searched_by not like '%,%' )
***/

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

--commented by hareeshj on 11/16/2015 5:00pm
--INSERT impp_grant_images_NEW_t(appl_id, accession_num, total_pages_num, appl_received_date)
--SELECT appl_id, accession_num, total_pages_num, appl_received_date
--FROM openquery(IRDB, 'select a.appl_id, g.accession_num, g.total_pages_num, a.appl_received_date 
--from GRANT_IMAGES_MV g, appls_t a where g.accession_num=a.accession_num and a.serial_num is not null')

/*--commented on 1/20/2016 by hareeshj at 3:35pm
--added by hareeshj on 11/16/2015 5:00pm
INSERT impp_grant_images_NEW_t(appl_id, accession_num, total_pages_num, appl_received_date)
SELECT appl_id, accession_num, total_pages_num, 
--appl_received_date
CASE WHEN isdate(appl_received_date) = 1 THEN convert(datetime, appl_received_date, 121) END appl_received_date
FROM openquery(IRDB, 'select a.appl_id, g.accession_num, g.total_pages_num, 
--a.appl_received_date 
CAST(a.appl_received_date AS varchar2(30)) appl_received_date
from GRANT_IMAGES_MV g, appls_t a 
where g.accession_num=a.accession_num 
and a.serial_num is not null')
*/

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

--------------------------------------------------------------------------------------------------------
--Add New RPPRs  @impacID
--These are doctype_code= IGI mapped to GMM as e-Applications
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
and D.compliant_publications_flag is null
AND D.REVIEWER_USER_ID in (''Agency'',''NIH'')
and D.PRAM_FLAG in (''Y'',''R'')   ')

print 'Total PRAM count in IRDB=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PRM'

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

/***
-------------------------------------------------------------------------------------------------------
--ADD NEW MYP 4/24/2014
--ENHANCEMENT  9/25/2014   DOCUMENT DATE CORRECTION
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

--select @Old_CATEGORYID=category_id from categories where impac_doc_type_code='MYP'
select @CATEGORYID=category_id from categories where impac_doc_type_code='MYP' and category_name='Progress Report'

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id,nga_rpt_seq_num)
SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id), gi.rppr_doc_id
from IMPP_RPPR_MYP_NEW gi,egrants e
where  gi.APPL_ID=e.appl_id
and gi.RPPR_DOC_ID not in (select distinct nga_rpt_seq_num from egrants where nga_rpt_seq_num is not null and created_by_person_id= @impacID
and category_id in (select category_id from categories where impac_doc_type_code='MYP') and url is null and egrants.appl_id=gi.appl_id)

--SELECT gi.appl_id, @CATEGORYID, gi.DOCUMENT_DATE, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id),gi.rppr_doc_id
--FROM IMPP_RPPR_MYP_NEW gi, vw_appls a
--WHERE a.appl_id=gi.appl_id 
--and gi.appl_id not in 
--(select distinct x.APPL_ID from dbo.IMPP_RPPR_MYP_NEW x, egrants y
--where y.created_by_person_id= @impacID and x.APPL_ID=y.appl_id and y.category_id in 
--	(select category_id from categories where impac_doc_type_code='MYP') 
--and x.rppr_doc_id=y.nga_rpt_seq_num
--)

print 'MYP (Progress Report) documents Added to eGrants=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***/

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
---Add new JIT Info
-------------------------------------------------------------------------------------------------------
--Imran 4/26/2019 commenting out this untill further
--Exec sp_egrants_maint_pop_JIT

/************  5/29/2018 : Commented by Imran
UPDATE appls 
SET jit_date=submitted_file_date
FROM appls a, openquery(IRDB, 'select doc_key_id, submitted_file_date from DOC_AVAILABLE_MV where doc_type_code=''JIT''') c
WHERE a.appl_id=c.doc_key_id

PRINT 'updated JIT date =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

INSERT documents(appl_id, category_id, created_by_person_id, file_type, created_date, document_date, profile_id)
SELECT appls.appl_id, 60, @impacID, 'pdf', getdate(), appls.jit_date, dbo.fn_grant_profile_id(appls.grant_id)
FROM  egrants RIGHT OUTER JOIN appls ON egrants.appl_id = appls.appl_id and egrants.category_name = 'JIT Info' and created_by='impac'
WHERE appls.jit_date IS NOT NULL and egrants.category_name IS NULL

print 'New JIT Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
******************/

--Add revised IMPAC II JIT link when user has modified existing eGrants row
--Imran 7/17/2017: following code does not make sence so I am commenting it untill furthre investigation.
/**
INSERT [documents](appl_id, category_id, created_by_person_id, file_type, document_date, created_date, 
profile_id, qc_date, qc_reason) --, qc_person_id
SELECT xx.appl_id, 60, @impacID,'pdf',xx.jd,xx.jd,dd.profile_id--,dbo.fn_find_file_modify_person_id(xx.appl_id, 60)
,getdate(),'Change'--Newer Impac JIT'
FROM documents dd inner join (select d.appl_id, max(d.created_date) cd, max(a.jit_date) jd, max(document_id) document_id 
from documents d inner join appls a on d.appl_id = a.appl_id where d.category_id=60 and d.disabled_date is null 
and d.url is not null and d.created_by_person_id=@impacID and d.appl_id not in 
(Select distinct appl_id from documents where category_id=60 and url is null and created_by_person_id=@impacID 
and  (disabled_date is null or (disabled_date is not null and created_date >= a.jit_date))) group by d.appl_id 
having max(a.jit_date)>max(d.created_date)) xx on dd.document_id =xx.document_id
***/

-- Remove from QC if this document is already Bundles 
--[Fix provided by Imran; Added here by Leon on 10/05/2012]
--Imran 7/17/2017: bundles are gone hence commenting this code.
/**
update documents set qc_date=null where category_id=60 and created_by_person_id=@impacID and url is null 
and disabled_date is null and parent_id is not null and qc_date is not null 
**/

/********************* 5/29/2018 : Cpmmented by Imran
--Update JIT date with latest date in IMPAC II 
--[Fix provided by Joel; Added here by Hareesh on 01/04/2011]
UPDATE documents 
SET document_date = a.jit_date 
FROM documents d 
INNER JOIN appls a ON d.appl_id = a.appl_id
WHERE d.category_id = 60
AND a.jit_date > d.document_date 
AND d.disabled_date IS NULL
AND d.url IS NULL
print 'New JIT date updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
***************/
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


--INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id)
--SELECT A.appl_id, @CATEGORYID, @impacID, 'pdf', A.SUBMITTED_DATE,dbo.fn_grant_profile_id(B.grant_id)	
--from dbo.IMPP_FRPPR A, appls B where A.appl_id=B.appl_id
--AND B.appl_id not in (
--SELECT distinct appl_id from documents where category_id=@CATEGORYID and created_by_person_id=@impacID and disabled_by_person_id is null
--and created_date>'1/1/2017'
--)

--UPDATE appls
--SET fpr_date=submitted_date
--FROM appls a, openquery(IMPAC,'select appl_id, submitted_date from docs_t d, closeout_docs c where d.key_id=c.closeout_doc_id and d.doc_type_code=''FPR'' ') c
--WHERE a.appl_id=c.appl_id

--commented by hareeshj on 1/15/16 2:25pm
--UPDATE appls
--SET fpr_date=submitted_date
--FROM appls a, openquery(IRDB,'select appl_id, submitted_date from docs_MV d, closeout_docs_MV c where d.key_id=c.closeout_doc_id and d.doc_type_code=''FPR'' ') c
--WHERE a.appl_id=c.appl_id

/*  commented by Imran 2/15/2017*
UPDATE appls
SET fpr_date=submitted_date
FROM appls a, openquery(IRDB,'select appl_id, submitted_date 
from doc_available_MV d, closeout_docs_MV c where d.doc_key_id=c.closeout_doc_id and d.doc_type_code=''FPR'' ') c
WHERE a.appl_id=c.appl_id


INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id)
SELECT appls.appl_id, 50, @impacID, 'pdf', appls.fpr_date,dbo.fn_grant_profile_id(appls.grant_id)	
FROM  egrants RIGHT OUTER JOIN appls ON egrants.appl_id = appls.appl_id and egrants.category_id=50 and created_by='impac'
WHERE appls.fpr_date IS NOT NULL and egrants.category_name IS NULL
*/
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

Truncate table dbo.GreenSheet_data

Insert dbo.GreenSheet_data(appl_id, PGM_FORM_STATUS, PGM_FORM_SUBMITTED_DATE, SPEC_FORM_STATUS, SPEC_FORM_SUBMITTED_DATE, APPL_TYPE_CODE, ACTION_TYPE, GPMATS_CANCELLED_FLAG, GPMATS_CLOSED_FLAG, DUMMY_FLAG, MULTIYEAR_AWARD_FLAG, REVISION_TYPE_CODE )
Select appl_id, PGM_FORM_STATUS, PGM_FORM_SUBMITTED_DATE, SPEC_FORM_STATUS, SPEC_FORM_SUBMITTED_DATE, APPL_TYPE_CODE, ACTION_TYPE, GPMATS_CANCELLED_FLAG, GPMATS_CLOSED_FLAG, DUMMY_FLAG, MULTIYEAR_AWARD_FLAG, REVISION_TYPE_CODE  
FROM OPENQUERY(CIIP, 'select g.appl_id, g.PGM_FORM_STATUS, g.PGM_FORM_SUBMITTED_DATE, g.SPEC_FORM_STATUS, g.SPEC_FORM_SUBMITTED_DATE, g.APPL_TYPE_CODE, gav.action_type,  g.GPMATS_CANCELLED_FLAG, g.GPMATS_CLOSED_FLAG, g.DUMMY_FLAG, gav.MULTIYEAR_AWARD_FLAG, gav.REVISION_TYPE_CODE  
					  from form_grant_vw g 
					  inner join GM_ACTION_QUEUE_VW gav on gav.appl_id = g.appl_id and gav.action_type = ''AWARD''')
					  
--					  UNION ALL
--Select appl_id, PGM_FORM_STATUS, PGM_FORM_SUBMITTED_DATE, SPEC_FORM_STATUS, SPEC_FORM_SUBMITTED_DATE, APPL_TYPE_CODE, ACTION_TYPE, GPMATS_CANCELLED_FLAG, GPMATS_CLOSED_FLAG, DUMMY_FLAG, MULTIYEAR_AWARD_FLAG, REVISION_TYPE_CODE    
--FROM OPENQUERY(CIIP, 'select g.appl_id, g.PGM_FORM_STATUS, g.PGM_FORM_SUBMITTED_DATE, g.SPEC_FORM_STATUS, g.SPEC_FORM_SUBMITTED_DATE, g.APPL_TYPE_CODE, gav.action_type,  g.GPMATS_CANCELLED_FLAG, g.GPMATS_CLOSED_FLAG, g.DUMMY_FLAG, gav.MULTIYEAR_AWARD_FLAG, gav.REVISION_TYPE_CODE  
--					  from appl_forms_t af 
--					  inner join GM_ACTION_QUEUE_VW gav on gav.id = af.agt_id 
--					  inner join form_grant_vw g on g.appl_id=gav.appl_id				  
--					  where gav.action_type = ''REVISION''')

--Greensheet_data Table has so many appls that are not in vw_appls. so delete them
delete greensheet_data where appl_id not in (select appl_id from vw_appls)

--There are so many historical data in GS. Delete those who has been deleted/purged
delete greensheet_data where appl_id in (select appl_id from vw_appls where frc_destroyed =1)--1239

--Delete non NCI data
delete greensheet_data where appl_id in (select appl_id from vw_appls where admin_phs_org_code<>'CA')--19

UPDATE documents SET disabled_by_person_id=1899, disabled_date=getdate()
WHERE document_id in (
SELECT document_id FROM egrants WHERE category_id in (73,74,612) and (admin_phs_org_code <>'ca')
)
------------------------------------------
---Update date for green sheets
---Once a GS has been submitted we get submitted date in greensheet_date table soto update it

update documents set documents.document_date = x.pgm_form_submitted_date
from GreenSheet_data x where documents.appl_id=x.appl_id
and convert(date,isnull(documents.document_date,''))<>convert(date,isnull(x.pgm_form_submitted_date,''))
and documents.category_id=73
print 'Total PGM GreenSheet date updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

update documents set documents.document_date = x.spec_form_submitted_date
from GreenSheet_data x where documents.appl_id=x.appl_id
and convert(date,isnull(documents.document_date,''))<>convert(date,isnull(x.spec_form_submitted_date,''))
and documents.category_id=74
print 'Total SPEC GreenSheet date updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


-------------------------------------------------------------------------------------------------------
---Add new Greensheet PGM 
---The links to *Program and Specialist *gs are displayed immediately (even when gs is NOT Saved), 
---users can navigate to the Greensheets system and start filling up the form.
---The link to Revision gs is displayed only when Revision gs is FROZEN. Therefore, Revision type of 
---greensheet is always read-only when accessed via eGrants. 
---The initial discussion was that we will keep ARC rules consistent with Revision. 


INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
SELECT appl_id,73,@impacID,'pdf',1, pgm_form_submitted_date
FROM greensheet_data gd
WHERE appl_id NOT IN (select appl_id from documents where category_id=73) 
and gd.action_type = 'AWARD'
and isnull(gd.GPMATS_CANCELLED_FLAG, 'N') <> 'Y'
and isnull(gd.GPMATS_CLOSED_FLAG, 'N') <> 'Y' 
and isnull(gd.DUMMY_FLAG,'N') = 'N' 
and gd.pgm_form_status <> 'NOT STARTED'

/*
INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
SELECT appl_id,73,@impacID,'pdf',1, pgm_form_submitted_date
FROM greensheet_data gd
WHERE appl_id NOT IN (select appl_id from documents where category_id=73) 
and gd.action_type = 'REVISION' and gd.pgm_form_status = 'FROZEN'
*/

/*
Greensheets for GPMATS Action Type = Award, for all grants types, including type 3, should be displayed in eGrants regardless of greensheet status.
*/

print 'Inserted GreenSheet PGM=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
SELECT appl_id,74,@impacID,'pdf',1, spec_form_submitted_date
FROM greensheet_data gd
WHERE appl_id NOT IN (select appl_id from documents where category_id=74) and
 gd.action_type = 'AWARD'
and isnull(gd.GPMATS_CANCELLED_FLAG, 'N') <> 'Y'
and isnull(gd.GPMATS_CLOSED_FLAG, 'N') <> 'Y' 
and isnull(gd.DUMMY_FLAG,'N') = 'N' 
and gd.spec_form_status <> 'NOT STARTED'


--INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
--SELECT appl_id,74,@impacID,'pdf',1, spec_form_submitted_date
--FROM greensheet_data gd
--WHERE appl_id NOT IN (select appl_id from documents where category_id=74) 
--and gd.action_type = 'REVISION' and gd.spec_form_status = 'FROZEN'
--and isnull(gd.GPMATS_CANCELLED_FLAG, 'N') <> 'Y'
--and isnull(gd.DUMMY_FLAG,'N') = 'N' 


print 'Inserted GreenSheet SPEC=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
-------------------------------------------------------------------------------------------------------


GO


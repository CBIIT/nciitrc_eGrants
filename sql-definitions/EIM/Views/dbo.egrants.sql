SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF




--CREATE OR ALTER PROCEDURE [dbo].[sp_egrants_maint_impacdocs_evening] 
--AS

--DECLARE @impacID int
--SELECT @impacID=person_id FROM people WHERE userid='impac'
--DECLARE @CATEGORYID smallint
--DECLARE @DOCTYPE VARCHAR(5)
--DECLARE @PROC_NAME VARCHAR(50)
--DECLARE @RUN_DATETIME DATETIME
--DECLARE @IMPAC_DOWNLOAD_CNT INT
--DECLARE @EGRANTS_UPLOAD_CNT INT
--SET @PROC_NAME = 'sp_egrants_maint_impacdocs'
--SET @RUN_DATETIME = GETDATE()

--print '====>>Proc sp_egrants_maint_impacdocs_EVENING Started @' + cast(getdate() as varchar)
---------------------------------------------
----: Greensheet ARC (ARC)
----Imran:12/11/2018
----eGrants Category Map : Award Review Certification
----Sub category Text = automatic from gm_actions_queue_vw  (Ask Lisa?)
---------------------------------------------
--TRUNCATE TABLE dbo.Greensheet_data_ARC

--INSERT dbo.Greensheet_data_ARC(APPL_ID,agt_id,SUBMITTED_DATE)
--SELECT APPL_ID,agt_id,SUBMITTED_DATE FROM OPENQUERY(CIIP, '
--SELECT G.appl_id,B.AGT_ID,A.submitted_date FROM FORMS_T A, APPL_FORMS_T B, APPL_GM_ACTIONS_T G
--WHERE A.ID=B.FRM_ID AND G.ID=B.AGT_ID AND A.FORM_ROLE_CODE=''ARC'' and A.FORM_STATUS=''FROZEN'' ')  

--SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='ARC'

--INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num)
--select zz.appl_id,@CATEGORYID,zz.SUBMITTED_DATE,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.AGT_ID 
--from Greensheet_data_ARC zz, vw_appls ee 
--where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
--and zz.AGT_ID not in 
--(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
--and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  ---AND x.profile_id=1 
--print 'NEW Greensheet ARC INSERTED =' + cast(@@ROWCOUNT as varchar)

--/*
--5/5/2021 - BSHELL - MOVED SPEC REV to Separate SP

--*/
--EXEC sp_egrants_maint_SPEC_REV


--/*----------------------------------------------------------
--4/9/2021 - BSHELL - INSERT New PGM Revision Population Here per eGrants-52
--*/

--EXEC sp_egrants_maint_PGM_REV

---------------------------------------------
----3/30/2017: Prior approval carry Over (PRACOV)
----Imran:6/13/2017
----eGrants Category Map : CarryOver
----Sub category Text = None
----12/12/2017: Imran: Change : There must not be multiple PRACOV for a given appl. If user annotate and upload(by replacing the impacII doc) 
----there should not be any new download form ImpacII Unless ImpacII has a new document with a new Key ID
----5/8/2019 : iMRAN : Requirement change. Latest version deployment using stored procedure
---------------------------------------------
--EXEC sp_egrants_maint_impacdocs_PRACOV


--EXEC sp_egrants_maint_impacdocs_PRACPC

--EXEC sp_egrants_maint_impacdocs_PRANCE

--TRUNCATE TABLE dbo.IMPP_Admin_Supplements

--INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE)
--select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE
--from OPENQUERY(IRDB,'SELECT A.APPL_ID, A.APPL_TYPE_CODE||A.GRANT_NUM AS FULL_GRANT_NUM, A.FORMER_NUM, B.ACTION_DATE, B.ADMIN_SUPP_ACTION_CODE,
--A.SERIAL_NUM, A.APPL_TYPE_CODE, A.ACTIVITY_CODE, A.ADMIN_PHS_ORG_CODE, A.SUPPORT_YEAR, A.SUFFIX_CODE 
--FROM APPLS_MV A, ADMIN_SUPP_ROUTINGS_T B
--WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
--ORDER BY APPL_ID DESC,ACTION_DATE DESC

--Update IMPP_Admin_Supplements
--SET IMPP_Admin_Supplements.Former_appl_id=B.appl_id
--from IMPP_Admin_Supplements a, vw_appls b
--where a.Former_Num=b.full_grant_num

--INSERT INTO dbo.IMPP_Admin_Supplements_WIP(serial_num,Supp_appl_id,Full_grant_num,Former_Num,Former_appl_id,Submitted_date,file_type,category_id,doc_url)
--SELECT A.serial_num,A.Supp_appl_id,A.Full_grant_num,A.Former_Num,A.Former_appl_id,A.Action_date,'PDF',38,
----'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='+convert(varchar,A.Supp_appl_id)+'&docType=IGI'
--'https://s2s.era.nih.gov/docservice/dataservices/document/once/applId/'+convert(varchar,A.Supp_appl_id)+'/' + 'IGI'
--FROM dbo.IMPP_Admin_Supplements A
--where A.admin_supp_action_code='STA'
--and A.Supp_appl_id NOT in (select distinct Supp_appl_id  from IMPP_Admin_Supplements_WIP WHERE Supp_appl_id IS NOT NULL)
----and A.serial_num NOT in (select distinct serial_num  from IMPP_Admin_Supplements_WIP WHERE serial_num IS NOT NULL)
--AND Action_date > '10/1/2015'
--order by a.serial_num
--print 'ADMINISTRATIVE SUPPLEMENT ACTIONS DOWNLOADED =' + cast(@@ROWCOUNT as varchar)

----Insert all accepted supplement action into placeholder to send email
--insert dbo.adsup_accepted(supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code)
--select supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code 
--from dbo.IMPP_Admin_Supplements where admin_supp_action_code='ACC'
--and Supp_appl_id not in (select distinct Supp_appl_id from adsup_accepted)
--print 'ADMIN SUPPLEMENT NEWLY ACCEPTED COUNT=' + cast(@@ROWCOUNT as varchar)

----Create email entry to go on next run from oga stage machine dts
--Exec dbo.adsupp_create_notification_accgrant
--print 'ADMIN SUPPLEMENT EMAIL CREATED=' + cast(@@ROWCOUNT as varchar)



---------------------------------------------------------------------------------------------------------
----set arra_flag at the grant level
--update grants set arra_flag='n' where arra_flag='y'
--update grants SET arra_flag='y'
--WHERE grant_id IN(select distinct grant_id from vw_appls_arra where admin_phs_org_code='CA')
--print 'ARRA FLAG SET count=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
---------------------------------------------------------------------------------------------------------


---------------------------------------------------------------------------------------------------------
----Add new PRAM Added by Imran on 2/4/2014
----ENHANCEMENT : IMRAN 9/25/2014
----Enhancement : Imran 5/11/2015 : There are two types of PRAM one with Flag=y (non-compl by system) and other flag=R (add matarial requested by specialist)
----Hence adding this clause D.PRAM_FLAG in (''Y'',''R'')
----6/20/2018: mOVED PRAM CODE TO FOLLOWING SP
--EXEC sp_egrants_maint_pop_PRAM


--EXEC sp_egrants_maint_pop_FRM


---------------------------------------------
----3/9/2015: Imran : Add FPA into eGrants

--TRUNCATE TABLE IMPP_FPA

--INSERT dbo.IMPP_FPA(appl_id,document_date)
--SELECT appl_id,document_date
--FROM OPENQUERY(IRDB,'select a.appl_id,b.SUBMITTED_FILE_DATE as document_date from appls_t a, IRDB.DOC_AVAILABLE_MV b
--where a.appl_id=B.DOC_KEY_ID
--and A.ADMIN_PHS_ORG_CODE=''CA''
--and B.DOC_TYPE_CODE=''FPA'' ')


--SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='FPA'

--INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
--SELECT distinct gi.appl_id, @CATEGORYID, convert(datetime,convert(varchar,gi.DOCUMENT_DATE,101),101), @impacID,'pdf', NULL AS url,dbo.fn_grant_profile_id(e.grant_id)
--from IMPP_FPA gi,egrants e
--where  gi.APPL_ID=e.appl_id
--and gi.appl_id not in (select distinct appl_id from egrants where category_id = @CATEGORYID)

--print 'FPA Added into eGrants=' + cast(@@ROWCOUNT as varchar)

-------------------------------------------------------------------------------------------------------
-----Add new AWS (Award Worksheet)
-----
-----
-----
--------------------------------------------------------------------------------------------------------
--TRUNCATE TABLE dbo.impp_AWS

--INSERT dbo.impp_AWS(doc_id,appl_id,doc_type_code,created_date,rpt_seq_num)
--select doc_id, impp.appl_id, doc_type_code, impp.SUBMITTED_FILE_DATE, rpt_seq_num 
--from openquery(IRDB,'select d.doc_key_id as doc_id, a.appl_id, doc_type_code, d.SUBMITTED_FILE_DATE, r.rpt_seq_num 
--from DOC_AVAILABLE_MV d, RPT_JOBS_MV r, appls_t a 
--where d.doc_key_id = r.RPT_SEQ_NUM 
--and r.ID = a.APPL_ID 
--and r.EVENT_STATUS_CODE =''C'' 
--and d.DOC_TYPE_CODE = ''AWS'' 
--and A.ADMIN_PHS_ORG_CODE=''CA'' ') impp --130726

--SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='AWS'

--INSERT documents(appl_id, category_id, created_by_person_id, file_type, document_date, profile_id, nga_rpt_seq_num)
--SELECT appl_id, @CATEGORYID, @impacID , 'pdf' , created_date, 1, RPT_SEQ_NUM
--FROM impp_AWS  where rpt_seq_num not in 
--(select distinct x.nga_rpt_seq_num from Documents x where x.nga_rpt_seq_num is not null 
--and x.category_id=@CATEGORYID and x.url is null AND x.profile_id=1 and x.created_by_person_id=@impacID) --7046
--SELECT @EGRANTS_UPLOAD_CNT = @@ROWCOUNT

--print 'New AWS Added =' + cast(@EGRANTS_UPLOAD_CNT as varchar)+' @ '+ cast(getdate() as varchar)


--GO


CREATE      VIEW [dbo].[egrants]
AS

WITH supps as (
	Select movedto_document_id, CAST (REPLACE(movedto_document_id, 'as','') as bigint) as DocumentID 
	  from IMPP_Admin_Supplements_WIP
     where movedto_document_id not like 'as%' and moved_date is not null and movedto_document_id is not null 
)

SELECT     dbo.profiles.profile AS ic, dbo.grants.grant_id, dbo.grants.admin_phs_org_code, dbo.grants.serial_num, 
                      dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, dbo.grants.serial_num), 6) AS grant_num, dbo.grants.is_tobacco, 
                      dbo.grants.to_be_destroyed, dbo.grants.mechanism_code, dbo.grants.grant_close_date, 
                      dbo.appls.appl_type_code + dbo.appls.activity_code COLLATE SQL_Latin1_General_CP1_CI_AS + dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar,
                      dbo.grants.serial_num), 6) + '-' + RIGHT('0' + CONVERT(varchar, dbo.appls.support_year), 2) + ISNULL(dbo.appls.suffix_code, '') AS full_grant_num, 
                      dbo.appls.appl_type_code, dbo.appls.activity_code, dbo.appls.support_year, dbo.appls.suffix_code, dbo.appls.project_title, dbo.appls.former_num, 
                      dbo.appls.rfa_pa_number, dbo.appls.council_meeting_date, dbo.appls.external_org_id, ISNULL(dbo.appls.first_name + ' ', '') 
                      + ISNULL(dbo.appls.mi_name + ' ', '') + dbo.appls.last_name AS pi_name, dbo.appls.person_id, dbo.appls.last_name, dbo.appls.first_name, 
                      dbo.appls.mi_name, dbo.appls.org_name, dbo.appls.prog_class_code, dbo.appls.appl_received_date, dbo.appls.irg_code, dbo.appls.irg_flex_code, 
                      dbo.appls.summary_statement_flag, dbo.appls.appl_status_group_descrip, dbo.appls.fy, dbo.appls.created_date AS appl_created_date, 
                      dbo.appls.last_upd_date, dbo.appls.receipt_date, ISNULL(dbo.appls.frc_destroyed, 0) AS frc_destroyed, dbo.appls.latest_nga_date, dbo.appls.jit_date, 
                      dbo.categories.category_name, dbo.categories.package, dbo.documents.category_id, dbo.documents.appl_id, dbo.documents.document_id, 
                      dbo.documents.document_size, dbo.documents.document_date, dbo.documents.page_count, dbo.documents.profile_id, dbo.documents.added_date, 
                      dbo.documents.corrupted, dbo.documents.nga_rpt_seq_num, dbo.documents.nga_id, dbo.documents.is_destroyed, dbo.documents.inventoried, 
                      dbo.documents.external_upload_id, dbo.documents.mail_upload_id, dt.txt_size, ISNULL(attachments.attachment_count, 0) AS attachment_count, 
                      dbo.documents.file_type, dbo.documents.alias, dbo.documents.created_date, dbo.documents.created_by_person_id, 
                      people_create.person_name AS created_by, dbo.documents.file_modified_date, dbo.documents.file_modified_by_person_id, 
                      people_file_modify.person_name AS file_modified_by, dbo.documents.modified_date, dbo.documents.index_modified_by_person_id, 
                      people_index_modify.person_name AS modified_by, dbo.documents.qc_date, dbo.documents.qc_reason, dbo.documents.qc_person_id, 
                      people_qc.person_name AS qc_userid, ISNULL(people_qc.person_name, people_qc.userid) AS qc_person_name, 
                      dbo.documents.stored_by_person_id, dbo.documents.stored_date, people_store.person_name AS stored_by, dbo.documents.disabled_date, 
                      dbo.documents.disabled_by_person_id, people_disable.person_name AS disabled_by, people_problem.person_name AS problem_reported_by, 
                      dbo.fn_doc_descr(dbo.documents.document_id) AS description, --'' AS description, 
                      dbo.fn_unix_file(dbo.documents.url) AS unix_file, dbo.documents.parent_id,dbo.documents.url, 
                      dbo.fn_unix_directory(dbo.documents.url) AS unix_directory, 
                      dbo.fn_appl_deleted_by_impac (appls.appl_id) deleted_by_impac,--- added by leon 5/4/2015
                      dbo.categories.impac_doc_type_code as impac_doc_type,			--- added by leon 9/8/2015
                      
                      dbo.documents.sub_category_name,
                      CASE 
						WHEN dbo.categories.category_name like '%:IGNORE%' THEN LEFT(dbo.categories.category_name,CHARINDEX(':IGNORE',dbo.categories.category_name)-1)
						WHEN dbo.documents.sub_category_name is null or dbo.documents.sub_category_name='' THEN dbo.categories.category_name
                      ELSE dbo.categories.category_name+':  '+dbo.documents.sub_category_name
                      END as document_name,   --- added by leon 5/23/2017       
                      
                      CASE WHEN qc_reason = 'error' AND qc_date IS NOT NULL THEN problem_msg ELSE NULL END AS problem_msg, 
                      
                      CASE WHEN people_create.userid = 'impac' THEN 'y' ELSE 'n' END AS impac_doc, 

                      CASE 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND (url LIKE 'data/%' or url LIKE '/data/%') AND people_create.userid != 'system' AND parent_id IS NULL THEN 'y' 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND (dbo.documents.category_id IS NULL OR dbo.documents.appl_id IS NULL) AND dbo.documents.qc_date IS NOT NULL THEN 'y'
                      ELSE 'n' 
                      END AS can_modify_index, 
                      
                      CASE 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='y' THEN 'n' 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND category_name IN ('Greensheet SPEC', 'Greensheet PGM', 'Greensheet DMC','Greensheet SPEC Rev','Award Review Certification', 'NGA Compilation', 'Final Invention Statement', 
                      'Progress Final', 'Award Worksheet', 'NGA', 'Funding', 'Award Worksheet Report') AND people_create.userid IN ('impac', 'system') OR
                      parent_id IS NOT NULL THEN 'n' 
                      ELSE 'y' 
                      END AS can_upload, 
                      
                      CASE 
                      WHEN category_name IN ('Greensheet SPEC', 'Greensheet PGM', 'Greensheet DMC','Greensheet SPEC Rev','Award Review Certification', 'NGA Compilation', 'Final Invention Statement', 'Progress Final', 'Financial Report', 'Award Worksheet', 'NGA', 'Summary Statement',
                       'Funding', 'Award Worksheet Report') AND people_create.userid IN ('impac', 'system') OR
                      parent_id IS NOT NULL THEN 'n' 
                      ELSE 'y' 
                      END AS can_delete, 
                      
                      CASE 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND documents.document_id >= 23316776 AND people_create.userid NOT IN ('impac', 'efile') AND parent_id IS NULL AND 
                      (url LIKE 'data/funded/nci/modify%' or url LIKE '/data/funded/nci/modify%') THEN 'y' 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND people_create.userid IN ('impac', 'efile') AND (url LIKE 'data/funded/nci/modify%' or url LIKE '/data/funded/nci/modify%') AND category_name IN ('Application File', 'Summary Statement', 
                      'JIT Info', 'Financial Report') AND parent_id IS NULL THEN 'y' 
					  WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND exists (select * from supps where DocumentID = documents.document_id) AND 
                      (url LIKE 'data/funded/nci/modify%' or url LIKE '/data/funded/nci/modify%') THEN 'y' 
                      ELSE 'n' 
                      END AS can_restore, 
                      
                      CASE WHEN qc_date IS NOT NULL AND documents.appl_id IS NOT NULL AND parent_id IS NULL THEN 'y' 
                      ELSE 'n' END AS can_store
                      
FROM         dbo.documents LEFT OUTER JOIN
                      dbo.people AS people_problem ON dbo.documents.problem_reported_by_person_id = people_problem.person_id LEFT OUTER JOIN
                      dbo.people AS people_disable ON dbo.documents.disabled_by_person_id = people_disable.person_id LEFT OUTER JOIN
                      dbo.people AS people_file_modify ON dbo.documents.file_modified_by_person_id = people_file_modify.person_id LEFT OUTER JOIN
                      dbo.people AS people_index_modify ON dbo.documents.index_modified_by_person_id = people_index_modify.person_id LEFT OUTER JOIN
                      dbo.people AS people_store ON dbo.documents.stored_by_person_id = people_store.person_id LEFT OUTER JOIN
                      dbo.people AS people_create ON dbo.documents.created_by_person_id = people_create.person_id LEFT OUTER JOIN
                      dbo.profiles ON dbo.documents.profile_id = dbo.profiles.profile_id LEFT OUTER JOIN
                      dbo.people AS people_qc ON dbo.documents.qc_person_id = people_qc.person_id LEFT OUTER JOIN
                      dbo.categories ON dbo.documents.category_id = dbo.categories.category_id 
                      AND dbo.documents.category_id = dbo.categories.category_id 
                      LEFT OUTER JOIN dbo.grants INNER JOIN
                      dbo.appls ON dbo.grants.grant_id = dbo.appls.grant_id ON dbo.documents.appl_id = dbo.appls.appl_id LEFT OUTER JOIN
                      ncieim_b.dbo.documents_text AS dt ON dbo.documents.document_id = dt.document_id LEFT OUTER JOIN
                          (SELECT     document_id, COUNT(*) AS attachment_count
                            FROM          dbo.attachments AS attachments_1
                            GROUP BY document_id) AS attachments ON dbo.documents.document_id = attachments.document_id
WHERE     (dbo.documents.disabled_date IS NULL)



GO


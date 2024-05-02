USE [EIM]
GO

/****** Object:  View [dbo].[egrants]    Script Date: 5/2/2024 8:46:33 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- includes prior approval


ALTER             VIEW [dbo].[egrants]
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
                      WHEN (dbo.documents.category_id in (
						 select (category_id) from categories where (category_name = 'Application File' OR category_name = 'Data Management and Sharing (DMS) Plan'
							OR category_name = 'Prior Approval' OR sub_category_name = 'Past Due Documents Reminder')
							OR sub_category_name like '%DCI-InTh%')
					  ) then 'y'
					  WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND (url LIKE 'data/%' or url LIKE '/data/%') AND people_create.userid != 'system' AND parent_id IS NULL THEN 'y' 
                      WHEN dbo.fn_appl_deleted_by_impac (dbo.appls.appl_id)='n' AND (dbo.documents.category_id IS NULL OR dbo.documents.appl_id IS NULL) AND dbo.documents.qc_date IS NOT NULL THEN 'y'
                      ELSE 'n' 
                      END AS can_modify_index, 
                      
                      CASE 
					  WHEN dbo.documents.sub_category_name = 'JIT Submitted' THEN 'n'
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
                      'JIT Info', 'Financial Report', 'Data Management and Sharing (DMS) Plan') AND parent_id IS NULL THEN 'y' 
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



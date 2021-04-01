SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE VIEW [dbo].[vw_egrants_virtual_docs_funding]
AS
SELECT

3	AS tag, 
2	AS parent,
grant_id		[grant!1!grant_id!element], 
NULL			[grant!1!serial_num!element],
CAST(null as varchar)	[grant!1!admin_phs_org_code!element],
CAST(null as varchar)	[grant!1!former_grant_num!element],   
CAST(null as varchar)    	[grant!1!future_grant_num!element],  
CAST(null as varchar)	[grant!1!arra_flag!element],
CAST(null as varchar)  	[grant!1!paper_file!element],
CAST(null as varchar) 	[grant!1!stop_sign!element],
CAST(null as varchar)   	[grant!1!org_name!element],                                       
CAST(null as varchar)  	[grant!1!pi_name!element],              
CAST(null as varchar) 	[grant!1!project_title!element],   
CAST(null as varchar)	[grant!1!award_package!element],
CAST(null as varchar)	[grant!1!application_package!element],
CAST(null as varchar) 	[grant!1!correspondence_package!element],
CAST(null as varchar) 	[grant!1!closeout_package!element],
CAST(null as varchar) 	[grant!1!prog_class_code!element],
CAST(null as varchar) 	[grant!1!is_tobacco!element],
CAST(null as varchar)  	[grant!1!pi_report!element],
                   
appl_id			[appl!2!appl_id!element],
CAST(null as varchar)	[appl!2!full_grant_num!element],
CAST(null as varchar)	[appl!2!support_year!element],
CAST(null as varchar)	[appl!2!project_title!element],
null			[appl!2!person_id!element],
CAST(null as varchar)	[appl!2!pi_name!element],
null			[appl!2!external_org_id!element],CAST(null as varchar)	[appl!2!org_name!element],
CAST(null as varchar)	[appl!2!competing!element],
null            		[appl!2!fsr_count!element], 
null			[appl!2!frc_destroyed!element], 
CAST(null as varchar)	[appl!2!can_add_funding!element], 
null			[doc!3!document_id!element],
null			[doc!3!document_date!element],
'Funding'		[doc!3!category_name!element],
37			[doc!3!category_id!element],
created_by		[doc!3!created_by!element],
created_date		[doc!3!created_date!element],
CAST(null as varchar)	[doc!3!modified_by!element],
CAST(null as datetime)	[doc!3!modified_date!element],
CAST(null as varchar)	[doc!3!file_modified_by!element],
CAST(null as datetime)	[doc!3!file_modified_date!element],
CAST(null as varchar)	[doc!3!problem_msg!element],
CAST(null as varchar)	[doc!3!problem_reported_by!element],
null			[doc!3!page_count!element],
null			[doc!3!attachment_count!element],
CAST(null as varchar)	[doc!3!description!element],
url			[doc!3!url!element],
CAST(null as varchar)	[doc!3!file_type!element],
CAST(null as datetime)	[doc!3!qc_date!element],
CAST(null as varchar)	[doc!3!qc_reason!element],
null			[doc!3!qc_person_id!element],
CAST(null as varchar)	[doc!3!qc_person_name!element],
null			[doc!3!doc_rank!element],
CAST(null as varchar)	[doc!3!uploadid!element],
'n'			[doc!3!can_upload!element],
'n'			[doc!3!can_modify_index!element],
'n'			[doc!3!can_delete!element],
'n'			[doc!3!can_restore!element],
'n'			[doc!3!can_store!element],
CAST(null as varchar)	[doc!3!ic!element]

FROM vw_funding






GO


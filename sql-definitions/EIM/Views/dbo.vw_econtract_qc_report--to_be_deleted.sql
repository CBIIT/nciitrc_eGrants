SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE view [dbo].[vw_econtract_qc_report--to_be_deleted] 

as

SELECT	ed.contract_id, ed.document_id, ed.full_contract_number, ed.color_sequence, ed.uploaded_flag, ed.qc_flag, ed.stored_flag, 
		ed.color_name, ed.tab_sequence, ed.tab_name, ed.seq_no, ed.image_url,ed.original_url, ed.file_type,
		er.transaction_id, er.action, er.transaction_date as error_date, er.person_name, ed.uploaded_date,er.description, er.email
FROM	vw_econtract_documents ed left outer join 
		(select et.*, pp.person_name, et.operator + '@mail.nih.gov' as email from vw_econtract_doc_transactions et
		inner join econtract_documents ed on et.document_id = ed.document_id
		inner join people pp on et.operator = pp.userid
		where et.action_type = 'error' and ed.active_flag=1 and ed.stored_flag=0) er on ed.document_id = er.document_id
WHERE	stored_flag = 0 and uploaded_flag = 1



GO


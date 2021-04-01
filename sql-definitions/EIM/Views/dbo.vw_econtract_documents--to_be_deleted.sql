SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE view [dbo].[vw_econtract_documents--to_be_deleted] 

as

SELECT ec.full_contract_number, ec.contract_number, ec.contract_id, ed.document_id, ed.file_type, ed.active_flag, et.tab_name AS tab,
		CASE
		WHEN ed.uploaded_flag = 1 THEN
			CASE 
			WHEN (fm.transaction_date is null or rs.transaction_date > fm.transaction_date) THEN  up.description
			ELSE  fm.description
			END 
		ELSE ''
		END
		AS image_url, 
		CASE
		WHEN ed.uploaded_flag = 1 THEN
			CASE 
			WHEN (fm.transaction_date is null or rs.transaction_date > fm.transaction_date) THEN  'main'
			ELSE  'modify'
			END 
		ELSE ''
		END
		AS url_loc, 
	ed.uploaded_flag, ed.qc_flag, ed.stored_flag, et.color_id, c.color_name, c.color_sequence, 
	ed.seq_no, ed.page_count, ed.tab_id, tdc.tab_doc_count,
	CASE 
	WHEN et.allow_seq_no=0 THEN  et.tab_name
	ELSE  et.tab_name + '_' + convert(varchar, ed.seq_no)
	END  as tab_name, et.tab_sequence, 
   	cr.transaction_date as created_date, cr.operator as created_by, 
	da.transaction_date as disabled_date, da.operator as disabled_by, 
	st.transaction_date as stored_date, st.operator as stored_by, 
	er.transaction_date as error_reported_date, er.operator as error_reported_by, er.description as error_description,
	im.transaction_date as index_modified_date, im.operator as index_modified_by, 
	fm.transaction_date as file_modified_date, fm.operator as file_modified_by, fm.description as modified_url,
	qc.transaction_date as qc_date, qc.operator as qc_by, qc.description as qc_description,
	up.transaction_date as uploaded_date, up.operator as uploaded_modified_by, up.description as original_url,
	mg.transaction_date as merge_date, mg.operator as merge_modified_by, mg.description as merge_description,
	rs.transaction_date as restore_date, rs.operator as restore_modified_by
FROM   vw_contracts ec LEFT OUTER JOIN 
	econtract_documents ed on ec.contract_id = ed.contract_id INNER JOIN 
   	econtract_tabs et on ed.tab_id = et.tab_id INNER JOIN 
	vw_econtract_tabs tdc on ec.contract_id = tdc.contract_id and ed.tab_id = tdc.tab_id INNER JOIN 
	econtract_colors c on et.color_id = c.color_id LEFT OUTER JOIN 
	(SELECT document_id, transaction_date, operator from vw_econtract_transactions
	WHERE action_type = 'created' ) cr on ed.document_id = cr.document_id LEFT OUTER JOIN 
	(SELECT document_id, transaction_date, operator from vw_econtract_transactions
	WHERE action_type = 'disabled' ) da on ed.document_id = da.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator from vw_econtract_transactions 
	WHERE action_type = 'stored' ) st on ed.document_id = st.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from vw_econtract_transactions 
	WHERE action_type = 'error' ) er on ed.document_id = er.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator from vw_econtract_transactions 
	WHERE action_type = 'index_modified' ) im on ed.document_id = im.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from dbo.vw_econtract_transactions
	WHERE action_type = 'file_modified' ) fm on ed.document_id = fm.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from vw_econtract_transactions
	WHERE action_type = 'qc' ) qc on ed.document_id = qc.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from vw_econtract_transactions
	WHERE action_type = 'uploaded' ) up on ed.document_id = up.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from vw_econtract_transactions
	WHERE action_type = 'restore' ) rs on ed.document_id = rs.document_id LEFT OUTER JOIN
	(SELECT document_id, transaction_date, operator, description from vw_econtract_transactions
	WHERE action_type = 'merge' ) mg on ed.document_id = mg.document_id
	WHERE  active_flag = 1




GO


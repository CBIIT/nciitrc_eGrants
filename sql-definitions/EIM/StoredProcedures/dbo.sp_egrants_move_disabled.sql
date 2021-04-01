SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE sp_egrants_move_disabled 
AS
BEGIN


insert documents_disabled
(
document_id, url, page_count, appl_id, stamp, category_id, document_size, problem_msg, file_type, profile_id, corrupted, alias, inventoried, qc_reason,
document_date, created_date, modified_date, added_date, stored_date, file_modified_date, qc_date, disabled_date, qc_person_id, created_by_person_id,
stored_by_person_id, index_modified_by_person_id, file_modified_by_person_id, disabled_by_person_id, nga_id,external_upload_id, status_id,
uid, aws_id, impp_doc_id
)

select 
document_id, url, page_count, appl_id, stamp, category_id, document_size, problem_msg, file_type, profile_id, corrupted, alias, inventoried, qc_reason,
document_date, created_date, modified_date, added_date, stored_date, file_modified_date, qc_date, disabled_date, qc_person_id, created_by_person_id,
stored_by_person_id, index_modified_by_person_id, file_modified_by_person_id, disabled_by_person_id, nga_id,external_upload_id, status_id,
uid, aws_id, impp_doc_id
from documents where disabled_date is not null

delete documents where disabled_date is not null


END

GO


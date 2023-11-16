

-- select * from documents where category_id in (select category_id from categories where impac_doc_type_code = 'DMS')

delete from documents where category_id in (select category_id from categories where impac_doc_type_code = 'DMS')


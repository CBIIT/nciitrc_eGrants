SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE VIEW [dbo].[vw_funding_master]
AS
SELECT DISTINCT 
parent_category_name, category_name, category_id, level_id, parent_id, grand_parent_id, category_fy, 
CASE category_id 
WHEN (3)  THEN CONVERT(varchar, document_date, 101) + ' ' + ISNULL(rfa_pa_number, '') 
WHEN (28) THEN ISNULL(rfa_pa_number, '') 
WHEN (30) THEN ISNULL(rfa_pa_number, '') 
WHEN (31) THEN ISNULL(rfa_pa_number, '') 
WHEN (42) THEN ISNULL(rfa_pa_number, '') 
ELSE CONVERT(varchar, document_date, 101) 
END AS doc_label, 
url,document_id, created_date, NULL AS arra_flag, NULL AS serial_num, document_fy
FROM vw_funding_docs
WHERE level_id > 1 Or category_id = 3 
UNION
SELECT DISTINCT 
parent_category_name, category_name, category_id, level_id, parent_id, grand_parent_id, category_fy, full_grant_num AS doc_label, url, document_id,
created_date, arra_flag, serial_num, document_fy
FROM  vw_funding
WHERE level_id = 1 AND category_id <> 3



GO


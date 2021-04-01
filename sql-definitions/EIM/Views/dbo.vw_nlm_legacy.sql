SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE VIEW vw_nlm_legacy
AS
SELECT n.*, pi_name, admin_phs_org_code, grant_num,
CASE category_name WHEN 'Grant File' THEN 'MISC' ELSE category_name END AS category_name
FROM documents_nlm_legacy n INNER JOIN vw_grants g ON g.grant_id=n.grant_id INNER JOIN categories c ON c.category_id=n.category_id




GO


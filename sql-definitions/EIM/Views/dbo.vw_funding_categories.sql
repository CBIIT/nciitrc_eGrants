SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE VIEW dbo.vw_funding_categories
AS
SELECT f.category_id,f.parent_id,f.level_id,f.category_fy,f.category_name,f.grand_parent_id,
	parent.category_id AS parent_category_id,parent.category_name AS parent_category_name,  
	grand_parent.category_id AS grand_parent_category_id,grand_parent.category_name AS grand_parent_category_name
FROM funding_categories f LEFT OUTER JOIN funding_categories AS parent ON f.parent_id = parent.category_id 
LEFT OUTER JOIN funding_categories AS grand_parent ON f.grand_parent_id = grand_parent.category_id 




GO


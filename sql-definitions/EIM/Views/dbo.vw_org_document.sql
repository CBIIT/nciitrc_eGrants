SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE VIEW [dbo].[vw_org_document]
AS
SELECT     dbo.Org_Document.document_id, dbo.Org_Categories.doctype_name AS category_name, dbo.Org_Document.doctype_id AS category_id, dbo.Org_Master.Org_name, 
                      dbo.Org_Document.org_id, CONVERT(varchar, dbo.Org_Document.created_date, 101) AS created_date, dbo.Org_Document.start_date_ShowFlag, CONVERT(varchar, dbo.Org_Document.start_date_ShowFlag, 
                      101) AS start_date, dbo.Org_Document.end_date_showFlag, CONVERT(varchar, dbo.Org_Document.end_date_showFlag, 101) AS end_date, dbo.Org_Categories.tobe_flagged, dbo.Org_Document.file_type, 
                      dbo.Org_Document.url,dbo.Org_Document.created_by_person_id,dbo.people.person_name AS created_by
FROM         dbo.Org_Document INNER JOIN
                      dbo.Org_Categories ON dbo.Org_Document.doctype_id = dbo.Org_Categories.doctype_id INNER JOIN 
                      dbo.people ON dbo.Org_Document.created_by_person_id = dbo.people.person_id LEFT OUTER JOIN
                      dbo.Org_Master ON dbo.Org_Document.org_id = dbo.Org_Master.org_id
WHERE     (dbo.Org_Document.disabled_date IS NULL)



GO


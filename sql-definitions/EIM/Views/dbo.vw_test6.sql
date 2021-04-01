SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_test6
AS
SELECT     dbo.documents.document_id, dbo.documents.appl_id, dbo.documents.created_date, dbo.documents.modified_date, dbo.documents.added_date, 
                      dbo.documents.profile_id, dbo.documents.category_id, dbo.documents.created_by_person_id, dbo.documents.stored_by_person_id, url
FROM         dbo.vw_appls RIGHT OUTER JOIN
                      dbo.documents ON dbo.vw_appls.appl_id = dbo.documents.appl_id
WHERE     (dbo.documents.disabled_date IS NULL) AND (dbo.documents.appl_id IS NOT NULL) AND (dbo.vw_appls.appl_id IS NULL)


GO


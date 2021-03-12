SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE VIEW [dbo].[vw_funding_docs]
AS

SELECT     dbo.funding_documents.document_id, dbo.funding_documents.document_date, dbo.funding_documents.created_date, dbo.funding_documents.sub_string,
                      dbo.people.person_name AS created_by, dbo.funding_documents.created_by_person_id, dbo.funding_documents.document_fy, 
                      dbo.funding_documents.stored_date, dbo.funding_documents.stored_by_person_id, dbo.funding_categories.category_id, 
                      dbo.funding_categories.level_id, dbo.funding_categories.category_fy, dbo.funding_categories.category_name, dbo.funding_documents.sub_category,
                      parent.category_name AS parent_category_name, dbo.funding_categories.parent_id, dbo.funding_categories.grand_parent_id, 
                      /*Imran : PIV Migration change 6/7/2014*/
                      --'https://egrants-data.nci.nih.gov/funded/nci/funding/upload/' + RIGHT('00000' + CONVERT(varchar, dbo.funding_documents.document_id), 6) 
                      '/data/funded/nci/funding/upload/' + RIGHT('00000' + CONVERT(varchar, dbo.funding_documents.document_id), 6) 
                      + '.pdf' AS url, dbo.fn_funding_RFA(dbo.funding_documents.document_id) AS rfa_pa_number, 
                      dbo.fn_funding_RFA_count(dbo.funding_documents.document_id) AS rfa_pa_number_count  
FROM         dbo.funding_documents INNER JOIN
                      dbo.people ON dbo.funding_documents.created_by_person_id = dbo.people.person_id INNER JOIN
                      dbo.funding_categories ON dbo.funding_documents.category_id = dbo.funding_categories.category_id LEFT OUTER JOIN
                      dbo.funding_categories AS parent ON dbo.funding_categories.parent_id = parent.category_id INNER JOIN 
                      (select distinct document_id from funding_appls where disabled_date is null) fa
						on dbo.funding_documents.document_id =fa.document_id






GO


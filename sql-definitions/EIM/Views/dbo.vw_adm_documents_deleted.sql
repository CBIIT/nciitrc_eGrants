SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON







create VIEW [dbo].[vw_adm_documents_deleted]
AS
select [file_id], [file_name], url, CONVERT(varchar, dbo.adm_files.created_date, 101) AS created_date, CONVERT(varchar, disabled_date, 101) AS disabled_date,
		created_by_person_id, disabled_by_person_id,p1.person_name AS created_by,p2.person_name AS disabled_by
from dbo.adm_files INNER JOIN dbo.people p1 ON created_by_person_id = p1.person_id INNER JOIN dbo.people p2 ON disabled_by_person_id = p2.person_id 
where disabled_date IS NOT NULL







GO


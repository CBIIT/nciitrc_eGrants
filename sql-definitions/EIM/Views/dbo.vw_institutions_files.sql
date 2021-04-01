SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE VIEW [dbo].[vw_institutions_files]
AS
SELECT     dbo.institutions_files.ins_id, dbo.institutions_index.ins_index, dbo.institutions_index.index_seq, dbo.institutions_files.file_type, 
					/*Imran : PIV Migration change 6/7/2014*/	
                      --'https://egrants-data.nci.nih.gov/funded/nci/institutional/' + CONVERT(varchar, dbo.institutions_files.ins_id) + '.' + dbo.institutions_files.file_type AS url, 
                      '/data/funded/nci/institutional/' + CONVERT(varchar, dbo.institutions_files.ins_id) + '.' + dbo.institutions_files.file_type AS url, 
                      dbo.institutions_files.index_id, dbo.institutions_files.institution_name, dbo.institutions_files.created_by_person_id, dbo.institutions_files.created_date, 
                      dbo.institutions_files.modified_date, dbo.institutions_files.modified_by_person_id
FROM         dbo.institutions_files INNER JOIN
                      dbo.institutions_index ON dbo.institutions_files.index_id = dbo.institutions_index.index_id
WHERE     (dbo.institutions_files.disabled_date IS NULL)



GO


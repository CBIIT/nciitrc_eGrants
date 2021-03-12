SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_script_arra]
AS
SELECT     'cp ' + unix_file + ' /egrants/scripts/arra_files/' + CONVERT(varchar, appl_id) 
                      + '_' + CASE category_name WHEN 'Revised Abstract' THEN 'abstract' WHEN 'Revised Specific Aims' THEN 'aims' WHEN 'Revised Public Health Relevance'
                       THEN 'relevance' WHEN 'ARRA Award Documents: Modified Scope' THEN 'nibib' END + '_' + REPLACE(CONVERT(varchar, created_date, 
                      101) COLLATE Latin1_General_CI_AS, '/', '') + '.' + file_type AS command, created_date
FROM         dbo.egrants
WHERE     (category_name IN ('Revised Abstract', 'Revised Specific Aims', 'Revised Public Health Relevance', 'ARRA Award Documents: Modified Scope')) AND 
                      (appl_id > 0) AND (fy >= 2009)

GO


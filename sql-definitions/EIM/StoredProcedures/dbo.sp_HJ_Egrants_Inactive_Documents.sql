SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_HJ_Egrants_Inactive_Documents]
AS

SELECT 
SUBSTRING(url,(CHARINDEX('gov', url) + 4), LEN(url)) Filename_with_path, 
appl_id, 
profile_id
FROM [eim].[dbo].[documents]
WHERE disabled_date IS NOT NULL 
AND disabled_by_person_id IS NOT NULL
AND created_by_person_id NOT IN (SELECT person_id FROM people WHERE person_name = userid)
GO


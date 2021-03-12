SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE sp_HJ_Get_Document_List
(@IC int)
AS

BEGIN

SELECT d.profile_id, d.appl_id, d.Document_ID, d.url
FROM documents d
INNER JOIN appls a ON d.appl_id = a. appl_id
INNER JOIN profiles p ON d.profile_id = p.profile_id
WHERE d.profile_id = @IC
AND d.disabled_date IS NOT NULL
AND d.url IS NOT NULL
AND ISNULL(d.is_destroyed,0) <> 1
ORDER BY d.appl_ID

END
GO


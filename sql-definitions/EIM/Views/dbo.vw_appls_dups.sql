SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_appls_dups
AS
SELECT grant_id, appl_type_code, activity_code, support_year, suffix_code
FROM  dbo.appls
GROUP BY grant_id, appl_type_code, activity_code, support_year, suffix_code
HAVING (COUNT(*) > 1)

GO


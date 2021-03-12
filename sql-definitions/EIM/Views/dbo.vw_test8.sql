SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW [dbo].[vw_test8]
AS
SELECT *
FROM  dbo.appls_ciip
WHERE (APPL_ID IN
                   (SELECT DISTINCT appl_id
                    FROM   vw_test6))

GO


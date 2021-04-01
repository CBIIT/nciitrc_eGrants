SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_test
AS
SELECT dbo.appls.appl_id, ISNULL(impac.impac_nga_count, 0) AS impac_nga_count, ISNULL([local].local_nga_count, 0) AS local_nga_count
FROM  (SELECT appl_id, COUNT(document_id) AS local_nga_count
               FROM   egrants
               WHERE category_name IN ('NGA', 'Award File') AND impac_doc = 'n'
               GROUP BY appl_id, impac_doc) [local] RIGHT OUTER JOIN
               dbo.appls ON [local].appl_id = dbo.appls.appl_id LEFT OUTER JOIN
                   (SELECT appl_id, COUNT(document_id) AS impac_nga_count
                    FROM   egrants
                    WHERE category_name IN ('NGA', 'Award File', 'Grant File') AND impac_doc = 'y'
                    GROUP BY appl_id, impac_doc) impac ON dbo.appls.appl_id = impac.appl_id

GO


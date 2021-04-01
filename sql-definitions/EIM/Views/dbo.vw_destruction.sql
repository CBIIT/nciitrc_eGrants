SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_destruction
AS
SELECT     dbo.destruction.grant_id, dbo.egrants.grant_num, dbo.destruction.destruction_reason, MIN(dbo.egrants.grant_close_date) AS grant_close_date, 
                      COUNT(DISTINCT dbo.egrants.appl_id) AS appl_count, COUNT(dbo.egrants.document_id) AS doc_count
FROM         dbo.destruction LEFT OUTER JOIN
                      dbo.egrants ON dbo.destruction.grant_id = dbo.egrants.grant_id AND dbo.egrants.url LIKE 'https://egrants%'
GROUP BY dbo.destruction.grant_id, dbo.egrants.grant_num, dbo.destruction.destruction_reason

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_searches_by_60_months
AS
SELECT     TOP 100 PERCENT YEAR(query_date) AS year, MONTH(query_date) AS month, COUNT(*) AS [Count of Search]
FROM         dbo.queries
GROUP BY YEAR(query_date), MONTH(query_date)
ORDER BY year DESC, month DESC

GO


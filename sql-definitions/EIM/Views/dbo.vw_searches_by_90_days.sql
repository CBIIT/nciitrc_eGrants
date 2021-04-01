SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_searches_by_90_days]
AS

SELECT  TOP 90 PERCENT YEAR(query_date) AS year, MONTH(query_date) AS month, DAY(query_date) AS day, COUNT(*) AS [Count of Search]
FROM dbo.queries
WHERE query_date>=(dateadd(d, -90, Getdate()))
GROUP BY YEAR(query_date), MONTH(query_date), DAY(query_date)
ORDER BY YEAR(query_date) desc, MONTH(query_date) desc, DAY(query_date) desc

/**
SELECT     TOP 100 PERCENT YEAR(query_date) AS year, MONTH(query_date) AS month, DAY(query_date) AS day, COUNT(*) AS [Count of Search]
FROM         dbo.queries
GROUP BY YEAR(query_date), MONTH(query_date), DAY(query_date)
ORDER BY YEAR(query_date) DESC, MONTH(query_date) DESC, DAY(query_date) DESC
**/


GO


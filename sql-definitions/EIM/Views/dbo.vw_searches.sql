SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_searches]
AS

SELECT    s.search_id, q.query_id, s.search_string, q.query_date, q.searched_by AS userid, q.ic, q.execution_time, q.page,q.browser_type,
	      CASE WHEN search_string like 'appl_id:%' and ISNUMERIC(right(search_string,len(search_string)-8))=1
			THEN rtrim(ltrim(right(search_string,len(search_string)-8)))
			ELSE NULL 
		  END as appl_id
FROM	dbo.searches s INNER JOIN dbo.queries q ON s.search_id = q.search_id


----SELECT	s.search_id, q.query_id, s.search_string, q.query_date, q.searched_by AS userid, q.ic, q.execution_time, q.page,q.browser_type,
----		CASE WHEN search_string like 'appl_id:%' THEN right(search_string,len(search_string)-8) ELSE NULL END as appl_id
----FROM	dbo.searches s INNER JOIN dbo.queries q ON s.search_id = q.search_id
----WHERE s.search_string <>'' and s.search_string is not null


GO


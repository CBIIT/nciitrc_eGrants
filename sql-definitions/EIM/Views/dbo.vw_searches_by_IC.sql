SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE VIEW vw_searches_by_IC
AS
select top 27 ic, year(query_date) year, month(query_date) month, count(*) as 'amount of search'
from queries
where ic is not null and ic<>''
group by year(query_date), month(query_date),ic
order by  year desc, month desc, ic asc






GO


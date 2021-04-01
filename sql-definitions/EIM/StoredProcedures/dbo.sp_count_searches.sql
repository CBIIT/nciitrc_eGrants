SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE Procedure sp_count_searches
(
@date1		datetime,
@date2		datetime,
@userid		varchar(50)
)
AS

--execution: sp_count_searches '12/31/2013', '01/01/2015', 'omairi'


BEGIN

DECLARE @totsearches int
DECLARE @indsearches int

--DECLARE @date1 datetime
--DECLARE @date2 datetime
--DECLARE @userid varchar(10)

--SET @date1 = '12/31/2013'
--SET @date2 = '01/01/2015'
--SET @userid = 'qians'

select @totsearches=count(*)
from queries q
left outer join people p on q.searched_by = p.userid
where query_date > @date1
and query_date < @date2
and searched_by <> ''
and p.active <> 0 --640,241

select @indsearches = count(*)
--person_name,
--searched_by,
from queries q
left outer join people p on q.searched_by = p.userid
where 
query_date > @date1
and query_date < @date2 --575
and p.active <> 0 --540
and searched_by = @userid
group by person_name
--, searched_by

select @totsearches as Total_Searches, @indsearches as Individual_Searches

END
GO


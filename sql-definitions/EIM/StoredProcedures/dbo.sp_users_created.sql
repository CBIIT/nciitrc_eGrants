SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE Procedure [dbo].[sp_users_created]
(
@date1 datetime, 
@date2 datetime
)
AS

BEGIN

--Declare @date1 datetime, @date2 datetime

--set @date1 = '1/1/2015'
--set @date2 = getdate()

select created_by, count(*) cnt
into #tmp
from people
where created_date > @date1
and created_date < @date2
group by created_by
order by created_by 

--select * from #tmp

select p.person_name as [Created By], t.cnt as [Count]
from #tmp t inner join people p on t.created_by = p.person_id

Drop table #tmp

END

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE Procedure [dbo].[sp_users_created_detail]
(
@date1 datetime, 
@date2 datetime
)
AS

--exec sp_users_created_detail '1/1/2015', '6/15/2015'

BEGIN

--Declare @date1 datetime, @date2 datetime

--set @date1 = '1/1/2015'
--set @date2 = getdate()

select person_name, userid, position_id, active, created_date, created_by
into #tmp
from people
where created_date > @date1
and created_date < @date2
order by created_by 

--select * from #tmp

select t.person_name [New User Name], t.userid [User ID], 
pp.position_name [Position Title], convert(varchar(10),t.created_date,101) [Date Created], 
t.active as Active, p.person_name [Created By]
from #tmp t inner join people p on t.created_by = p.person_id
inner join people_positions pp on t.position_id = pp.position_id
order by pp.position_name,t.created_date

Drop table #tmp

END

GO


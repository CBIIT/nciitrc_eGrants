SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE PROCEDURE [dbo].[sp_users_date_last_accessed] 
@CutOffDays INT
AS

--sp_users_date_last_accessed 92 --737

Begin

--Drop table #tmp

--Declare @CutOffDays int
--Set @CutOffDays = 30

--SELECT GETDATE()- 30

select a.userid as UserID, a.person_name as person_name, p.position_name as PositionName
--, a.end_date, a.active
into #tmp
from people a inner join people_positions p on a.position_id = p.position_id
where getdate()-@CutOffDays >= (select MAX(query_date) from vw_searches where userid=a.userid)
and a.userid is not null
and end_date is null
and active=1  -- 1100

--select count(*) from #tmp
--select * from #tmp

select t.UserID  as UserID, t.person_name, convert(varchar(16), max(s.query_date), 121) as Date_Last_Accessed--, t.PositionName
from #tmp t inner join vw_searches s on t.userid= s.userid
where t.userid <> ''
group by t.UserID, t.person_name--, t.PositionName

End

GO


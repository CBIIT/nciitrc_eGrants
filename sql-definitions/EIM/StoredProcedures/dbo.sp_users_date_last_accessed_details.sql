SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE PROCEDURE [dbo].[sp_users_date_last_accessed_details]
@CutOffDays INT
AS

--sp_users_date_last_accessed_details 92 --1091

Begin

--Drop table #tmp

--Declare @CutOffDays int
--Set @CutOffDays = 30

--SELECT GETDATE()- 30

select a.userid as UserID, a.person_name as Person_Name, p.position_name as Position, 
convert(varchar(16),a.created_date,121) AS Date_Created, a.active as Active
--, a.end_date, a.active
into #tmp
from people a inner join people_positions p on a.position_id = p.position_id
where getdate()-@CutOffDays >= (select MAX(query_date) from vw_searches where userid=a.userid)
and a.userid is not null
and end_date is null
and active=1  -- 1100

--select count(*) from #tmp
--select * from #tmp

select t.Person_Name, t.UserID, t.Date_Created, t.Active, t.Position, 
convert(varchar(16), max(s.query_date), 121) Date_Last_Accessed
from #tmp t inner join vw_searches s on t.userid= s.userid
where t.userid <> ''
group by t.Person_Name, t.UserID, t.Position, t.Date_Created, t.Active
order by t.userid --1096

--select * from #tmp

END
GO


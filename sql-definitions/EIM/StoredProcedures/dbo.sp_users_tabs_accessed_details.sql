SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE Procedure [dbo].[sp_users_tabs_accessed_details]
As

--sp_users_tabs_accessed_details

Begin

--select * from vw_people
--select * from eim.dbo.people_positions


select Userid, person_name as Person_Name, person_id, CASE WHEN active = 1 THEN 'Active' ELSE 'Disabled' END [Status], 
pp.position_name as Position_title, [start_date] as Date_Created,
CASE WHEN econ =1 THEN 'Yes' ELSE 'No' END DocMan,
CASE WHEN egrants =1 THEN 'Yes' ELSE 'No' END eGrants,
CASE WHEN admin =1 THEN 'Yes' ELSE 'No' END [Admin],
CASE WHEN cft =1 THEN 'Yes' ELSE 'No' END Cft,
CASE WHEN cft =1 THEN 'Yes' ELSE 'No' END Mgt
from vw_people p inner join people_positions pp on p.position_id = pp.position_id
where userid <> ''
and person_name <> ''
order by person_name

End

GO


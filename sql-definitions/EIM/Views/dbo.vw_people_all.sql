SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE view [dbo].[vw_people_all]
AS
SELECT eg.person_id, ep.person_name, ep.userid, ep.nedid, ep.profile_id, pr.profile as ic, pr.admin_phs_org_code,
sum(eg.CFT) 'cft', sum(eg.eCon) 'econ', sum(eg.eGrants) 'egrants', 
sum(eg.gft) 'gft', sum(eg.mgt) 'mgt'
FROM eim.dbo.vw_people_all_staging eg
INNER JOIN eim.dbo.egrants_people ep ON eg.person_id = ep.person_id
INNER JOIN eim.dbo.profiles pr on ep.profile_id = pr.profile_id
WHERE ep.end_date IS NULL
GROUP BY eg.person_id, ep.person_name, ep.userid, ep.nedid, ep.profile_id, pr.profile, pr.admin_phs_org_code
GO


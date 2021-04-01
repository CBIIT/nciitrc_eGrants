SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- Get the list of competing years


Create view CompetingFY_Stage1 as 
Select a.appl_id, cyainfo.appl_id cy_appl_id, a.grant_id, (cyainfo.competingFy - a.fy) as diff
from APPLS a inner join 
(SELECT a.grant_id, a.fy as CompetingFy, a.appl_id --, a.appl_type_code --, a.suffix_code
FROM APPLS A 
WHERE (A.suffix_code IS NULL OR A.suffix_code LIKE '%A%')
and a.appl_type_code IN ('1', '2', '6', '9')
and not a.fy is null
--order by grant_id
) cyainfo

on a.grant_id = cyainfo.grant_id
--group by a.appl_id, cyainfo.appl_id, a.grant_id

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_spore_impac]
AS


select vw_appls.appl_id, full_grant_num from vw_appls INNER JOIN

(
select distinct d.appl_id, created_by_person_id from documents d INNER JOIN 

(
select appl_id from vw_appls
where 

(admin_phs_org_code = 'CA') AND (prog_class_code LIKE '%OS%') AND
(fy >= 2004) AND (appl_type_code IN (1, 2, 3, 5)) AND 
(appl_status_group_descrip = 'awarded')


and appl_id NOT IN
(select appl_id from spore)
) app

ON d.appl_id=app.appl_id
WHERE category_id=38
) t

ON vw_appls.appl_id=t.appl_id and t.created_by_person_id=530




/*
SELECT DISTINCT appl_id,full_grant_num
FROM         dbo.egrants
WHERE     (admin_phs_org_code = 'CA') AND (prog_class_code LIKE '%OS%') AND (fy >= 2004) AND (category_name = 'Application File') AND 
                      (created_by = 'impac') AND (appl_type_code IN (1, 2, 3, 5)) AND (appl_status_group_descrip = 'awarded')

and appl_id NOT IN
(select appl_id from spore)
*/



GO


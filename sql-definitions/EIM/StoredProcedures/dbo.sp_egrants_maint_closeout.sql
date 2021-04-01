SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_maint_closeout] AS

-- The destroyed flags of appl_id are in-consistent 
-- with the same grant_id in e-grant.

select t.grant_id, df_cnt, app_cnt, appl_id, frc_destroyed from 
( -- show all grant with appls <> frc_destroyed counts
select d.grant_id, df_cnt, app_cnt  from 
( -- show frc_destroyed counted by grant_id
select grant_id, count(frc_destroyed) df_cnt
from appls
where frc_destroyed is not null
group by frc_destroyed, grant_id
) d inner join 
( -- show appls counted by grant_id
select grant_id, count(grant_id) app_cnt
from appls
group by grant_id
) a on d.grant_id = a.grant_id
where df_cnt <> app_cnt
) f inner join appls t on f.grant_id = t.grant_id  
order by t.grant_id

-- The destroyed flags of appl_id in e-grant 
-- are in-consistent with grant_close_date at impac 

select grant_id, a.appl_id, frc_destroyed, i.appl_id, a.grant_close_date
FROM openquery(IRDB, '
select appl_id, to_char(grant_close_date, ''yyyymmdd'') as grant_close_date
from gm_closeouts 
where grant_close_date is not null
and to_char(grant_close_date, ''yyyymmdd'') < to_char(sysdate, ''yyyy'') -6 || 1001
') i right join vw_appls a on i.appl_id = a.appl_id
where (frc_destroyed is not null or a.grant_close_date is not null)
and not (frc_destroyed is not null and a.grant_close_date is not null)
-- and grant_id = 2995
order by grant_id

-- check duplicate grant_close_date by grant_id in impac
-- more than two group grant_id with different grant_close_date
select grant_id, count(grant_id) as g_dup from 
(
select grant_id, count(grant_id) as c_gdt, i.grant_close_date
FROM openquery(IRDB, '
select appl_id, to_char(grant_close_date, ''yyyymmdd'') as grant_close_date
from gm_closeouts 
where grant_close_date is not null
and to_char(grant_close_date, ''yyyymmdd'') < to_char(sysdate, ''yyyy'') -6 || 1001
') i left join vw_appls a on i.appl_id = a.appl_id
group by i.grant_close_date, grant_id
having count(grant_id) > 1
) g 
group by grant_id
having count(grant_id) > 1
order by grant_id


-- exec sp_egrants_maint_closeout


GO


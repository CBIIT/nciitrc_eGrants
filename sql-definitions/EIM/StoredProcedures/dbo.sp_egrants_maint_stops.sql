SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE PROCEDURE [dbo].[sp_egrants_maint_stops] AS
BEGIN
UPDATE grants SET stop_sign='no' WHERE stop_sign='yes'

-------------------------------------
--IMRAN : CHANGED FROM IMPACII TO IRDB
-------------------------------------
--FROM openquery(IMPAC1, 'select  *  from gm_closeouts where ltr1_date>=''1998-04-01'' and closeout_status_code=''O'' and (final_report_date is null or final_invention_stmnt_code=''N'' or final_invention_stmnt_code is null)' ) c, vw_appls a 

----------------------------------------------
--  Joel 06/06/2013 for IRDB date comparison
----------------------------------------------
--FROM openquery(IRDB, 'select  *  from gm_closeouts_MV where ltr1_date>=''1998-04-01'' and closeout_status_code=''O'' and (final_report_date is null or final_invention_stmnt_code=''N'' or final_invention_stmnt_code is null)' ) c, vw_appls a 
/*
-- bshell 10-10-2020 disabled due to retired link to gm_closeout
UPDATE grants SET stop_sign='yes' 
WHERE grant_id in (
SELECT grant_id 
FROM openquery(IRDB, 'select * from gm_closeouts_MV 
where to_date(to_char(ltr1_date,''yyyy-mm-dd''),''yyyy-mm-dd'')>=to_date(''1998-04-01'',''yyyy-mm-dd'')
and closeout_status_code=''O'' 
and (final_report_date is null or final_invention_stmnt_code=''N'' or final_invention_stmnt_code is null)' ) c, 
vw_appls a 
WHERE a.appl_id=c.appl_id 
and a.admin_phs_org_code='CA' 
and getdate()>dateadd(DAY,90,c.ltr1_date) 
)
*/
------------------------
--exceptions:

-------------------------------------
--IMRAN : CHANGED FROM IMPACII TO IRDB
-------------------------------------
/*
-- bshell 10-10-2020 disabled due to retired link to gm_closeout
 UPDATE grants SET stop_sign='no' 
--FROM grants g,appls a, openquery(IMPAC, 'select appl_id from gm_closeouts where final_report_date is not null') c
FROM grants g,appls a, openquery(IRDB, 'select appl_id from gm_closeouts_MV where final_report_date is not null') c
WHERE g.grant_id=a.grant_id and a.appl_id=c.appl_id and activity_code in ('R13','R25', 'T32', 'S15') and stop_sign='yes'
*/

--extend stop notice to grants withthe same PI

UPDATE grants 
SET stop_sign='yes' 
WHERE grant_id in (
SELECT distinct g1.grant_id 
FROM vw_grants g1,vw_grants g2
WHERE g2.stop_sign='yes' and g1.stop_sign='no' and g1.admin_phs_org_code IN ('CA','ES') 
and replace(g1.pi_name,'.','')=replace(g2.pi_name,'.','') 
and (g1.active_grant_flag='Y' or g1.fy>=(YEAR(getdate())-2))
and g1.grant_id !=1094636) --added by Hareesh on 11/29/2012 4:44 pm
--end


--added by hareeshj on 3/26/2014 at 5:27pm
update grants
set stop_sign = 'no'
where grant_id = 1187995


--added by hareeshj on 5/5/2015 at 4:35pm
update grants
set stop_sign = 'yes'
where grant_id in (
859157,
93664,
1101896,
1212044,
1212052,
1220711,
1229574,
1243892,
1275426
)
and stop_sign = 'yes'


END

GO


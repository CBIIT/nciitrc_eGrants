SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_egrants_maint_grant_flags_set_up_testing]

/****************************************************************************************************/
/***									 													***/
/***	Procedure Name: sp_egrants_maint_grant_flags_set_up									***/
/***	Description:	set up to_be_destoried and  is_tobacco flags for grant				***/
/***	Created:	06/21/2007	Jimmy, Leon													***/
/***	Modified:	06/25/2007	Leon														***/
/***	Modified:	05/11/2011	Hareesh 													***/
/***	Modified:	05/12/2011	Hareesh 													***/
/***	Modified:	06/12/2015	Hareesh: Commented Tobacco flag setup						***/
/****************************************************************************************************/

AS

declare @appls  table  (grant_id int, appl_id int, grant_close_date smalldatetime)

/*
--Added by hareeshj on 05/11/2011 4:52 pm to update tobacco grants based on vw_tobacco_grants 
--Update is_tobacco =0 in grants table
UPDATE grants
SET is_tobacco =0 
WHERE is_tobacco = 1


--Update is_tobacco =1 in grants table based on view vw_tobacco_grants
UPDATE grants
SET is_tobacco =1 
WHERE grant_id IN (
SELECT DISTINCT a.grant_id 
FROM eim.dbo.vw_tobacco_grants t LEFT OUTER JOIN eim.dbo.appls a ON a.appl_id = t.appl_id)
*/


/*--Commented out by hareeshj on 05/12/2011, 5:11 pm so that above peice of code could be used
--set up is_tobacco flag [changed cay_code from TR to TC on 1/19/2011 --hareesh]
update grants set is_tobacco =1 where ISNULL(is_tobacco,0)=0 and grant_id in(
select distinct grant_id from 
OPENQUERY(CIIP, 'select distinct appl_id from nci_appl_vw where cay_code = ''TC''and admin_phs_org_code = ''CA'' and appl_id>''1'' ') c
LEFT OUTER JOIN appls a ON a.appl_id=c.appl_id)
*/

--set up grant_close_date fro appl level
--IMPAC1 changed by hareesh on 3/7/13 11:00pm


-- 10-07-2020 -  bshell 
insert @appls (grant_id, appl_id, grant_close_date)
select grant_id, a.appl_id, i.grant_close_date
from (select  appl_id,
CAST(SUBSTRING(grant_close_date,1,23) as smalldatetime) as grant_close_date
from BKP_gm_closeouts_20201002 
where not grant_close_date is null
and cast(substring(grant_close_date,1,4) as int) >= 1900 
and cast(substring(grant_close_date,1,4) as int) < 2079 
) i
inner join appls a on i.appl_id = a.appl_id

select * from @appls

--set up max grant_close_date fro grant level
update grants set grant_close_date =a.grant_close_date
from grants g,(select grant_id,max(grant_close_date) as grant_close_date from @appls where grant_close_date is not null  group by grant_id) a
where g.grant_id=a.grant_id

--seu up to_be_destroyed flag for grant
update grants set to_be_destroyed=1
where grant_close_date is not null and grant_close_date<'10/01/'+convert(varchar,(YEAR(getdate())-6))

---add grants that have been FRC-destroyed

/*
UPDATE grants
SET to_be_destroyed=1, destruction_reason='Destroyed at FRC'
FROM grants g INNER JOIN vw_folder_appls f
ON g.grant_id=f.grant_id_appl
WHERE frc_destroyed_date is not null and to_be_destroyed=0 and
is_tobacco=0
*/

-------------------------------


GO


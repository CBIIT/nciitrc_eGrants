SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_egrants_maint_closeout_report_testing] 
AS

/*****************************************************************************************************************
Procedure Name:		sp_egrants_maint_closeout_report
Description:		generate grants close out report
Created:			Dan													
Modified:			On 03/31/2011 5:22 pm, Hareesh modified openquery linked server from impp1 to in Insert 
					Block of code
Modified:			On 04/01/2011 11:45 am, Hareesh commented out "is_closed_out" in 
					"detail multiple grant close date" and "detail grant close date" because "is_closed_out" column 
					does not exist in grants table of eim db. My investigations have uncovered that "to_be_destroyed" 
					column in grants table was earlier referred to as "is_closed_out" and someone might have renamed 
					it for reasons known to them
******************************************************************************************************************/

-- download grant close date to @appls
drop table #appls 
create table #appls  (grant_id int, appl_id int, grant_close_date smalldatetime)


--10-07-2020 bshell - Updated to point to a local backup of remote data to retiring link to GM_Closeout
insert #appls (grant_id, appl_id, grant_close_date)
select grant_id, a.appl_id, i.grant_close_date
from (select  appl_id,
CAST(SUBSTRING(grant_close_date,1,23) as smalldatetime) as grant_close_date
from BKP_gm_closeouts_20201002 
where not grant_close_date is null
and cast(substring(grant_close_date,1,4) as int) >= 1900 
and cast(substring(grant_close_date,1,4) as int) < 2079 
) i
inner join appls a on i.appl_id = a.appl_id


-- total grants over 6 years with lastest grant_close_date (4553)
select grant_id, max(grant_close_date) as grant_close_date from #appls
where grant_close_date is not null and grant_close_date<'10/1/2001'
group by grant_id


-- multiple grant close date
select grant_id, count(grant_id) cnt
from (
select distinct a.grant_id, a.grant_close_date
from #appls a, grants g
where a.grant_id = g.grant_id and a.grant_close_date is not null and a.grant_close_date<'10/1/2001') t
group by grant_id having count(grant_id) > 1


-- detail multiple grant close date
select * from
(
   select grant_id, appl_id, grant_close_date from #appls
   where grant_close_date<'10/1/2001'
   union
   select grant_id, appl_id, null as grant_close_date from appls
   where grant_id in (select grant_id from grants where to_be_destroyed=1) --where is_closed_out=1) (earlier code)
   and appl_id not in (select appl_id from #appls)
   ) t where grant_id in (
        select grant_id from (
           select grant_id, count(grant_id) as count
           from (
              select distinct a.grant_id, a.grant_close_date
              from #appls a, grants g
              where a.grant_id = g.grant_id
              and a.grant_close_date is not null
              and a.grant_close_date<'10/1/2001'
           ) t
           group by grant_id
           having count(grant_id) > 1
       ) m 
     ) 
order by grant_id

-- detail grant close date
select grant_id, grant_close_date, to_be_destroyed as is_closed_out--, is_closed_out (earlier code)
from grants										 
where to_be_destroyed=1 --where is_closed_out=1 (earlier code)
order by grant_close_date desc

GO


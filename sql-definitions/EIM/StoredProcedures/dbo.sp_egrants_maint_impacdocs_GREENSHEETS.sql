SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE    PROCEDURE [dbo].[sp_egrants_maint_impacdocs_GREENSHEETS]

AS

DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'

-------------------------------------------------------------------------------------------------------
/****** BUILDING GREEN SHEET DATA FROM NCI/CIIP SCHEMA**/

--NOTE  There can be multiple suffixes.  In those cases take the last suffix from the end of the string.  
--Before running the stored_procedure export the matching criteria so that we can follow the information .  

Truncate table dbo.GreenSheet_data

Insert dbo.GreenSheet_data(appl_id, PGM_FORM_STATUS, PGM_FORM_SUBMITTED_DATE, SPEC_FORM_STATUS, SPEC_FORM_SUBMITTED_DATE, APPL_TYPE_CODE, SUFFIX_CODE, ACTION_TYPE, GPMATS_CANCELLED_FLAG, GPMATS_CLOSED_FLAG, DUMMY_FLAG, MULTIYEAR_AWARD_FLAG, CURRENT_ACTION_STATUS_DESC )
Select appl_id, PGM_FORM_STATUS, PGM_FORM_SUBMITTED_DATE, SPEC_FORM_STATUS, SPEC_FORM_SUBMITTED_DATE, APPL_TYPE_CODE, SUFFIX_CODE, ACTION_TYPE, GPMATS_CANCELLED_FLAG, GPMATS_CLOSED_FLAG, DUMMY_FLAG, MULTIYEAR_AWARD_FLAG, CURRENT_ACTION_STATUS_DESC  
FROM OPENQUERY(CIIP, 'select g.appl_id, g.PGM_FORM_STATUS, g.PGM_FORM_SUBMITTED_DATE, g.SPEC_FORM_STATUS, g.SPEC_FORM_SUBMITTED_DATE, g.APPL_TYPE_CODE, g.SUFFIX_CODE, gav.action_type,  g.GPMATS_CANCELLED_FLAG, g.GPMATS_CLOSED_FLAG, g.DUMMY_FLAG, gav.MULTIYEAR_AWARD_FLAG, gav.CURRENT_ACTION_STATUS_DESC  
					  from form_grant_vw g 
					  inner join GM_ACTION_QUEUE_VW gav on gav.appl_id = g.appl_id and gav.action_type = ''AWARD'' and g.DUMMY_FLAG <> ''Y''')

--Greensheet_data Table has so many appls that are not in vw_appls. so delete them
delete greensheet_data where appl_id not in (select appl_id from vw_appls)

--There are so many historical data in GS. Delete those who has been deleted/purged
delete greensheet_data where appl_id in (select appl_id from vw_appls where frc_destroyed =1)--1239

--Delete non NCI data
delete greensheet_data where appl_id in (select appl_id from vw_appls where admin_phs_org_code<>'CA')--19

UPDATE documents SET disabled_by_person_id=1899, disabled_date=getdate()
WHERE document_id in (
SELECT document_id FROM egrants WHERE category_id in (73,74,612) and (admin_phs_org_code <>'ca')
)
------------------------------------------
---Update date for green sheets
---Once a GS has been submitted we get submitted date in greensheet_date table soto update it

update documents set documents.document_date = x.pgm_form_submitted_date
from GreenSheet_data x where documents.appl_id=x.appl_id
and convert(date,isnull(documents.document_date,''))<>convert(date,isnull(x.pgm_form_submitted_date,''))
and documents.category_id=73
print 'Total PGM GreenSheet date updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

update documents set documents.document_date = x.spec_form_submitted_date
from GreenSheet_data x where documents.appl_id=x.appl_id
and convert(date,isnull(documents.document_date,''))<>convert(date,isnull(x.spec_form_submitted_date,''))
and documents.category_id=74
print 'Total SPEC GreenSheet date updated =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


-------------------------------------------------------------------------------------------------------
---Add new Greensheet PGM 
---The links to *Program and Specialist *gs are displayed immediately (even when gs is NOT Saved), 
---users can navigate to the Greensheets system and start filling up the form.
---The link to Revision gs is displayed only when Revision gs is FROZEN. Therefore, Revision type of 
---greensheet is always read-only when accessed via eGrants. 
---The initial discussion was that we will keep ARC rules consistent with Revision. 


Select appl_id, pgm_form_submitted_date, gshc.show_greensheet
into #PGM_GreenSheets
FROM greensheet_data gd
left join Greensheets_Show_Hide_Criteria gshc 
	on gd.appl_type_code = gshc.appl_type_id 
	 and gd.Action_Type = gshc.action_type 
	 and gd.pgm_form_status = gshc.greensheets_Status
	 and gd.GPMATS_CANCELLED_FLAG = gshc.GPMATS_Cancel_Flag 
--	 AND ((gd.suffix_code is null and gshc.Suffix_code is null) OR (RIGHT(gd.SUFFIX_CODE,3) LIKE  '%'+gshc.suffix_code+'%'))
	 AND gshc.active = 1


UPDATE documents SET disabled_by_person_id=null, disabled_date=null
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #PGM_GreenSheets pgs on d.appl_id = pgs.appl_id 
where d.category_id = 73  and not(disabled_by_person_id is null) and (pgs.show_greensheet = 1 or pgs.show_greensheet is null)
)  

print 'PGM GreenSheet Enabled=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
SELECT appl_id,73,@impacID,'pdf',1, pgm_form_submitted_date
FROM #PGM_GreenSheets pgs
WHERE pgs.show_greensheet = 1 and appl_id NOT IN (select appl_id from documents where category_id=73) 


print 'Inserted GreenSheet PGM=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

UPDATE documents SET disabled_by_person_id=1899, disabled_date=getdate()
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #PGM_GreenSheets pgs on d.appl_id = pgs.appl_id
where d.category_id = 73  and (disabled_by_person_id is null) and show_greensheet = 0
)  

print 'PGM Greensheets REMOVED =' + cast(@@ROWCOUNT as varchar)

-----SPEC --------


Select appl_id, spec_form_submitted_date, gshc.show_greensheet
into #SPEC_GreenSheets
FROM greensheet_data gd
left join Greensheets_Show_Hide_Criteria gshc 
	on gd.appl_type_code = gshc.appl_type_id 
	 and gd.Action_Type = gshc.action_type 
	 and gd.spec_form_status = gshc.greensheets_Status
	 and gd.GPMATS_CANCELLED_FLAG = gshc.GPMATS_Cancel_Flag 
--	 AND ((gd.suffix_code is null and gshc.Suffix_code is null) OR (RIGHT(gd.suffix_code,3) LIKE  '%'+gshc.suffix_code+'%'))
	 and gshc.active = 1


UPDATE documents SET disabled_by_person_id=null, disabled_date=null
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #SPEC_GreenSheets sgs on d.appl_id = sgs.appl_id
where d.category_id = 74  and not(disabled_by_person_id is null) and (sgs.show_greensheet = 1 or sgs.show_greensheet is null)

)  
print 'SPEC Greensheets ENABLED =' + cast(@@ROWCOUNT as varchar)

INSERT documents(appl_id,category_id,created_by_person_id,file_type,profile_id,document_date)	
SELECT appl_id,74,@impacID,'pdf',1, spec_form_submitted_date
FROM #SPEC_GreenSheets sgs
WHERE (show_greensheet = 1 or show_greensheet is null) and appl_id NOT IN (select appl_id from documents where category_id=74)  

print 'Inserted GreenSheet SPEC=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


UPDATE documents SET disabled_by_person_id=1899, disabled_date=getdate()
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #SPEC_GreenSheets sgs on d.appl_id = sgs.appl_id
where d.category_id = 74  and (disabled_by_person_id is null) and (sgs.show_greensheet = 0)
)  
print 'SPEC Greensheets REMOVED =' + cast(@@ROWCOUNT as varchar)




GO


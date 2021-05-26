SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE     PROCEDURE [dbo].[sp_egrants_maint_PGM_REV] 
AS

BEGIN
DECLARE @impacID int
DECLARE @CATEGORYID smallint
DECLARE @DOCTYPE VARCHAR(5)
SELECT @impacID=person_id FROM people WHERE userid='impac'

TRUNCATE TABLE dbo.[CIIP_Program_Revision_Greensheet]

INSERT dbo.CIIP_Program_Revision_Greensheet(APPL_ID, AGT_ID, FORM_STATUS, SUBMITTED_DATE, CURRENT_ACTION_STATUS_CODE, CANCELLED_FLAG, REVISION_TYPE_CODE, Revision_Number, ACTION_FY, appl_type_code, suffix_code  )
Select APPL_ID, AGT_ID, FORM_STATUS, SUBMITTED_DATE, CURRENT_ACTION_STATUS_CODE, CASE WHEN CANCEL_REASON_ID is null then 'N' ELSE 'Y' END as CANCELLED_FLAG, REVISION_TYPE_CODE, REVISION_NUM, ACTION_FY, appl_type_code, suffix_code    
FROM OPENQUERY(CIIP, 'select gav.appl_id, af.agt_id, f.FORM_STATUS, f.submitted_date, gav.current_action_status_code, GAV.Cancel_Reason_id, GAV.REVISION_TYPE_CODE, GAV.REVISION_NUM, GAV.ACTION_FY, gav.appl_type_code, gav.suffix_code
					  from forms_t f inner join appl_forms_t af on f.ID = af.FRM_ID 
					  left join GM_ACTION_QUEUE_VW gav on gav.id = af.agt_id 				  
					  where f.FORM_ROLE_CODE = ''PGMREV'' and gav.DUMMY_ACTION_FLAG <> ''Y''')

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='PGMREV'

Select appl_id, agt_id, SUBMITTED_DATE, gshc.show_greensheet, REVISION_TYPE_CODE, gd.FORM_STATUS
into #PGM_Rev_GreenSheets
FROM CIIP_Program_Revision_Greensheet gd
left join Greensheets_Show_Hide_Criteria gshc 
	on gd.appl_type_code = gshc.appl_type_id 
	 and gshc.action_type = 'Revision'
	 and gd.FORM_STATUS = gshc.greensheets_Status
	 and gd.CANCELLED_FLAG = gshc.GPMATS_Cancel_Flag 
--	 AND ((gd.suffix_code is null and gshc.Suffix_code is null) OR (RIGHT(gd.SUFFIX_CODE,3) LIKE  '%'+gshc.suffix_code+'%'))
	 AND gshc.active = 1

WHERE gd.REVISION_TYPE_CODE in ('MULTIYR') 
AND gd.current_action_status_code in ('UNDERADGMR',
'CANCELLED',
'REVRDYREL',
'NEW',
'AWAITGMR',
'COMPLETED',
'CLOSED')

UPDATE documents SET disabled_by_person_id=null, disabled_date=null
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #PGM_Rev_GreenSheets pgrgs on pgrgs.agt_id = d.nga_rpt_seq_num and pgrgs.appl_id = d.appl_id
where d.category_id = @CATEGORYID and (pgrgs.show_greensheet = 1 or pgrgs.show_greensheet is null)
)  

print 'Existing Program Revision Greensheets ENABLED =' + cast(@@ROWCOUNT as varchar)


INSERT documents(appl_id, 
				 category_id, 
				 document_date, 
				 created_by_person_id,
				 created_date, 
				 file_type,
				 url,
				 profile_id,
				 nga_rpt_seq_num,
				 sub_category_name)

select	zz.appl_id,
		@CATEGORYID,
		zz.SUBMITTED_DATE,
		@impacID,
		getdate(),
		'pdf',
		NULL,
		dbo.fn_grant_profile_id(ee.grant_id),
		zz.AGT_ID,
		CASE WHEN ZZ.REVISION_TYPE_CODE = 'MULTIYR' THEN 'Multi-Year Funded' END as sub_category_name
from #PGM_Rev_GreenSheets zz inner join vw_appls ee on zz.appl_id = ee.appl_id 
where (zz.show_greensheet = 1 or zz.show_greensheet is null) 
	  and ee.admin_phs_org_code='CA'
	  and zz.REVISION_TYPE_CODE = 'MULTIYR'
	  and zz.AGT_ID not in 
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null) -- and x.created_by_person_id=@impacID)  
 

---AND x.profile_id=1 

print 'NEW Program Revision Greensheets INSERTED =' + cast(@@ROWCOUNT as varchar)
-------------------------------------------

UPDATE documents SET disabled_by_person_id=1899, disabled_date = GETDATE()
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #PGM_Rev_GreenSheets pgrgs on pgrgs.agt_id = d.nga_rpt_seq_num and pgrgs.appl_id = d.appl_id
where pgrgs.show_greensheet = 0 and d.category_id = @CATEGORYID
)  

print 'NEW Program Revision Greensheets REMOVED =' + cast(@@ROWCOUNT as varchar)

END


GO


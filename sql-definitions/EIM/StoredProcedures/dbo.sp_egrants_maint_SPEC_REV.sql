SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE    PROCEDURE [dbo].[sp_egrants_maint_SPEC_REV]

AS

DECLARE @CATEGORYID int;
DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'

-------------------------------------------
--6/16/2017: Greensheet Revision (REV)
--Imran:6/16/2017
--eGrants Category Map : Greensheet SPEC Rev
--Sub category Text = automatic from gm_actions_queue_vw
-------------------------------------------
TRUNCATE TABLE dbo.CIIP_Greensheet_Rev

--Imran : 9/24/2018 : Change the new query from David Chang --3736 are new entries
INSERT dbo.CIIP_Greensheet_Rev(APPL_ID,agt_id,SUBMITTED_DATE,current_action_status_code, appl_type_code, suffix_code, [GPMATS_CANCELLED_FLAG])
SELECT APPL_ID,agt_id,SUBMITTED_DATE, current_action_status_code, appl_type_code, suffix_code, CASE WHEN cancel_reason_id is null then 'N' ELSE 'Y' END as CANCEL_FLAG 
FROM OPENQUERY(CIIP, ' 
SELECT c.appl_id,B.AGT_ID,A.submitted_date, current_action_status_code, c.appl_type_code,c.suffix_code, gav.cancel_reason_id
FROM FORMS_T A INNER JOIN APPL_FORMS_T B ON A.ID=B.FRM_ID  
INNER JOIN APPL_GM_ACTIONS_T C ON b.agt_id = c.id
LEFT JOIN GM_ACTION_QUEUE_VW gav on gav.id = b.agt_id 		
WHERE A.FORM_ROLE_CODE=''REV'' AND GAV.DUMMY_ACTION_FLAG <> ''Y'' ')  


UPDATE X SET X.Revision_type_description=Z.REVISION_TYPE_DESCRIP
FROM dbo.CIIP_Greensheet_Rev X,OPENQUERY(CIIP, 'SELECT C.ID,C.REVISION_TYPE_DESCRIP FROM GM_ACTION_QUEUE_VW C') Z
WHERE X.agt_id=Z.ID 

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code='REV'


Select appl_id, agt_id, SUBMITTED_DATE, gshc.show_greensheet, gd.Revision_type_description
into #SPEC_Rev_GreenSheets
FROM CIIP_Greensheet_Rev gd
left join Greensheets_Show_Hide_Criteria gshc 
	on gd.appl_type_code = gshc.appl_type_id 
	 and gshc.action_type = 'Revision'
	 and gd.current_action_status_code = gshc.greensheets_Status
	 and gd.GPMATS_CANCELLED_FLAG = gshc.GPMATS_Cancel_Flag 
--	 AND ((gd.suffix_code is null and gshc.Suffix_code is null) OR (RIGHT(gd.SUFFIX_CODE,3) LIKE  '%'+gshc.suffix_code+'%'))
	 AND gshc.active = 1



--Enable any DISABLED SPEC REVs 
UPDATE documents SET disabled_by_person_id=null, disabled_date=null
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #SPEC_Rev_GreenSheets srg on srg.agt_id = d.nga_rpt_seq_num and srg.appl_id = d.appl_id
where (srg.show_greensheet = 1 or srg.show_greensheet is null) AND d.category_id = @CATEGORYID
and NOT (disabled_by_person_id is null and disabled_date is null) 
)  

print 'Existing Program Revision Greensheets ENABLED =' + cast(@@ROWCOUNT as varchar)



--Insert Any New Spec REVs
INSERT documents(appl_id, category_id, document_date, created_by_person_id,created_date, file_type,url,profile_id,nga_rpt_seq_num,sub_category_name)
select zz.appl_id,@CATEGORYID,zz.SUBMITTED_DATE,@impacID,getdate(),'pdf',NULL,dbo.fn_grant_profile_id(ee.grant_id),zz.AGT_ID,ZZ.Revision_type_description
from #SPEC_Rev_GreenSheets zz, vw_appls ee 
where zz.appl_id=ee.appl_id and ee.admin_phs_org_code='CA'
and zz.AGT_ID not in 
(select distinct x.nga_rpt_seq_num from egrants x where x.nga_rpt_seq_num is not null 
and x.category_id=@CATEGORYID and x.url is null and x.created_by_person_id=@impacID)  AND (zz.show_greensheet = 1 or zz.show_greensheet is null) 

print 'NEW Greensheet SPEC REV INSERTED =' + cast(@@ROWCOUNT as varchar)

--Disable any SPEC REVs 
UPDATE documents SET disabled_by_person_id=@impacID, disabled_date=GETDATE()
WHERE document_id in 
(
Select d.document_id 
from documents d inner join #SPEC_Rev_GreenSheets cgr on cgr.agt_id = d.nga_rpt_seq_num and cgr.appl_id = d.appl_id
where  d.category_id = @CATEGORYID and cgr.show_greensheet = 0
)  




GO


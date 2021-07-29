SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE   PROCEDURE dbo.sp_egrants_maint_impacdocs_IMPP_Supplements
AS

-------------------------------------------
--11/25/2015: Bring All Administrative Supplements with any action_code
-------------------------------------------
TRUNCATE TABLE dbo.IMPP_Admin_Supplements

INSERT dbo.IMPP_Admin_Supplements(Supp_appl_id,Full_grant_num,Former_Num,Action_date,admin_supp_action_code,serial_num,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE, ACCESSION_NUMBER, eRa_TS)
select APPL_ID,FULL_GRANT_NUM,FORMER_NUM,ACTION_DATE,ADMIN_SUPP_ACTION_CODE,SERIAL_NUM,APPL_TYPE_CODE,ACTIVITY_CODE,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE, ACCESSION_NUM, CREATED_DATE
from OPENQUERY(IRDB,'SELECT A.APPL_ID, A.APPL_TYPE_CODE||A.GRANT_NUM AS FULL_GRANT_NUM, A.FORMER_NUM, B.ACTION_DATE, B.ADMIN_SUPP_ACTION_CODE,
A.SERIAL_NUM, A.APPL_TYPE_CODE, A.ACTIVITY_CODE, A.ADMIN_PHS_ORG_CODE, A.SUPPORT_YEAR, A.SUFFIX_CODE, A.ACCESSION_NUM, A.CREATED_DATE 
FROM APPLS_MV A, ADMIN_SUPP_ROUTINGS_T B
WHERE A.APPL_ID=B.APPL_ID AND A.ADMIN_PHS_ORG_CODE=''CA'' ')
ORDER BY APPL_ID DESC,ACTION_DATE DESC

Update IMPP_Admin_Supplements
SET IMPP_Admin_Supplements.Former_appl_id=B.appl_id
from IMPP_Admin_Supplements a, vw_appls b
where a.Former_Num=b.full_grant_num

INSERT INTO dbo.IMPP_Admin_Supplements_WIP(serial_num,Supp_appl_id,Full_grant_num,Former_Num,Former_appl_id,Submitted_date,file_type,category_id,doc_url,accession_number,eRa_TS )
SELECT A.serial_num,A.Supp_appl_id,A.Full_grant_num,A.Former_Num,A.Former_appl_id,A.Action_date,'PDF',38,
--'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='+convert(varchar,A.Supp_appl_id)+'&docType=IGI' commented on 10/17/18
'https://s2s.era.nih.gov/docservice/dataservices/document/once/applId/'+convert(varchar,A.Supp_appl_id)+'/' + 'IGI',
a.accession_number,
a.eRa_TS
FROM dbo.IMPP_Admin_Supplements A
where A.admin_supp_action_code='STA'
and A.Supp_appl_id NOT in (select distinct Supp_appl_id  from IMPP_Admin_Supplements_WIP WHERE Supp_appl_id IS NOT NULL)
--and A.serial_num NOT in (select distinct serial_num  from IMPP_Admin_Supplements_WIP WHERE serial_num IS NOT NULL)
AND Action_date > '10/1/2015'
order by a.serial_num
print 'ADMINISTRATIVE SUPPLEMENT ACTIONS DOWNLOADED =' + cast(@@ROWCOUNT as varchar)

--Insert all accepted supplement action into placeholder to send email
insert dbo.adsup_accepted(supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code)
select supp_appl_id,full_grant_num,Former_num,serial_num,Former_appl_id,Action_date,admin_supp_action_code 
from dbo.IMPP_Admin_Supplements where admin_supp_action_code='ACC'
and Supp_appl_id not in (select distinct Supp_appl_id from adsup_accepted)
print 'ADMIN SUPPLEMENT NEWLY ACCEPTED COUNT=' + cast(@@ROWCOUNT as varchar)

--Create email entry to go on next run from oga stage machine dts
Exec dbo.adsupp_create_notification_accgrant
print 'ADMIN SUPPLEMENT EMAIL CREATED=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)



GO


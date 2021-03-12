SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_egrants_DocsLoad_BOTTOMOFTHE_HOUR] 
AS

DECLARE @impacID int
SELECT @impacID=person_id FROM people WHERE userid='impac'
DECLARE @CATEGORYID smallint
DECLARE @DOCTYPE VARCHAR(5)
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_impacdocs'
SET @RUN_DATETIME = GETDATE()

print '====>>Proc sp_egrants_maint_impacdocs_latest Started @' + cast(getdate() as varchar)

-------------------------------------------
--7/17/2017: ADD JIT
-------------------------------------------
--Imran 5/29/2018 : I am commenting the following line to test the morning 3:30 Job I will enable this later.
Exec sp_egrants_maint_pop_JIT


-------------------------------------------------------------------------------------------------------
--Add New RPPRs  @impacID
-- Note: We will have to remove the following in future (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158)
--Update 10/28/2014 : Imran
--------------------------------------------------------------------------------------------------------
TRUNCATE TABLE impp_rpprs_t 
--INSERT impp_rpprs_t(appl_id, created_date, last_upd_date,correct_rcvd_date)
--SELECT appl_id, created_date, last_upd_date,RECEIVED_DATE
--FROM openquery(IRDB, 'select R.appl_id, R.created_date,ER.LAST_UPD_DATE ,ER.RECEIVED_DATE
--from RPPRs_T R,EAPPL_ROUTINGS_MV ER 
--WHERE R.RPPR_ID=ER.RPPR_ID
--AND ER.REVIEWER_USER_ID=''NIH''
--AND R.RPPR_STATUS_CODE=''FIN''
--AND ER.PRAM_FLAG=''N'' ')

INSERT impp_rpprs_t(appl_id, created_date, last_upd_date,correct_rcvd_date,correct_rcvd_date1)
SELECT appl_id, created_date, last_upd_date,RECEIVED_DATE,APPL_RECEIVED_DATE
FROM openquery(IRDB, 'select A.appl_id, R.created_date,ER.LAST_UPD_DATE ,ER.RECEIVED_DATE,A.APPL_RECEIVED_DATE
from APPLS_T A, RPPRs_T R, EAPPL_ROUTINGS_MV ER 
WHERE A.APPL_ID=R.APPL_ID
AND (R.RPPR_ID=ER.RPPR_ID OR R.APPL_ID=ER.APPL_ID)
AND R.RPPR_STATUS_CODE=''FIN''
AND ER.EAPPL_ACTION_CODE =''SUBMIT''
AND ER.COMPLIANT_PUBLICATIONS_FLAG IS NULL
AND ER.PRAM_FLAG=''N'' ')


print 'Total RPPRs in IRDB=' + cast(@@ROWCOUNT as varchar)

INSERT documents(appl_id, category_id, document_date, created_by_person_id, file_type,url,profile_id)
SELECT gi.appl_id, 38, gi.correct_rcvd_date1, @impacID,'pdf', NULL AS url, dbo.fn_grant_profile_id(a.grant_id)
FROM impp_rpprs_t gi, vw_appls a
WHERE a.appl_id=gi.appl_id and gi.appl_id not in 
(
select distinct a.appl_id from egrants a, impp_rpprs_t b where a.appl_id=b.appl_id and a.category_id=38
and a.created_by_person_id in (@impacID,398,856,548,1936,397,3271,2768,2,1931,3258,2319,2629,306,1937,1158) 
)

print 'Total RPPRs Added today =' + cast(@@ROWCOUNT as varchar)




GO


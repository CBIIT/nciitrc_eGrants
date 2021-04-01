SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_egrants_maint_pop_FSR]

AS
DECLARE @impacID int
DECLARE @CATEGORYID smallint
DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
DECLARE @IMPAC_DOWNLOAD_CNT INT
DECLARE @EGRANTS_UPLOAD_CNT INT
DECLARE @UPDT_CNT INT
DECLARE @INSRT_CNT INT
SET @PROC_NAME = 'sp_egrants_maint_pop_FSR'
SET @RUN_DATETIME = GETDATE()

PRINT 'BEGIN PROC: [sp_egrants_maint_pop_FSR] @ '+ cast(getdate() as varchar)

TRUNCATE TABLE impp_fsrs

/**
Bring accepted FSR from IRDB  {fsr_accepted_code='Y'}
**/

select @CATEGORYID=category_id from categories where impac_doc_type_code='FSR' and category_name='FFR'
SELECT @impacID=person_id FROM people WHERE userid='impac'

INSERT impp_fsrs(appl_id, fsr_date)
SELECT appl_id, last_upd_date 
FROM openquery(IRDB,'select appl_id, last_upd_date  from appls_t 
where to_date(to_char(last_upd_date,''yyyy-mm-dd''),''yyyy-mm-dd'')>=to_date(''2003-05-01'',''yyyy-mm-dd'') and fsr_accepted_code=''Y'' ')
print 'Brought FSRs from IRDB. FSR Count =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)

SELECT @UPDT_CNT=count(*) 
FROM impp_fsrs i, appls a
WHERE i.appl_id=a.appl_id 
and i.appl_id not in (select appl_id from  egrants where created_by_person_id=530 and category_id in (select category_id from categories where impac_doc_type_code='FSR') )

IF @UPDT_CNT>0   
BEGIN
	UPDATE documents
	SET documents.document_date=v.fsr_date 
	FROM documents d,impp_fsrs v
	WHERE d.category_id in (select category_id from categories where impac_doc_type_code='FSR')
	and d.created_by_person_id=@impacID and d.url is null and d.appl_id=v.appl_id and d.document_date<>v.fsr_date 
	print 'updated FSR last_upd_date in documents table =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
END

SELECT @INSRT_CNT=count(*)
FROM impp_fsrs i, appls a
WHERE i.appl_id=a.appl_id and i.appl_id not in (select appl_id from  egrants where created_by_person_id=530 
and category_id in (select category_id from categories where impac_doc_type_code='FSR') )

IF @INSRT_CNT > 0
BEGIN
	INSERT documents(appl_id, category_id, document_date, page_count, created_by_person_id, file_type, profile_id)
	SELECT i.appl_id, @CATEGORYID, i.fsr_date,1, @impacID, 'pdf',dbo.fn_grant_profile_id(a.grant_id)
	FROM impp_fsrs i, appls a
	WHERE i.appl_id=a.appl_id and i.appl_id not in (select appl_id from  egrants where created_by_person_id=@impacID 
	and category_id in (select category_id from categories where impac_doc_type_code='FSR') )
	print 'New FFR Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)
END

/**
Bring and insert all FSR to impp_fsrs_all
**/ 
TRUNCATE TABLE impp_fsrs_all
INSERT impp_fsrs_all
SELECT appl_id,fsr_id,fsr_seq_num,accepted_date
FROM openquery(IRDB,'select APPL_ID, FSR_ID, FSR_SEQ_NUM, ACCEPTED_DATE, SUBMITTED_DATE from fsrs where to_date(to_char(ACCEPTED_DATE,''yyyy-mm-dd''),''yyyy-mm-dd'')>to_date(''2003-05-01'',''yyyy-mm-dd'') ')
print 'All FSR Added =' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


PRINT 'END PROC:==> [sp_egrants_maint_pop_FSR] @ '+ cast(getdate() as varchar)

GO


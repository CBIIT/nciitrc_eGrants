SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE    PROCEDURE [dbo].[sp_egrants_maint_impacdocs_CSTUDY] 
AS
BEGIN
-------------------------------------------
--11/22/2020: eGrants 13 CSTUDY Documents
--Benny Shell: 11/22/2020
--eGrants RPPR Study Comparison 
--Sub category Text = ?? RPPR Study Comparison Document
-------------------------------------------
DECLARE @CATEGORYID int;
DECLARE @impacID int;
DECLARE @doctypecode varchar(15);

SELECT @impacID=person_id FROM people WHERE userid='impac'
SET @doctypecode = 'CSTUDY'

TRUNCATE TABLE dbo.RPPR_STUDY_COMPARISON_IMPORT

INSERT dbo.RPPR_STUDY_COMPARISON_IMPORT(APPL_ID,DOC_KEY_ID,SUBMITTED_DATE)
SELECT APPL_ID,STUDIES_COMPARE_REQUEST_ID,CREATED_DATE
FROM openquery(IRDB, 'select STUDIES_COMPARE_REQUEST_ID,APPL_ID,CREATED_DATE from 
APPL_STUDIES_COMPARE_REQ_T')

SELECT @CATEGORYID=category_id from categories where impac_doc_type_code=@doctypecode

TRUNCATE TABLE dbo.RPPR_STUDY_COMPARISON

INSERT dbo.RPPR_STUDY_COMPARISON(DOC_KEY_ID,APPL_ID,SUBMITTED_DATE) 
Select a.DOC_KEY_ID, b.APPL_ID, b.SUBMITTED_DATE 
FROM RPPR_STUDY_COMPARISON_IMPORT a JOIN (Select APPL_ID, MAX(SUBMITTED_DATE) as SUBMITTED_DATE
FROM RPPR_STUDY_COMPARISON_IMPORT
GROUP BY APPL_ID) b ON a.SUBMITTED_DATE = b.SUBMITTED_DATE and a.APPL_ID = b.APPL_ID


UPDATE B
set B.url = 'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'+CAST(DOC_KEY_ID as varchar) +'/' + @doctypecode  , B.added_date = GETDATE(), B.document_date = A.SUBMITTED_DATE 
from	dbo.RPPR_STUDY_COMPARISON A 
LEFT JOIN	documents B on A.appl_id=B.appl_id and b.category_id = @CATEGORYID 
LEFT JOIN	appls c on a.appl_id = c.appl_id
WHERE not(b.appl_id is null) and b.document_date <> a.SUBMITTED_DATE and NOT(b.url LIKE 'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'+CAST(DOC_KEY_ID as varchar) +'/' + @doctypecode)


INSERT documents(appl_id,category_id,created_by_person_id,file_type,document_date,profile_id, [url])
SELECT A.appl_id, @CATEGORYID, @impacID, 'pdf', A.SUBMITTED_DATE,dbo.fn_grant_profile_id(c.grant_id),'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'+CAST(DOC_KEY_ID as varchar) +'/' + @doctypecode 	
from		dbo.RPPR_STUDY_COMPARISON A 
LEFT JOIN	documents B on A.appl_id=B.appl_id and b.category_id = @CATEGORYID 
LEFT JOIN	appls c on a.appl_id = c.appl_id
WHERE b.appl_id is null
END


GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE procedure [dbo].[sp_egrants_convert_file_old]
@File varchar(255)
AS

--curently only processing excel files

DECLARE @mainDir varchar(255)
DECLARE @URL	varchar(255)
DECLARE @DocID	int
DECLARE @FType	varchar(5)

--SET @mainDir='https://egrants-data.nci.nih.gov/funded2/nci/main/'
--SET @mainDir='/data/funded2/nci/main/' coumment out by Leon 5/16/2016
SET @mainDir='data/funded2/nci/main/'

IF @File NOT LIKE '%.xls' RETURN
IF ISNUMERIC(LEFT(@File, len(@File)-4))=0 RETURN

SET @DocID=convert(int,LEFT(@File, len(@File)-4))

IF 
(
SELECT count(*) from egrants
WHERE document_id=@DocID and category_name='Spread Sheet' and profile_id=1
)=1
SET @FType='pdf'
ELSE SET @FType='xls' --leave as excel

BEGIN TRAN

UPDATE documents
SET url= @mainDir + convert(varchar, document_id) + '.' + @FType,
file_type=@FType,

file_modified_date=
(CASE 
WHEN file_type='xls' and @FType='xls' THEN file_modified_date
ELSE getdate()
END),

file_modified_by_person_id=
(CASE 
WHEN file_type='xls' and @FType='xls' THEN file_modified_by_person_id
ELSE 1899
END)

where document_id=@DocID

IF @@rowcount>1 ROLLBACK TRAN

COMMIT TRAN

return
GO


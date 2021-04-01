SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_egrants_doc_create]

@ApplID 		int,
@CategoryID 	int,
@SubCategory	varchar(35)=null,
@DocDate 		smalldatetime,
@file_type		varchar(5),
@Operator		varchar(50),
@ic 			varchar(10),
@DocumentID     int OUTPUT

AS
/************************************************************************************************************/
/***									 							        ***/
/***	Procedure Name: sp_web_egrants_doc_create							***/
/***	Description:	create or edit document						        ***/
/***	Created:	01/05/2017	Frances								        ***/
/***	Modified:   01/18/2017	Frances							            ***/
/***	Modified:   05/23/2017	Leon add @SubCategory						***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id		smallint,
@person_id		int,
@url 			varchar(300)
--@DocumentID     int 

SET @url='data/funded2/nci/main/'

--find the profile_id
SET @ic=LOWER(@ic)
SELECT @profile_id=profile_id FROM profiles WHERE profile=@ic 

--find the operator's person_id
SELECT @person_id = person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id

--check file type
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))

---to create new document
INSERT documents(appl_id,category_id,sub_category_name,document_date,file_type,created_date,created_by_person_id,qc_date,profile_id,qc_reason)
SELECT @ApplID,@CategoryID,ISNULL(@SubCategory,null),@DocDate, @file_type, getdate(), @person_id, getdate(), @profile_id, 'LocalScan'

SELECT @DocumentID = @@IDENTITY
UPDATE documents SET url=REPLACE(@url + convert(varchar,@DocumentID) + '.' + @file_type,' ', '') WHERE document_id=@DocumentID

---insert document transaction information
EXEC sp_web_egrants_doc_transaction @DocumentID, @Operator,'created', null

SELECT @DocumentID	

GO


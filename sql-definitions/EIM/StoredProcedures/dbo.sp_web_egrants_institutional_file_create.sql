SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF


CREATE   PROCEDURE [dbo].[sp_web_egrants_institutional_file_create]

@org_id				int,
@category_id		int,
@file_type			varchar(5),
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50),
@comments			varchar(256),
@document_id		varchar(10) OUTPUT

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_institutional_file_create					***/
/***	Description:create org files										***/
/***	Created:	01/10/2017	Leon	create it for MVC						***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@person_id		int,
@profile_id		int,
@doc_id			int,
@category_name	varchar(100),
@file_location	varchar(200)


/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

SET @file_location='/data/funded/nci/institutional/'
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))
SET @category_name=(select doctype_name from Org_Categories where doctype_id=@category_id)

IF @category_name<>'Site Visit' 
BEGIN
SET @start_date=null
SET @end_date=null
END

----create new document 
INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,end_date_showFlag,comments)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),ISNULL(@end_date,null),@comments

SET @doc_id=@@IDENTITY

UPDATE dbo.Org_Document SET url=@file_location + convert(varchar,@doc_id) + '.' + @file_type WHERE document_id=@doc_id

SELECT @document_id=convert(varchar, @doc_id) 

RETURN

GO


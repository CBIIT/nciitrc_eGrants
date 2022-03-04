SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF


CREATE   PROCEDURE [dbo].[sp_web_egrants_institutional_file_update]

@category_id		int,
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50),
@comments			varchar(256),
@document_id		varchar(10) 

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_institutional_file_create					***/
/***	Description:create org files											***/
/***	Created:	03/02/20	Madhu		create it for MVC					***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@person_id		int,
@profile_id		int,
@doc_id			int,
@category_name	varchar(100),
@file_location	varchar(200),
@tobe_flag char(1)


/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

SET @file_location='/data/funded/nci/institutional/'
select @category_name= doctype_name , @tobe_flag = isNull(tobe_flagged,0) from Org_Categories where doctype_id=@category_id

IF @tobe_flag<>'1'
BEGIN
SET @start_date=null
SET @end_date=null
END


----Update document 
UPDATE dbo.Org_Document
SET doctype_id = @category_id,
updated_date = getdate(),
updated_by_person_id = @person_id,
start_date_ShowFlag=ISNULL(@start_date,null),
end_date_showFlag=ISNULL(@end_date,null),
comments = @comments
where
document_id = @document_id

RETURN

GO


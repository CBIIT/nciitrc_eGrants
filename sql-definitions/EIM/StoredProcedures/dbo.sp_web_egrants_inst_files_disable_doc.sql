SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

GO


CREATE   OR ALTER     PROCEDURE [dbo].[sp_web_egrants_inst_files_disable_doc]
(
@doc_id		int,
@user_id varchar(50) = null,
@person_id	int = 0

)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_disable_doc     			***/
/***	Description:files disable doc 										***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON
declare @pperson_id int

-------------------
if (@person_id is null)
BEGIN
Select @pperson_id = p.person_id  from people p where p.userid = @user_id 
END
ELSE
set @pperson_id = @person_id

-- disable_doc:

update dbo.Org_Document set disabled_date=GETDATE(), disabled_by_person_id=@pperson_id where document_id=@doc_id	

RETURN

GO


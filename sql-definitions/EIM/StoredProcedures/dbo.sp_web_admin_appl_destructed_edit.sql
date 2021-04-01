SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_admin_appl_destructed_edit]

@act					varchar(30),
@id						int,
@detail					varchar(50),
@code					varchar(20),
@ic						varchar(10),
@Operator				varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name: sp_web_admin_appl_destructed_edit						***/
/***	Description: create, edit or delete exception							***/
/***	created:	Leon 4/10/2017 Leon	for MVC									***/
/***	modified:	Leon 4/11/2017 Leon 										***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id						smallint,
@person_id						int

SET @profile_id	=(select profile_id	from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

IF @act='add' GOTO add_exception_code
IF @act='edit' GOTO edit_exception_code
IF @act='delete' GOTO delete_exception_code

------------------
add_exception_code:

Insert dbo.IMPAC_DESTRUCT_OGA_EXCEPTION(code, detail, created_by_person_id, created_date)
SELECT @code,@detail,@person_id, GETDATE();

return
-----------------
edit_exception_code:

UPDATE dbo.IMPAC_DESTRUCT_OGA_EXCEPTION 
SET Detail=@detail, Last_Update_by_person_id=@person_id, Last_update_date=GETDATE() 
WHERE id=@id

return
---------------------
delete_exception_code:
UPDATE dbo.IMPAC_DESTRUCT_OGA_EXCEPTION 
SET disable_date=GETDATE(),disabled_by_person_id=@person_id 
WHERE id=@id

return


GO


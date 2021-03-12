SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

create PROCEDURE [dbo].[sp_web_admin_category_edit]

@act 			varchar(20),
@category_id	smallint,
@category_name	varchar(50),
@ic				varchar(10),
@operator 		varchar(50),
@return_notice	varchar(200) OUTPUT

AS
/*****************************************************************************************/
/***																***/
/***	Procedure Name: sp_web_admin_category_edit					***/
/***	Description:	edit categories for each institution		***/
/***	Created:	03/20/2014	Leon								***/
/***	Modified:	03/24/2014	Leon								***/
/***	Modified:	11/13/2014	Leon moved to admin					***/
/*****************************************************************************************/

SET NOCOUNT ON

DECLARE @profile_id			smallint
DECLARE @pos				int
DECLARE @person_id 			int
declare @count				int

--find person_id and profile_id
SELECT @profile_id=profile_id FROM profiles WHERE [profile]=@ic	
SELECT @person_id= person_id FROM vw_people WHERE userid=@operator and profile_id=@profile_id

--pick action
---IF @act='' or @act IS NULL	GOTO header
IF @act='create_new'	GOTO to_create
IF @act='add_in'		GOTO to_add
IF @act='remove_out'	GOTO to_remove

RETURN
--------------------------------------------------------------------
to_create:

SET @count=(select category_id from vw_categories where category_name=@category_name and ic=@ic)
IF @count>0 
BEGIN
SET @return_notice=@category_name + ' has already been in ' + UPPER(@ic) + ' local category list.'
GOTO header
END
ELSE
BEGIN
INSERT categories (category_name,created_date,created_by_person_id,can_upload) SELECT @category_name,GETDATE(),@person_id,'Yes'
INSERT categories_ic(category_id, ic,added_by_person_id,added_date) SELECT category_id,@ic,@person_id,GETDATE() FROM categories WHERE category_name=@category_name
SET @return_notice=@category_name + ' has been created.'
END

GOTO header

RETURN
-----------------------------------------------------------------
to_remove:

SET @category_name=(SELECT category_name FROM categories WHERE category_id=@category_id)

UPDATE categories_ic SET removed_by_person_id=@person_id,removed_date=getdate() WHERE category_id=@category_id and ic=@ic
SET @return_notice=@category_name + ' has been removed from ' + UPPER(@ic) + ' local category list.'

GOTO header

RETURN
------------------------------------------
to_add:

SET	@category_name=(select category_name from categories where category_id=@category_id)

---has been added
IF (select COUNT(*)from categories_ic WHERE category_id=@category_id and ic=@ic and added_date is not null and removed_date is null)=1
BEGIN
SET @return_notice=@category_name + ' has already been in ' + UPPER(@ic) + ' local category list.'
END

---has been removed
IF (select COUNT(*)from categories_ic WHERE category_id=@category_id and ic=@ic and added_date is not null and removed_date is not null)=1
BEGIN
UPDATE categories_ic SET added_by_person_id=@person_id, added_date=GETDATE(), removed_date=null, removed_by_person_id=null WHERE category_id=@category_id and ic=@ic
SET @return_notice=@category_name + ' has been added in ' + UPPER(@ic) + ' local category list.'
END

---to add
IF (select COUNT(*)from categories_ic where category_id=@category_id and ic=@ic)=0 
BEGIN
INSERT categories_ic (category_id, ic,added_by_person_id,added_date) SELECT @category_id, @ic, @person_id,GETDATE()
SET @return_notice=@category_name + ' has been added in ' + UPPER(@ic) + ' local category list.'
END

GOTO header

RETURN
-------------------------------------
header:

SELECT @return_notice

GO


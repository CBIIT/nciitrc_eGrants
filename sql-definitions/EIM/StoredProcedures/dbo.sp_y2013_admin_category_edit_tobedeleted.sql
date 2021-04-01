SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_y2013_admin_category_edit_tobedeleted]

@act 		varchar(50),
@descr		varchar(100),
@inst		varchar(10),
@operator 	varchar(50)

AS
/*****************************************************************************************/
/***																***/
/***	Procedure Name: sp_y2013_admin_category_edit				***/
/***	Description:	edit categories for each institution		***/
/***	Created:	01/25/2006	Leon								***/
/***	Modified:	03/29/2013	Leon								***/
/***	Modified:	09/26/2014	Leon simplify						***/
/***	Modified:	11/10/2014	Leon moved it to admin				***/
/*****************************************************************************************/

SET NOCOUNT ON

DECLARE @profile_id			smallint
DECLARE @category_id		smallint
DECLARE @pos				int
DECLARE @CategoryName		varchar(50)
DECLARE @CategoryPackage	varchar(50)
DECLARE @person_id 			int
DECLARE @count				int
declare @return_notice		varchar(200)
declare @cat_name			varchar(50)
declare @xmlout				varchar(max)
declare	@X					Xml

SELECT @profile_id=profile_id FROM profiles WHERE [profile]=@inst	
SELECT @person_id= person_id FROM vw_people WHERE userid=@operator and profile_id=@profile_id

--check permisson
SET @count=(select COUNT(*) FROM vw_people p inner join vw_adm_menu_assignment a on p.person_id=a.person_id
WHERE p.userid=@operator and application_type='egrants' and menu_title='Category Edit')

--return user's info
IF @count=1
BEGIN
SET @X = (
SELECT *,convert(varchar,getdate(),101) as today
FROM vw_people as user_info
WHERE userid=@operator 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
END
ELSE
BEGIN
SELECT null
END

--return user's meun 
SET @X = (
SELECT menu_id,menu_title,menu_url,menu_hover 
FROM  dbo.vw_adm_menu_assignment as menu 
WHERE person_id=@person_id and menu_url is not null
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--pick action
IF @act='' or @act IS NULL	GOTO header
IF @act='to_create' GOTO to_create
IF @act='to_add' GOTO to_add
IF @act='to_remove' GOTO to_remove

RETURN
--------------------------------------------------------------------
to_create:

SET @pos=PATINDEX('%,%', @descr)	

IF @pos>0
BEGIN
SET @CategoryName	= LEFT(@descr,@pos-1 )	
SET @CategoryPackage= RIGHT(@descr,LEN(@descr)-@pos )
END
ELSE
BEGIN
SET @CategoryName=@descr
SET @CategoryPackage=NULL
END

IF (select COUNT(*) from vw_categories where category_name=@CategoryName)>0
BEGIN
SET @return_notice='Category '+ @CategoryName + ' has already been created.'
END
ELSE
BEGIN
INSERT categories (category_name,package,created_date,created_by_person_id, can_upload) SELECT @CategoryName, @CategoryPackage, getdate(),@person_id, 'Yes'
INSERT categories_ic(category_id,ic,added_by_person_id,added_date) SELECT category_id,UPPER(@inst),@person_id,GETDATE()FROM categories WHERE category_name=@CategoryName
SET @return_notice='Category ' + @CategoryName + ' has been created.'
END

GOTO header

RETURN
-----------------------------------------------------------------
to_remove:

UPDATE categories_ic SET removed_by_person_id=@person_id,removed_date=GETDATE() WHERE category_id=convert(smallint,@descr) and ic=@inst
SET @CategoryName=(select category_name from categories where category_id=convert(smallint,@descr))
SET @return_notice='Category ' + @CategoryName + ' has been removed.'

---DELETE FROM categories_ic WHERE category_id=convert(smallint,@descr) and ic=@inst

GOTO header

RETURN
------------------------------------------
to_add:

SET @CategoryName=(select category_name from categories where category_id=convert(smallint,@descr))

--has been added
IF (select COUNT(*)from categories_ic WHERE category_id=convert(smallint,@descr) and ic=@inst and removed_date is null)= 1
BEGIN
SET @return_notice='Category ' + @CategoryName + ' has alreadly been added in ' + UPPER(@inst) + ' locale category list.'
END

---has been removed
IF (select COUNT(*)from categories_ic WHERE category_id=convert(smallint,@descr) and ic=@inst and removed_date is not null)=1
BEGIN
UPDATE categories_ic SET added_by_person_id=@person_id, added_date=GETDATE(), removed_date=null, removed_by_person_id=null WHERE category_id=convert(smallint,@descr) and ic=@inst
SET @return_notice='Category ' + @CategoryName + ' has been added in ' + UPPER(@inst) + ' locale category list.'
GOTO header
END

---to added
IF (select COUNT(*)from categories_ic WHERE category_id=convert(smallint,@descr)and ic=@inst)=0
BEGIN
INSERT categories_ic (category_id, ic,added_by_person_id,added_date) SELECT convert(smallint,@descr), UPPER(@inst),@person_id,GETDATE()
SET @return_notice='Category ' + @CategoryName + ' has been added in ' + UPPER(@inst) + ' locale category list.'
END

GOTO header

RETURN
-------------------------------------
header:

---return local categories
SET @X = (
SELECT distinct category_id as category_id, category_name 	---dbo.fn_clean_characters(category_name) as category_name 
FROM vw_categories AS category WHERE ic=@inst
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

---return all categories
SET @X = (
SELECT distinct category_id,category_name ---dbo.fn_clean_characters(category_name) as category_name
FROM vw_categories AS category 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

SET @X = (
SELECT @return_notice FROM performance AS return_info FOR XML AUTO, ELEMENTS 
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout


GO


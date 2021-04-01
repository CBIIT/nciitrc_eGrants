SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_y2013_egrants_category_edit_tobedeleted]

@act 		varchar(50),
@descr		varchar(100),
@operator 	varchar(50),
@inst		varchar(10)

AS
/*****************************************************************************************/
/***																***/
/***	Procedure Name: sp_y2013_egrants.category_edit				***/
/***	Description:	edit categories for each institution		***/
/***	Created:	01/25/2006	Leon								***/
/***	Modified:	03/29/2013	Leon								***/
/***																***/
/*****************************************************************************************/

SET NOCOUNT ON

DECLARE @profile_id			smallint
DECLARE @category_id		smallint
DECLARE @pos				int
DECLARE @CategoryName		varchar(50)
DECLARE @CategoryPackage	varchar(50)
DECLARE @person_id 			int
declare @char				varchar(1)
declare @cat_name			varchar(50)

SELECT @profile_id=profile_id from profiles where profile=@inst	

IF @operator IS NULL RETURN
ELSE
BEGIN
SELECT @person_id= person_id from people where userid=@operator
END

--pick action
IF @act='' or @act IS NULL	
GOTO header

IF @act='category_create'
GOTO category_create

IF @act='category_add'
GOTO category_add

IF @act='category_remove'
GOTO category_remove

RETURN
--------------------------------------------------------------------
category_create:

SET @pos=PATINDEX('%,%', @descr)	

IF @pos>0
BEGIN
SET @CategoryName	=LEFT(@descr,@pos-1 )	
SET @CategoryPackage=RIGHT(@descr,LEN(@descr)-@pos )
END
ELSE
BEGIN
SET @CategoryName=@descr
SET @CategoryPackage=NULL
END

INSERT categories (category_name, package, created_date, created_by_person_id) SELECT @CategoryName, @CategoryPackage, getdate(),@person_id
INSERT categories_ic(category_id, ic) SELECT category_id, @inst FROM categories WHERE category_name=@CategoryName

GOTO header

RETURN
-----------------------------------------------------------------
category_remove:

DELETE FROM categories_ic WHERE category_id=convert(smallint,@descr) and ic=@inst

GOTO header

RETURN
------------------------------------------
category_add:

SET @pos=(select COUNT(*)from categories_ic WHERE category_id=convert(smallint,@descr) and ic=@inst)

IF @pos=1 GOTO header

IF @pos=0 INSERT categories_ic (category_id, ic) SELECT convert(smallint,@descr), @inst

GOTO header

RETURN
-------------------------------------
header:

/**
SELECT
1		AS tag, 
null		AS parent,
null		[packages!1!packages!element], 
null		[package!2!package!element]
UNION ALL
SELECT distinct 
2, 
1,
null,
package
FROM vw_categories
FOR XML EXPLICIT
**/

SELECT distinct package FROM vw_categories AS packages 

/**
SELECT
1		AS tag, 
null		AS parent,
null		[categories!1!categories!element], 
null		[category!2!category_id!element], 
null		[category!2!category_name!element]
UNION ALL
SELECT
2, 
1,
null,
category_id,
category_name 
FROM vw_categories
WHERE ic=@inst
FOR XML EXPLICIT
**/
SELECT 
distinct category_id as ic_category_id, 
dbo.fn_clean_characters(category_name) as ic_category_name 
FROM vw_categories AS categories WHERE ic=@inst

/**
SELECT
1		AS tag, 
null		AS parent,
null		[categories_all!1!categories!element], 
null		[category!2!category_id!element], 
null		[category!2!category_name!element]
UNION ALL
SELECT
2, 
1,
null,
category_id,
category_name  
FROM categories
FOR XML EXPLICIT
**/

SELECT 
distinct category_id, 
dbo.fn_clean_characters(category_name) as category_name
FROM vw_categories AS all_categories 

--SELECT * FROM profiles WHERE profile_id=@profile_id FOR XML AUTO, ELEMENTS 

/** find user info***/
declare @first_name		varchar(50)
declare @separate		int
declare @person_name	varchar(50)
declare @qc_date		smalldatetime
declare @daydiff 		int

SELECT @qc_date=MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null
IF @qc_date IS NOT NULL
BEGIN
SELECT @daydiff=DATEDIFF(day, getdate(), @qc_date)-1
END

set @person_name=(select person_name from people where person_id=@person_id)

IF @person_name<>'' and @person_name is not null 
BEGIN
select  @separate=PATINDEX('%,%', @person_name)
select  @first_name=LEFT(@person_name,@separate-1 )
END 

SELECT person_id, person_name,ISNULL(@first_name, NULL) as first_name, userid, 
profile_id, [profile] as ic, @daydiff as qc_days,application_type AS user_type,
position_id,can_egrants,can_cft,can_mgt,can_docman,can_admin
FROM vw_people AS user_info 
WHERE userid=@operator 
--FOR XML AUTO, ELEMENTS

RETURN
------------------------------------------------------------------------------
GO


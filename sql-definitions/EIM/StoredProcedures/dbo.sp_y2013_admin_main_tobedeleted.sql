SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_y2013_admin_main_tobedeleted]

@Operator		varchar(50),
@Inst			varchar(10)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_y2013_admin_main									***/
/***	Description: This procedure will display instruction for admin		***/
/***	Created:	12/16/2013	Leon										***/
/***	Modifed:	11/24/2013	Leon										***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id	smallint,
@person_id	int,
@grant_id	int,
@sql		varchar(800),
@xmlout		varchar(max),
@X			Xml,
@count		int

SET @profile_id	=(select profile_id	from profiles where [profile]=@Inst)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

/** find user info***/
--declare @person_name	varchar(50)
--declare @first_name		varchar(50)
--declare @separate		int

--return user's info
SET @count=(SELECT COUNT(*) from vw_people where userid=LOWER('qians') and application_type='egrants' and (select COUNT(*) from vw_adm_menu_assignment where person_id=@person_id)>=1)

IF @count=1
BEGIN
SET @X = (
SELECT *, convert(varchar,getdate(),101) as today
FROM vw_people as user_info 
WHERE user_info.userid=@operator  
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



GO


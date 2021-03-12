SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_admin_flag_maintenance]

@act			varchar(50),
@flag_type		varchar(50),
@admin_code 	char(2),
@serial_num		int, 
@id_string		varchar(800),----could be appl_id, grant_id, gf_id, serial number, creator or date with mm/dd/yyyy
@ic				varchar(10),
@operator		varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_web_flag_maintenance								***/
/***	Description: This procedure supports flag maintenance web page.		***/
/***				 As of 12/2/2013, it will only work FDA flag			***/
/***	Created:	04/23/2013	Leon										***/
/***	Modified:	04/23/2013	Leon										***/
/***	Modified:	12/02/2013	Hareesh - updated description				***/
/***	Modified:	12/08/2014	Leon - modified								***/
/***	Modified:	01/09/2015	Leon - modified								***/
/***	Modified:	07/01/2015	Leon - modified	pagination					***/
/***	Modified:	08/11/2015	Leon - modified	all							***/
/***	Modified:	11/08/2016	Leon - modified	for MVC						***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id				smallint,
@person_id				int,
@grant_id				int,
@grant_num				varchar(50),
@sql					varchar(800),
@count					int,
@flag_application_code	char(1)
		
SET @profile_id	=(select profile_id	from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@operator and profile_id=@profile_id)
SET @act=LOWER(@act)
SET @flag_type=LOWER(@flag_type)
SET @id_string=LTRIM(RTRIM(@id_string))

--CREATE TABLE #g(grant_id int)
--CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, grant_id int)

---get flag_application_code
IF (@flag_type<>'' and @flag_type is not null) SET @flag_application_code=(select flag_application_code from dbo.Grants_Flag_Master where flag_type_code=@flag_type)

---get grant_id
IF (@serial_num is not null and @serial_num<>'') and (@admin_code is not null and @admin_code<>'')
BEGIN
SET @grant_id=(select grant_id from vw_grants where admin_phs_org_code=@admin_code and serial_num=@serial_num)
END

---for set up or remove flags and to save idstring
IF @act='setup_flags' or @act='remove_flags' and (@id_string<>'' and @id_string is not null) 
BEGIN
CREATE TABLE #f(id int)

-----to load gf_id by @id_string
IF @act='remove_flags' SET @sql='INSERT #f SELECT gf_id from vw_Grants_Flag_Construct where gf_id in (' + @id_string + ')'

-----to load grant_id by @id_string
IF @act='setup_flags' and @flag_application_code='G' SET @sql='INSERT #f SELECT grant_id from vw_grants where grant_id in (' + @id_string + ')'

----to load appl_id by @id_string
IF @act='setup_flags' and (@flag_application_code='B' or @flag_application_code='A' ) SET @sql='INSERT #f SELECT appl_id from vw_appls where appl_id in (' + @id_string + ')'
EXEC (@sql)
END 

---find act type
IF @act='show_flag' GOTO show_flag
IF @act='show_flags' GOTO show_flags
IF @act='show_appls' GOTO show_appls
IF @act='setup_flag' GOTO setup_flag
IF @act='setup_flags' GOTO setup_flags
IF @act='remove_flags' GOTO remove_flags
IF @act='show_grant_destructed' GOTO show_grant_destructed
IF @act='search_grant_destructed' GOTO search_grant_destructed

---------------------
show_flag:---show single flag with serial_num and admin_phs_org_code

IF @flag_type='' or @flag_type is null	
BEGIN
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,flag_application,UPPER(flag_type) as flag_type, flag_icon_namepath,null as flag,
CASE WHEN flag_application='G' THEN grant_num ELSE full_grant_num END AS full_grant_num
FROM vw_Grants_Flag_Construct 
WHERE grant_id=@grant_id
END
ELSE
BEGIN
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,flag_application,UPPER(flag_type) as flag_type, flag_icon_namepath,null as flag,
CASE WHEN @flag_application_code='G' THEN grant_num ELSE full_grant_num END AS full_grant_num
FROM vw_Grants_Flag_Construct 
WHERE grant_id=@grant_id and flag_type=@flag_type
END

RETURN
---------------------
show_flags:---show flags with @flag_type

IF @flag_type='' or @flag_type is null	
BEGIN
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,flag_application,UPPER(flag_type) as flag_type, flag_icon_namepath,null as flag,
CASE WHEN flag_application='G' THEN grant_num ELSE full_grant_num END AS full_grant_num
FROM vw_Grants_Flag_Construct 
END

ELSE	
										
BEGIN
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,flag_application,UPPER(flag_type) as flag_type, flag_icon_namepath, null as flag,
CASE WHEN @flag_application_code='G' THEN grant_num ELSE full_grant_num END AS full_grant_num
FROM vw_Grants_Flag_Construct 
WHERE flag_type=@flag_type 

END

RETURN
--------------------
show_appls:

SELECT distinct gf_id,a.serial_num,a.support_year,a.grant_id,a.appl_id,a.grant_num,g.flag_application,UPPER(g.flag_type) as flag_type, flag_icon_namepath,
CASE WHEN @flag_application_code='G' THEN a.grant_num ELSE a.full_grant_num END AS full_grant_num,
CASE WHEN flag_type=@flag_type THEN 'y' ELSE 'n' END AS flag
FROM vw_appls a left outer join vw_Grants_Flag_Construct g on a.appl_id=g.appl_id
WHERE a.grant_id=@grant_id and frc_destroyed=0
ORDER BY support_year desc

--SELECT grant_id,appl_id,serial_num,support_year,
--CASE WHEN @flag_application_code='G' THEN [appl].grant_num ELSE [appl].full_grant_num END AS full_grant_num,
--(SELECT CASE WHEN flag_type=@flag_type THEN 'y' ELSE null END AS flag
--FROM vw_Grants_Flag_Construct as flags WHERE appl_id=[appl].appl_id
--)
--FROM vw_appls [appl] 
--WHERE [appl].grant_id=@grant_id and frc_destroyed=0 

RETURN
-----------
setup_flag:---setup single flag with @grant_id 

SET @grant_num=(select grant_num from vw_grants where admin_phs_org_code=@admin_code and serial_num=@serial_num)
SET @count=(select COUNT(*) from dbo.Grants_Flag_Construct where Flag_type=UPPER(@flag_type) and Grant_id=@grant_id)

IF @count=0		---insert new data
BEGIN
INSERT dbo.Grants_Flag_Construct(Grant_id,Flag_type,Flag_Application,start_dt,created_by,created_dt,Last_updated_by,last_updated_date)
SELECT @grant_id,UPPER(@flag_type),UPPER(@flag_application_code), GETDATE(),@person_id,GETDATE(),@person_id,GETDATE()
END
ELSE			---reactive data
BEGIN
UPDATE dbo.Grants_Flag_Construct 
SET end_dt=null,created_by=@person_id,last_updated_by=@person_id,last_updated_date=GETDATE()
WHERE grant_id=@grant_id and flag_type=UPPER(@flag_type)
END

---update flag_persist
UPDATE dbo.Grants_Flag_Construct SET flag_persist=0 
FROM dbo.Grants_Flag_Construct f inner join dbo.Grants_Flag_Master m on f.flag_type=m.flag_type_code
WHERE grant_id=@grant_id and flag_type=UPPER(@flag_type) and flag_gen_type ='automatic'

RETURN
------------------------
setup_flags:---set up flags with appl_id

IF @flag_application_code='G'
BEGIN
--reactive data 
UPDATE dbo.Grants_Flag_Construct 
SET end_dt=NULL,Last_updated_by=@person_id,last_updated_date=GETDATE()		---created_by=@person_id
FROM dbo.Grants_Flag_Construct g inner join #f f on g.grant_id=f.id
WHERE flag_type=@flag_type 

--insert new data
INSERT dbo.Grants_Flag_Construct(grant_id,Flag_type,Flag_Application,start_dt,created_by,created_dt,Last_updated_by,last_updated_date)
SELECT grant_id,@flag_type,UPPER(@flag_application_code),GETDATE(),@person_id,GETDATE(),@person_id,GETDATE()
FROM vw_grants g inner join #f f on g.grant_id=f.id
WHERE f.id not in(select grant_id from dbo.Grants_Flag_Construct where flag_type=@flag_type)

---update flag_persist
UPDATE dbo.Grants_Flag_Construct SET flag_persist=0 
FROM dbo.Grants_Flag_Construct g inner join #f f on g.grant_id=f.id inner join dbo.Grants_Flag_Master m on g.flag_type=m.flag_type_code
WHERE grant_id=@grant_id and g.flag_type=UPPER(@flag_type) and m.flag_gen_type ='automatic'

END

IF @flag_application_code='B' or @flag_application_code='A'
BEGIN
--reactive data 
UPDATE dbo.Grants_Flag_Construct 
SET end_dt=NULL,Last_updated_by=@person_id,last_updated_date=GETDATE()		---created_by=@person_id
FROM dbo.Grants_Flag_Construct g inner join #f f on g.appl_id=f.id
WHERE flag_type=@flag_type 

--insert new data
INSERT dbo.Grants_Flag_Construct(appl_id,Grant_id,Flag_type,Flag_Application,start_dt,created_by,created_dt,Last_updated_by,last_updated_date)
SELECT appl_id,grant_id,@flag_type,UPPER(@flag_application_code),GETDATE(),@person_id,GETDATE(),@person_id,GETDATE()
FROM vw_appls a inner join #f f on a.appl_id=f.id
WHERE f.id not in(select appl_id from dbo.Grants_Flag_Construct where flag_type=@flag_type)

---update flag_persist
UPDATE dbo.Grants_Flag_Construct SET flag_persist=0 
FROM dbo.Grants_Flag_Construct g inner join #f f on g.appl_id=f.id inner join dbo.Grants_Flag_Master m on g.flag_type=m.flag_type_code
WHERE g.flag_type=UPPER(@flag_type) and m.flag_gen_type ='automatic'

END

RETURN
--------------------
remove_flags:

UPDATE dbo.Grants_Flag_Construct 
SET end_dt=GETDATE(),last_updated_date=GETDATE(),Last_updated_by=@person_id
WHERE gf_id in (select id from #f )

---update flag_persist
UPDATE dbo.Grants_Flag_Construct SET flag_persist=1 
FROM dbo.Grants_Flag_Construct g inner join dbo.Grants_Flag_Master m on g.flag_type=m.flag_type_code
WHERE gf_id in (select id from #f ) and flag_gen_type ='automatic'

RETURN
-----------------
show_grant_destructed:

SELECT appl_id,fgn, convert(varchar,creator_id) as creator, convert(varchar,created_date, 101) as created_date,exclusion_reason, created_date as sorting_date
FROM dbo.IMPAC_DESTRUCTION_EXCLUSION as appls 
ORDER BY sorting_date desc

RETURN
----------------
search_grant_destructed:

SELECT appl_id,fgn, convert(varchar,creator_id) as creator, convert(varchar,created_date, 101) as created_date,exclusion_reason, created_date as sorting_date
FROM dbo.IMPAC_DESTRUCTION_EXCLUSION 
WHERE fgn like '%'+@id_string+'%' or creator_id like '%'+@id_string+'%' or convert(varchar,created_date, 101) like '%'+@id_string+'%'
ORDER BY sorting_date desc

RETURN
----------------

GO


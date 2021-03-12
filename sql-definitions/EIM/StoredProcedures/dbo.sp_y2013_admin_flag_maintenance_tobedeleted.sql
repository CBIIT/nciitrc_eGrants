SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_admin_flag_maintenance_tobedeleted]

@act			varchar(50),
@flag_type		varchar(50),
@admin_code 	char(2),
@serial_num		int, 
@id_string		varchar(800),----could be appl_id, grant_id or gf_id
@current_tab 	int,
@current_page	int,
@Operator		varchar(50),
@Inst			varchar(10)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_y2013_flag_maintenance							***/
/***	Description: This procedure supports flag maintenance web page.		***/
/***				 As of 12/2/2013, it will only work FDA flag			***/
/***	Created:	04/23/2013	Leon										***/
/***	Modified:	04/23/2013	Leon										***/
/***	Modified:	12/02/2013	Hareesh - updated description				***/
/***	Modified:	12/08/2014	Leon - modified								***/
/***	Modified:	01/09/2015	Leon - modified								***/
/***	Modified:	07/01/2015	Leon - modified	pagination					***/
/***	Modified:	08/11/2015	Leon - modified	all							***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id				smallint,
@person_id				int,
@grant_id				int,
@grant_num				varchar(50),
@sql					varchar(800),
@count					int,
@flag_application_code	char(1),
@xmlout					varchar(max),
@X						Xml

SET @profile_id	=(select profile_id	from profiles where [profile]=@Inst)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

--return admin_phs_org_code
SET @X = (
SELECT DISTINCT admin_phs_org_code FROM vw_appls AS admin_phs_org_code FOR XML AUTO, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

SET @act=LOWER(@act)
SET @flag_type=LOWER(@flag_type)

---get flag_application_code
IF (@flag_type<>'' and @flag_type is not null) SET @flag_application_code=(select flag_application_code from dbo.Grants_Flag_Master where flag_type_code=@flag_type)

---get grant_id
IF (@serial_num is not null and @serial_num<>'') and (@admin_code is not null and @admin_code<>'')
BEGIN
SET @grant_id=(select grant_id from vw_grants where admin_phs_org_code=@admin_code and serial_num=@serial_num)
END

---find and save idstring
IF @id_string<>'' and @id_string is not null
BEGIN

CREATE table #f(id int)

---to load gf_id by @id_string
IF @act='remove_flag' SET @sql='INSERT #f SELECT gf_id from vw_Grants_Flag_Construct where gf_id in (' + @id_string + ')'

---to load grant_id by @id_string
IF @act='setup_flag' and @flag_application_code='G' SET @sql='INSERT #f SELECT grant_id from vw_grants where grant_id in (' + @id_string + ')'

--to load appl_id by @id_string
IF @act='setup_flag' and (@flag_application_code='B' or @flag_application_code='A' ) SET @sql='INSERT #f SELECT appl_id from vw_appls where appl_id in (' + @id_string + ')'
EXEC (@sql)
END 

--for pagination
declare @per_page			int
declare @per_tab			int
declare @start				int
declare @end				int
declare @total_pages		int
declare @total_tabs			int
declare @total_counts		int

/**set @current_page to defult of 1 if not entered**/
IF @current_page is null or @current_page='' SET @current_page=1
IF @current_tab is null or @current_tab='' SET @current_tab=1
SET @total_pages=1
SET @total_tabs=1
SET @per_page=50
SET @per_tab=30

CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, grant_id int)
CREATE TABLE #tabs (tab_number int)
CREATE TABLE #pages(tab_number int, page_number int)
CREATE TABLE #g(grant_id int)

---find act type
IF @act='' or @act is null or @act ='show_button' GOTO head

IF @act='show_flag' GOTO show
IF @act='show_appls' GOTO show_appls

IF @act='setup_flag' and @serial_num is not null and @admin_code is not null GOTO setup_flag
IF @act='setup_flag' and (@serial_num is null or @serial_num='') and (@admin_code is null or @admin_code='') GOTO setup_flags

IF @act='remove_flag' GOTO remove_flag
IF @act='create_flag' GOTO create_flag
---------------------
show:

IF (@serial_num is null or @serial_num='') and (@admin_code is null or @admin_code='') GOTO show_flags
IF @serial_num is not null and @admin_code is not null GOTO show_flag
----------------------
show_flags:

IF @flag_type is null or @flag_type =''	---show all kind flags
BEGIN
INSERT #t(grant_id) 
SELECT [grant].grant_id
FROM vw_Grants_Flag_Construct AS [grant] 
ORDER BY serial_num
END
ELSE									---show flags with @flag_type
BEGIN
INSERT #t (grant_id) 
SELECT [grant].grant_id
FROM vw_Grants_Flag_Construct AS [grant] 
WHERE flag_type=@flag_type 
ORDER BY serial_num
END

SET @total_counts=(select COUNT(*) from #t)

IF @total_counts<=@per_page 
BEGIN

SELECT tab_number,page_number FROM #pages ORDER BY tab_number FOR XML AUTO, TYPE, ELEMENTS 

SET @X = (
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,full_grant_num,flag_application,flag_type,flag_icon_namepath
FROM vw_Grants_Flag_Construct AS [grant] 
WHERE flag_type=@flag_type 
FOR XML AUTO, TYPE, ELEMENTS 
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

END

ELSE GOTO pagination

GOTO head
--------------------
pagination:

---set start page and end page
SET @start=@per_page * (@current_page-1) + 1
SET @end=@per_page * @current_page

---find total number from #t
SELECT @total_pages = ceiling(convert(real,@total_counts)/@per_page)
SELECT @total_tabs = ceiling(convert(real,@total_counts)/@per_page/@per_tab)

--insert all pages 
IF @total_tabs>=1 
BEGIN
DECLARE @i INT
SET @i=1
WHILE @i<=@total_pages
BEGIN
	INSERT #pages(page_number) SELECT @i
	SET @i=@i+1
END

UPDATE #pages SET tab_number=ceiling(convert(real,page_number)/@per_tab)

--insert all tabs 
INSERT #tabs(tab_number)SELECT distinct tab_number FROM #pages 

---return pagination data
SELECT tab_number,(select page_number from #pages as page where tab_number=tab.tab_number FOR XML AUTO, TYPE, ELEMENTS)
FROM #tabs as tab order by tab_number FOR XML AUTO, TYPE, ELEMENTS

INSERT #g(grant_id) SELECT grant_id FROM #t WHERE #t.id between @start and @end 

END

IF @flag_type is null or @flag_type =''	---show all kind flags
BEGIN
SET @X = (
SELECT distinct gf_id,serial_num,[grant].grant_id,appl_id,grant_num,full_grant_num,flag_application,flag_type,flag_icon_namepath
FROM vw_Grants_Flag_Construct as [grant] inner join #g g On [grant].grant_id=g.grant_id
FOR XML AUTO, TYPE, ELEMENTS 
)
END
ELSE									---show flags with @flag_type
BEGIN
SET @X = (
SELECT distinct gf_id,serial_num,[grant].grant_id,appl_id,grant_num,full_grant_num,flag_application,flag_type,flag_icon_namepath
FROM vw_Grants_Flag_Construct as [grant] inner join #g g On [grant].grant_id=g.grant_id
WHERE flag_type=@flag_type 
FOR XML AUTO, TYPE, ELEMENTS 
)
END

select @xmlout = cast(@X as varchar(max))
select @xmlout

GOTO head
---------------------
show_flag:---show single flag with flag_type, serial_num and admin_phs_org_code

---return empty tabs data recordset
SELECT tab_number,page_number FROM #pages ORDER BY tab_number FOR XML AUTO, TYPE, ELEMENTS 

IF @flag_type is null or @flag_type =''	---show all kind flags
BEGIN
SET @total_counts=(SELECT COUNT(*) FROM vw_Grants_Flag_Construct WHERE grant_id=@grant_id)
SET @X = (
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,full_grant_num,flag_application,flag_type,flag_icon_namepath
FROM vw_Grants_Flag_Construct as [grant] 
WHERE grant_id=@grant_id
FOR XML AUTO, TYPE, ELEMENTS 
)
END
ELSE
BEGIN
SET @total_counts=(SELECT COUNT(*) FROM vw_Grants_Flag_Construct WHERE grant_id=@grant_id and flag_type=@flag_type)
SET @X = (
SELECT distinct gf_id,serial_num,grant_id,appl_id,grant_num,full_grant_num,flag_application,flag_type,flag_icon_namepath 
FROM vw_Grants_Flag_Construct as [grant] 
WHERE grant_id=@grant_id and flag_type=@flag_type
FOR XML AUTO, TYPE, ELEMENTS 
)
END

select @xmlout = cast(@X as varchar(max))
select @xmlout

GOTO head
--------------------
show_appls:---show all appls to setup flags

SET @X=(
SELECT grant_id,appl_id,serial_num,support_year,
CASE WHEN @flag_application_code='G' THEN [appl].grant_num ELSE [appl].full_grant_num END AS full_grant_num,
(SELECT CASE WHEN flag_type=@flag_type THEN 'y' ELSE null END AS flag
FROM vw_Grants_Flag_Construct as flags WHERE appl_id=[appl].appl_id
FOR XML AUTO, TYPE, ELEMENTS
)
FROM vw_appls [appl] 
WHERE [appl].grant_id=@grant_id and frc_destroyed=0 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

GOTO head

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

SET @X = (
Select UPPER(@flag_type)+ ' flag for '+@grant_num+' has been setup' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
--------------------
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

SET @X = (
Select UPPER(@flag_type)+ ' flag(s) has been setup for selected grant year(s)' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head
--------------------
remove_flag:

UPDATE dbo.Grants_Flag_Construct 
SET end_dt=GETDATE(),last_updated_date=GETDATE(),Last_updated_by=@person_id
WHERE gf_id in (select id from #f )

---update flag_persist
UPDATE dbo.Grants_Flag_Construct SET flag_persist=1 
FROM dbo.Grants_Flag_Construct g inner join dbo.Grants_Flag_Master m on g.flag_type=m.flag_type_code
WHERE gf_id in (select id from #f ) and flag_gen_type ='automatic'

SET @X = (
Select UPPER(@flag_type)+' flag(s) has been removed from selected grant(s)' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head
------------------
create_flag:

--declare @icon_type varchar(5)

--SET @flag_application_code=ltrim(rtrim(@admin_code))
--SET @icon_type =ltrim(rtrim(@id_string))	

--INSERT dbo.Grants_Flag_Master(flag_type_code, flag_application_code,flag_icon_namepath, [start_date],flag_gen_type,created_by_person_id)
--SELECT @flag_type,@flag_application_code,'/images/flag_'+@flag_type+'.'+@icon_type,GETDATE(),'manual',@person_id

GOTO head
------------------
head:

--return user's meun 
SET @X = (
SELECT menu_id,menu_title,menu_url,menu_hover 
FROM dbo.vw_adm_menu_assignment as menu 
WHERE person_id=@person_id and menu_url is not null
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--return all flags type info
SET @X = (
SELECT flag_type_code as flag_type,flag_application_code as flag_application
FROM Grants_Flag_Master AS flag
WHERE end_date is null
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

/**return all searching info**/
SET @X = (
SELECT 
@total_counts	as total_counts,
@total_tabs		as total_tabs,
@total_pages	as total_pages,
@per_tab		as per_tab,
@per_page		as per_page,
@current_page	as current_page
FROM performance as searching_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--check permission
SET @count=(select COUNT(*) FROM vw_people p inner join vw_adm_menu_assignment a on p.person_id=a.person_id
WHERE p.userid=@operator and application_type='egrants' and menu_title='Flag Maintenance')

--find user info
IF @count=1
BEGIN
SET @X = (
SELECT *,convert(varchar,getdate(),101) as today
FROM vw_people as user_info
WHERE userid=LOWER(@operator)
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
END
ELSE
BEGIN
SELECT null
END


GO


SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_egrants_access_control_06_07_2019]

@act  				varchar(20),
@index_id			int,
@active_id			int,
@user_id			int,
@login_id			varchar(50),
@first_name			varchar(50),
@middle_name		varchar(20),
@last_name			varchar(50),
@email_address		varchar(100),
@phone_number		varchar(10),
@coordinator_id		int,
@position_id		smallint,
@ic_id				smallint,
@egrants_tab		int,
@mgt_tab			int,
@admin_tab			int,
@docman_tab			int,
@cft_tab			int,
@dashboard_tab		int,
@iccoord_tab		int,
@is_coordinator		bit,
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50)

AS
/************************************************************************************************************/
/***									 											***/
/***	Procedure Name:sp_web_egrants_access_control (sp_web_admin_user_edit)	***/
/***	Description:edit user info													***/
/***	Created:	08/23/2013	Leon												***/
/***	Modified:	12/12/2013	Leon	added @active_type							***/
/***	Modified:	03/28/2014	Leon	added start_date, end_date					***/
/***	Modified:	09/16/2014	Leon	change code for visual studio				***/
/***	Modified:	10/04/2016	Leon	change code for .net MVC					***/
/***	Modified:	01/24/2019	Leon	add code for iccoord_tab					***/
/************************************************************************************************************/
SET NOCOUNT ON

declare @person_id			int
declare @profile_id			int 
declare @profile			varchar(10)
declare @people_history_id	int
declare @user_name			varchar(20)

SET @profile_id	=(SELECT profile_id	FROM profiles WHERE [profile]=@ic)
SET @person_id=(SELECT person_id FROM people WHERE userid=@Operator and profile_id=@profile_id)

IF @login_id <>'' and @login_id is not null SET @login_id=rtrim(ltrim(@login_id))
IF @user_name <>'' and @user_name is not null SET @user_name=rtrim(ltrim(@user_name))
IF @end_date='' or @end_date is null SET @end_date=null
IF @is_coordinator is null set @is_coordinator=0
IF @is_coordinator=1 set @coordinator_id=null

/**set default act**/
if (@act='load') goto to_load
if (@act ='create') or (@act ='create_by_request') goto to_create
if (@act ='update') goto to_update
if (@act ='search') goto to_search
if (@act ='active') goto to_active
if (@act ='inactive') goto to_inactive
--if (@act='review') goto to_view
----------------------
--to_view:--review new user data by ic coordinator created

--select 
--id as person_id, 
--user_login as userid, 
--user_fname as first_name, 
--user_lname as last_name,
--user_mi as middle_name,
--email, phone_number, 
--cord_id as coordinator_id, 
--1 as profile_id, 
--access_type as active, 
--pp.position_id, 
--pp.position_name, 
--'nci' as ic, 
--convert(varchar,start_date,101) as start_date, 
--end_date 
--FROM dbo.cord_manager inner join people_positions as pp 
--on dbo.fn_get_position_id(access_type) = pp.position_id 
--WHERE id = @user_id

--RETURN
------------------
to_create:

---create new user's data
INSERT INTO people 
(
userid,
active,
application_type,
position_id, 
profile_id,
first_name,
middle_initial,
last_name,
person_name,
email,
phone_number,
egrants,
mgt,
[admin],
iccoord,
docman,
cft,
dashboard,
is_coordinator,
ic_coordinator_id,
[start_date],
[end_date],
Created_by,Created_date,
last_updated_by,last_updated_date
)
SELECT 
@login_id,
@active_id, 
'egrants',
@position_id,
@ic_id,
@first_name,
ISNULL(@middle_name, null),
@last_name,
@last_name+', '+@first_name,
ISNULL(@email_address,null),
ISNULL(@phone_number,null),
ISNULL(@egrants_tab,0),
ISNULL(@mgt_tab,0),
ISNULL(@admin_tab,0),
ISNULL(@docman_tab,0),
ISNULL(@cft_tab,0),
ISNULL(@iccoord_tab,0),
ISNULL(@dashboard_tab,0),
@is_coordinator,
@coordinator_id,
GETDATE(),
isnull(@end_date,null),
@person_id,GETDATE(),
@person_id,GETDATE()

if @act ='create_by_request' 
begin
UPDATE dbo.cord_manager SET status='approved' WHERE user_login=@login_id  and disabled_date is null
end

RETURN
-------------------------
to_update:

---update user's data
UPDATE people 
SET 
userid=@login_id,
first_name=@first_name,
middle_initial=ISNULL(@middle_name,null),
last_name=ISNULL(@last_name,null),
person_name=@last_name+', '+@first_name,
phone_number=ISNULL(@phone_number,null),
email=ISNULL(@email_address,null),
position_id=@position_id,
egrants=ISNULL(@egrants_tab,0),
mgt=ISNULL(@mgt_tab,0),
[admin]=ISNULL(@admin_tab,0),
iccoord = ISNULL(@iccoord_tab,0),
docman=ISNULL(@docman_tab,null),
cft=ISNULL(@cft_tab,null),
dashboard = ISNULL(@dashboard_tab,0),
ic_coordinator_id=ISNULL(@coordinator_id,null), 
is_coordinator=ISNULL(@is_coordinator,0),
end_date=ISNULL(@end_date,null),	
last_updated_by=@person_id,
last_updated_date=GETDATE()
WHERE person_id=@user_id

IF @is_coordinator=0 and @coordinator_id>0 
BEGIN
UPDATE people set ic_coordinator_id=@coordinator_id, is_coordinator=0 WHERE person_id=@user_id
END
ELSE IF @is_coordinator=1
BEGIN
UPDATE people set ic_coordinator_id=null, is_coordinator=1 WHERE person_id=@user_id
END

RETURN
-------------------------
to_active:

/**update user's data**/
update people set active=1,last_updated_by=@person_id,last_updated_date=GETDATE() where person_id=@user_id

RETURN
-------------------------
to_inactive:

/**update user's data**/
update people set active=0,end_date=GETDATE(),last_updated_by=@person_id,last_updated_date=GETDATE() where person_id=@user_id

RETURN
-------------------------
to_search:

SELECT 
person_id,
userid,
person_name,
first_name,
last_name,
middle_initial as middle_name,
email,
phone_number,
ic_coordinator_id as coordinator_id,
is_coordinator,
UPPER(@ic) as ic,
active,
application_type,
CASE WHEN egrants>0 THEN 'y' ELSE 'n' END as can_egrants,
CASE WHEN [admin]>0 THEN 'y' ELSE 'n' END as can_admin,
CASE WHEN mgt>0 THEN 'y' ELSE 'n' END as can_mgt,
CASE WHEN docman>0 THEN 'y' ELSE 'n' END as can_docman,
CASE WHEN cft>0 THEN 'y' ELSE 'n' END as can_cft,
CASE WHEN iccoord>0 THEN 'y' ELSE 'n' END as can_iccoord,
CASE WHEN dashboard>0 THEN 'y' ELSE 'n' END as can_dashboard,
convert(varchar,start_date,101)as [start_date], 
convert(varchar,end_date,101)as end_date, 
CASE 
	WHEN end_date is null 
	THEN 'active' 
	ELSE 'inactive' 
	END 
as [status],
pp.position_id,
pp.position_name
FROM dbo.people AS [user] inner join people_positions as pp on [user].position_id=pp.position_id
WHERE person_id=@user_id 

RETURN
--------------------------
to_load:

-----display all users data 
IF @active_id = 2 
BEGIN
SELECT distinct
person_id,
userid,
person_name,
first_name,
last_name,
middle_initial as middle_name,
email,
phone_number,
ic_coordinator_id as coordinator_id,
is_coordinator,
UPPER(@ic) as ic,
active,
application_type,
CASE WHEN egrants>0 THEN 'y' ELSE 'n' END as can_egrants,
CASE WHEN [admin]>0 THEN 'y' ELSE 'n' END as can_admin,
CASE WHEN mgt>0 THEN 'y' ELSE 'n' END as can_mgt,
CASE WHEN docman>0 THEN 'y' ELSE 'n' END as can_docman,
CASE WHEN cft>0 THEN 'y' ELSE 'n' END as can_cft,
CASE WHEN iccoord>0 THEN 'y' ELSE 'n' END as can_iccoord,
CASE WHEN dashboard>0 THEN 'y' ELSE 'n' END as can_dashboard,
convert(varchar,[start_date],101) as [start_date],
convert(varchar,end_date,101) as end_date,
pp.position_id,
pp.position_name
FROM dbo.character_index c, people as [user] inner join people_positions as pp on [user].position_id=pp.position_id
WHERE application_type='egrants' and last_name is not null and first_name is not null 
and LEFT(last_name,1)=c.character_index and profile_id=@profile_id and c.index_id=@index_id
ORDER BY person_name
END

ELSE
	
---display active or inactive users data 
BEGIN
SELECT 
person_id,
userid,
person_name,
first_name,
last_name,
middle_initial as middle_name,
email,
phone_number,
ic_coordinator_id as coordinator_id,
is_coordinator,
UPPER(@ic) as ic,
active,
application_type,
CASE WHEN egrants>0 THEN 'y' ELSE 'n' END as can_egrants,
CASE WHEN [admin]>0 THEN 'y' ELSE 'n' END as can_admin,
CASE WHEN mgt>0 THEN 'y' ELSE 'n' END as can_mgt,
CASE WHEN docman>0 THEN 'y' ELSE 'n' END as can_docman,
CASE WHEN cft>0 THEN 'y' ELSE 'n' END as can_cft,
CASE WHEN iccoord>0 THEN 'y' ELSE 'n' END as can_iccoord,
CASE WHEN dashboard>0 THEN 'y' ELSE 'n' END as can_dashboard,
convert(varchar,[start_date],101) as [start_date],
convert(varchar,end_date,101) as end_date,
pp.position_id,
pp.position_name
FROM dbo.character_index c, people as [user] inner join people_positions as pp on [user].position_id=pp.position_id
WHERE application_type='egrants' and last_name is not null and first_name is not null and active=@active_id and
LEFT(last_name,1)=c.character_index and profile_id=@profile_id	and c.index_id=@index_id
ORDER BY person_name
END

RETURN
-------------------------

GO


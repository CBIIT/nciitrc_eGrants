SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_admin_supplement_tobedeleted]

@act				varchar(50),
@pa					nchar(10),
@detail				varchar(8000),	---could be email body
@id					int,
@name				varchar(200),		---could be pa new name, full grant num, template_name or email CC
@subject			varchar(200),
@current_page		int,
@Operator			varchar(50),
@Inst				varchar(10)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_y2013_admin_supplement							***/
/***	Description: for show, create or edit supplement					***/
/***	Created:	12/10/2014	Leon										***/
/***	Modified:	12/16/2014	Leon										***/
/***	Modified:	04/17/2014	Leon										***/
/***	Modified:	12/05/2015	Leon	run show_notification after deleted	***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id	smallint,
@person_id	int,
@appl_id	int,
@count		int,
@xmlout		varchar(max),
@X			Xml

SET @profile_id	=(select profile_id	from profiles where [profile]=@Inst)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

SET @pa=ltrim(rtrim(@pa))
SET @name=ltrim(rtrim(@name))

--check permisson
SET @count=(select COUNT(*) FROM vw_people p inner join vw_adm_menu_assignment a on p.person_id=a.person_id
WHERE p.userid=@operator and application_type='egrants' and menu_title='Supplement')

--return user's info
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

/** find user info***/
--declare @person_name	varchar(50)
--declare @first_name		varchar(50)
--declare @separate		int

----find user name
--SET @person_name=(select person_name from people where person_id=@person_id)
--IF @person_name<>'' and @person_name is not null 
--BEGIN
--select  @separate=PATINDEX('%,%', @person_name)
--select  @first_name=LEFT(@person_name,@separate-1 )
--END 

----return user's info
--SELECT distinct p.person_id, p.person_id, p.person_name,ISNULL(@first_name, NULL) as first_name,p.userid,profile_id,[profile] as ic,
--admin_phs_org_code,position_id,convert(varchar,getdate(),101) as today,can_egrants,can_egrants_upload,can_cft,can_mgt,can_docman,can_admin
--FROM vw_people p inner join vw_adm_menu_assignment a on p.person_id=a.person_id
--WHERE p.userid=@operator and application_type='egrants' and menu_title='Supplement' 

if @act='' or @act is null or @act='add_et' GOTO head
if @act='status_pa' GOTO status_pa

---nt = notification
if @act='show_nt' GOTO show_notification
if @act='search_nt' GOTO search_notification
if @act='delete_nt' GOTO delete_notification
if @act='edit_nt' GOTO edit_notification

--en = email notification
if @act='resent_en' GOTO resent_email_notification

--et = email template
if @act='show_et' GOTO show_et
if @act='create_et' GOTO create_et
if @act='edit_et' GOTO edit_et

--eu = workflow
if @act='show_eu' GOTO show_eu
if @act='save_eu' GOTO save_eu
if @act='create_eu' GOTO create_eu
if @act='clone_eu' GOTO clone_eu
if @act='delete_eu' GOTO delete_eu

-----------------
show_notification:

declare @sql				varchar(800)
declare @total_appls		int
declare @total_pages		int
declare @total_tabs			int
declare @per_page			int
declare @per_tab			int
declare @start				int
declare @end				int

SET @per_page=30
SET @per_tab=30

---create table to save data
CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, nt_id int, full_grant_num varchar(20),appl_id int, pa varchar(10), subjectLine varchar(200),NotificationBody varchar(2000), NotRcvd_dt smalldatetime, created_date smalldatetime)
CREATE TABLE #tabs (tab_number int)
CREATE TABLE #pages (tab_number int, page_number int)

---get all data from dbo.adsup_notification follow the order
SET @sql='insert #t (nt_id,full_grant_num,appl_id, pa, subjectLine, NotificationBody, NotRcvd_dt, created_date) 
select id,full_grant_num,appl_id, pa, subjectLine, NotificationBody, NotRcvd_dt, created_date 
from dbo.adsup_notification 
where Full_grant_num is not null and disabled_date is null
order by created_date desc' 
EXEC(@sql)

--find all searching info
--IF @current_tab is null or @current_tab='' SET @current_tab=1
IF @current_page is null or @current_page='' SET @current_page=1
SET @total_appls=(SELECT count(*)FROM #t)
SET @total_pages = ceiling(convert(real,@total_appls)/@per_page)
SET @total_tabs = ceiling(convert(real,@total_appls)/@per_page/@per_tab)

--return all searching info
SET @X = (
SELECT 
@total_appls	as total_appls,
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

--insert tabs and pages data
DECLARE @i INT
SET @i=1
WHILE @i<=@total_pages
BEGIN
	INSERT #pages(page_number) SELECT @i
	SET @i=@i+1
END
UPDATE #pages SET tab_number=ceiling(convert(real,page_number)/@per_tab)

INSERT #tabs(tab_number) SELECT distinct tab_number FROM #pages 

--display all tabs and pages data
select tab_number,
(select page_number from #pages as page where tab_number=tab.tab_number FOR XML AUTO, TYPE, ELEMENTS)
from #tabs as tab order by tab_number FOR XML AUTO, TYPE, ELEMENTS

--set current_page, start page and end page
SET @start=@per_page * (@current_page-1) + 1
SET @end=@per_page * @current_page

--return Notifications after user searching
SET @X = (
SELECT nt_id,full_grant_num,appl_id,pa,subjectLine,NotificationBody,dbo.fn_adsupp_OGANOTSENT_Status(nt_id)as [status],
convert(varchar,NotRcvd_dt,101) as recieved_date,convert(varchar,created_date,01) as created_date 
FROM #t as [notification]
WHERE id between @start and @end
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
-----------------------
search_notification:

--return Notifications by searil_number searching
SET @X = (
SELECT id as 'nt_id',Full_grant_num AS full_grant_num,appl_id,pa,subjectLine,NotificationBody,subjectLine,NotificationBody,
convert(varchar,NotRcvd_dt,101) as recieved_date,convert(varchar,created_date,01) as created_date, dbo.fn_adsupp_OGANOTSENT_Status(id) as [status]
FROM dbo.adsup_notification as notification
WHERE Full_grant_num like '%'+convert(varchar, @id) +'%'
ORDER BY created_date DESC
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
-----------------------
status_pa:

SET @X = (
select distinct ltrim(rtrim(email_position_code)) as position_code from dbo.adsup_email_position_master as position FOR XML AUTO, ELEMENTS
--select distinct position from dbo.adsup_Notification_email_status as position FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

SET @X = (
select full_grant_num, pa,id,
(select document_id,CONVERT(varchar,document_date,101)as document_date,url,c.category_name+'' as category_name
from documents doc,categories c
where doc.category_id=c.category_id and appl_id in( select dbo.adsup_notification.appl_id from dbo.adsup_notification where id=@id)
FOR XML AUTO, TYPE, ELEMENTS)
FROM dbo.adsup_notification as pa where id=@id
FOR XML AUTO, type, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--check email status for PA
SET @X = (
select id,email as email_type, position,person_name,email_address,
convert(varchar,created_date,101) as created_date,
convert(varchar,email_date,101) as email_date,
email_send_status,convert(varchar,reply_recieved_date,101) as reply_recieved_date,reply_status 
from adsup_Notification_email_status as email
where Notification_id=@id
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
-----------------------
edit_notification:

SET @appl_id=(select appl_id from vw_appls where full_grant_num=@name)
IF @appl_id is null 
BEGIN
SET @X = (
SELECT 'The full grant number youin serted is incorrect' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout
END

SET @count=(select count(distinct pa) from dbo.adsup_notification where pa=@pa)
IF @count=0
BEGIN
SET @X = (
SELECT 'The PA you inserted is incorrect' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout
END

IF @count=1 and @appl_id is not null 
BEGIN
UPDATE dbo.adsup_notification SET appl_id=@appl_id,pa=@pa,full_grant_num=@name,Last_updated_by=@person_id,Last_updated_date=GETDATE() WHERE id=@id
SET @X = (
SELECT 'The data for the notification you seleted has been edited' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout
END

GOTO show_notification

RETURN
--------------------------
delete_notification:

update dbo.adsup_notification set disabled_date=GETDATE(),disabled_by_personid=@person_id where id=@id

SET @X = (
SELECT 'The notiofication for '+ Full_grant_num +' has been deleted' as info 
FROM dbo.adsup_notification AS return_info 
WHERE id=@id
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO show_notification

RETURN
--------------------------
resent_email_notification:

DECLARE @email table(
email_type			char(2),
person_position		nchar(10),
person_name			varchar(50),
email_address		varchar(300)
)

DECLARE @DocHandle int
DECLARE @XmlDocument nvarchar(1000)

SET @detail=(SELECT REPLACE(@detail,'(','<'))
SET @detail=(SELECT REPLACE(@detail,')','>'))

--set @detail ='<email email_type ="to" position="PI" person_name="qians" email_address="qians@mail.nih.gov"></email>
--<email email_type ="cc" position="PD" person_name="qians" email_address="qians@mail.nih.gov"></email>'

SET @XmlDocument = N'<ROOT>'+char(39) + @detail + char(39)+'</ROOT>'
-- Create an internal representation of the XML document.

EXEC sp_xml_preparedocument @DocHandle OUTPUT, @XmlDocument
-- Execute a SELECT statement using OPENXML rowset provider.

INSERT @email (email_type,person_position,person_name,email_address)
SELECT email_type,person_position,person_name,email_address
FROM OPENXML (@DocHandle, '/ROOT/email',1)
      WITH (email_type		char(2),
            person_position	nchar(10),
			person_name		varchar(50),
            email_address	varchar(500)
            )
EXEC sp_xml_removedocument @DocHandle

declare @notification_id		int
declare @email_template_id		int

--find latest notification_id and email_template_id
SET @notification_id=(SELECT TOP 1 notification_id
FROM dbo.adsup_Notification_email_status
WHERE notification_id=@id
ORDER BY email_date desc
)

SET @email_template_id=(SELECT TOP 1 email_template_id
FROM dbo.adsup_Notification_email_status
WHERE notification_id=@id
ORDER BY email_date desc
)

INSERT dbo.adsup_Notification_email_status(notification_id,email,position,person_name,email_address,email_template_id,created_date)
SELECT @notification_id,email_type,person_position,person_name,email_address,@email_template_id,GETDATE()
FROM @email 

SET @X = (
SELECT 'New email notification has been created' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
------------------
show_et:

--return email master
SET @X = (
SELECT id,ltrim(rtrim(template_name)) as template_name,[subject], body --dbo.fn_clean_characters(body) as 
FROM  dbo.adsup_email_master as email
WHERE id=@id
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
--------------------
create_et:

INSERT dbo.adsup_email_master (template_name,[subject],body,created_by_person_id,created_date)
SELECT @name,@subject,@detail,@person_id,GETDATE()

SET @X = (
SELECT 'Email template '+@name+' has been created' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
------------------
edit_et:

UPDATE dbo.adsup_email_master
SET template_name=@name,[subject]=@subject,body=@detail,modified_by=@person_id,modified_date=GETDATE()
WHERE id=@id 

SET @X = (
SELECT 'Email template '+ltrim(rtrim(@name))+' has been edited' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
-----------------------
show_eu:

SET @count=(SELECT count(*) FROM dbo.adsup_email_rules er WHERE pa=@pa)

IF @count=0
--display data to create workflow
BEGIN
SET @X = (
SELECT ltrim(rtrim(pa)) as pa From dbo.adsup_pa_master AS er WHERE pa=@pa
FOR XML AUTO, ELEMENTS
)
END

ELSE
--display current workflow to edit
BEGIN
SET @X = (
SELECT ltrim(rtrim(pa))as pa,email_to,email_cc,convert(varchar,start_date,101) as 'start_date',convert(varchar,end_date,101) as 'end_date',email_template_id,
(SELECT template_name,[subject], body, person_name 
FROM dbo.adsup_email_master em, vw_people vp 
WHERE id=er.email_template_id and created_by_person_id = vp.person_id FOR XML AUTO,TYPE,ELEMENTS)
FROM dbo.adsup_email_rules as er 
WHERE pa=@pa
FOR XML AUTO, ELEMENTS
)
END

SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
------------------
create_eu:

SET @count=(SELECT COUNT(*) FROM dbo.adsup_email_rules WHERE pa=@pa)

IF @count=0----create new workfolw
BEGIN
INSERT dbo.adsup_email_rules(pa,email_to,email_cc,created_by_person_id,created_date,email_template_id)
SELECT @pa,@subject,@name,@person_id,GETDATE(),@id 

SET @X = (
SELECT 'The workfolw for ' + @pa + ' has been created' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
END
ELSE	--return message
BEGIN
SET @X = (
SELECT 'The workfolw for ' + @pa + ' has already been created' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
END

GOTO head

RETURN
---------------
save_eu:

SET @count=(SELECT COUNT(*) FROM dbo.adsup_email_rules WHERE pa=@pa)

IF @count=0 GOTO create_eu ---to create new workfolw
ELSE---to edit current workfolw
BEGIN
UPDATE dbo.adsup_email_rules
SET email_template_id=@id,email_to=@subject,email_cc=@name,last_modified_by=@person_id,last_modified_date=GETDATE()
WHERE pa=@pa

SET @X = (
SELECT 'The current workfolw for '+@pa+' has been edited and saved' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
END

SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
---------------
clone_eu:

SET @count=(SELECT count(*) FROM dbo.adsup_email_rules er WHERE pa=convert(nchar,@name))

IF @count=0
---to clone new workflow
BEGIN
INSERT dbo.adsup_email_rules(pa, email_to,email_cc,start_date, created_by_person_id,created_date,email_template_id)
SELECT convert(nchar,@name),er.email_to,er.email_cc,GETDATE(),@person_id,GETDATE(),er.email_template_id
FROM dbo.adsup_email_rules as er WHERE pa=@pa

SET @X = (
SELECT ltrim(rtrim(@name)) + ' has been cloned workflow by ' + @pa as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
END

ELSE
---to update workflow
BEGIN
UPDATE dbo.adsup_email_rules 
SET
email_to= (select email_to FROM dbo.adsup_email_rules WHERE pa = @pa),
email_cc= (select email_cc FROM dbo.adsup_email_rules WHERE pa = @pa),
last_modified_by=@person_id,
last_modified_date=GETDATE(),
email_template_id=(select email_template_id FROM dbo.adsup_email_rules WHERE pa = @pa)
WHERE pa = convert(nchar,@name)

SET @X = (
SELECT ltrim(rtrim(@name)) + ' workflow has been cloned and updated by ' + @pa as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
END

SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
---------
delete_eu:

---to delete workflow
UPDATE dbo.adsup_email_rules 
SET end_date=GETDATE(),last_modified_by=@person_id,last_modified_date=GETDATE()
WHERE pa=@pa

SET @X = (
SELECT 'Current workflow for '+@pa+' has been deleted' as info From performance AS return_info
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

GOTO head

RETURN
----------------
head:

--return user's meun 
SET @X = (
SELECT menu_id,menu_title,menu_url,menu_hover 
FROM  dbo.vw_adm_menu_assignment as menu 
WHERE person_id=@person_id and menu_url is not null
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--return email rules by adsup_email_rules
SET @X = (
SELECT id, ltrim(rtrim(pa)) as pa 
FROM dbo.adsup_email_rules as et
WHERE end_date is null
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

--return email master 
SET @X = (
SELECT ltrim(rtrim(template_name)) as template_name,id,[subject],dbo.fn_clean_characters(body) as body,created_by_person_id,convert(varchar,created_date,101) as created_date 
FROM dbo.adsup_email_master as em
FOR XML AUTO, ELEMENTS
)
SELECT @xmlout = cast(@X as varchar(max))
SELECT @xmlout

RETURN






GO


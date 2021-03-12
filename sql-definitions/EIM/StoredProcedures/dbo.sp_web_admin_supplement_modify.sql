SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_admin_supplement_modify]

@act				varchar(50),
@pa					nchar(10),
@detail				varchar(8000),		---could be email body, full grant num
@id					int,	
@name				varchar(200),		---could be pa new name, full grant num, template_name or email CC
@subject			varchar(200),		---could be serial number
@ic					varchar(10),
@Operator			varchar(50),
@return_notice		varchar(200) OUTPUT

AS
/******************************************************************************/
/***									 									***/
/***	Procedure Name: sp_web_admin_supplement_modify						***/
/***	Description: for show, create or edit supplement					***/
/***	Created:	12/10/2014	Leon										***/
/***	Modified:	12/16/2014	Leon										***/
/***	Modified:	04/17/2014	Leon										***/
/***	Modified:	12/05/2015	Leon	run show_notification after deleted	***/
/***	Modified:	11/29/2016	Leon	modified for MVC					***/
/******************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id	smallint,
@person_id	int,
@appl_id	int,
@count		int,
@xmlout		varchar(max),
@X			Xml,
@current_page	varchar(100),
@fgn			varchar(20)

SET @person_id=(select person_id from vw_people where userid=@Operator)

SET @pa=LTRIM(rtrim(@pa))
SET @name=LTRIM(rtrim(@name))
SET @subject=LTRIM(rtrim(@subject))
SET @detail	=LTRIM(rtrim(@detail))

---nt = notification
if @act='delete_notification' GOTO delete_notification
if @act='edit_notification' GOTO edit_notification

--en = email notification
if @act='resent_email_notification' GOTO resent_email_notification

--et = email template
--if @act='show_et' GOTO show_et
if @act='create_email_template' GOTO create_email_template
if @act='edit_email_template' GOTO edit_email_template

--eu = workflow
--if @act='show_eu' GOTO show_eu
if @act='create_email_rule' GOTO create_email_rule
if @act='save_email_rule' GOTO save_email_rule
if @act='clone_email_rule' GOTO clone_email_rule
if @act='delete_email_rule' GOTO delete_email_rule
-----------------------
edit_notification:

SET @count=(select count(appl_id) from vw_appls where full_grant_num=@name)
IF @count=0
BEGIN
SET @return_notice='The full grant number youin inserted is incorrect' 
END
ELSE
BEGIN
SET @appl_id=(select appl_id from vw_appls where full_grant_num=@name)
END

IF @appl_id is not null SET @count=(select count(distinct pa) from dbo.adsup_notification where pa=@pa)
IF @count=0
BEGIN
SET @return_notice='The PA you inserted is incorrect' 
END

IF @count=1 and @appl_id is not null 
BEGIN
UPDATE dbo.adsup_notification SET appl_id=@appl_id,pa=@pa,Full_grant_num=@name, Last_updated_by=@person_id,Last_updated_date=GETDATE() WHERE id=@id
SET @return_notice='The notification for '+ @name +' has been edited' 
END

SELECT @return_notice

RETURN
--------------------------
delete_notification:

SET @fgn=(SELECT Full_grant_num FROM dbo.adsup_notification WHERE id=@id)
update dbo.adsup_notification set disabled_date=GETDATE(),disabled_by_personid=@person_id where id=@id

SET @return_notice='The notiofication for '+ @fgn +' has been deleted'

SELECT @return_notice

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

SET @return_notice='New email notification has been created' 

SELECT @return_notice

RETURN
--------------------
create_email_template:

INSERT dbo.adsup_email_master (template_name,[subject],body,created_by_person_id,created_date)
SELECT @name,@subject,@detail,@person_id,GETDATE()

SET @return_notice = 'Email template '+@name+' has been created' 

SELECT @return_notice

RETURN
------------------
edit_email_template:

UPDATE dbo.adsup_email_master
SET template_name=@name,[subject]=@subject,body=@detail,modified_by=@person_id,modified_date=GETDATE()
WHERE id=@id 

SET @return_notice = 'Email template '+ltrim(rtrim(@name))+' has been edited' 

SELECT @return_notice

--RETURN
-----------------------
create_email_rule:

SET @count=(SELECT COUNT(*) FROM dbo.adsup_email_rules WHERE pa=@pa)

IF @count=0 ----create new workfolw
BEGIN
INSERT dbo.adsup_email_rules(pa,email_to,email_cc,created_by_person_id,created_date,email_template_id)
SELECT @pa,@subject,@name,@person_id,GETDATE(),@id 
SET @return_notice= 'The workfolw for ' + @pa + ' has been created' 
END
ELSE	--return message
BEGIN
SET @return_notice = 'The workfolw for ' + @pa + ' has already been created' 
END

SELECT @return_notice

RETURN
---------------
save_email_rule:

SET @count=(SELECT COUNT(*) FROM dbo.adsup_email_rules WHERE pa=@pa)

IF @count>0
BEGIN
UPDATE dbo.adsup_email_rules SET email_template_id=@id,email_to=@subject,email_cc=@name,last_modified_by=@person_id,last_modified_date=GETDATE()WHERE pa=@pa
SET @return_notice = 'The current workfolw for '+@pa+' has been edited and saved' 
END
ELSE
BEGIN
SET @return_notice = 'Could not save to '+@pa
END

SELECT @return_notice

RETURN
---------------
delete_email_rule:

---to delete workflow
UPDATE dbo.adsup_email_rules 
SET end_date=GETDATE(),last_modified_by=@person_id,last_modified_date=GETDATE()
WHERE pa=@pa

SET @return_notice='Current workflow for '+@pa+' has been deleted' 

SELECT @return_notice

RETURN
---------------
clone_email_rule:

SET @count=(SELECT count(*) FROM dbo.adsup_email_rules er WHERE pa=convert(nchar,@name))

IF @count=0-----to clone new workflow
BEGIN
INSERT dbo.adsup_email_rules(pa, email_to,email_cc,start_date, created_by_person_id,created_date,email_template_id)
SELECT convert(nchar,@name),er.email_to,er.email_cc,GETDATE(),@person_id,GETDATE(),er.email_template_id
FROM dbo.adsup_email_rules as er WHERE pa=@pa
SET @return_notice= @name + ' has been cloned workflow by '+ @pa 
END
ELSE-----to update workflow
BEGIN
UPDATE dbo.adsup_email_rules 
SET email_to= (select email_to FROM dbo.adsup_email_rules WHERE pa = @pa),
email_cc= (select email_cc FROM dbo.adsup_email_rules WHERE pa = @pa),
email_template_id=(select email_template_id FROM dbo.adsup_email_rules WHERE pa = @pa),
last_modified_by=@person_id,last_modified_date=GETDATE()
WHERE pa = convert(nchar,@name)
SET @return_notice = @name + ' workflow has been cloned and updated by ' + @pa 
END

SELECT @return_notice

RETURN


GO


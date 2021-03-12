SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_admin_appl_destructed]

@act					varchar(30),
@year					int,
@status_code			varchar(20),
@exception_code			varchar(20),
@str					varchar(50),
@id_string				varchar(800),
@exception_type			varchar(20),
@ic						varchar(10),
@Operator				varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name: sp_web_admin_appl_destructed							***/
/***	Description: proccess for destructed appls from IMPAC.					***/
/***	created:	Leon 6/19/2015												***/
/***	modified:	Leon 6/22/2015												***/
/***	modified:	Leon 7/09/2015												***/
/***	modified:	Leon 4/03/2017 for MVC										***/

---APPL_STATUS_GRP_DESCRIP 
--0.Excluded or Retained (for various reasons) from deletion either by a user or by a proc
--1.Waiting : waiting for approval or Just downloaded from impac
--2.Approved: Ready for approval either by a uaser or automated proc
--3.WIP : System is locating files on various file server
--4.Error :Error in Locating files.
--5.Deletion Complete : Deletion of documents completed, Links are unlinked, notice on website created etc....
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id						smallint,
@person_id						int,
@grant_id						int,
@sql							varchar(800),
@processable					varchar(1),
@personal_process_status_code	int

SET @profile_id	=(select profile_id	from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)
SET @personal_process_status_code=(select DESTRUCT_PROCESS_STATUS_CODE from  dbo.IMPAC_Destruction_Process_WorkFlow where ID=CONVERT(int,@year) and ADMIN_ID=@person_id)

--CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, appl_id int)
DECLARE @a table(appl_id int)

IF @act='' or @act is null GOTO to_search

IF @act='create' GOTO create_exception
IF @act='release' GOTO release_exception

IF @act='approve_next' GOTO approve_next
IF @act='approve_previous' GOTO approve_previous
------------------
create_exception:

SET @sql='UPDATE IMPAC_DESTRUCTED_APPL 
SET DESTRUCT_PROCESS_STATUS_CODE=0, 
DESTRUCTION_EXCEPTION_CODE='+char(39)+@exception_type+char(39)+
',DESTRUCTION_EXCEPTION_CREATED_BY='+char(39)+convert(varchar,@person_id)+char(39)+  
',DESTRUCTION_EXCEPTION_CREATED_DATE='+char(39)+convert(varchar,getdate())+char(39)+  
' WHERE APPL_ID in('+@id_string+')'
EXEC (@sql)

GOTO to_search
----------------
release_exception:

SET @sql='UPDATE IMPAC_DESTRUCTED_APPL 
SET DESTRUCT_PROCESS_STATUS_CODE='+char(39)+convert(varchar,@personal_process_status_code)+char(39)+ 
',DESTRUCTION_EXCEPTION_CODE=null,
DESTRUCTION_EXCEPTION_CREATED_BY=null, 
DESTRUCTION_EXCEPTION_CREATED_DATE=null 
WHERE APPL_ID in('+@id_string+')'
EXEC (@sql)

GOTO to_search
---------------
approve_previous:

EXEC sp_Change_Destruct_Workflow_status @year,@person_id,'Previous'

GOTO to_search
-----------------
approve_next:

EXEC sp_Change_Destruct_Workflow_status @year,@person_id,'Next'

GOTO to_search
-----------------
to_search:

IF @year is not null and (@str ='' or @str is null) and (@status_code='' or @status_code is null) and (@exception_code='' or @exception_code is null)
BEGIN
INSERT @a SELECT appl_id FROM dbo.IMPAC_DESTRUCTED_APPL WHERE year(EGRANTS_CREATED_DATE)=@year ORDER BY DESTRUCT_PROCESS_STATUS_CODE asc,serial_num desc
END 

IF @year is not null and @status_code is not null and (@str ='' or @str is null) and (@exception_code='' or @exception_code is null)
BEGIN
INSERT @a SELECT appl_id FROM dbo.IMPAC_DESTRUCTED_APPL WHERE year(EGRANTS_CREATED_DATE)=@year 
and APPL_STATUS_GRP_DESCRIP=@status_code ORDER BY DESTRUCT_PROCESS_STATUS_CODE asc,serial_num desc
END

IF @year is not null and @exception_code is not null and (@str ='' or @str is null) and (@status_code ='' or @status_code is null)
BEGIN
INSERT @a SELECT appl_id FROM dbo.IMPAC_DESTRUCTED_APPL WHERE year(EGRANTS_CREATED_DATE)=@year 
and DESTRUCTION_EXCEPTION_CODE=@exception_code ORDER BY DESTRUCT_PROCESS_STATUS_CODE asc,serial_num desc
END

IF @year is not null and @str is not null and (@status_code ='' or @status_code is null) and (@exception_code =''or @exception_code is null)
BEGIN
INSERT @a SELECT appl_id FROM dbo.IMPAC_DESTRUCTED_APPL WHERE year(EGRANTS_CREATED_DATE)=@year 
and FGN like '%'+@str +'%' ORDER BY DESTRUCT_PROCESS_STATUS_CODE asc,serial_num desc
END

--create processble 
--SET @processable=(select dbo.fn_is_Archival_admin(@year,@person_id))

SELECT appl.appl_id,FGN as full_grant_num,serial_num,
DESTRUCTION_EXCEPTION_CODE as exception_code, 
APPL_STATUS_GRP_DESCRIP as status_code,
DESTRUCT_PROCESS_STATUS_CODE as step_code,
CASE
WHEN DESTRUCT_PROCESS_STATUS_CODE=0 THEN 'y'
WHEN DESTRUCT_PROCESS_STATUS_CODE=@personal_process_status_code THEN 'y'
ELSE 'n' 
END as appl_editable
FROM IMPAC_DESTRUCTED_APPL AS appl, @a a
WHERE appl.APPL_ID=a.appl_id  

return

GO


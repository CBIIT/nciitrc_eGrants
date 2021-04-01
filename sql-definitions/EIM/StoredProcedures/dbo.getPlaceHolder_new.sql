SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[getPlaceHolder_new]

@PARENTAPPLID			INT,		--Remember this record entry is for a supplement, therefore there has to be a parent. and this is the parent appl id
@pa				varchar(10),		--PA : from eRA Notifications email
@Rcvd_dt		SMALLDATETIME,		--Document date equivalent of documents table
@Catname		varchar(50),		--Example : eRA Notification (exact category_name from categories table)
@filetype		varchar(3),			--Example : txt or pdf
@Sub			varchar(200),		--Used in case of email uploading Otherwise null
@body			ntext,				--Used in case of email uploading Otherwise null
@SubCatname		varchar(35)		--Example : eRA Notification: Supplement Requested,Correspondence: Supplement Response Required

--@fgn			varchar(20)

AS

/**********************************************************************************************************************/
/***	Created by : Imran Omair on  10/31/2015							 											***/
/***	Description: This proc will be called automaticaly for loading eRA Notifications and OGA Notifications 		***/
/***				  which is stored under sorrespondence. This can allso be called to store other documents		***/
/***				  SUCH as PFR etc.. BUT only eRA Notifications will create entry to send emails to PD etc..		***/
/***													***/
/*** bUG fIX : iMRAN 5/10/2016 iNCREASSING size of ##CC and To table to 100***/
/*** Addition : iMRAN 7/8/2017 Added subcatname***/
/************************************************************************************************************/
SET NOCOUNT ON;
BEGIN

declare 

@Categoryid int,
@Parent_fgn varchar(19),
@newurl varchar(200),
@WIP_sequencer int,
@filename_number varchar(20),
@NewNotificationID Int,
@SupportYear tinyint,
@SrlNumber int,
@SuffixCode varchar(4),
@full_grant_num	varchar(20),
@SQL nvarchar(Max),
@paforTemplate	varchar(10),
@emailto varchar(200),@emailcc varchar(200),@emailTempID int,@pos varchar(50)

    SELECT @Categoryid=CATEGORY_ID FROM categories WHERE category_name=@Catname
	SELECT @full_grant_num=full_grant_num FROM vw_appls WHERE appl_id=@PARENTAPPLID	
	select @SrlNumber=serial_num, @SupportYear=support_year, @SuffixCode=suffix_code,@Parent_fgn=full_grant_num from vw_appls where appl_id=@PARENTAPPLID
	
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('PARENTAPPLID',cast(@PARENTAPPLID as varchar),GETDATE())
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('pa',@pa,GETDATE())
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('Rcvd_dt',cast(@Rcvd_dt as varchar),GETDATE())
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('Catname',@Catname,GETDATE())
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('@filetype',@filetype,GETDATE())
	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('Parent_fgn',cast(@Parent_fgn as varchar),GETDATE())

		
	INSERT dbo.IMPP_Admin_Supplements_WIP(Serial_num,Full_grant_num,Former_num,Former_appl_id,Submitted_date,file_type,category_id,sub_category_name)
	VALUES(@SrlNumber,STUFF(@Parent_fgn,1,1,'3'),@Parent_fgn,@PARENTAPPLID,@Rcvd_dt,@filetype,@Categoryid,@SubCatname)
	SET @WIP_sequencer=@@identity
	
	--Print @SrlNumber +'=='+ STUFF(@Parent_fgn,1,1,'3')+'=='+@Parent_fgn+'=='+@PARENTAPPLID+'=='+@Rcvd_dt+'=='+@filetype+'=='+@Categoryid
	SELECT @filename_number='as'+CAST(@SrlNumber AS VARCHAR)+CAST(@WIP_sequencer AS VARCHAR)

	INSERT dbo.Imm_Capture_Error(Error,Detail,timestmp)
	VALUES('INSERTED INTO WIP','FILENAME='+@filename_number,GETDATE())
	
	--The following file server has to be coordinated with the job in visual cron job called Add Supp Workflow.
	SELECT @newurl='/data/funded2/nci/main/'+@filename_number+'.'+@filetype
	update dbo.IMPP_Admin_Supplements_WIP SET doc_url=@newurl,movedto_document_id=@filename_number WHERE adm_supp_wip_id=@WIP_sequencer
	
--PRINT @Rcvd_dt
--PRINT @Sub			
--PRINT @body		
	
	--THIS PROC CALLED FOR @CatnaME='eRA Notification' WILL ONLY CREATE NOTIFICATION ENTRY
	--CALLED FOR OTHER @CatnaME WILL ONLY CREATE INTRY IN WIP TABLE.
	IF (@Catname='eRA Notification') AND NOT EXISTS (select * from dbo.adsup_notification where Replace(subjectLine,' ','')=Replace(@Sub,' ','') and disabled_date is null)
		-- and convert(smalldatetime,NotRcvd_dt,101)=convert(smalldatetime,@Rcvd_dt,101) )
	BEGIN
		
		
		--RECORD NEW EMAIL/SUPPLEMENT AND IT'S NOTIFICATION (SEQUENCER)
		INSERT adsup_notification(APPL_ID,PA,subjectLine, NotificationBody, NotRcvd_dt, Full_grant_num,Created_by,Created_date )
		VALUES (@PARENTAPPLID,@pa, @Sub, @body, convert(datetime,@Rcvd_dt,101), @full_grant_num, 1899, GETDATE())
		SET @NewNotificationID=@@identity 
		
		/*cREATE EMAIL DISTRIBUTION LIST BASED ON EMAIL RULES*/	
		--Find correct PAfor template: 
		--4/14/2017(Imran): VVIMP : PA DOESNOT COME IN STANDARD FORMAT OR ONE FORMAT IN ERA NOTIFICATIO IT CAN BE PA16-288 OR PA-16-288 ETC
		--TOMAKE IT UNIFORM STRIP THE SLASHES WHEN EVER YOU COMPARE WITH ADSUP_EMAIL_RULES
		--IF NOT EXISTS (select * from dbo.adsup_email_rules where PA =@pa)
		IF NOT EXISTS (select * from dbo.adsup_email_rules where Replace(PA,'-','') =Replace(@pa,'-',''))
			SET @paforTemplate='AllOther'
		ELSE
			SET @paforTemplate=@pa
		--END
		
		SELECT @emailto=email_to,@emailcc=EMAIL_CC,@emailTempID=email_template_id FROM dbo.adsup_email_rules WHERE Replace(PA,'-','') =Replace(@paforTemplate,'-','')

		IF OBJECT_ID('tempdb..#EMAILTOO') IS NOT NULL DROP TABLE #EMAILTOO
		CREATE TABLE #EMAILTOO (POSITION VARCHAR(50))  --BUG FIX REPLACED 20 WITH 50 
		INSERT INTO #EMAILTOO(POSITION)
		SELECT Value FROM dbo.Split_String_To_Table(@emailto, ',')
		
		IF OBJECT_ID('tempdb..#EMAILCCC') IS NOT NULL DROP TABLE #EMAILCCC
		CREATE TABLE #EMAILCCC (POSITION VARCHAR(50))	--BUG FIX REPLACED 20 WITH 50
		INSERT INTO #EMAILCCC(POSITION)
		SELECT Value FROM dbo.Split_String_To_Table(@emailcc, ',')
		
		IF OBJECT_ID('tempdb..#contacts1') IS NOT NULL DROP TABLE #contacts1
		CREATE TABLE #contacts1 (PD_NAM varchar(50),PD_EM varchar(100),SPEC_NAM varchar(50),SPEC_EM varchar(100), PI_NAM varchar(50), PI_EM varchar(100), appl_id int)

		SET @SQL='INSERT #contacts1 (PD_NAM ,PD_EM,SPEC_NAM,SPEC_EM, PI_NAM,PI_EM,appl_id) '
		SET @SQL=@SQL + 'SELECT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,PI_FULL_NAME,PI_EMAIL_ADDRESS,'+CAST(@PARENTAPPLID AS VARCHAR)+' FROM '
		SET @SQL=@SQL + ' openquery(CIIP,'+CHAR(39)+'select  DISTINCT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS, PI_FULL_NAME, PI_EMAIL_ADDRESS '
		SET @SQL=@SQL + ' FROM GM_ACTION_QUEUE_VW WHERE APPL_ID in (select appl_id  FROM GM_ACTION_QUEUE_VW WHERE support_year in (select max(support_year) '
		SET @SQL=@SQL + ' FROM GM_ACTION_QUEUE_VW where serial_num='+CAST(@SrlNumber AS VARCHAR)+') and serial_num='+CAST(@SrlNumber AS VARCHAR)+')'+ CHAR(39) + ')'
		
		--PRINT @SQL
		
		--select * from #EMAILTOO
		--select * from #EMAILCCC
		--select * from #contacts1
		
		--SET @SQL=@SQL + 'SELECT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,PI_FULL_NAME,PI_EMAIL_ADDRESS,'+CAST(@PARENTAPPLID AS VARCHAR)+' FROM '
		--SET @sql= @sql + 'openquery(CIIP,'+ char(39) + 'select  DISTINCT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS, PI_FULL_NAME, PI_EMAIL_ADDRESS FROM GM_ACTION_QUEUE_VW WHERE APPL_ID='+CAST(@PARENTAPPLID AS VARCHAR)+ char(39) +')'
		exec (@sql)
		
		DECLARE db_cursor CURSOR FOR  
		SELECT position	FROM #EMAILTOO
		OPEN db_cursor  
		FETCH NEXT FROM db_cursor INTO @pos

		WHILE @@FETCH_STATUS = 0  
		BEGIN  
			--PRINT ' TO :'
			IF @pos='PD'
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'to',PD_NAM,'PD',PD_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID;
			else IF @pos='PI'			
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'to',PI_NAM,'PI',PI_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID;
			else IF @pos='SPEC'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'to',SPEC_NAM,'SPEC',SPEC_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID	;				
			else IF @pos='TESTPD'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'to','Emily Deriskell TESTPD','TESTPD','omairi@mail.nih.gov',GETDATE(),@emailTempID)				
			else IF @pos='TESTSPEC'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'to','Imran Omair TESTSPEC','TESTSPEC','omairi@mail.nih.gov',GETDATE(),@emailTempID)									
			else IF @pos='TESTPI'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'to','Brayn Baker TESTPI','TESTPI','omairi@mail.nih.gov',GETDATE(),@emailTempID)				
			else			
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES(@NewNotificationID,'to','','OTHERS',@POS,GETDATE(),@emailTempID) 
			
			FETCH NEXT FROM db_cursor INTO @pos
		END  
		CLOSE db_cursor  
		DEALLOCATE db_cursor 
		DECLARE db_cursor CURSOR FOR  
		SELECT position	FROM #EMAILCCC
		OPEN db_cursor  
		FETCH NEXT FROM db_cursor INTO @pos

		WHILE @@FETCH_STATUS = 0  
		BEGIN  
			--PRINT ' CC :'
			IF @pos='PD'
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'cc',PD_NAM,'PD',PD_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID;
			else IF @pos='PI'			
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'cc',PI_NAM,'PI',PI_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID;
			else IF @pos='SPEC'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					SELECT DISTINCT @NewNotificationID,'cc',SPEC_NAM,'SPEC',SPEC_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@PARENTAPPLID	;				
			else IF @pos='TESTPD'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'cc','Emily Deriskell TESTPD','TESTPD','omairi@mail.nih.gov',GETDATE(),@emailTempID)				
			else IF @pos='TESTSPEC'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'cc','Imran Omair TESTSPEC','TESTPI','omairi@mail.nih.gov',GETDATE(),@emailTempID)								
			else IF @pos='TESTPI'	
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES( @NewNotificationID,'cc','Bryan baker TESTPI','TESTPI','omairi@mail.nih.gov',GETDATE(),@emailTempID)								
			else			
					INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
					VALUES(@NewNotificationID,'cc','','OTHERS',@POS,GETDATE(),@emailTempID) 
			
			FETCH NEXT FROM db_cursor INTO @pos
		END  
		CLOSE db_cursor  
		DEALLOCATE db_cursor 
	
		--CREATE ENTRY FOR dbo.adsup_Notification_impac_status/CREATE AN ENTRY FOR IMPACII MONITORING,NOTIFICATION_ID AND @PARENTAPPLID
		INSERT dbo.adsup_Notification_impac_status(Notification_id,APPL_ID)
		VALUES(@NewNotificationID,@PARENTAPPLID);
	END
	----Record new file name id.
	--PRINT 'FINALLY::>' + @filename_number
	IF OBJECT_ID('tempdb..#output') IS NOT NULL DROP TABLE #output
	CREATE TABLE #output (ABC varchar(20))
	INSERT #output(ABC) VALUES (@filename_number) 
	select * from #output
	
END





GO


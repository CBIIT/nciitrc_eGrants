USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[adsupp_create_notification_accgrant]    Script Date: 9/9/2024 11:08:01 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		IMRAN OMAIR
-- Create date: 11/21/2015
-- Description:	CREATE NOTIFICATIONS FOR ACCEPTED ADMIN SUPP
-- =============================================
ALTER   PROCEDURE [dbo].[adsupp_create_notification_accgrant]
AS

DECLARE @PROC_NAME VARCHAR(50)
DECLARE @RUN_DATETIME DATETIME
SET @PROC_NAME = 'adsupp_create_notification_accgrant'
SET @RUN_DATETIME = GETDATE()

print '====>>Proc ' + @PROC_NAME + ' Started @' + cast(getdate() as varchar)

BEGIN

	DECLARE @Formerapplid INT, @Formernum VARCHAR(19), @Actiondate SMALLDATETIME, @seq int, @NewNotificationID int, @SerialNum Int
	DECLARE @emailto varchar(200),@emailcc varchar(200),@emailTempID int,@pos varchar(20),  @SQL nvarchar(Max)
	

	DECLARE db_cursor CURSOR FOR  
		SELECT id, Former_num,Former_appl_id,Action_date from dbo.adsup_accepted where notification_id is null
		OPEN db_cursor  
		print 'Start db_cursor'
		FETCH NEXT FROM db_cursor INTO @seq, @Formernum, @Formerapplid ,@Actiondate
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		IF NOT EXISTS(SELECT * FROM dbo.adsup_notification WHERE appl_id=@Formerapplid AND Full_grant_num=@Formernum AND NotRcvd_dt=@Actiondate and disabled_date is null)
		BEGIN
			SELECT @SerialNum=serial_num FROM vw_appls WHERE appl_id=@Formerapplid
			INSERT adsup_notification(APPL_ID,PA,NotRcvd_dt, Full_grant_num,Created_by,Created_date )
			VALUES (@Formerapplid,NULL,@Actiondate, @Formernum, 1899, GETDATE())
			SET @NewNotificationID=@@identity 
			
			update dbo.adsup_accepted set Notification_id=@NewNotificationID where id=@seq
			
			--hardcoding ADSUP_ACC WORKFLOW FOR PI ACC LETTER
			SELECT @emailto=email_to,@emailcc=EMAIL_CC,@emailTempID=email_template_id FROM dbo.adsup_email_rules WHERE PA ='ADSUP_ACC'
			
			IF OBJECT_ID('tempdb..#EMAIL2') IS NOT NULL DROP TABLE #EMAIL2
			CREATE TABLE #EMAIL2 (POSITION VARCHAR(20))
			INSERT INTO #EMAIL2(POSITION)
			SELECT Value FROM dbo.Split_String_To_Table(@emailto, ',')
				
			IF OBJECT_ID('tempdb..#EMAILCEE') IS NOT NULL DROP TABLE #EMAILCEE
			CREATE TABLE #EMAILCEE (POSITION VARCHAR(20))
			INSERT INTO #EMAILCEE(POSITION)
			SELECT Value FROM dbo.Split_String_To_Table(@emailcc, ',')
				
			IF OBJECT_ID('tempdb..#contacts1') IS NOT NULL DROP TABLE #contacts1
			CREATE TABLE #contacts1 (PD_NAM varchar(50),PD_EM varchar(100),SPEC_NAM varchar(50),SPEC_EM varchar(100), PI_NAM varchar(50), PI_EM varchar(100), appl_id int)
			
			SET @SQL='INSERT #contacts1 (PD_NAM ,PD_EM,SPEC_NAM,SPEC_EM, PI_NAM,PI_EM,appl_id) '
			SET @SQL=@SQL + 'SELECT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,PI_FULL_NAME,PI_EMAIL_ADDRESS,'+CAST(@Formerapplid AS VARCHAR)+' FROM '
			SET @SQL=@SQL + ' openquery(CIIP,'+CHAR(39)+'select  DISTINCT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS, PI_FULL_NAME, PI_EMAIL_ADDRESS '
			SET @SQL=@SQL + ' FROM GM_ACTION_QUEUE_VW WHERE APPL_ID in (select appl_id  FROM GM_ACTION_QUEUE_VW WHERE support_year in (select max(support_year) '
			SET @SQL=@SQL + ' FROM GM_ACTION_QUEUE_VW where serial_num='+CAST(@SerialNum AS VARCHAR)+') and serial_num='+CAST(@SerialNum AS VARCHAR)+')'+ CHAR(39) + ')'

			--SET @SQL='INSERT #contacts1 (PD_NAM ,PD_EM,SPEC_NAM,SPEC_EM, PI_NAM,PI_EM,appl_id) '			
			--SET @SQL=@SQL + 'SELECT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,PI_FULL_NAME,PI_EMAIL_ADDRESS,'+CAST(@Formerapplid AS VARCHAR)+' FROM '
			--SET @sql= @sql + 'openquery(CIIP,'+ char(39) + 'select  DISTINCT PD_FULL_NAME,PD_EMAIL_ADDRESS,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS, PI_FULL_NAME, PI_EMAIL_ADDRESS FROM GM_ACTION_QUEUE_VW WHERE APPL_ID='+CAST(@Formerapplid AS VARCHAR)+ char(39) +')'

			exec (@sql)
					
			DECLARE db_cursor1 CURSOR FOR  
			SELECT position	FROM #EMAIL2
			OPEN db_cursor1 

			print 'Start db_cursor1'
			FETCH NEXT FROM db_cursor1 INTO @pos

			WHILE @@FETCH_STATUS = 0  
			BEGIN  

				IF @pos='PD'
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'to',PD_NAM,'PD',PD_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and PD_EM IS NOT NULL;

				else IF @pos='PI'			
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'to',PI_NAM,'PI',PI_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and PI_EM IS NOT NULL;
				else IF @pos='SPEC'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'to',SPEC_NAM,'SPEC',SPEC_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and SPEC_EM IS NOT NULL;				
				else IF @pos='TESTPD'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES( @NewNotificationID,'to','Emily Deriskell TESTPD','TESTPD','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)				
				else IF @pos='TESTSPEC'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES( @NewNotificationID,'to','Imran Omair TESTSPEC','TESTSPEC','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)				
				else IF @pos='TESTPI'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)						
						VALUES( @NewNotificationID,'to','Brayn Baker TESTPI','TESTPI','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)				
				else			
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES(@NewNotificationID,'to','','OTHERS',@POS,GETDATE(),@emailTempID) 
				
				FETCH NEXT FROM db_cursor1 INTO @pos
			END  
			CLOSE db_cursor1  
			DEALLOCATE db_cursor1 
			DECLARE db_cursor1 CURSOR FOR  
			SELECT position	FROM #EMAILCEE
			OPEN db_cursor1  
			FETCH NEXT FROM db_cursor1 INTO @pos

			WHILE @@FETCH_STATUS = 0  
			BEGIN  
				IF @pos='PD'
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'cc',PD_NAM,'PD',PD_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and PD_EM IS NOT NULL;
				else IF @pos='PI'			
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'cc',PI_NAM,'PI',PI_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and PI_EM IS NOT NULL;
				else IF @pos='SPEC'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						SELECT DISTINCT @NewNotificationID,'cc',SPEC_NAM,'SPEC',SPEC_EM,GETDATE(),@emailTempID from #contacts1 where appl_id=@Formerapplid and SPEC_EM IS NOT NULL;			
				else IF @pos='TESTPD'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES( @NewNotificationID,'cc','Emily Deriskell TESTPD','TESTPD','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)				
				else IF @pos='TESTSPEC'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES( @NewNotificationID,'cc','Imran Omair TESTSPEC','TESTPI','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)								
				else IF @pos='TESTPI'	
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES( @NewNotificationID,'cc','Bryan baker TESTPI','TESTPI','egrantsdevs@mail.nih.gov',GETDATE(),@emailTempID)									
				else		
						INSERT dbo.adsup_Notification_email_status(Notification_id,email,person_name,position,email_address,created_date,email_template_id)
						VALUES(@NewNotificationID,'cc','','OTHERS',@POS,GETDATE(),@emailTempID) 
				
				FETCH NEXT FROM db_cursor1 INTO @pos
			END  
			CLOSE db_cursor1  
			print 'Close db_cursor1'
			DEALLOCATE db_cursor1 					
		END
	FETCH NEXT FROM db_cursor INTO @seq, @Formernum, @Formerapplid ,@Actiondate		
	END  
	CLOSE db_cursor  
	print 'Close db_cursor'
	DEALLOCATE db_cursor 	
END


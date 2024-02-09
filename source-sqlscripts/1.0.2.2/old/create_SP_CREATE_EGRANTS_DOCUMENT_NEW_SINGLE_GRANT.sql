USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[SP_CREATE_EGRANTS_DOCUMENT_NEW_SINGLE_GRANT]    Script Date: 12/1/2023 10:37 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



-- =============================================
-- Author:		copied from <Imran Omair>'s SP_CREATE_EGRANTS_DOCUMENT_NEW
-- Create date: <12/1/2023>

-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_CREATE_EGRANTS_DOCUMENT_NEW_SINGLE_GRANT]

--@FGN VARCHAR(30),	--CombinedPIID
@DOCID VARCHAR(10), --If there is a doc id and an appl_id this means to replace existing doc
@CAT VARCHAR(100),	--Category Name
@APPID VARCHAR(10),--
@PROFILEID smallint,
--@UPLOADID VARCHAR(20), -- ONLY COME FROM NHLBI
@DD VARCHAR(10),	--Document Date is email recieved date
@UID VARCHAR(50),	--Sender user id
@FT VARCHAR(5),		--
@QCFLAG VARCHAR(3),
@SUB VARCHAR(35)

AS
	SET NOCOUNT ON;
BEGIN
	DECLARE @UserInterNalID int 
	DECLARE @dcdate smalldatetime
	DECLARE @ApplId int
	DECLARE @newdocumentid int
	DECLARE @catID int
	DECLARE @qcreason varchar(20)
	DECLARE @qcpersonid int
	DECLARE @url varchar(200)
	DECLARE @ICCODE varchar(10)
	DECLARE @Error int
	DECLARE @ErrorStr nvarchar(200)
	DECLARE @qcreasen nvarchar(200)
	SET @catID=NULL
	SET @Error=0	
	SET @qcpersonid=NULL
	SET @dcdate=NULL
	SET @ErrorStr=''
	SET @ICCODE='NCI'
	
	IF LEN(LTRIM(RTRIM(CAST(@SUB AS VARCHAR)))) = 0
		SET @SUB=NULL

	--IF @PROFILEID='5' 
	--	SET @ICCODE='NHLBI'
	
	--IF @UID = 'NCIOGAPROGRESS'
	--begin
	--  sET @UID='Email'
	--  SET @CAT='Correspondence';  --Craeted_by_person_ID should be system fr all Faxes added on 10/25/12
	--end


	SET @ApplId=CAST(@APPID	AS INT)
	--If @UID <> 'ncioatrackingsystem'
	--go to unaouthorizeduser_Error

	CREATE TABLE #output (name nvarchar(20),value nvarchar(200))
	IF LEN(LTRIM(RTRIM(CAST(@DOCID AS VARCHAR)))) = 0
		SET @DOCID=NULL
	IF LEN(LTRIM(RTRIM(CAST(@APPID AS VARCHAR)))) = 0
		SET @APPID=NULL
		
	--PRINT CAST(@DOCID AS VARCHAR)
	--PRINT @APPID
	--VALIDATE DOCID COMMING FROM OUT SIDE IF THERE IS THEN REPLACE THAT WITH NEW ONE
	--FIRST DISABLE THE DOCID AND CREATE A NEW ONE WITH NO QC ALARM
	IF @DOCID IS NOT NULL and @ApplId is not null
	BEGIN
		--print "1"
		--IF THIS IS A CORRUPTED DOCID PASSED
		IF NOT EXISTS( SELECT * FROM documents where document_id= @DOCID AND APPL_ID=@ApplId AND disabled_date is null) 
		BEGIN
			--PRINT "1.1"
			SET @Error=2;
			SET @ErrorStr=@ErrorStr + ' Document Does not Exist to replace' 
		END
		else BEGIN 
			SELECT @CAT=A.CATEGORY_NAME,@catID=A.CATEGORY_ID FROM categories A, documents B
			WHERE A.category_id=B.category_id AND B.document_id= @DOCID AND B.APPL_ID=@ApplId
			print 'CATID='+CAST(@catid AS VARCHAR)
			pRINT 'CATNAME=' + @CAT
			--SELECT @catID=CATEGORY_ID FROM documents WHERE document_id= @DOCID AND APPL_ID=@ApplId
			--SELECT @cat=CATEGORY_NAME FROM CATEGORIES WHERE document_id= @DOCID AND APPL_ID=@ApplId
		END	
	END
	ELSE IF @DOCID IS NULL and @ApplId is not null
	BEGIN
		--PRINT "2"
		--VALIDATE APPL ID COMMING FROM OUT SIDE
		IF NOT EXISTS( SELECT * FROM appls where appl_id= @APPID) 
		BEGIN
			SET @Error=1;
			SET @ErrorStr=@ErrorStr + ' Grant Does not Exist.' 
			SET @ApplId=NULL  --mark un identified document
			set @qcreason = 'Email'
			SET @QCFLAG='yes'
			SET @qcpersonid=397  --408 for imran   --397 for Dave
		END
	END ELSE IF @DOCID IS NOT NULL and @ApplId is null
	BEGIN
			--LESS CHANCE TO MESS & UPDATE DATA WITH ONE KEY VAL
			SET @Error=2;
			SET @ErrorStr=@ErrorStr + ' Not Enough information to upload' 	
	END ELSE IF @DOCID IS NULL and @ApplId is null AND @CAT ='Fax'
	BEGIN
			SET @Error=1;
			SET @ErrorStr=@ErrorStr + ' Fax: Forward to QC' 	
	END ELSE IF @DOCID IS NULL and @ApplId is null
	BEGIN
			SET @Error=2;
			SET @ErrorStr=@ErrorStr + ' Not Enough information to upload' 	
	END


	
	--Validate Category
	IF replace(@CAT,' ','') <>''
	  IF NOT EXISTS( SELECT categories.category_ID FROM categories  
			inner join dbo.categories_ic on categories.category_id=categories_ic.category_id
			WHERE ic=@ICCODE and replace(category_name,' ','') = replace(@CAT,' ','')	)
		begin
			--If there is no category_id passed set it to default "Correspondence"
 		    SELECT 	@catID=categories.category_ID FROM categories  
			inner join dbo.categories_ic on categories.category_id=categories_ic.category_id
			WHERE ic='NCI' and replace(category_name,' ','') = 'Correspondence'		
			--print '3'
			SET @Error=1;
			SET @ErrorStr=@ErrorStr + ' Category=' + @CAT + ':Does not Exist.' 
			--SET @ApplId=NULL  
			--SET @catID=NULL
			set @qcreason = 'Email'
			SET @QCFLAG='yes'
			SET @qcpersonid=397  --408 for imran  --397 for Dave
		end
		else
		  SELECT 	@catID=categories.category_ID FROM categories  
			inner join dbo.categories_ic on categories.category_id=categories_ic.category_id
			WHERE ic='NCI' and replace(category_name,' ','') = replace(@CAT,' ','')		

	--VALIDATE UPLOADER
	IF NOT EXISTS (SELECT * FROM dbo.people WHERE userid = LTRIM(RTRIM(@UID)) and profile_id=@PROFILEID and position_id>=2 and application_type='egrants' AND active=1)
	begin
	  --print '4'
	  --SET @Error=2;
	  SET @Error=1;
	  SET @ErrorStr= ' user=' + @UID + ': Not Allowed to Upload documents.' 
	  set @qcreason = 'Email'
	  SET @QCFLAG='yes'	  
	end
	  else
		  SELECT @UserInterNalID=person_id FROM dbo.people 
		  WHERE userid = LTRIM(RTRIM(@UID)) and profile_id=@PROFILEID and position_id>=2 and application_type='egrants'
		print cast(@UserInterNalID as varchar)
		
	IF @UID IN ( 'FAXMASTER','NCIOGAPROGESS')
	begin
	  SET @UserInterNalID=1899;  --Craeted_by_person_ID should be system fr all Faxes added on 10/25/12
	end

	--validate QCflag
	If lower(@QCFLAG)='yes' and @catID is NULL
	  begin
		--PRINT '5'
	    SET @Error=1;
	    SET @ErrorStr=@ErrorStr + ' Junck attachments found' 
	    --SET @ApplId=NULL  --mark un identified document
	    set @qcreason = 'Fax/Email'
	    Set @dcdate = getdate()
	    SET @qcpersonid=397  --408 for imran  --397 for Dave
	  end
	  
  --print 'Error->' + cast(@Error as varchar)
  
  IF @Error < 2 
  BEGIN
	If @FT is null or @FT ='' SET @FT = 'pdf'
	--SET @url = 'https://egrants-data.nci.nih.gov/funded2/nci/main/'
	--SET @url = '/data/funded2/nci/main/' comment out by leon 5/16/2016
	SET @url = 'data/funded2/nci/main/'
    --print 'before update' 
    --print cast(@docid as varchar)
    --print cast(@ApplId as varchar)
    --TO REPLACE BY DISABLING AN EXISTING DOCID AND CREATING ONE
    IF @DOCID IS NOT NULL and @ApplId is not null
    BEGIN
		--print 'update'
		UPDATE documents SET disabled_date=GETDATE(),disabled_by_person_id=@UserInterNalID
		WHERE document_id=@DOCID AND appl_id=@ApplId
		
		INSERT documents(document_date, created_date, created_by_person_id, profile_id,mail_upload_id, 
		qc_reason, qc_person_id, file_type, uid, category_id, appl_id, qc_date, stored_date)	
			
		SELECT convert(smalldatetime,@DD) as document_date, 
		getdate() as created_date,
		@UserInterNalID as created_by_person_id,
		profile_id,
		NULL as mail_upload_id,
		@qcreason as qc_reason,
		NULL as qc_person_id,
		lower(@FT) as file_type, 
		@UID as uid,
		category_id as category_id,		
		@ApplId as appl_id,
		NULL as qc_date,
		NULL as stored_date
		FROM documents WHERE document_id=@DOCID
		
		SET @newdocumentid = @@IDENTITY
		
		UPDATE documents
		SET file_type=CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END,
		url=@url + convert(varchar,document_id) + '.' + CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END
		WHERE document_id=@newdocumentid AND url IS NULL
		
    END
    ELSE IF @DOCID IS NULL and @ApplId is not null   --CREATE A NEW ENTRY
    BEGIN
        --print 'Insert 1'
        --print '@cat='+@CAT
		INSERT documents(document_date, created_date, created_by_person_id, profile_id,mail_upload_id, 
		qc_reason,qc_person_id,	file_type,  uid, category_id, appl_id, qc_date, sub_category_name)

		SELECT convert(smalldatetime,@DD) as document_date, 
			getdate() as created_date,
			@UserInterNalID as created_by_person_id,
			@PROFILEID as profile_id,
			NULL as mail_upload_id,
			@qcreason as qc_reason,
			@qcpersonid as qc_person_id,
			lower(@FT) as file_type, 
			@UID as uid,
			@catID as category_id,		
			@ApplId as appl_id,
			@dcdate as qc_date,
			@SUB AS sub_category_name
			
			SET @newdocumentid = @@IDENTITY

			UPDATE documents
			SET file_type=CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END,
			url=@url + convert(varchar,document_id) + '.' + CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END
			WHERE document_id=@newdocumentid AND url IS NULL --AND qc_reason IN ('Fax', 'Email')
	END ELSE IF @DOCID IS NULL and @ApplId is null 
    BEGIN
		--print 'Insert 2'
		INSERT documents(document_date, created_date, created_by_person_id, profile_id,mail_upload_id, 
		qc_reason,qc_person_id,	file_type,  uid, category_id, appl_id, qc_date, sub_category_name)

		SELECT convert(smalldatetime,@DD) as document_date, 
			getdate() as created_date,
			@UserInterNalID as created_by_person_id,
			@PROFILEID as profile_id,
			NULL as mail_upload_id,
			@qcreason as qc_reason,
			@qcpersonid as qc_person_id,
			lower(@FT) as file_type, 
			@UID as uid,
			@catID as category_id,		
			@ApplId as appl_id,
			@dcdate as qc_date,
			@SUB AS sub_category_name

			SET @newdocumentid = @@IDENTITY

			UPDATE documents
			SET file_type=CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END,
			url=@url + convert(varchar,document_id) + '.' + CASE file_type WHEN 'tif' THEN 'pdf' ELSE file_type END
			WHERE document_id=@newdocumentid AND url IS NULL --AND qc_reason IN ('Fax', 'Email')
	END 
  END IF @Error=1
  BEGIN 
		SET @ErrorStr='[Forwarded to QC] ' + @ErrorStr 
		INSERT #output(name,value)
		VALUES ('Advisory',@newdocumentid)   --@documentid
  END ELSE IF @Error=2
  BEGIN 
	  	--SET @ErrorStr='[Forwarded to QC] ' + @ErrorStr 
		INSERT #output(name,value)
		VALUES ('Error',@ErrorStr)
  END ELSE 
  BEGIN  
		INSERT #output(name,value)
		VALUES ('Success',@newdocumentid)
  END  
  --SELECT * FROM DOCUMENTS WHERE DOCUMENT_ID=@documentid

  select * from #output

END



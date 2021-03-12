SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_mail_old]

AS

SET NOCOUNT ON

---read the next XML File in the directory
declare @xmlPath 		varchar(255)
declare @xmlFile 		varchar(255)
declare @cmd 			varchar(2000)
declare @xmlString 		varchar(8000)
declare @idoc 			int
declare @sql 			varchar(300)
declare @document_id		int
declare @operator		varchar(50)
declare @count			int
declare @max_documentid int

CREATE TABLE #cmdout (content varchar(8000))

--SET @xmlPath='d:\util\emails\msg_testing\out\'
SET @xmlPath='d:\util\emails\'

SET @cmd= 'dir /B /W ' + @xmlPath + '*.xml'

INSERT #cmdout
EXEC master..xp_cmdshell @cmd

SELECT @xmlFile=content FROM #cmdout WHERE content IS not null 		-----and content <>'File Not Found'

IF @xmlFile='' or @xmlFile IS null  or @xmlFile='File Not Found' RETURN
DELETE #cmdout

PRINT 'XML FILE IS:'
PRINT @xmlFile

SET @xmlFile = @xmlPath + @xmlFile


SET @sql='BULK INSERT #cmdout  FROM ' + char(39) + @xmlFile + char(39) + ' WITH (FIELDTERMINATOR = ''|'', ROWTERMINATOR = ''$$'')'
EXECUTE (@sql)

SELECT @xmlString=content FROM #cmdout

EXEC sp_xml_preparedocument @idoc output, @xmlString

CREATE TABLE #emails(mail_id  int identity(1,1), uploadid varchar(20), grantnumber varchar(50), applid int, alias varchar(100),uid varchar(50))

INSERT  #emails (grantnumber, applid,  alias, uid, uploadid)
SELECT grantnumber, applid,  alias, uid, uploadid
FROM  OPENXML (@idoc, '/Root/file', 2)   WITH  (grantnumber varchar(50), applid int, alias varchar(100),  uid varchar(20), uploadid varchar(20)) XML

--BEGIN TRAN
INSERT documents(alias, document_date, created_date, created_by_person_id, profile_id, qc_reason, file_type, mail_upload_id, uid, category_id, appl_id, qc_date, stored_date)
SELECT @xmlPath + alias, [date] as document_date, getdate() as created_date, person_id, isnull(profile_id, 1) as profile_id, 'Email', lower(file_type), uploadid, uid,
CASE WHEN (category IS NULL or category='') THEN 59 ELSE category_id END as category_id,
CASE WHEN (applid IS NULL or applid='' or applid=0) THEN NULL ELSE applid END as appl_id,
CASE WHEN (applid IS NULL or applid='' or applid=0 )THEN getdate() ELSE NULL END as qc_date,
CASE WHEN (applid IS NOT NULL and category IS NOT NULL ) THEN getdate() ELSE NULL END as stored_date

FROM OPENXML (@idoc, '/Root/file', 2)   WITH  
(documentid int, grantnumber varchar(20), applid int, category varchar(50), uploadid varchar(20), alias varchar(100), name varchar(100), date smalldatetime, uid varchar(20), file_type varchar(5), uploadid varchar(20)) xml
LEFT OUTER JOIN people p ON xml.uid=p.userid LEFT OUTER JOIN categories c ON xml.category=c.category_name
WHERE XML.documentid is null  or XML.documentid=0 or XML.documentid=''

/*
IF @@ERROR>0
BEGIN
RAISERROR('', 16, 1)
ROLLBACK TRAN
RETURN
END 
*/

SELECT @max_documentid= @@IDENTITY
SET @document_id=@max_documentid

WHILE @count>0
BEGIN
  	SELECT @operator = userid FROM people WHERE person_id=(SELECT created_by_person_id FROM documents WHERE document_id=@document_id)
	EXEC sp_rmc_egrants_doc_transaction @document_id, @operator,'created', 'created by email'
	SET @count=@count -1
	SET @document_id=@max_documentid-@count
      	CONTINUE
END


UPDATE documents 
SET 
--file_type=TXML.file_type,
processed_date=getdate(),
alias=@xmlPath + TXML.alias
FROM  OPENXML (@idoc, '/Root/file', 2)   WITH  (documentid int, alias varchar(50),  file_type varchar(4)) TXML
WHERE (TXML.documentid is not null or TXML.documentid<>0 or TXML.documentid<>'') and documents.document_id=TXML.documentid

INSERT  attachments (document_id,file_name,file_type,attachment_alias,document_alias)
SELECT documentid, name,  file_type,@xmlPath + alias,@xmlPath + parentalias
FROM  OPENXML (@idoc, '/Root/file/attachment', 2)   WITH  (documentid varchar(50), name varchar(100),alias varchar(100), file_type varchar(20),parentalias varchar(50)) XML

--COMMIT TRAN

SET @Cmd='del ' +@xmlFile
EXEC master..xp_cmdshell @Cmd
EXEC sp_xml_removedocument @idoc
GO


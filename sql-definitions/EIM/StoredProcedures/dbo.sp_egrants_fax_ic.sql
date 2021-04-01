SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_fax_ic]

@IC			 varchar(10)

AS

SET NOCOUNT ON

declare @Cmd 			varchar(4000)
declare @ProfileID 		tinyint
declare @PersonID 		int
declare @Path 			varchar(100)
declare @DirFile 		varchar(255)
declare @FaxFile 		varchar(255)
declare @max_documentid	int
declare @document_id		int
declare @count			int
declare @srvPath varchar(255)

SET @srvPath='d:\util\emails\'

--create table to save fax file info
CREATE TABLE #cmdout (content varchar(8000))

SELECT @PersonID=person_id from people where userid='fax'
SELECT @ProfileID=profile_id from profiles WHERE profile=@IC

SET @Path=@srvPath +  + LOWER(@IC) + '\'
SET @DirFile=@srvPath + 'faxdir.txt'

DELETE dir

--find all fax file 
SET @Cmd= 'dir ' + @Path + '  *.fax  /B /W >' + @DirFile

--insert fax file info to temporal table
INSERT #cmdout
EXEC master..xp_cmdshell @Cmd

select * from #cmdout

SELECT @FaxFile=content FROM #cmdout WHERE content IS not null 	---and content<>'File Not Found' 

---stop processing if nothing in the  temporal table
IF @FaxFile='' or @FaxFile IS null  RETURN		--or @faxFile='File Not Found' 
DELETE #cmdout

--copy fax fiel to data server
SELECT @Cmd= 'bcp eim..dir in ' + @DirFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '

PRINT @cmd
EXEC master..xp_cmdshell @Cmd

select * from dir

CREATE TABLE #fax (fax_id int identity(1,1), alias varchar(100), document_date smalldatetime)
INSERT #fax  (alias, document_date) SELECT @Path + path, convert(varchar,DATEADD (s, convert(int,left(path, len(path)-4)),'1/1/2004'),101) FROM dir

--insert file index to document table
INSERT documents(alias, category_id, document_date, created_date, qc_date, created_by_person_id, profile_id, qc_reason, file_type)
SELECT alias, 59, document_date, getdate(), getdate(), @PersonID, @ProfileID, 'fax' , 'tif' 
FROM #fax 
--SELECT @Path + path, 59, convert(varchar,DATEADD (s, convert(int,left(path, len(path)-4)),'1/1/2004'),101), getdate(), getdate(), @PersonID, @ProfileID,'fax' , 'tif' FROM dir

--find total fax count and largest document_id
SET @count = (SELECT max(fax_id)FROM #fax)

IF @count >=1
--insert transaction information
BEGIN
SELECT @max_documentid= @@IDENTITY
SET  @document_id=@max_documentid

---Leon's transaction update code...hmm...
WHILE @count>0
BEGIN
--sp_rmc_egrants_doc_transaction
  	EXEC sp_y2013_egrants_doc_transaction  @document_id,'fax','created', 'created by fax'
	SET @count=@count -1
	SET @document_id=@max_documentid - @count
      	CONTINUE
END

END

GO


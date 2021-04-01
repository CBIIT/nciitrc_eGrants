SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_misc_PDFImportNew]

@Path varchar(255)

AS

declare @CPath varchar(255)
declare @Cmd varchar(255)
declare @TempFile varchar(255)

--SET ANSI_NULLS ON
--SET ANSI_WARNINGS ON

declare @LDocID int
declare @Query varchar(255)
declare @RenameFile varchar(255)

SET @RenameFile='c:\pdf_rename.bat'
SELECT @LDocID=max(document_id) from pdf

SELECT @TempFile='c:\directory.txt'

SELECT @Cmd= 'dir ' + @Path + '*.pdf /B /W >>' + @TempFile
EXEC master..xp_cmdshell @Cmd


IF OBJECT_ID('tempdb..##directory') IS NOT NULL
DROP TABLE ##directory

CREATE TABLE ##directory(path varchar(255))

SELECT @Cmd= 'bcp ##directory in ' + @TempFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '
EXEC master..xp_cmdshell @Cmd

SELECT @Cmd='DEL ' + @TempFile
EXEC master..xp_cmdshell @Cmd

insert pdf(path,institute, serial_number,support_year,suffix)
select @Path + path, left(path,2) institute,convert(int,substring(path,3,6)) serial_number,
convert(int,substring(path,9,2)) support_year,substring(path,11,len(path)-14)
from ##directory

update pdf set suffix=null where suffix=''

update pdf
set appl_id=a.appl_id,document_date=created_date
from pdf, vw_appls a
where document_id>@LDocID and 
institute=admin_phs_org_code and
serial_number=serial_num and
pdf.support_year=a.support_year and
(suffix=suffix_code or (suffix is null and suffix_code is null))


---------------------------------------------------------------------------------------------------------------


--Rename the pdf files using numeric document_id

set @query=
'select ' + char(39) + 'ren ' + char(39) + ' + path + ' + char(39) + ' ' + char(39) + ' + convert(varchar,document_id) + ' + char(39) + '.pdf' + char(39)+ 
' from pdf'+
' where document_id>' + convert(varchar,@LDocID)


SET @Cmd= 'bcp "' + @Query + '" queryout ' + @RenameFile + '  -c -S ' + @@ServerName + ' -T'
EXEC master..xp_cmdshell @Cmd

--run the file
SET @Cmd=@RenameFile
EXEC master..xp_cmdshell @Cmd


--Get page info from pdf file files
SET @Cmd='e:\sqlbackup\appl\info_dir ' + @Path
EXEC master..xp_cmdshell @Cmd

EXEC sp_DocInfoDir @Path

--Add the new docs to the documents table
set identity_insert documents on

insert documents(document_id,category_id,revision,document_date,page_count,appl_id,url,file_type,created_by)
select document_id,38 category_id,0 revision,document_date,page_count,appl_id, 'https://web-grants.nci.nih.gov/unfunded/pdf1/' + convert(varchar,document_id) + '.pdf','pdf','remac'
from pdf 
where  document_id not in(select document_id from documents)

set identity_insert documents off

--move the newly renamed docs to ftp site
--create a txt ftp file pdf_ftpnew in Appl and then run f.bat ftpnew.txt
GO


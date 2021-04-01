SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE sp_egrants_check_ftp

AS

--SET NOCOUNT ON

declare @Cmd varchar(255)
declare @TempFile varchar(255)


SET @Cmd='e:\sqlbackup\appl\ls.bat'
SELECT @TempFile='c:\directory.txt'

EXEC master..xp_cmdshell @Cmd

IF OBJECT_ID('tempdb..##directory') IS NOT NULL
DROP TABLE ##directory

CREATE TABLE ##directory(path varchar(255))

SELECT @Cmd= 'bcp ##directory in ' + @TempFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '
EXEC master..xp_cmdshell @Cmd

--select * from ##directory

SELECT @Cmd='DEL ' + @TempFile
EXEC master..xp_cmdshell @Cmd

--return

delete ##directory where (path not like '%.pdf%') or path is null
delete ##directory where path like '%ls%'
delete ##directory where path like 'User%'

create table #t(document_id int)

--insert all the present accessions in the directory
insert #t
select convert(int,left(path,len(path)-5)) from ##directory

/* old code
select 
convert(int,
left(
right( path, Patindex('%/%',reverse(path))-1 ),
len(right( path, Patindex('%/%',reverse(path))-1))-5
))
from ##directory
*/


--delete the ones we already have
delete #t where document_id in
(
select document_id from documents where file_type='pdf'
)



update documents
set url='https://egrants-data.nci.nih.gov/funded/nci/nms/pdf/pdf_multipage/' + 
convert(varchar,#t.document_id) + '.pdf',
file_type='pdf',
converted_by='nms'
from documents,#t
where #t.document_id=documents.document_id
GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
---------------------------------------------------

----

CREATE PROCEDURE sp_egrants_facepage_zone

@zone varchar(3)

AS

SET NOCOUNT ON

declare @Path varchar(255)
declare @cmd varchar(255)
declare @TempFile varchar(255)
declare @CPath varchar(255)
declare @CDocID int
declare @sql varchar(255)


SELECT @TempFile='c:\directory.txt'


SET @Path='e:\docs\face\' + @zone + '\'

SELECT @Cmd= 'dir ' + @Path + '*.txt /B /W >>' + @TempFile
EXEC master..xp_cmdshell @Cmd

delete dir


SELECT @Cmd= 'bcp dir in ' + @TempFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '
EXEC master..xp_cmdshell @Cmd

SELECT @Cmd='DEL ' + @TempFile
EXEC master..xp_cmdshell @Cmd


insert face_pages(document_id)
select document_id from egrants, dir
where document_id=convert(int,left(path,patindex('%.%',path)-1)) and 
category_name in ('Application File','Application Update','SNAP Supplemental','Grant File') and
document_id not in (select document_id from face_pages)




declare cur CURSOR FOR
select path from dir order by path


open cur

FETCH NEXT FROM cur INTO @CPath

WHILE @@FETCH_STATUS=0

BEGIN

SET @CDocID=convert(int,left(@CPath,patindex('%.%',@CPath)-1))
SET @CPath=@Path + @CPath

PRINT @CDocID
PRINT @CPath

SET @sql='UPDATE face_pages SET ' + @zone + '='' '' WHERE document_id=' + convert(varchar,@CDocID)
exec (@sql)


SET @cmd='c:\mssql\mssql\binn\textcopy.exe ' + '/S ' + @@SERVERNAME +
 ' /U sa /P rubmkm /D eim /T face_pages  /W "where document_id=' +
 convert(varchar,@CDocID) + '"  /I /C ' + @zone + ' /F ' + @CPath


EXEC master..xp_cmdshell @cmd



FETCH NEXT FROM cur INTO @CPath

END

close cur
deallocate cur

GO


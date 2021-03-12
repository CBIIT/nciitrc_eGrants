SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_misc_BookmarkDir] 

@Dir varchar(255)
AS

declare @CFile varchar(255)
declare @CDocID int
declare @CPath varchar(500)


declare @cmd varchar(500)
declare @TempFile varchar(500)


SELECT @TempFile='c:\directory.txt'

SELECT @Cmd= 'dir ' + @Dir + '*.pdf /B /W >>' + @TempFile
EXEC master..xp_cmdshell @Cmd

IF OBJECT_ID('tempdb..##directory') IS NOT NULL
DROP TABLE ##directory

CREATE TABLE ##directory(path varchar(255))



SELECT @Cmd= 'bcp ##directory in ' + @TempFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '
EXEC master..xp_cmdshell @Cmd

SELECT @Cmd='DEL ' + @TempFile
EXEC master..xp_cmdshell @Cmd




DECLARE cur CURSOR FOR
SELECT path from ##directory

OPEN cur

FETCH NEXT FROM cur
INTO @CPath

WHILE @@Fetch_Status=0
BEGIN

SET @CDocID=convert(int,left(@CPath,len(@CPath)-4))
SET @CFile=@Dir + convert(varchar,@CDocID) + '.pdf.ind'

PRINT @CDocID
PRINT @CFile

exec sp_BookmarkFile @CDocID,@CFile



FETCH NEXT FROM cur
INTO @CPath

END


CLOSE cur
DEALLOCATE cur
GO


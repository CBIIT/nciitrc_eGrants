SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_misc_BookmarkCreate]

@DirIn varchar(255),
@DirOut varchar(255)
AS


declare @cmd varchar(255)
declare @CPdf varchar(255)
declare @CInd varchar(255)
declare @CNew varchar(255)
declare @CDocID int
declare @CPath varchar(255)

declare @TempFile varchar(500)

SELECT @TempFile='c:\directory.txt'

SELECT @Cmd= 'dir ' + @DirIn + '*.pdf /B /W >>' + @TempFile
EXEC master..xp_cmdshell @Cmd

IF OBJECT_ID('tempdb..##directory') IS NOT NULL
DROP TABLE ##directory

CREATE TABLE ##directory(path varchar(255))

SELECT @Cmd= 'bcp ##directory in ' + @TempFile + ' -c -t , -r \n -S ' + @@SERVERNAME + ' -T '
EXEC master..xp_cmdshell @Cmd

SELECT @Cmd='DEL ' + @TempFile
EXEC master..xp_cmdshell @Cmd




declare cur CURSOR FOR
select  path from ##directory

OPEN cur

FETCH NEXT from cur INTO @CPath


WHILE @@Fetch_Status=0
BEGIN


SET @CDocID=convert(int,  left(@CPath,len(@CPath)-4) )
SET @CPdf=@DirIn + convert(varchar,@CDocID) + '.pdf'
SET @CInd=@CPdf + '.ind'
SET @CNew=@DirOut + convert(varchar,@CDocID) + '.pdf'


SET @cmd='\\nt_gab_fs\home\ryabinsd\appl\bmark\pdcat -r -b -l ' + @CInd + ' ' + @Cpdf + ' ' + @CNew

print @cmd
EXEC master..xp_cmdshell @Cmd

FETCH NEXT from cur INTO @CPath


END

CLOSE cur
DEALLOCATE cur
GO


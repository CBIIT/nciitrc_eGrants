SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_misc_BookmarkFile]

@DocumentID int,
@File varchar(100)
 AS

declare @Cmd varchar(500)
declare @Query varchar(500)
declare @BaseFile varchar(255)
declare @TempFile varchar(255)


SET @BaseFile='\\nt_gab_fs\home\ryabinsd\index\base.txt'
SET @TempFile='\\nt_gab_fs\home\ryabinsd\index\temp.txt'

--SET @Cmd='@echo Bookmarks >>' + @File
--EXEC master..xp_cmdshell @Cmd


SET @Query=  'select char(48) + char(32) + char(34) + category_name + char(34) + char(32) +convert(varchar,page) from bookmarks where document_id='
+ convert(varchar,@DocumentID) +
' and page_id not in ' +
'(select b1.page_id from bookmarks b1,bookmarks b2   where b1.document_id=b2.document_id  and b2.page=b1.page+1 and b2.category_name=b1.category_name and b1.document_id=' +
convert(varchar,@DocumentID) + ')' +
' order by page'


--print @Query





SET @Cmd= 'bcp "' + @Query + '" queryout ' + @TempFile + '  -c -S ' + @@ServerName + ' -T'
EXEC master..xp_cmdshell @Cmd


SET @Cmd='copy ' + @BaseFile + '+'  + @TempFile + ' ' + @File
EXEC master..xp_cmdshell @Cmd
GO


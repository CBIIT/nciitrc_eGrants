SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =============================================
-- Author:		<Imran Omair>
-- Create date: <6/4/2010>
-- Description:	<This proc to be run once a day generates docs uploaded and to be inventoried>
-- Drawback: <If a file is uploaded from back end withought letting egrants know will not be inventoried>
-- =============================================
CREATE PROCEDURE [dbo].[sp_egrants_maint_script_PDFInventory] 
AS
BEGIN
DECLARE @cmd varchar(4000)
DECLARE @Dt varchar(20)
DECLARE @stmp varchar(50)
declare @Query varchar(2000)

set @stmp=getdate()
set @stmp=replace(@stmp,' ','')
set @stmp=replace(@stmp,':','_')


SELECT @Dt=convert(varchar,last_run_date,101) from scripts where script='PDFInventory'
SET @Dt= char(39) + @Dt + char(39)

SET @query=' select convert(varchar,e.document_id)+''.''+e.file_type as document, e.unix_directory from eim.dbo.egrants e, eim.dbo.scripts s where s.script=''PDFInventory'' '
SET @query=@query+' and (e.created_date >= s.last_run_date OR e.modified_date >= s.last_run_date) and e.profile_id=1 and e.url is not null and e.file_type=''pdf'' '
SET @query=@query+' and e.url like ''%.pdf'' and e.unix_file is not null and e.unix_directory is not null and page_count is null order by created_date desc'

--SET @query='select convert(varchar,e.document_id) +''.pdf'' as document, e.unix_directory '
--SET @query=@query+' from eim..documents d, eim..egrants e where d.document_id=e.document_id '
--SET @query=@query+' and d.url like ''%.pdf'' and d.url is not null and d.disabled_date is null'
--SET @query=@query+' and (convert(varchar(20),d.created_date,101) ='+@DT+' OR convert(varchar(20),d.modified_date,101) ='+@DT+') order by 2'

--SET @query='select convert(varchar,e.document_id) +''.pdf'' as document, e.unix_directory  from eim..egrants e where e.url like ''%.pdf'' and e.disabled_date is null and e.unix_directory   is not null and e.page_count is null and e.file_type=''pdf'' order by 2'
--print @query

SET @Cmd= 'bcp "' + @query + '" queryout \\NCIDB-P232-V\util\scripts\PDFInventory_' + @stmp + '.txt -c -T -S ' + @@SERVERNAME;


print @Cmd

EXEC master..xp_cmdshell @cmd

SET @query=''
SET @Cmd=''

SET @query='select convert(varchar,e.document_id)+''.''+file_type as document, e.unix_directory '
SET @query=@query+' from eim.dbo.egrants e, eim.dbo.scripts s where (e.created_date >= s.last_run_date or e.modified_date >= s.last_run_date)'
SET @query=@query+' and s.script=''PDFInventory'' and e.profile_id=1 and e.category_id=59 and e.url is not null  and e.unix_directory is not null and e.unix_file is not null'

--print @query

--SET @query='select convert(varchar,e.document_id)+''.''+file_type as document, e.unix_directory '
--SET @query=@query+' from eim..egrants e where e.category_id=59 '
----SET @query=@query+' and d.category_id in (59) '  --119,120,583,584,598 for all correspondence
--SET @query=@query+' and e.url is not null and e.disabled_date is null'
--SET @query=@query+' and (convert(varchar(20),e.created_date,101) >='+@DT+' OR convert(varchar(20),e.modified_date,101) >='+@DT+') order by 2'

--SET @query='select convert(varchar,e.document_id) +''.pdf'' as document, e.unix_directory '
--SET @query=@query+' from eim..documents d, eim..egrants e where d.document_id=e.document_id '
--SET @query=@query+' and d.category_id in (59) '  --119,120,583,584,598 for all correspondence
--SET @query=@query+' and d.url is not null and d.disabled_date is null'
--SET @query=@query+' and (convert(varchar(20),d.created_date,101) ='+@DT+' OR convert(varchar(20),d.modified_date,101) ='+@DT+') order by 2'

--SET @query = 'select convert(varchar,e.document_id) +''.pdf'' as document, e.unix_directory  from eim..egrants e where e.category_id =59 and e.page_count is null  and e.url is not null and e.disabled_date is null and e.unix_directory   is not null order by 2'

SET @Cmd= 'bcp "' + @query + '" queryout \\NCIDB-P232-V\util\scripts\Corres_' + @stmp + '.txt -c -T -S ' + @@SERVERNAME;

EXEC master..xp_cmdshell @cmd
print @Cmd


UPDATE scripts SET last_run_date=getdate() WHERE script='PDFInventory'
END

GO


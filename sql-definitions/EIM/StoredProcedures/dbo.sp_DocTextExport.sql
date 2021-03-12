SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE sp_DocTextExport 

@DocID int

AS

declare @Dir varchar(100)
declare @File varchar(100)
declare @cmd varchar(200)

declare @Fclause varchar(50)
declare @Wclause varchar(50)

SET @Dir='e:\txt\'
SET @File=@Dir + convert(varchar,@DocID) +'.txt'

SET @Fclause='/F ' + @File + ' '
SET @Wclause='/W ' +'"where document_id=' + convert(varchar,@DocID) + '" '

SET @cmd='c:\mssql/mssql/binn/textcopy /S SQLDB /U egrants /P egrants /D ncieim_b /T documents_text /C txt /O ' + @Wclause + @Fclause

--print @cmd

exec master..xp_cmdshell @cmd
GO


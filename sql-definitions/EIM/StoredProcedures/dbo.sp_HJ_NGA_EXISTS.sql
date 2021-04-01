SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_HJ_NGA_EXISTS]
(
@input_appl_id int, 
@input_doc_type_code nvarchar(3)
)
AS

BEGIN

DECLARE @appl_id int, @doc_type_code nvarchar(3), @SQL nvarchar(500), @param nvarchar(100)
DECLARE @Cnt int, @Cnt_Out int

SET @appl_id = @input_appl_id
SET @doc_type_code = @input_doc_type_code

--SET @appl_id = 1188884
--SET @doc_type_code = 'AWS'

SET @SQL = 'SELECT * FROM OPENQUERY(IRDB, ''SELECT COUNT(*) FROM docs 
WHERE key_id = '+ CAST(@appl_id  AS VARCHAR(10))+' AND doc_type_code = '''''+ @doc_type_code + ''''''')'
PRINT @SQL

--EXEC (@SQL)

SET @param = '@Cnt int output'

EXEC sp_executesql @sql, @param, @Cnt_Out out

END

GO


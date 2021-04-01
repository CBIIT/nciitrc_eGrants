SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_isValidNOA] (@applid INT, @doctype VARCHAR(10))
RETURNS VARCHAR(200) AS
BEGIN
declare @s varchar(200)
set @s ='''select cnt from openquery(IRDB, ''select count(*) as cnt from docs where key_id='+CAST(@APPLID AS VARCHAR)+' AND DOC_TYPE_CODE='''''+@doctype+''''''')''' 
--SELECT @S
RETURN
(
@S
--EXEC(@S)
)
END

GO


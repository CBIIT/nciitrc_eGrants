SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create FUNCTION [dbo].[fn_str_decode](@str varchar(800))

RETURNS varchar(800)

AS
BEGIN 

DECLARE 
@count		int, 
@c			char(1), 
@cenc		char(2), 
@i			int, 
@strReturn	varchar(800) 

SET @count = Len(@str) 
SET @i = 1 
SET @strReturn = '' 
    
WHILE (@i <= @count) 
BEGIN 
SET @c = substring(@str, @i, 1) 
        
	IF @c LIKE '[!%]' ESCAPE '!' 
	BEGIN 
		SET @cenc = substring(@str, @i + 1, 2) 
		SET @c = CHAR(CASE WHEN SUBSTRING(@cenc, 1, 1) LIKE '[0-9]' 
		THEN CAST(SUBSTRING(@cenc, 1, 1) as int) 
	ELSE CAST(ASCII(UPPER(SUBSTRING(@cenc, 1, 1)))-55 as int) 
	END * 16 + CASE WHEN SUBSTRING(@cenc, 2, 1) LIKE '[0-9]' 
		THEN CAST(SUBSTRING(@cenc, 2, 1) as int) 
		ELSE CAST(ASCII(UPPER(SUBSTRING(@cenc, 2, 1)))-55 as int) 
	END) 
            
	SET @strReturn = @strReturn + @c 
	SET @i = @i + 2 
END 
ELSE 
	BEGIN 
		SET @strReturn = @strReturn + @c 
	END 
SET @i = @i +1 
END 
    
    RETURN @strReturn
END


GO


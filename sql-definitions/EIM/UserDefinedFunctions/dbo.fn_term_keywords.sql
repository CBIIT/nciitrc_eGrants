SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_term_keywords](@term varchar(50))

RETURNS varchar(100) AS  
BEGIN 

DECLARE @S varchar(100)
SET @S=''

SELECT @S= isnull(keywords,'') from terms where term=@term
RETURN @S


END

GO


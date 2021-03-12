SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create FUNCTION [dbo].[fn_get_personal_assigment] (@person_id int, @widget_id int)
  
RETURNS char

BEGIN

DECLARE @count		int
DECLARE @assigment	char(1)

SET @count =(select count(*) from dbo.DB_WIDGET_ASSIGNMENT where widget_id=@widget_id and person_id=@person_id and end_date is null)

IF @count = 1 SET @assigment='y' ElSE SET @assigment='n'

RETURN @assigment

END
GO


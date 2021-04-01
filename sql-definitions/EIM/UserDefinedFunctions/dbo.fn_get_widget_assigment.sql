SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE FUNCTION [dbo].[fn_get_widget_assigment] (@userid varchar(20), @widget_id int)
  
RETURNS char

BEGIN

DECLARE @count		int
DECLARE @assigment	char(1)

SET @count =(select count(*) from dbo.DB_WIDGET_ASSIGNMENT where widget_id=@widget_id and userid=@userid and end_date is null)

IF @count = 1 SET @assigment='y' ElSE SET @assigment='n'

RETURN @assigment

END
GO


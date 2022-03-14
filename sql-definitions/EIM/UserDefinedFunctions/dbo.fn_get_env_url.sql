SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE FUNCTION [dbo].[fn_get_env_url] (@name varchar(max))

RETURNS varchar(max) 
BEGIN 

DECLARE @url varchar(800)

select @url=url from [dbo].[EnvUrl]	
where ServerName = @@SERVERNAME
and name  = @name

RETURN @url

END


GO


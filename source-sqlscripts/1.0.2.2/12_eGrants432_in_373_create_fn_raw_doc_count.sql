USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_raw_doc_count]    Script Date: 1/4/2024 10:55:49 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE OR ALTER FUNCTION [dbo].[fn_raw_doc_count]  (@ApplID int)  
RETURNS int

AS  
BEGIN 

declare @count  int

select @count=count(*) from documents where appl_id=@ApplID

--IF @count=0 select @count=count(*) from funding_appls where appl_id=@ApplID and disabled_date is null

return @count 

END

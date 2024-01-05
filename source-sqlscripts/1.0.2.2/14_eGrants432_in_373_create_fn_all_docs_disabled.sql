USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_doc_count]    Script Date: 1/4/2024 10:55:49 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE OR ALTER FUNCTION [dbo].[fn_all_docs_disabled]  (@ApplID int)  
RETURNS int

AS  
BEGIN 

	declare @countAllDocuments int
	declare @countDisabledDocuments int
	select @countAllDocuments = count(*) from documents where appl_id = @ApplID
	select @countDisabledDocuments = count(*) from documents where appl_id = @ApplID and disabled_date is NOT null
	--print(CONCAT('disabled: ', @countDisabledDocuments))
	--print(CONCAT('all: ', @countAllDocuments))
	return(case when @countDisabledDocuments = @countAllDocuments THEN 1 else 0 end)

END

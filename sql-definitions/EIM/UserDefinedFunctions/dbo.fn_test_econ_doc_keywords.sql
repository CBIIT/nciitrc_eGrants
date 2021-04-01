SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE   FUNCTION [dbo].[fn_test_econ_doc_keywords]  (@DocID int)  
RETURNS varchar(1000)

AS  
BEGIN 

RETURN (
		select  ''
		as keywords ) 
END

GO


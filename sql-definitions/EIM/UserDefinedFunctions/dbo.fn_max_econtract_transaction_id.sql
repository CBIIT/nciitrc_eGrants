SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE FUNCTION [dbo].[fn_max_econtract_transaction_id](
		@document_id int,
		@action_type varchar(20) )
RETURNS int as
--smalldatetime AS  
BEGIN 

declare @t_id int


SELECT @t_id = max (transaction_id)
from dbo.econtract_transactions
WHERE action_type = @action_type and document_id = @document_id 

RETURN @t_id
end
GO


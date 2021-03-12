SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE FUNCTION [dbo].[fn_max_closeout_date](@grant_id int)
RETURNS smalldatetime AS  
BEGIN 

declare @max_date smalldatetime

select @max_date=max(grant_close_out_date)
from appls
where grant_id = @grant_id

RETURN @max_date
end
GO


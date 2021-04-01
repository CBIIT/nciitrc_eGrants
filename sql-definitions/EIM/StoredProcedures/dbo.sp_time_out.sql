SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create PROCEDURE sp_time_out
as 

EXEC sp_configure 'remote query timeout', 0

GO


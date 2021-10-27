SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create procedure sp_email_get_rules
as 
BEGIN
Select * from EmailRules
END
GO


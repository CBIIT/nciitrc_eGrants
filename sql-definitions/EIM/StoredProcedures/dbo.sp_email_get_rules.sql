SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

create   procedure [dbo].[sp_email_get_rules]
 @all bit = 0
as 
BEGIN
Select * from EmailRules where ((@all = 0  and Enabled = 1) or (@all = 1))
END

GO


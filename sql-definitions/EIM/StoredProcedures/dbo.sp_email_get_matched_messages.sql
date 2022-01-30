SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON




CREATE         procedure [dbo].[sp_email_get_matched_messages]
	@ruleid INT,
	@all bit
as 
BEGIN
Select [EmailRuleId]
      ,[EmailMessageId]
      ,[CreatedDate]
      ,[ActionsCompleted]
      ,[Matched]
  FROM [dbo].[EmailRulesMatchedMessages]
  where EmailRuleId = @ruleid and Matched = 1 and ((@all = 1) OR (@all= 0 AND ActionsCompleted = 0))

END

GO


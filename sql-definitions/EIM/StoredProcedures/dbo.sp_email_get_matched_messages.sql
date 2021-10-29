SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE       procedure [dbo].[sp_email_get_matched_messages]
	@ruleid INT
as 
BEGIN
Select [EmailRuleId]
      ,[EmailMessageId]
      ,[CreatedDate]
      ,[ActionsCompleted]
      ,[Matched]
  FROM [dbo].[EmailRulesMatchedMessages]
  where EmailRuleId = @ruleid and Matched = 1

END

GO


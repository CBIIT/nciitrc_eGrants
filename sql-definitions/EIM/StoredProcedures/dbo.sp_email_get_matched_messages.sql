SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON







CREATE    procedure [dbo].[sp_email_get_matched_messages]
	@ruleid int,
	@all bit,
	@msgId int
as 
BEGIN
Select [EmailRuleId]
      ,[EmailMessageId]
      ,[CreatedDate]
      ,[ActionsCompleted]
      ,[Matched]
  FROM [dbo].[EmailRulesMatchedMessages]
  where Matched = 1 and ((@ruleId = 0) or (EmailRuleId = @ruleid)) and ((@all = 1) OR (@all= 0 AND ActionsCompleted = 0)) and ((EmailMessageId = @msgId) or (@msgId = 0))

END

GO


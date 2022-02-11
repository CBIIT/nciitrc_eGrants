SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON







CREATE       procedure [dbo].[sp_email_get_matched_messages_full]
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
      ,[EmailMonitoredMailboxId]
      ,[GraphId]
      ,[CreatedDateTime]
      ,[LastModifiedDateTime]
      ,[ReceivedDateTime]
      ,[SentDateTime]
      ,[HasAttachments]
      ,[Subject]
      ,[BodyPreview]
      ,[Importance]
      ,[ParentFolderId]
      ,[IsRead]
      ,[Body]
      ,[Sender]
      ,[EmailFrom]
      ,[ToRecipients]
      ,[CcRecipients]
  FROM [dbo].[EmailRulesMatchedMessages] match inner join EmailMessages em on match.EmailMessageId = em.Id 
  where Matched = 1 and (@ruleId = 0 or EmailRuleId = @ruleid) and ((@all = 1) OR (@all= 0 AND ActionsCompleted = 0)) and (EmailMessageId = @msgId and not(@msgId = 0))

END

GO


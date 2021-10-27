SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE   procedure [dbo].[sp_email_get_messages]
	@ruleid INT
as 
BEGIN
Select [Id]
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
      ,[From]
      ,[ToRecipients]
      ,[CcRecipients]
	  
from EmailMessages em 
	 left join EmailRulesMatchedMessages matched on em.Id = matched.EmailMessageId
where matched.EmailMessageId is null 
	  and matched.EmailRuleId = @ruleid

END

GO


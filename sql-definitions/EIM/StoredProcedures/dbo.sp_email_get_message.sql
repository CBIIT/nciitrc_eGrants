SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE       procedure [dbo].[sp_email_get_message]
	@msgid INT
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
      ,[EmailFrom]
      ,[ToRecipients]
      ,[CcRecipients]
	  
from EmailMessages 
where Id = @msgid

END

GO


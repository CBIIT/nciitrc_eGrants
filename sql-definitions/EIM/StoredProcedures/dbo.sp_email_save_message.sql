SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



/*
		cmd.Parameters.Refresh
		cmd.Parameters(1).Value=CItem.ReceivedTime
		cmd.Parameters(2).Value=CItem.To
		cmd.Parameters(3).Value=CItem.Recipients
		cmd.Parameters(4).Value=CItem.SenderName
		cmd.Parameters(5).Value=CItem.CC
		cmd.Parameters(6).Value=CItem.BCC
		cmd.Parameters(7).Value=CItem.Subject
		cmd.Parameters(8).Value=CItem.BodyFormat
		cmd.Parameters(9).Value=CItem.Body
		cmd.Parameters(10).Value=CItem.HTMLBody
*/

CREATE   Procedure [dbo].[sp_email_save_message]
		@ReceivedTime varchar(255), 
		@To varchar(255),
--		@Recipients varchar(255),
		@SenderEmailAddress varchar(255),
--		@CC varchar(255),
		@Subject varchar(255),
		@BodyFormat int,
		@Body varchar(MAX),
		@HTMLBody varchar(MAX),
		@HasAttachments int
as 
BEGIN

Declare @bitAttachments bit = 0

If @HasAttachments > 0-- 'true' 
 set @bitAttachments = 1 


INSERT INTO EmailMessages
	  ([EmailMonitoredMailboxId]
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
      ,[EmailFrom]
	  ,[Sender]
      ,[ToRecipients])
--      ,[CcRecipients])

	  VALUES
	  (1
      ,'xxx'
      ,@ReceivedTime
      ,GETDATE()
      ,@ReceivedTime
      ,''
      ,@bitAttachments
      ,@Subject
      ,null
      ,null
      ,null
      ,null
      ,@Body
      ,@SenderEmailAddress
	  ,@SenderEmailAddress
       ,@To)
--      ,@CC)

--	  Select @@IDENTITY 
END



GO


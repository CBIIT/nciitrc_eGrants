SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE   procedure [dbo].[sp_email_save_matches]
	@EmailRuleId INT
    , @EmailMessageId VARCHAR(200)
    , @CreatedDate smalldatetime
    , @ActionsCompleted bit
    , @Matched bit
as 
BEGIN

INSERT INTO EmailRulesMatchedMessages(
      EmailRuleId
	  ,[EmailMessageId]
      ,[CreatedDate]
      ,[ActionsCompleted]
      ,[Matched])
	  VALUES (
	  @EmailRuleId
	 ,@EmailMessageId
     ,@CreatedDate
     ,@ActionsCompleted 
     ,@Matched)

END


GO


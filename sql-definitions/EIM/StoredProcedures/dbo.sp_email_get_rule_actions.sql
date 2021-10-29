SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON




CREATE     procedure [dbo].[sp_email_get_rule_actions]
	@ruleid INT
as 
BEGIN
SELECT [Id]
      ,[EmailRulesId]
      ,[Order]
      ,[Description]
      ,[ActionType]
      ,[TargetValue]
      ,[CreatedByPersonId]
      ,[CreatedDate]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
      ,[EmailTemplateId]
  FROM [dbo].[EmailRulesActions]
where EmailRulesId = @ruleid

END

GO


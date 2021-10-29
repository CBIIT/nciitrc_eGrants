SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE     procedure dbo.sp_email_get_rule_criteria
	@ruleid INT
as 
BEGIN
SELECT [Id]
      ,[Order]
      ,[EmailRulesId]
      ,[CriteriaType]
      ,[FieldToEval]
      ,[EvalType]
      ,[EvalValue]
      ,[CreatedByPersonId]
      ,[CreatedDate]
      ,[LastModifiedBy]
      ,[LastModifiedDate]
  FROM [dbo].[EmailRulesCriteria]
where EmailRulesId = @ruleid

END

GO


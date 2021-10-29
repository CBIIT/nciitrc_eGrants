SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesCriteria](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Order] [int] NULL,
	[EmailRulesId] [int] NOT NULL,
	[CriteriaType] [int] NOT NULL,
	[FieldToEval] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[EvalType] [int] NOT NULL,
	[EvalValue] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[CreatedByPersonId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedDate] [smalldatetime] NULL
) ON [PRIMARY]

GO


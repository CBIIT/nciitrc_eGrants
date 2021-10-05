SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmailRuleId] [int] NOT NULL,
	[RunDate] [smalldatetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedDate] [smalldatetime] NULL
) ON [PRIMARY]

GO


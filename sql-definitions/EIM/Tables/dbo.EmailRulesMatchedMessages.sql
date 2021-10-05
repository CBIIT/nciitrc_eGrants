SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesMatchedMessages](
	[EmailRuleId] [int] NOT NULL,
	[EmailMessageId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[ActionsCompleted] [bit] NULL
) ON [PRIMARY]

GO


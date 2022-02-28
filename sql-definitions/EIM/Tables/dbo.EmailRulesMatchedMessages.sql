SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesMatchedMessages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmailRuleId] [int] NOT NULL,
	[EmailMessageId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[ActionsCompleted] [bit] NULL,
	[Matched] [bit] NULL,
	[LastModifiedDate] [smalldatetime] NULL
) ON [PRIMARY]

GO


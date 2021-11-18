SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesActionResults](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ActionId] [int] NOT NULL,
	[RuleId] [int] NOT NULL,
	[MessageId] [int] NOT NULL,
	[Successful] [bit] NULL,
	[ActionStarted] [bit] NULL,
	[ActionCompleted] [bit] NULL,
	[ActionMessage] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ExceptionText] [varchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[ActionDataPassed] [varchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


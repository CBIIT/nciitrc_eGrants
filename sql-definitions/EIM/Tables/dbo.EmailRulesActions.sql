SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRulesActions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Order] [int] NULL,
	[Description] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[ActionType] [int] NOT NULL,
	[TargetValue] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[CreatedByPersonId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedDate] [smalldatetime] NULL,
	[EmailTemplateId] [int] NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailRules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Criteria_any] [bit] NOT NULL,
	[Enabled] [bit] NOT NULL,
	[Nextrun] [datetime] NULL,
	[Lastrun] [datetime] NULL,
	[Interval] [int] NOT NULL,
	[CreatedByPersonId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedDate] [smalldatetime] NULL
) ON [PRIMARY]

GO


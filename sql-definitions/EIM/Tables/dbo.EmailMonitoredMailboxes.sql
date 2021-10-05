SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailMonitoredMailboxes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Description] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[MailboxName] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[MailboxUseName] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[OAuthToken] [varchar](600) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Enabled] [bit] NOT NULL,
	[CreatedByPersonId] [int] NOT NULL,
	[CreatedDate] [smalldatetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModifiedDate] [smalldatetime] NULL
) ON [PRIMARY]

GO


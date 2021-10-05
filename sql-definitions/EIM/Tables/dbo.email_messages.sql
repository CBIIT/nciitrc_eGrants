SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_messages](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[email_monitored_mailbox_id] [int] NOT NULL,
	[graphid] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[createdDateTime] [datetime] NOT NULL,
	[lastModifiedDateTime] [datetime] NOT NULL,
	[receivedDateTime] [datetime] NOT NULL,
	[sentDateTime] [datetime] NOT NULL,
	[hasAttachments] [bit] NULL,
	[subject] [varchar](400) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[bodyPreview] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[importance] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[parentFolderId] [varchar](300) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[isRead] [bit] NULL,
	[body] [varchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[sender] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[from] [varchar](250) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[toRecipients] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[ccRecipients] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


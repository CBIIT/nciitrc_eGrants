SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailMessagesStaging](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmailMonitoredMailboxId] [int] NULL,
	[GraphId] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
	[LastModifiedDateTime] [datetime] NOT NULL,
	[ReceivedDateTime] [datetime] NOT NULL,
	[SentDateTime] [datetime] NOT NULL,
	[HasAttachments] [bit] NULL,
	[Subject] [varchar](400) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[BodyPreview] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Importance] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ParentFolderId] [varchar](300) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[IsRead] [bit] NULL,
	[Body] [varchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Sender] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[EmailFrom] [varchar](250) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[ToRecipients] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[CcRecipients] [varchar](1000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


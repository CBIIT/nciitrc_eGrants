SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_monitored_mailboxes](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[description] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[mailbox_name] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[mailbox_user_name] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[email_token] [varchar](600) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[enabled] [bit] NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[last_modified_by] [int] NULL,
	[last_modified_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


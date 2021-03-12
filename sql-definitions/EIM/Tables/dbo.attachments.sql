SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[attachments](
	[attachment_id] [int] IDENTITY(1,1) NOT NULL,
	[document_id] [int] NULL,
	[file_name] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[file_type] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[attachment_alias] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[document_alias] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


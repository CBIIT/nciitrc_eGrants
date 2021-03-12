SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[niaid_nga](
	[document_id] [int] NULL,
	[url] [varchar](8000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[full_grant_num] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_id] [int] NULL,
	[impac] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


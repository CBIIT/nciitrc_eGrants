SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[documents_nlm_legacy](
	[document_id] [int] NOT NULL,
	[grant_id] [int] NULL,
	[category_id] [smallint] NULL,
	[appl_id] [int] NULL,
	[url] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


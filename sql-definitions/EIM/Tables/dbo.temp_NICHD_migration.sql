SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[temp_NICHD_migration](
	[fgn] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[category] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[document_id] [int] NULL,
	[zipflnm] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[uxflnm] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


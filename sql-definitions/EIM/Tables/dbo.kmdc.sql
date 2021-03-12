SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[kmdc](
	[appl_id] [int] NOT NULL,
	[document_id] [int] NULL,
	[descr] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[corrupted] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


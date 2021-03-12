SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[DB_EARLY_CONCUR_FLAG](
	[userid] [nchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_id] [int] NULL,
	[fgn] [nvarchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[budget_start_date] [datetime] NULL,
	[NCAB_date] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


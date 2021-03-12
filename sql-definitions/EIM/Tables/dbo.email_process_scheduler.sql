SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_process_scheduler](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[folder_path] [nvarchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[default_ic] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[default_category_id] [nchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[default_category] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[lastRun_dt] [datetime] NOT NULL,
	[RunStatus] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ProdStatus] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[SaveBody] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[MovetoOld] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[QC] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


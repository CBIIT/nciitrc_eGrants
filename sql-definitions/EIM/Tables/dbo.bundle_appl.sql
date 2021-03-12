SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[bundle_appl](
	[appl_id] [int] NULL,
	[run_date] [datetime] NULL,
	[awarded_date] [datetime] NULL,
	[package_id] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[wip_stage] [smallint] NULL,
	[stage_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


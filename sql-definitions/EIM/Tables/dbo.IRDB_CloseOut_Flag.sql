SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IRDB_CloseOut_Flag](
	[appl_id] [int] NULL,
	[appl_type_code] [varchar](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[full_grant_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[fy] [int] NULL,
	[project_period_end_date] [datetime] NULL,
	[closeout_status_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[grant_close_date] [datetime] NULL,
	[ltr1_date] [datetime] NULL,
	[ltr2_date] [datetime] NULL,
	[ltr3_date] [datetime] NULL,
	[progress_rpt_accepted_date] [datetime] NULL,
	[final_report_date] [datetime] NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[impp_gm_closeouts](
	[appl_id] [int] NOT NULL,
	[closeout_status_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ltr1_date] [datetime] NULL,
	[final_report_date] [datetime] NULL,
	[final_invention_stmnt_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


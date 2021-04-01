SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[document_destruction](
	[tab_id] [int] NULL,
	[grant_id] [int] NULL,
	[serial_num] [int] NULL,
	[full_grant_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[activity_code] [char](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_id] [int] NULL,
	[fy] [smallint] NULL,
	[pi_name] [varchar](92) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[org_name] [varchar](120) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_status_group_descrip] [varchar](60) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[grant_close_date] [smalldatetime] NULL,
	[project_start_date] [smalldatetime] NULL,
	[project_end_date] [smalldatetime] NULL,
	[released_date] [smalldatetime] NULL,
	[selected_tobe_destroyed] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[selected_by] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[selected_date] [datetime] NULL,
	[destruction_reason] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[notes] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[approved_tobe_destroyed] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[approved_by] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[approved_date] [datetime] NULL,
	[added_date] [smalldatetime] NOT NULL,
	[proposed_destruction_date] [smalldatetime] NULL,
	[destroyed_date] [smalldatetime] NULL,
	[destroyed_by] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[destroyed_bkup_file_path] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[destroyed_bkup_retention_period] [int] NULL,
	[funded_not_funded] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[impp_closeout_dt] [datetime] NULL
) ON [PRIMARY]

GO


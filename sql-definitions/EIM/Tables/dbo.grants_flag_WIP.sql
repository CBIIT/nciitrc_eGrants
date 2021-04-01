SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[grants_flag_WIP](
	[grant_id] [int] NOT NULL,
	[appl_id] [int] NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[stop_sign] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[arra_flag] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


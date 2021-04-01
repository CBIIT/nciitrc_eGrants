SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[grants_flag_DS_WIP](
	[appl_id] [int] NOT NULL,
	[grant_id] [int] NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[appl_type_code] [int] NULL,
	[Diversity_reentry_flag] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


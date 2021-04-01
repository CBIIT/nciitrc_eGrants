SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[grants_flag_MS_WIP](
	[appl_id] [int] NOT NULL,
	[grant_id] [int] NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[support_year] [int] NULL,
	[suffix_code] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[nci_budget_subclass_code] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


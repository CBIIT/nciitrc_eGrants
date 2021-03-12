SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[appls_nigms_dups](
	[appl_id] [int] NOT NULL,
	[grant_id] [int] NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[appl_type_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[activity_code] [char](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[support_year] [tinyint] NULL,
	[suffix_code] [varchar](4) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[mechanism_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[exist_flag] [int] NULL
) ON [PRIMARY]

GO


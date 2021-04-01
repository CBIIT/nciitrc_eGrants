SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[grants_backup](
	[grant_id] [int] NOT NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[serial_num] [int] NOT NULL,
	[mechanism_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[close_out_date] [smalldatetime] NULL,
	[former_admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[former_serial_num] [int] NULL,
	[future_admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[future_serial_num] [int] NULL,
	[stop_sign] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[paperless] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL
) ON [PRIMARY]

GO


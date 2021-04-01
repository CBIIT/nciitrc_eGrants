SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[grants](
	[grant_id] [int] IDENTITY(1,1) NOT NULL,
	[admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[serial_num] [int] NOT NULL,
	[mechanism_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[close_out_date] [smalldatetime] NULL,
	[former_admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[former_serial_num] [int] NULL,
	[future_admin_phs_org_code] [char](2) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[future_serial_num] [int] NULL,
	[stop_sign] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[paperless] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[is_tobacco] [bit] NULL,
	[grant_close_date] [smalldatetime] NULL,
	[to_be_destroyed] [bit] NULL,
	[destruction_reason] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[arra_flag] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[fda_flag] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


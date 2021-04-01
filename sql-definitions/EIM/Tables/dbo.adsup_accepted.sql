SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adsup_accepted](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[supp_appl_id] [int] NOT NULL,
	[full_grant_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Former_num] [varchar](19) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[serial_num] [int] NULL,
	[Former_appl_id] [int] NULL,
	[Action_date] [smalldatetime] NULL,
	[admin_supp_action_code] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Notification_id] [int] NULL
) ON [PRIMARY]

GO


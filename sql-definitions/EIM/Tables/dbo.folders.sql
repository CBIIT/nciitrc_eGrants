SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[folders](
	[folder_id] [int] IDENTITY(1,1) NOT NULL,
	[bar_code] [nvarchar](8) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[created_date] [datetime] NULL,
	[destroyed_date] [datetime] NULL,
	[grant_id] [int] NULL,
	[location_id] [smallint] NULL,
	[specialist_id] [smallint] NULL,
	[box_id] [int] NULL,
	[to_be_scanned] [bit] NOT NULL,
	[latest_move_date] [datetime] NULL,
	[latest_access_date] [datetime] NULL,
	[profile_id] [smallint] NULL,
	[is_transfer] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[removed_content_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


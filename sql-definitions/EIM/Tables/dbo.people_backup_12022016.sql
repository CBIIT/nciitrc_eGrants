SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[people_backup_12022016](
	[person_id] [int] IDENTITY(1,1) NOT NULL,
	[person_name] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[userid] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[profile_id] [smallint] NULL,
	[position_id] [smallint] NULL,
	[type] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[active] [bit] NULL,
	[application_type] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[team_id] [int] NULL,
	[cft] [int] NULL,
	[econ] [int] NULL,
	[gft] [int] NULL,
	[mgt] [int] NULL,
	[egrants] [int] NULL,
	[admin] [int] NULL,
	[docman] [int] NULL,
	[start_date] [datetime] NOT NULL,
	[end_date] [datetime] NULL,
	[created_by] [int] NOT NULL,
	[created_date] [datetime] NOT NULL,
	[last_updated_by] [int] NULL,
	[last_updated_date] [datetime] NULL
) ON [PRIMARY]

GO


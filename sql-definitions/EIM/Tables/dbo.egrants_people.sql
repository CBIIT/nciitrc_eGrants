SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[egrants_people](
	[person_id] [int] IDENTITY(2863,1) NOT NULL,
	[person_name] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[userid] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[nedid] [char](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[profile_id] [smallint] NULL,
	[start_date] [smalldatetime] NOT NULL,
	[end_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


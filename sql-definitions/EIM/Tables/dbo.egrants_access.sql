SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[egrants_access](
	[access_id] [int] IDENTITY(1,1) NOT NULL,
	[person_id] [int] NOT NULL,
	[application_type] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[position_id] [smallint] NOT NULL,
	[start_date] [smalldatetime] NOT NULL,
	[end_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


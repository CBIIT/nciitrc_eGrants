SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GPM_Report_User](
	[User_id] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[User_Name] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[start_date] [smalldatetime] NULL,
	[end_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


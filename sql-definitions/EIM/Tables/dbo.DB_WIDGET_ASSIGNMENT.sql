SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[DB_WIDGET_ASSIGNMENT](
	[person_id] [int] NULL,
	[userid] [varchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[widget_id] [int] NULL,
	[sortorder] [int] NULL,
	[start_date] [datetime] NULL,
	[end_date] [datetime] NULL
) ON [PRIMARY]

GO


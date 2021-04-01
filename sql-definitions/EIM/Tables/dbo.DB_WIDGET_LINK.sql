SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[DB_WIDGET_LINK](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Category_name] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Link_title] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Link_url] [varchar](300) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[sort_order] [int] NULL,
	[start_date] [datetime] NULL,
	[end_date] [datetime] NULL,
	[category_id] [int] NULL,
	[icon_name] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


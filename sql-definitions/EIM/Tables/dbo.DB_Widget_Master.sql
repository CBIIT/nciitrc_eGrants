SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[DB_Widget_Master](
	[widget_id] [int] IDENTITY(1,1) NOT NULL,
	[widget_title] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[description] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[start_date] [datetime] NULL,
	[end_date] [datetime] NULL,
	[widget_method] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[widget_method_param] [varchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[widget_help_url] [varchar](300) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[template_name] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Access_Level] [smallint] NULL
) ON [PRIMARY]

GO


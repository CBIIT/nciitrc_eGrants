SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[dir_cmd_supp](
	[path] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[new_path] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[move_cmd] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


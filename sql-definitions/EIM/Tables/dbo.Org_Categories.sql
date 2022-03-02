SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Org_Categories](
	[doctype_id] [int] IDENTITY(1,1) NOT NULL,
	[doctype_name] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[tobe_flagged] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[icon_path] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Flag_period] [int] NULL,
	[comments_required] [bit] NULL
) ON [PRIMARY]

GO


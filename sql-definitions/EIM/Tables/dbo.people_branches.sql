SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[people_branches](
	[branch_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_name] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[profile_id] [int] NULL,
	[application_type] [char](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[branch_acronym] [char](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[people_teams](
	[team_id] [int] IDENTITY(1,1) NOT NULL,
	[team_name] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[branch_id] [int] NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Greensheets_Show_Hide_Criteria](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[appl_type_id] [int] NOT NULL,
	[suffix_code] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[action_type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[GREENSHEETS_Status] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[GPMATS_Cancel_Flag] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[show_greensheet] [bit] NOT NULL,
	[active] [bit] NOT NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[CIIP_Greensheet_Rev](
	[appl_id] [int] NULL,
	[agt_id] [int] NULL,
	[submitted_date] [datetime] NULL,
	[Revision_type_description] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[current_action_status_code] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_type_code] [int] NULL,
	[suffix_code] [nvarchar](6) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[GPMATS_CANCELLED_FLAG] [nvarchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


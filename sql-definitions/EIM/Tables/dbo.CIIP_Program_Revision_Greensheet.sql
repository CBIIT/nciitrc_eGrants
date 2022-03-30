SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[CIIP_Program_Revision_Greensheet](
	[appl_id] [int] NULL,
	[agt_id] [int] NULL,
	[FORM_STATUS] [nvarchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[submitted_date] [datetime] NULL,
	[REVISION_TYPE_CODE] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Revision_Number] [int] NULL,
	[DUMMY_ACTION_FLAG] [nvarchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[current_action_status_code] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[CANCELLED_FLAG] [nvarchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ACTION_FY] [int] NULL,
	[appl_type_code] [int] NULL,
	[suffix_code] [varchar](6) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


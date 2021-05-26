SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GreenSheet_data](
	[appl_id] [int] NULL,
	[spec_form_submitted_date] [datetime] NULL,
	[spec_form_status] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[pgm_form_submitted_date] [datetime] NULL,
	[pgm_form_status] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_type_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Action_Type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[GPMATS_CANCELLED_FLAG] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[GPMATS_CLOSED_FLAG] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[DUMMY_FLAG] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[MULTIYEAR_AWARD_FLAG] [varchar](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[REVISION_TYPE_CODE] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[CURRENT_ACTION_STATUS_DESC] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[SUFFIX_CODE] [varchar](6) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GreenSheet_data](
	[appl_id] [int] NULL,
	[spec_form_submitted_date] [datetime] NULL,
	[spec_form_status] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[pgm_form_submitted_date] [datetime] NULL,
	[pgm_form_status] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[appl_type_code] [char](1) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Action_Type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


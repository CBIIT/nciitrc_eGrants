SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_rules_actions](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order] [int] NULL,
	[description] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[action_type] [int] NOT NULL,
	[target_value] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[last_modified_by] [int] NULL,
	[last_modified_date] [smalldatetime] NULL,
	[email_template_id] [int] NULL,
	[comment] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


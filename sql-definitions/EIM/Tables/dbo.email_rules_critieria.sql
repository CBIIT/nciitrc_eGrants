SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_rules_critieria](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[order] [int] NULL,
	[email_rules_id] [int] NOT NULL,
	[citeria_type] [int] NOT NULL,
	[field_to_eval] [int] NOT NULL,
	[eval_type] [int] NOT NULL,
	[eval_value] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[last_modified_by] [int] NULL,
	[last_modified_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_rules_matched_messages](
	[email_rule_id] [int] NOT NULL,
	[email_message_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[actions_completed] [bit] NULL
) ON [PRIMARY]

GO


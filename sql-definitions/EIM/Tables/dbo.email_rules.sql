SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_rules](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[description] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[criteria_any] [bit] NOT NULL,
	[enabled] [bit] NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[last_modified_by] [int] NULL,
	[last_modified_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


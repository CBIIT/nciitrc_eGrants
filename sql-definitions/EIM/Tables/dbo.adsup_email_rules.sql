SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adsup_email_rules](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[pa] [nchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[email_to] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[email_cc] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[start_date] [smalldatetime] NOT NULL,
	[end_date] [smalldatetime] NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[last_modified_by] [int] NULL,
	[last_modified_date] [smalldatetime] NULL,
	[email_template_id] [int] NULL,
	[comment] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


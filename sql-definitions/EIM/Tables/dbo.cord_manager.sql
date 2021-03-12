SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[cord_manager](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user_fname] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[user_mi] [nvarchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[user_lname] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[user_login] [nvarchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[badge_id] [nvarchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[division] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[status] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[status_date] [smalldatetime] NOT NULL,
	[date_Requested] [smalldatetime] NOT NULL,
	[access_Type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[date_LastAccess] [smalldatetime] NULL,
	[cord_id] [int] NOT NULL,
	[start_date] [smalldatetime] NOT NULL,
	[end_date] [smalldatetime] NULL,
	[comments] [nvarchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[person_id] [int] NULL,
	[email] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[phone_number] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[disabled_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


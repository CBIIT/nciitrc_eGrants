SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adm_files](
	[file_id] [int] IDENTITY(1,1) NOT NULL,
	[folder_id] [int] NOT NULL,
	[file_name] [varchar](150) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[file_type] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[key_word] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[url] [varchar](250) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[disabled_date] [smalldatetime] NULL,
	[disabled_by_person_id] [int] NULL
) ON [PRIMARY]

GO


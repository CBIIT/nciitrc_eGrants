SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[contract_documents](
	[document_id] [int] IDENTITY(1,1) NOT NULL,
	[contract_id] [int] NULL,
	[folder_id] [int] NULL,
	[color_id] [int] NULL,
	[package_id] [int] NULL,
	[tab_id] [int] NULL,
	[latest_action_date] [smalldatetime] NULL,
	[problem_msg] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[file_type] [char](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[qc_reason] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[created_date] [smalldatetime] NULL,
	[created_by_person_id] [int] NULL,
	[file_modified_date] [smalldatetime] NULL,
	[file_modified_by_person_id] [int] NULL,
	[index_modified_by_person_id] [int] NULL,
	[index_modified_date] [smalldatetime] NULL,
	[url] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[problem_reported_by_person_id] [int] NULL,
	[disabled_date] [smalldatetime] NULL,
	[disabled_by_person_id] [int] NULL,
	[qc_date] [smalldatetime] NULL,
	[qc_person_id] [int] NULL,
	[stored_date] [smalldatetime] NULL,
	[stored_by_person_id] [int] NULL,
	[profile_id] [int] NULL,
	[txt] [text] COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[page_count] [smallint] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


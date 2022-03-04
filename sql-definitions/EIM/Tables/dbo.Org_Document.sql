SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Org_Document](
	[document_id] [int] IDENTITY(600,1) NOT NULL,
	[org_id] [int] NOT NULL,
	[doctype_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[start_date_ShowFlag] [smalldatetime] NULL,
	[end_date_showFlag] [smalldatetime] NULL,
	[url] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[file_type] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[created_by_person_id] [int] NULL,
	[disabled_date] [smalldatetime] NULL,
	[disabled_by_person_id] [int] NULL,
	[comments] [varchar](256) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[updated_date] [smalldatetime] NULL,
	[updated_by_person_id] [int] NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[funding_documents](
	[document_id] [int] IDENTITY(1,1) NOT NULL,
	[category_id] [smallint] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[created_by_person_id] [int] NULL,
	[document_date] [smalldatetime] NULL,
	[disabled_date] [smalldatetime] NULL,
	[disabled_by_person_id] [int] NULL,
	[stored_date] [smalldatetime] NULL,
	[stored_by_person_id] [int] NULL,
	[document_fy] [int] NULL,
	[sub_category] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[sub_string] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


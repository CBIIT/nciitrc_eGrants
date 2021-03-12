SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[documents_fis_10292014](
	[appl_id] [int] NOT NULL,
	[document_id] [int] NULL,
	[filetype] [varchar](3) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[category_id] [int] NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[fis_date] [smalldatetime] NULL,
	[profile_id] [int] NULL
) ON [PRIMARY]

GO


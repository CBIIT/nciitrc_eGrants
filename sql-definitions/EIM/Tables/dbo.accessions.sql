SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[accessions](
	[accession_id] [int] IDENTITY(200,1) NOT NULL,
	[accession_number] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[accession_year] [smallint] NOT NULL,
	[accession_counter] [smallint] NULL,
	[destroyed_date] [smalldatetime] NULL,
	[contract] [bit] NOT NULL,
	[profile_id] [smallint] NULL
) ON [PRIMARY]

GO


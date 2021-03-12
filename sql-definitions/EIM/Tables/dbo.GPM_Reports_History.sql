SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GPM_Reports_History](
	[Report_id] [int] NULL,
	[Date_Executed] [smalldatetime] NULL,
	[xls_path] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[pdf_path] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[xls_url] [varchar](400) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[pdf_url] [varchar](400) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


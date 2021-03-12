SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[doc_transactions](
	[transaction_id] [int] NULL,
	[transaction_date] [smalldatetime] NULL,
	[date_string] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


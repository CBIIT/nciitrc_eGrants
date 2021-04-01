SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[queries](
	[query_id] [int] IDENTITY(1,1) NOT NULL,
	[query_date] [datetime] NULL,
	[execution_time] [int] NULL,
	[search_id] [int] NULL,
	[page] [smallint] NULL,
	[searched_by] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ic] [varchar](5) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[browser_type] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


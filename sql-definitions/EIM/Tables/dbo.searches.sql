SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[searches](
	[search_id] [int] IDENTITY(1,1) NOT NULL,
	[search_string] [varchar](500) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[search_date] [datetime] NULL,
	[appl_hits] [smallint] NULL,
	[grant_hits] [smallint] NULL,
	[all_hits] [smallint] NULL,
	[exhausted_appls] [bit] NOT NULL,
	[exhausted_grants] [bit] NOT NULL,
	[exhausted_all] [bit] NOT NULL,
	[exhausted_keywords] [bit] NOT NULL,
	[grant_total] [int] NULL,
	[appl_total] [int] NULL,
	[doc_total] [int] NULL,
	[page_total] [int] NULL,
	[grant_total_full] [int] NULL,
	[appl_total_full] [int] NULL,
	[doc_total_full] [int] NULL,
	[page_total_full] [int] NULL,
	[appl_id_perfect] [int] NULL
) ON [PRIMARY]

GO


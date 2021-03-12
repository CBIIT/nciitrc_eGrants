SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[quality_control](
	[qc_reason] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[person_id] [int] NOT NULL,
	[effort] [tinyint] NULL
) ON [PRIMARY]

GO


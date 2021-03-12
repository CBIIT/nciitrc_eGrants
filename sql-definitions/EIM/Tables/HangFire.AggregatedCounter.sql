SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [HangFire].[AggregatedCounter](
	[Key] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Value] [bigint] NOT NULL,
	[ExpireAt] [datetime] NULL
) ON [PRIMARY]

GO


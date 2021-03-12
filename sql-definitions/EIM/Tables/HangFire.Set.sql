SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [HangFire].[Set](
	[Key] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Score] [float] NOT NULL,
	[Value] [nvarchar](256) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[ExpireAt] [datetime] NULL
) ON [PRIMARY]

GO


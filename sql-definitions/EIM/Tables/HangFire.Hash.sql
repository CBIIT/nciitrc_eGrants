SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [HangFire].[Hash](
	[Key] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Field] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Value] [nvarchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ExpireAt] [datetime2](7) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


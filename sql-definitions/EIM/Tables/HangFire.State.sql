SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [HangFire].[State](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[JobId] [bigint] NOT NULL,
	[Name] [nvarchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Reason] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Data] [nvarchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


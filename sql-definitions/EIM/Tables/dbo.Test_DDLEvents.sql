SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Test_DDLEvents](
	[EventDate] [datetime] NOT NULL,
	[EventType] [nvarchar](64) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[EventDDL] [nvarchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[EventXML] [xml] NULL,
	[DatabaseName] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[SchemaName] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ObjectName] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[HostName] [varchar](64) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[IPAddress] [varchar](48) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[ProgramName] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[LoginName] [nvarchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


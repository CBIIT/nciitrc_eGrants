SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[egrants_audit_report](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Report_name] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[File_name] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Run_date] [datetime] NULL,
	[url] [nvarchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Unix_path] [nvarchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


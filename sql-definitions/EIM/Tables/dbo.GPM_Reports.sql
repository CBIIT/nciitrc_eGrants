SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[GPM_Reports](
	[Rep_ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Description] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[start_date] [smalldatetime] NULL,
	[end_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


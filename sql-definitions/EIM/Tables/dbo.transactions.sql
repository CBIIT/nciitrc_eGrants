SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[transactions](
	[transaction_id] [int] IDENTITY(1,1) NOT NULL,
	[folder_id] [int] NOT NULL,
	[transaction_date] [datetime] NOT NULL,
	[operator] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[action] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[object] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL
) ON [PRIMARY]

GO


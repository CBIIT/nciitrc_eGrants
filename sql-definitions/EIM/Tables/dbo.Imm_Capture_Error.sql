SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[Imm_Capture_Error](
	[Error] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Detail] [nvarchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[timestmp] [datetime] NULL
) ON [PRIMARY]

GO


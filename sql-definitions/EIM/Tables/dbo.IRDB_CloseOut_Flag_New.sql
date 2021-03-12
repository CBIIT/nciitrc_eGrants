SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IRDB_CloseOut_Flag_New](
	[appl_id] [int] NULL,
	[fy] [int] NULL,
	[GC_OBJECT_NAME] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[GC_ACTION_TYPE_CODE] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[GC_OBJECT_STATUS_CODE] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Action_date] [datetime] NULL
) ON [PRIMARY]

GO


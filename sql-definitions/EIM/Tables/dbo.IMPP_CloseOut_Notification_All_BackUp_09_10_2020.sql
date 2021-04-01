SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IMPP_CloseOut_Notification_All_BackUp_09_10_2020](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[appl_id] [int] NOT NULL,
	[Notification_Name] [varchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Created_date] [smalldatetime] NULL,
	[event_log_id] [int] NULL,
	[Imported_Date] [datetime] NULL
) ON [PRIMARY]

GO


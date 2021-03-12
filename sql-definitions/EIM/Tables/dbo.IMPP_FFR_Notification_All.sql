SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[IMPP_FFR_Notification_All](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[appl_id] [int] NOT NULL,
	[Notification_Name] [varchar](30) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[Created_date] [smalldatetime] NOT NULL,
	[Event_Log_Id] [int] NULL,
	[Imported_Date] [datetime] NULL
) ON [PRIMARY]

GO


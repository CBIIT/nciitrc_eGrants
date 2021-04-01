SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adsup_Notification_impac_status](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Notification_id] [int] NOT NULL,
	[APPL_ID] [int] NOT NULL,
	[Impac_status] [varchar](100) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Impac_status_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


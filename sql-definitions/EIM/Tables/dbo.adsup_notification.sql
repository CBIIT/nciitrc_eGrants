SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adsup_notification](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[appl_id] [int] NOT NULL,
	[pa] [varchar](10) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[subjectLine] [varchar](200) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[NotificationBody] [varchar](2000) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[NotRcvd_dt] [smalldatetime] NULL,
	[Full_grant_num] [varchar](20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[Created_by] [int] NOT NULL,
	[Created_date] [smalldatetime] NULL,
	[Last_updated_by] [int] NULL,
	[Last_updated_date] [smalldatetime] NULL,
	[disabled_date] [smalldatetime] NULL,
	[disabled_by_personid] [int] NULL
) ON [PRIMARY]

GO


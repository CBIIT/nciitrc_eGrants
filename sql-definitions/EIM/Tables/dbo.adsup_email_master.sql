SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[adsup_email_master](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[template_name] [varchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[subject] [nvarchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[body] [nvarchar](max) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[modified_by] [int] NULL,
	[modified_date] [smalldatetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


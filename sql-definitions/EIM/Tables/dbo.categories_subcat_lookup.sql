SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[categories_subcat_lookup](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[parent_category_id] [smallint] NOT NULL,
	[sub_category_name] [varchar](35) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[start_date] [smalldatetime] NOT NULL,
	[end_date] [smalldatetime] NULL,
	[created_by_person_id] [int] NOT NULL,
	[created_date] [smalldatetime] NOT NULL,
	[Changed_by_person_id] [int] NULL,
	[changed_date] [smalldatetime] NULL
) ON [PRIMARY]

GO


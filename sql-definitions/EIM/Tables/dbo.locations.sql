SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[locations](
	[location_id] [smallint] IDENTITY(62,1) NOT NULL,
	[location] [nvarchar](50) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[profile_id] [smallint] NULL,
	[container_type_id] [tinyint] NOT NULL
) ON [PRIMARY]

GO


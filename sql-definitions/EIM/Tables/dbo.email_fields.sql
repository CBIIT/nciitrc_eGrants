SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[email_fields](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[field_name] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[data_type_id] [int] NOT NULL
) ON [PRIMARY]

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[EmailFields](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FieldName] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[DataTypeId] [int] NOT NULL
) ON [PRIMARY]

GO


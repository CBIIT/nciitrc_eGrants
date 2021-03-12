SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE TABLE [dbo].[WSNodeMapping](
	[WSNodeMapping_Id] [int] IDENTITY(1,1) NOT NULL,
	[WSEndpoint_Id] [int] NOT NULL,
	[NodeName] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[DataType] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[DestinationField] [varchar](255) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NOT NULL,
	[TransformationFunc] [varchar](1024) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS NULL,
	[TransformData] [bit] NULL,
	[IsPrimaryKey] [bit] NULL
) ON [PRIMARY]

GO


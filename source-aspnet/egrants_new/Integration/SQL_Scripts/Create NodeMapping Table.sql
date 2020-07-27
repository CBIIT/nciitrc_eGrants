USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE dbo.WSNodeMapping(
	WSNodeMapping_Id INT IDENTITY (1,1),
	WSEndpoint_Id int NOT NULL, 
	NodeName [varchar](255) NOT NULL,
	DataType [varchar](255) NULL,
	DestinationTable [varchar](255) NULL,
	DestinationField [varchar](255) NOT NULL,
	TransformationFunc [varchar](1024) NULL,
	TransformData bit null,
	IsPrimaryKey bit null,
	CONSTRAINT [FK_WSNodeMapping_WSEndpoint_Id] FOREIGN KEY ([WSEndpoint_Id]) REFERENCES dbo.WSEndPoint ([WSEndpoint_Id])
) ON [PRIMARY]
GO

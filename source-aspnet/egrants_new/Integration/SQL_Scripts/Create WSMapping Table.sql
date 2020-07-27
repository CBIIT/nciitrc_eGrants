USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE dbo.WSMapping(
	[WSMapping_Id] INT IDENTITY (1,1),
	[WSEndpoint_Id] int not null,
	[Database] int NOT NULL, 
	[Schema] varchar(255) NOT NULL,
	[DestinationTable] varchar(255) NOT NULL,
	[ReconciliationBehavior] int NOT NULL,
	CONSTRAINT [PK_WSMappingId] PRIMARY KEY CLUSTERED (WSMapping_Id ASC),
	CONSTRAINT [FK_WSMapping_WSEndpoint_Id] FOREIGN KEY ([WSEndpoint_Id]) REFERENCES [dbo].[WSEndpoint] ([WSEndpoint_Id])
) ON [PRIMARY]
GO
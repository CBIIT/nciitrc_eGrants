USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE dbo.WSHistory(
	WSHistory_Id INT IDENTITY (1,1),
	WSEndpoint_Id int NOT NULL, 
	Result [varchar](MAX) NOT NULL,
	ResultStatusCode INT NOT NULL,
	DateTriggered DateTimeOffset NOT NULL,
	DateCompleted DateTimeOffset NOT NULL,
	CONSTRAINT [PK_WSHistoryId] PRIMARY KEY CLUSTERED (WSHistory_Id ASC),
	CONSTRAINT [FK_WSEndPoint_WSHistory_Id] FOREIGN KEY ([WSEndpoint_Id]) REFERENCES dbo.WSEndPoint ([WSEndpoint_Id])
) ON [PRIMARY]
GO
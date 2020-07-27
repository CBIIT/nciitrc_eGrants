USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE dbo.WSEndpoint(
[WSEndpoint_Id] int identity(1,1),
[Name] varchar(255) not null,
[Description] varchar(1024) not null,
[EndpointUri] varchar(2048) not null, 
[Action] varchar (1024) not null, 
[AcceptsHeader] varchar(60) not null,
[AuthenticationType] int not null,
[SourceOrganization] varchar(255),
[NextRun] DateTimeOffset null,
[LastRun] DateTimeOffset null,
[DestinationDatabase] varchar(255) null,
[DestinationTable] varchar(255) null,
[Interval] int not null,
[Enabled] bit not null,
[TriggerAuth] varchar(60) null,
[RetryOnFail] bit null,
[RetryInterval] int null,
[RetryFreq] int null,
[Frequency] int not null
	CONSTRAINT [PK_WSEndpointId] PRIMARY KEY CLUSTERED (WSEndpoint_Id ASC)
) ON [PRIMARY]
GO

/*
Alter Table WSEndpoint Add [TriggerAuth] varchar(60) null
Alter Table WSEndpoint Add [RetryOnFail] bit null
Alter Table WSEndpoint Add [RetryInterval] int null
Alter Table WSEndpoint Add [RetryFreq] int null
Alter Table WSEndpoint Add [Frequency] int not null
*/
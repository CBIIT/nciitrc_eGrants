USE [EIM]
GO
INSERT [dbo].[WSEndpoint] ([Name]
, [Description]
, [EndpointUri]
, [Action]
, [AcceptsHeader]
, [AuthenticationType]
, [SourceOrganization]
, [NextRun]
, [LastRun]
, [DestinationDatabase]
, [DestinationTable]
, [Interval]
, [Enabled]
, [TriggerAuth]
, [RetryOnFail]
, [RetryInterval]
, [RetryFreq]
, [Frequency]
, [WebRequestMethod]
, [KeepAlive]
, [Timeout]
, [Schema]
, [QueryString]
, [AllowRedirect]
, [Database]
, [ReconciliationBehavior]
, [CertificatePath]
, [CertificatePwd]
, [IntervalTimeSpan]) 
VALUES 
(N'Microsoft Graph - BennyShell329@msn.com'
, N'Testing Email from My Account'
, N'https://graph.microsoft.com/v1.0/me/'
, N'messages'
, N'application/json'
, 2
, N'eRa'
, CAST(N'2021-10-05T16:12:37.0795687-04:00' AS DateTimeOffset)
, CAST(N'2021-10-04T20:15:28.1735878-04:00' AS DateTimeOffset)
, N' EIM'
, N'EmailMessages'
, 10
, 1
, NULL
, 1
, 3
, 3
, 3
, N'GET'
, NULL
, NULL
, N'DBO'
, N'$top=100&$count=true&$filter=receivedDateTime+gt+##LastRun'
, 0
, N'EIM'
, 0
, 'BennyShellMSN'
, null
, N'0,0,1,0,0')
GO

--Delete from WSEndpoint where Enabled = 1

INSERT [dbo].[WSNodeMapping] ([WSEndpoint_Id], [NodeName], [DataType], [DestinationField], [TransformationFunc], [TransformData], [IsPrimaryKey]) 
VALUES 
(3, N'id', N'int', N'GraphId', NULL, 0, 0),
(3, N'createdDateTime', N'int', N'createdDateTime', NULL, 0, 0),
(3, N'lastModifiedDateTime', N'int', N'lastModifiedDateTime', NULL, 0, 0),
(3, N'receivedDateTime', N'int', N'receivedDateTime', NULL, 0, 0),
(3, N'sentDateTime', N'int', N'sentDateTime', NULL, 0, 0),
(3, N'hasAttachments', N'int', N'hasAttachments', NULL, 0, 0),
(3, N'subject', N'int', N'subject', NULL, 0, 0),
(3, N'bodyPreview', N'int', N'bodyPreview', NULL, 0, 0),
(3, N'importance', N'int', N'importance', NULL, 0, 0),
(3, N'isRead', N'int', N'parentFolderId', NULL, 0, 0),
(3, N'body', N'int', N'body', NULL, 0, 0),
(3, N'sender\emailAddress\address', N'int', N'sender', NULL, 0, 0),
(3, N'from\emailAddress\address', N'int', N'from', NULL, 0, 0),
(3, N'toRecipients\emailAddress\address', N'int', N'toRecipients', NULL, 0, 0),
(3, N'ccRecipients', N'int', N'ccRecipients', NULL, 0, 0)
GO


Select * 
Delete from WSNodeMapping where WSEndpoint_Id = 3
USE [EIM]
GO
SET IDENTITY_INSERT [dbo].[WSEndpoint] ON
GO
INSERT [dbo].[WSEndpoint] ([WSEndpoint_Id], [Name], [Description], [EndpointUri], [Action], [AcceptsHeader], [AuthenticationType], [SourceOrganization], [NextRun], [LastRun], [DestinationDatabase], [DestinationTable], [Interval], [Enabled], [TriggerAuth], [RetryOnFail], [RetryInterval], [RetryFreq], [Frequency], [WebRequestMethod], [KeepAlive], [Timeout], [Schema], [QueryString], [AllowRedirect], [Database], [ReconciliationBehavior], [CertificatePath], [CertificatePwd], [IntervalTimeSpan]) 
VALUES 
(1, N'eRa Webservices', N'WebService providing the integration with eRa webservice', N'https://s2s.stage.era.nih.gov/icdata/dataservices/ice', N'notification-data', N'application/json', 1, N'eRa', CAST(N'2020-08-05T16:12:37.0795687-04:00' AS DateTimeOffset), CAST(N'2020-08-04T20:15:28.1735878-04:00' AS DateTimeOffset), N' EIM', N'TMP_IMPP_FFR_Notification_All', 10, 1, NULL, 1, 3, 3, 3, N'GET', NULL, NULL, N'DBO', N'notificationTypes=FFR_REJECTION&lastRunDt=##LastRun', 0, N'EIM', 0, N'WSClientCertPath', N'WSClientCertPwd', N'0,0,1,0,0'),
(2, N'eRa Webservices', N'WebService providing the integration with eRa webservice Closeout Notifications All', N'https://s2s.stage.era.nih.gov/icdata/dataservices/ice', N'notification-data', N'application/json', 1, N'eRa', CAST(N'2020-05-05T16:12:37.0795687-04:00' AS DateTimeOffset), CAST(N'2020-05-04T20:15:28.1735878-04:00' AS DateTimeOffset), N' EIM', N'TMP_IMPP_CloseOut_Notification_All', 10, 1, NULL, 1, 3, 3, 3, N'GET', NULL, NULL, N'DBO', N'notificationTypes=FRAM_SUBMITTED_INTERNAL,GCM_CLOSEOUT_COMPLETE_LETTER,FPR_SUBMITTED_INTERNAL,FPR_SUBMITTED_EXTERNAL,GCM_GCC_LTR1,GCM_GCC_LTR2,GCM_GCC_LTR3&lastRunDt=##LastRun', 0, N'EIM', 0, N'WSClientCertPath', N'WSClientCertPwd', N'0,0,1,0,0')
GO
SET IDENTITY_INSERT [dbo].[WSEndpoint] OFF
GO
SET IDENTITY_INSERT [dbo].[WSNodeMapping] ON 
GO
INSERT [dbo].[WSNodeMapping] ([WSNodeMapping_Id], [WSEndpoint_Id], [NodeName], [DataType], [DestinationField], [TransformationFunc], [TransformData], [IsPrimaryKey]) 
VALUES 
(1, 1, N'eventKeyId', N'int', N'appl_id', NULL, 0, 0),
(2, 1, N'notificationName', N'int', N'Notification_Name', NULL, 0, 0),
(3, 1, N'createdDate', N'int', N'Created_date', NULL, 0, 0),
(4, 2, N'eventKeyId', N'int', N'appl_id', NULL, 0, 0),
(5, 2, N'notificationName', N'int', N'Notification_Name', NULL, 0, 0),
(6, 2, N'createdDate', N'int', N'Created_date', NULL, 0, 0)
GO
SET IDENTITY_INSERT [dbo].[WSNodeMapping] OFF
GO

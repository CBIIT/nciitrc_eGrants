
GO
/****** Object:  Table [dbo].[WSEndpoint]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WSEndpoint](
	[WSEndpoint_Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[Description] [varchar](1024) NOT NULL,
	[EndpointUri] [varchar](2048) NOT NULL,
	[Action] [varchar](1024) NOT NULL,
	[AcceptsHeader] [varchar](60) NOT NULL,
	[AuthenticationType] [int] NOT NULL,
	[SourceOrganization] [varchar](255) NULL,
	[NextRun] [datetimeoffset](7) NULL,
	[LastRun] [datetimeoffset](7) NULL,
	[DestinationDatabase] [varchar](255) NULL,
	[DestinationTable] [varchar](255) NULL,
	[Interval] [int] NOT NULL,
	[Enabled] [bit] NOT NULL,
	[TriggerAuth] [varchar](60) NULL,
	[RetryOnFail] [bit] NULL,
	[RetryInterval] [int] NULL,
	[RetryFreq] [int] NULL,
	[Frequency] [int] NOT NULL,
	[WebRequestMethod] [varchar](15) NOT NULL,
	[KeepAlive] [bit] NULL,
	[Timeout] [int] NULL,
	[Schema] [varchar](60) NOT NULL,
	[QueryString] [varchar](512) NULL,
	[AllowRedirect] [bit] NULL,
	[Database] [varchar](255) NOT NULL,
	[ReconciliationBehavior] [int] NULL,
	[CertificatePath] [varchar](512) NULL,
	[CertificatePwd] [varchar](128) NULL,
	[IntervalTimeSpan] [varchar](128) NULL,
 CONSTRAINT [PK_WSEndpointId] PRIMARY KEY CLUSTERED 
(
	[WSEndpoint_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WSHistory]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WSHistory](
	[WSHistory_Id] [int] IDENTITY(1,1) NOT NULL,
	[WSEndpoint_Id] [int] NOT NULL,
	[Result] [varchar](max) NOT NULL,
	[ResultStatusCode] [int] NOT NULL,
	[DateTriggered] [datetimeoffset](7) NOT NULL,
	[DateCompleted] [datetimeoffset](7) NOT NULL,
	[WebServiceName] [varchar](255) NOT NULL,
	[EndpointUriSent] [varchar](1024) NULL,
	[ExceptionMessage] [varchar](4000) NULL,
	[NotificationSent] [bit] NULL,
 CONSTRAINT [PK_WSHistoryId] PRIMARY KEY CLUSTERED 
(
	[WSHistory_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WSNodeMapping]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WSNodeMapping](
	[WSNodeMapping_Id] [int] IDENTITY(1,1) NOT NULL,
	[WSEndpoint_Id] [int] NOT NULL,
	[NodeName] [varchar](255) NOT NULL,
	[DataType] [varchar](255) NULL,
	[DestinationField] [varchar](255) NOT NULL,
	[TransformationFunc] [varchar](1024) NULL,
	[TransformData] [bit] NULL,
	[IsPrimaryKey] [bit] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[WSHistory]  WITH CHECK ADD  CONSTRAINT [FK_WSEndPoint_WSHistory_Id] FOREIGN KEY([WSEndpoint_Id])
REFERENCES [dbo].[WSEndpoint] ([WSEndpoint_Id])
GO
ALTER TABLE [dbo].[WSHistory] CHECK CONSTRAINT [FK_WSEndPoint_WSHistory_Id]
GO
ALTER TABLE [dbo].[WSNodeMapping]  WITH CHECK ADD  CONSTRAINT [FK_WSNodeMapping_WSEndpoint_Id] FOREIGN KEY([WSEndpoint_Id])
REFERENCES [dbo].[WSEndpoint] ([WSEndpoint_Id])
GO
ALTER TABLE [dbo].[WSNodeMapping] CHECK CONSTRAINT [FK_WSNodeMapping_WSEndpoint_Id]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_call_url]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		CAll a URL from SQL Server 
-- =============================================
CREATE   PROCEDURE [dbo].[sp_web_service_call_url]
	@url as nvarchar(500)
AS
BEGIN
SET NOCOUNT ON;
	
DECLARE @status int
DECLARE @responseText as table(responseText nvarchar(max))
DECLARE @res as Int;

EXEC sp_OACreate 'MSXML2.ServerXMLHTTP', @res OUT
EXEC sp_OAMethod @res, 'open', NULL, 'GET',@url,'false'
EXEC sp_OAMethod @res, 'send'
EXEC sp_OAGetProperty @res, 'status', @status OUT
INSERT INTO @ResponseText (ResponseText) EXEC sp_OAGetProperty @res, 'responseText'
EXEC sp_OADestroy @res
SELECT @status, responseText FROM @responseText
END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_endpoint]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService details
-- =============================================
CREATE         PROCEDURE [dbo].[sp_web_service_get_endpoint]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSEndpoint_Id]
      ,[Name]
      ,[Description]
      ,[EndpointUri]
      ,[Action]
      ,[AcceptsHeader]
      ,[AuthenticationType]
      ,[SourceOrganization]
      ,[NextRun]
      ,[LastRun]
      ,[DestinationDatabase]
      ,[DestinationTable]
      ,[Interval]
      ,[Enabled]
      ,[TriggerAuth]
      ,[RetryOnFail]
      ,[RetryInterval]
      ,[RetryFreq]
      ,[Frequency]
      ,[WebRequestMethod]
      ,[KeepAlive]
      ,[Timeout]
      ,[Schema]
      ,[QueryString]
      ,[AllowRedirect]
      ,[Database]
      ,[ReconciliationBehavior]
      ,[CertificatePath]
      ,[CertificatePwd]
	  ,IntervalTimeSpan
  FROM [dbo].[WSEndpoint]
  WHERE [WSEndpoint_Id] = @webserviceId;

END


GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_endpoint_due_to_fire]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:			Benny Shell
-- Create date:		06/20/2020
-- Description:		Retrieve the WebService Ids that are due to fire
-- =============================================
CREATE     PROCEDURE [dbo].[sp_web_service_get_endpoint_due_to_fire]

AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSEndpoint_Id]
  FROM [dbo].[WSEndpoint]
  WHERE [NextRun] < GETDATE() and [Enabled] = 1

END


GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_history]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService details
-- =============================================
CREATE     PROCEDURE [dbo].[sp_web_service_get_history]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSHistory_Id]
      ,[WSEndpoint_Id]
      ,[Result]
      ,[ResultStatusCode]
      ,[DateTriggered]
      ,[DateCompleted]
      ,[WebServiceName]
      ,[EndpointUriSent]
      ,[ExceptionMessage]
      ,[NotificationSent]
  FROM [dbo].[WSHistory]
  WHERE [WSEndpoint_Id] = @webserviceId;

END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_history_exceptions]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService details
-- =============================================
CREATE       PROCEDURE [dbo].[sp_web_service_get_history_exceptions]
AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSHistory_Id]
      ,[WSEndpoint_Id]
      ,[Result]
      ,[ResultStatusCode]
      ,[DateTriggered]
      ,[DateCompleted]
      ,[WebServiceName]
      ,[EndpointUriSent]
      ,[ExceptionMessage]
      ,[NotificationSent]
  FROM [dbo].[WSHistory]
  WHERE not ExceptionMessage is null and NotificationSent is null

END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_mapping]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService Mapping
-- =============================================
CREATE   PROCEDURE [dbo].[sp_web_service_get_mapping]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSMapping_Id]
      ,[WSEndpoint_Id]
      ,[Database]
      ,[Schema]
      ,[DestinationTable]
      ,[ReconciliationBehavior]
  FROM [dbo].[WSMapping]
  WHERE [WSEndpoint_Id] = @webserviceId;

END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_get_node_mapping]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService Mapping
-- =============================================
CREATE     PROCEDURE [dbo].[sp_web_service_get_node_mapping]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
SELECT wsnm.[WSNodeMapping_Id]
      ,wsnm.[WSEndpoint_Id]
      ,wsnm.[NodeName]
      ,wsnm.[DataType]
      ,wsnm.[DestinationField]
      ,wsnm.[TransformationFunc]
      ,wsnm.[TransformData]
      ,wsnm.[IsPrimaryKey]
  FROM [dbo].[WSNodeMapping] wsnm 
  WHERE wsnm.[WSEndpoint_Id] = @webserviceId;

END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_save_history]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Save WebService History Record 
-- =============================================
CREATE     PROCEDURE [dbo].[sp_web_service_save_history]
          @WSEndpoint_Id int
           ,@Result varchar(max)
           ,@ResultStatusCode int
           ,@DateTriggered datetimeoffset(7)
           ,@DateCompleted datetimeoffset(7)=null
           ,@WebServiceName varchar(255)
           ,@EndpointUriSent varchar(1024)
           ,@ExceptionMessage varchar(4000)=null
AS
BEGIN
SET NOCOUNT ON;
	
INSERT INTO [dbo].[WSHistory]
           ([WSEndpoint_Id]
           ,[Result]
           ,[ResultStatusCode]
           ,[DateTriggered]
           ,[DateCompleted]
           ,[WebServiceName]
           ,[EndpointUriSent]
           ,[ExceptionMessage])
     VALUES
           (@WSEndpoint_Id
           ,@Result
           ,@ResultStatusCode 
           ,@DateTriggered
           ,@DateCompleted
           ,@WebServiceName
           ,@EndpointUriSent
           ,@ExceptionMessage)
END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_save_schedule_updates]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:			Benny Shell
-- Create date:		06/21/2020
-- Description:		Save WebService History Record 
-- =============================================
CREATE     PROCEDURE [dbo].[sp_web_service_save_schedule_updates]
            @WSEndpoint_Id int
           ,@NextRun datetimeoffset(7)=null
           ,@LastRun datetimeoffset(7)=null
AS
BEGIN
SET NOCOUNT ON;
	
UPDATE [dbo].[WSEndpoint]
SET NextRun = @NextRun,
	LastRun = @LastRun
	Where [WSEndpoint_Id] = @WSEndpoint_Id

END

GO
/****** Object:  StoredProcedure [dbo].[sp_web_service_update_history]    Script Date: 8/6/2020 4:47:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Save WebService History Record 
-- =============================================

CREATE     PROCEDURE [dbo].[sp_web_service_update_history]
          @WSHistory_Id int
AS
BEGIN
SET NOCOUNT ON;
	
Update [WSHistory]
   SET NotificationSent = 1
 WHERE WSHistory_Id = @WSHistory_Id

END

GO

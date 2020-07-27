USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService details
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_get_endpoint]
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
  FROM [dbo].[WSEndPoint]
  WHERE [WSEndpoint_Id] = @webserviceId;

END


GO



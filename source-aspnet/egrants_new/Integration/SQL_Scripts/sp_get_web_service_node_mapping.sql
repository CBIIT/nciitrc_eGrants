USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Retrieve the WebService Mapping
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_get_node_mapping]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
SELECT [WSNodeMapping_Id]
      ,[WSEndpoint_Id]
      ,[NodeName]
      ,[DataType]
      ,[DestinationTable]
      ,[DestinationField]
      ,[TransformationFunc]
      ,[TransformData]
      ,[IsPrimaryKey]
  FROM [dbo].[WSNodeMapping]
  WHERE [WSEndpoint_Id] = @webserviceId;

END

GO



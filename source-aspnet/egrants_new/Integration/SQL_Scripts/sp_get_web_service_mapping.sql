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
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_get_mapping]
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



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
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_get_history]
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
  FROM [dbo].[WSHistory]
  WHERE [WSEndpoint_Id] = @webserviceId;

END

GO



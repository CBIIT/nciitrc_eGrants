SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



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


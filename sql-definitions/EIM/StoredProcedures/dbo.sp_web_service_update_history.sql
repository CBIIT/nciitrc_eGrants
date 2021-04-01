SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

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


USE [EIM]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:			Benny Shell
-- Create date:		06/07/2020
-- Description:		Check and then call a URL from SQL Server
--					This will trigger the service, but updates to the last run and next 
--					run will come from the C# web application, in case service is down or there was a problem and 
--					service needs to try again
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_trigger_web_services]

AS
BEGIN
SET NOCOUNT ON;
	
DECLARE @trigger_time = GETDATE();
DECLARE @wsendpointId int;
DECLARE @triggerauth

Declare webservices cursor
FOR

Select WSEndpoint_id, TriggeraAuth 
  from WSEndpoint 
 Where  [Enabled] = 1 
	AND NextRun < @trigger_time 

Open webservices;

Fetch next from dbs into @WSEndpointId;

while @@FETCH_STATUS = 0
Begin

DECLARE @url = 'http://localhost:52030/Integration/Trigger?webServiceId='+ @wsendpointId +'&triggerAuth=' +@triggerauth; 

exec sp_web_service_call_url @SupplierId = @url;

FETCH NEXT FROM webservices into @wsendpointid, @triggerauth

End

Close webservices
DEALLOCATE webservices




GO



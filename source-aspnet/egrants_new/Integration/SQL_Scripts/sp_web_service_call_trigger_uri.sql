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
CREATE OR ALTER PROCEDURE [dbo].[sp_web_service_trigger_WSEndpoint]
	@webserviceId int
AS
BEGIN
SET NOCOUNT ON;
	
DECLARE @status int
DECLARE @responseText as table(responseText nvarchar(max))
DECLARE @res as Int;
DECLARE @url as nvarchar(1000) = 'https://www.google.it'
EXEC sp_OACreate 'MSXML2.ServerXMLHTTP', @res OUT
EXEC sp_OAMethod @res, 'open', NULL, 'GET',@url,'false'
EXEC sp_OAMethod @res, 'send'
EXEC sp_OAGetProperty @res, 'status', @status OUT
INSERT INTO @ResponseText (ResponseText) EXEC sp_OAGetProperty @res, 'responseText'
EXEC sp_OADestroy @res
SELECT @status, responseText FROM @responseText
END

GO



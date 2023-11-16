USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_appl_get_year_label]    Script Date: 9/26/2023 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


/*******************************************************
**9/26/2023: Auther : Micah Hoover
**Description : This function returns the grant year name / label
**Usage:
*******************************************************/
CREATE OR ALTER FUNCTION [dbo].[fn_appl_get_year_label]  (@appl_id int)  
RETURNS NVARCHAR(10)

AS  
BEGIN 

DECLARE @grant_year_label	NVARCHAR(10)

SELECT @grant_year_label=a.[label] FROM dbo.appls a WHERE appl_id = @appl_id 

RETURN @grant_year_label

END




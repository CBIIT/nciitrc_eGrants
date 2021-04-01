SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_unix_file] (@url varchar(500))

RETURNS varchar(100)

AS  
BEGIN

--Imran : 2/9/2015
----return replace (@url,'https://egrants-data.nci.nih.gov', '/egrants')
----return replace (@url,'/data', '/egrants')				comment out by leon 4/22/2016
return replace (@url,'data', '/egrants')

END
GO


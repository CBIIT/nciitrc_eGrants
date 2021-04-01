SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_unix_directory] (@s varchar(500))

RETURNS varchar(255)

AS  
BEGIN


RETURN
(

SELECT 

CASE
--Imran : 2/9/2015
----WHEN @s like 'https://egrants-data.nci.nih.gov/%' THEN 

----'/egrants' + 
----substring(@s,33, 
----len(@s) - 31 - patindex('%/%',reverse(@s))
----)
----WHEN @s like '/data/%' THEN '/egrants' + substring(@s,6, len(@s) - 4 - patindex('%/%',reverse(@s)))			 comment out by leon 4/22/2016
WHEN @s like 'data/%' THEN '/egrants/' + substring(@s,6, len(@s) - 4 - patindex('%/%',reverse(@s)))
ELSE NULL

END


)



END


GO


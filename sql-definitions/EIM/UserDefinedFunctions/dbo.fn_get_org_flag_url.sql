SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE FUNCTION [dbo].[fn_get_org_flag_url] (@org_name varchar(200))

RETURNS varchar(100) AS 

BEGIN 

RETURN 
(
select top 1 url from vw_org_document v
where org_name =@org_name and convert(date,end_date) >= convert(date,GETDATE()) AND v.category_name = 'Site Visit'
ORDER BY end_date DESC 	
)

END



GO


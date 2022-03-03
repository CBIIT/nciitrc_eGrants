SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE   FUNCTION [dbo].[fn_get_org_flag_url] (@org_name varchar(200))
  
RETURNS varchar(100) AS 

BEGIN 

RETURN 
(
select url from vw_org_document
where org_name =@org_name and end_date=(
	select MAX(end_date)
	from vw_Org_Document 
	where org_name =@org_name and category_name = 'SITE VISIT'
	group by category_id
	) 
)

END


GO


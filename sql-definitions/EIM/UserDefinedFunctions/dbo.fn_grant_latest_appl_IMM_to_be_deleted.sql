SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
create FUNCTION [dbo].[fn_grant_latest_appl_IMM_to_be_deleted] (@GrantID int)
  
RETURNS int AS  


BEGIN 


RETURN (SELECT TOP 1 appl_id from appls where grant_id=@GrantID 
AND APPL_ID NOT IN (SELECT DISTINCT appl_id FROM appls_deleted_in_impac)
order by fy DESC,support_year DESC,suffix_code DESC

)


END







GO


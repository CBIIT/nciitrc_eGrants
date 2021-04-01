SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE FUNCTION [dbo].[fn_grant_org_name] (@GrantID int)
  
RETURNS varchar(100) AS  


BEGIN 


RETURN 
(

SELECT org_name from vw_appls where appl_id=(select max(appl_id) from appls where grant_id=@GrantID and appl_id not in (select appl_id from appls_deleted_in_impac)) 

)

END






GO


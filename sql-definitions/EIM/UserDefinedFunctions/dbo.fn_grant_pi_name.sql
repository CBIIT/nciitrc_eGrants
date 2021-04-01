SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION fn_grant_pi_name (@GrantID int)
  
RETURNS varchar(100) AS  


BEGIN 


RETURN 
(
SELECT pi_name from vw_appls where appl_id=
(select max(appl_id) from appls where grant_id=@GrantID)
)


END



GO


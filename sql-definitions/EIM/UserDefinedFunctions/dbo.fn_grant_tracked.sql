SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION fn_grant_tracked (@GrantID int)
  
RETURNS varchar(3) AS  

BEGIN 

IF (SELECT COUNT(*) FROM folders where grant_id=@GrantID)>0
RETURN 'yes'


RETURN 'no'



END


GO


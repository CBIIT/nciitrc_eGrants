SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_package_used] (@GrantID int,@Package varchar(20))  
RETURNS varchar(3) AS  
BEGIN 

/**
If (select count(*) from egrants d,appls a,categories c
   where a.appl_id=d.appl_id and c.category_id=d.category_id and a.grant_id=@GrantID AND c.package=@package)>0
return 'yes'


return 'no'
**/

IF @Package='Closeout' 
BEGIN
IF (select count(*) from egrants d,appls a,categories c where a.appl_id=d.appl_id and c.category_id=d.category_id and a.grant_id=@GrantID AND c.package=@package)>0 RETURN 'yes'

IF (select count(appl_id) from IMPP_CloseOut_Notification_All where appl_id in(select appl_id from vw_appls where grant_id =@GrantID))>0 RETURN 'yes'
END

---IF @Package='All'  and (select count(*) from egrants where grant_id=@GrantID)>0 RETURN 'yes'

ELSE 

IF (select count(*) from egrants d,appls a,categories c where a.appl_id=d.appl_id and c.category_id=d.category_id and a.grant_id=@GrantID AND c.package=@package)>0 RETURN 'yes'

RETURN 'no'

END

GO


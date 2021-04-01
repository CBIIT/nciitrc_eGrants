SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE FUNCTION [dbo].[fn_tobacco_flag_to_be_deleted] (@GrantID int)--@applID
  
RETURNS bit AS  

BEGIN

--DECLARE @GrantID INT

--SELECT @GrantID = grant_id FROM appls WHERE appl_id = @applID


--IF (select count(*) from grants_flag_construct where grant_id=@GrantID and flag_type='TBC' )>0

IF (SELECT COUNT(*) FROM grants_flag_construct WHERE grant_id=@GrantID AND flag_type='TBC' AND end_dt is null)>0  --AND appl_id = @applID
RETURN 1

RETURN 0

END


GO


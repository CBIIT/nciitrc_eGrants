SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE FUNCTION [dbo].[fn_show_closeout_flag] (@applID int)
  
RETURNS varchar(2) AS  

BEGIN

Declare @FRPPR int
Declare @IRPPR int
Declare @F	varchar(1)
Declare @I	varchar(1)

Set @FRPPR=(select category_id from categories where category_name='FRPPR')
Set @IRPPR=(select category_id from categories where category_name='IRPPR')

IF (select count(*) from egrants where appl_id = @applID and category_id=@FRPPR)>0 SET @F='f' ELSE SET @F=''
IF (select count(*) from egrants where appl_id = @applID and category_id=@IRPPR)>0 SET @I='i' ELSE SET @I=''

RETURN @F+@I

END



--comment out by Leon 12/5/2017
--RETURNS char(1) AS  

--BEGIN

--Declare @FRPPR int
--Declare @IRPPR int

--Set @FRPPR=(select category_id from categories where category_name='FRPPR')
--Set @IRPPR=(select category_id from categories where category_name='IRPPR')

--IF (select count(*) from egrants where appl_id = @applID and category_id in (@FRPPR, @IRPPR))>0

--RETURN 'y'

--RETURN 'n'

--END
--GO


GO


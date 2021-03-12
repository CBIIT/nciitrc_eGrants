SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE FUNCTION [dbo].[fn_get_qc_days] (@person_id int)
  
RETURNS int

BEGIN

DECLARE @qc_date	smalldatetime
DECLARE @daydiff	int

SET @qc_date=(SELECT MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null and stored_date is null and disabled_date is null)

IF CONVERT(varchar, @qc_date,101)=CONVERT(varchar, GETDATE(),101) SET @daydiff=1 
ELSE IF @qc_date IS NOT NULL SELECT @daydiff=DATEDIFF(day, @qc_date,getdate())-1  ELSE SET @daydiff=0 

--IF @qc_date IS NOT NULL SELECT @daydiff=DATEDIFF(day, @qc_date,getdate())-1 ELSE SET @daydiff=0 

RETURN @daydiff

END
GO


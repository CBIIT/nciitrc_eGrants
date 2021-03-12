SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF



-- =============================================
-- Author:		Imran Omair
-- Create date: 5/10/2013
-- Description:	get RPPR info when it is finalized at ImpacII
-- =============================================
CREATE PROCEDURE [dbo].[GetNewRPPR]
AS
BEGIN

	INSERT INTO RPPR(APPL_ID,LAST_UPD_DATE)
	select appl_id,last_upd_date from
	OPENQUERY(IRDB,'select appl_id,last_upd_date from RPPRS_T WHERE RPPR_STATUS_CODE = ''FIN''')
	WHERE APPL_ID NOT IN (SELECT APPL_ID FROM RPPR)END

	UPDATE RPPR SET FULL_GRANT_NUM=B.FULL_GRANT_NUM
	FROM RPPR A, VW_APPLS B WHERE A.APPL_ID=B.APPL_ID

	--SELECT APPL_ID,COUNT(*) FROM VW_APPLS GROUP BY APPL_ID
	----HAVING COUNT(*)>1 
	--SELECT COUNT(*) FROM RPPR

GO


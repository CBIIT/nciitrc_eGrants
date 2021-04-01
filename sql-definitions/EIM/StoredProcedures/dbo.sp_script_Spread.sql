SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_script_Spread]
AS

BEGIN

DECLARE @cmd varchar(1000)
DECLARE @sql varchar(1000)
DECLARE @lastRun smalldatetime

SET @cmd='bcp "select command from eim..vw_script_spread" queryout d:\util\scripts\spread.sh -c -T'

exec master..xp_cmdshell @cmd

END



RETURN


GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[get_admin_supp_latest_action]
(
	@serialnum int,@suppapplid int
)
RETURNS varchar(20)
AS
BEGIN
DECLARE @latestAction VARCHAR(20)
DECLARE @ADSUPPACTIONCODE VARCHAR(3)

	SET @ADSUPPACTIONCODE=(SELECT  TOP 1 T.ADMIN_SUPP_ACTION_CODE FROM dbo.IMPP_Admin_Supplements T WHERE T.serial_num=@serialnum
	AND T.Supp_appl_id=@suppapplid ORDER BY T.Action_date DESC)
	
	IF @ADSUPPACTIONCODE='NFD'
		SET @latestAction='Not Funded'
	ELSE IF @ADSUPPACTIONCODE='ACC'
		SET @latestAction='Accepted'
	ELSE IF @ADSUPPACTIONCODE='REJ'
		SET @latestAction='Refused'
	ELSE IF @ADSUPPACTIONCODE='PIW' 
		SET @latestAction='PD/PI WIP'
	ELSE IF @ADSUPPACTIONCODE='SOW' 
		SET @latestAction='SO WIP'	
	ELSE IF @ADSUPPACTIONCODE='STA'
		SET @latestAction='Submitted'
	ELSE IF @ADSUPPACTIONCODE='ACT'
		SET @latestAction='Activate'
	ELSE IF @ADSUPPACTIONCODE='PPA'
		SET @latestAction='Paid in parent'
	ELSE IF @ADSUPPACTIONCODE='EII'
		SET @latestAction='Edit ID Info'
	ELSE IF @ADSUPPACTIONCODE='ANT'
		SET @latestAction='Add Notes'
	ELSE
		SET @latestAction='N/A'
		
	RETURN @latestAction
END

GO


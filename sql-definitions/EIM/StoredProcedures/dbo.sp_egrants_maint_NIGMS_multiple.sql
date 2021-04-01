SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_maint_NIGMS_multiple]

AS 

/**********************************************************************************************************************/
/***									 			***/
/***	Procedure Name:  sp_egrants_maint_NIGMS_multiple					***/
/***	Description:	to fix some grants with multiple serial number for NIGMS			***/
/***	Created:	06/25/2007	Leon							***/
/***	Modified:										***/
/***												***/
/*********************************************************************************************************************/
--clean up nigms_dups
DELETE  appls_nigms_dups

--save appls with duplicate admin_phs_org_code and serial_num for NIGMS
INSERT appls_nigms_dups (appl_id,admin_phs_org_code,serial_num,appl_type_code,activity_code,support_year,suffix_code )
SELECT a.appl_id,a.admin_phs_org_code,a.serial_num,a.appl_type_code,a.activity_code,a.support_year,a.suffix_code
FROM appls_ciip a, (SELECT admin_phs_org_code, serial_num FROM grants GROUP BY admin_phs_org_code, serial_num HAVING COUNT(*)>1) dupl
WHERE a.admin_phs_org_code=dupl.admin_phs_org_code and a.serial_num=dupl.serial_num

IF (select count(*) from appls_nigms_dups) >=1
BEGIN

--delete all appls not belong to  'GM'
DELETE appls_ciip
FROM appls_ciip INNER JOIN appls_nigms_dups ON appls_nigms_dups.appl_id=appls_ciip.appl_id 
WHERE appls_nigms_dups.admin_phs_org_code<>'GM'

DELETE FROM appls_nigms_dups WHERE admin_phs_org_code<>'GM'

--set grant_id for existing appls
UPDATE appls_nigms_dups
SET grant_id=v.grant_id, mechanism_code=v.mechanism_code, exist_flag=1
FROM appls_nigms_dups a, vw_appls v
WHERE a.appl_id=v.appl_id

/*--code commented by hareeshj on 7/1/13 15:43pm
--add mechanism_code for not existing appls
UPDATE appls_nigms_dups
SET mechanism_code=t.mechanism_code
FROM openquery(IRDB,'select appl_id, mechanism_code from appls_t')t
WHERE appls_nigms_dups.mechanism_code is null and appls_nigms_dups.appl_id=t.appl_id
*/

--code added by hareeshj on 7/1/13 14:33pm
DELETE FROM appls_nigms_dups_appls_t

INSERT INTO appls_nigms_dups_appls_t
SELECT a.*
FROM openquery(IRDB,'select appl_id, mechanism_code from appls_t')t, appls_nigms_dups a
WHERE a.mechanism_code is null and a.appl_id=t.appl_id

--SELECT * FROM appls_nigms_dups_appls_t

UPDATE appls_nigms_dups
SET mechanism_code=t.mechanism_code
FROM appls_nigms_dups_appls_t t 
WHERE appls_nigms_dups.mechanism_code is null and appls_nigms_dups.appl_id=t.appl_id

--SELECT * FROM appls_nigms_dups_appls_t
--code addition complete


--add grant_id for not existing appls
UPDATE appls_nigms_dups
SET grant_id=v.grant_id
FROM appls_nigms_dups a, vw_grants v
WHERE a.grant_id is null and a.admin_phs_org_code=v.admin_phs_org_code and a.serial_num=v.serial_num and a.mechanism_code=v.mechanism_code

--set up grant_id to appls_ciip table for NIGMS duplicate
UPDATE appls_ciip 
SET grant_id=appls_nigms_dups.grant_id
FROM appls_ciip, appls_nigms_dups
WHERE appls_ciip.appl_id= appls_nigms_dups.appl_id

END

GO


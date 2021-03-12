SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE PROCEDURE [dbo].[sp_web_egrants_create_appl]

--@act 			varchar(50),
@admin_code		char(2),
@serial_num 	int,
@appl_type_code tinyint,
@activity_code 	char(3),
@support_year 	smallint,
@suffix_code	varchar(4),
@ic				varchar(10),
@operator 		varchar(50),
@return_notice	varchar(50) OUTPUT

AS
/************************************************************************************************************/
/***									 						***/
/***	Procedure Name: sp_web_egrants_create_appl				***/
/***	Description:Create Grant Year							***/
/***	Created:	03/10/2014	Leon							***/
/***	Modified:	03/12/2014	Leon							***/
/***	Modified:	03/24/2014	Leon add return notice			***/
/***    Modified    02/01/2014  Frances                         ***/
/************************************************************************************************************/

SET NOCOUNT ON

declare @applid 			int
declare @grantid 			int
declare @count 				int
declare @full_grant_num		varchar(100)

--check @suffix_code	
IF @suffix_code='' or @suffix_code is null SET @suffix_code=null

/**find grant_id**/
SET @grantid =(SELECT grant_id FROM grants WHERE admin_phs_org_code=@admin_code and serial_num=@serial_num)
IF @grantid IS NULL 
BEGIN
INSERT grants(admin_phs_org_code, serial_num)
VALUES(@admin_code, @serial_num)
SET @grantid=@@identity
END

---to check if it is existing
IF @suffix_code='' OR @suffix_code IS NULL
BEGIN
SET @count=(
SELECT COUNT(appl_id) FROM appls WHERE grant_id=@grantid and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year and suffix_code is null)
--SET @applid=(SELECT appl_id FROM appls WHERE grant_id=@grantid and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year)
END
ELSE
BEGIN
SET @count=(
SELECT COUNT(appl_id) FROM appls WHERE grant_id=@grantid and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year and suffix_code=@suffix_code)
---SET @applid=(SELECT appl_id FROM appls WHERE grant_id=@grantid and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year and suffix_code=@suffix_code)
END

IF @count=0
BEGIN---create appl_id

SELECT @applid=min(appl_id)-FLOOR(10000 * RAND()) FROM appls
INSERT appls(appl_id, appl_type_code,activity_code, grant_id, support_year,suffix_code)
VALUES (@applid, @appl_type_code, @activity_code, @grantid, @support_year, upper(isnull(@suffix_code,null)))

SET @full_grant_num=(SELECT full_grant_num FROM vw_appls WHERE appl_id=@applid)
SET @return_notice=@full_grant_num +' has been created'
END
ELSE
BEGIN
SET @return_notice='The grand year you want to create is duplicate'
END

SELECT @return_notice 


GO


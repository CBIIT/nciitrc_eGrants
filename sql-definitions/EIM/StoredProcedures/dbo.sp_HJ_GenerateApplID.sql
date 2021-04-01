SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_HJ_GenerateApplID] (@GrantNum varchar(50))
AS
BEGIN


DECLARE @GrantID int
DECLARE @ApplID int
DECLARE @offset smallint

SET @GrantNum=replace(@GrantNum,' ','')

--acceptable 5-digit numbers
IF 
(
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @GrantNum)>0
OR
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A-Z][1-9]%', @GrantNum)>0
OR
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A-Z][1-9][A-Z][1-9]%', @GrantNum)>0
)
BEGIN
SET @GrantNum=replace(@GrantNum,substring(@GrantNum,5,2),substring(@GrantNum,5,2) + '0')
END


--check if the number is kosher now
IF 
(
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @GrantNum)=0
AND
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A-Z][1-9]%', @GrantNum)=0
AND
patindex('%[1-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A-Z][1-9][A-Z][1-9]%', @GrantNum)=0
)
BEGIN
RETURN 0
END

SELECT @ApplID=dbo.fn_applid_match(@GrantNum)
IF @ApplID IS NOT NULL RETURN @ApplID


SELECT @GrantID=grant_id FROM grants
WHERE admin_phs_org_code=substring(@GrantNum,5,2) and serial_num=convert(int, substring(@GrantNum,7,6))


/*
IF @GrantID IS NULL 
BEGIN
INSERT grants(admin_phs_org_code, serial_num)
VALUES(substring(@GrantNum,5,2),convert(int, substring(@GrantNum,7,6)))
SET @GrantID=@@identity
END

SELECT @ApplID=min(appl_id)-1 FROM appls


INSERT appls(appl_id, appl_type_code, activity_code, grant_id, support_year,suffix_code)

select
@ApplID
,left(@GrantNum,1)
,substring(@GrantNum, 2,3)
,@GrantID
,convert(int, substring(@GrantNum,14,2)),

(CASE substring(@GrantNum,16,4)
WHEN '' THEN NULL
ELSE substring(@GrantNum,16,4)
END) as suffic_code
*/

RETURN @ApplID

END
GO


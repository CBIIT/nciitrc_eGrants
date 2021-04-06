SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE   VIEW [dbo].[vw_PD_GS_last_competing_fy]
as
Select A.pd_full_name,A.pd_email_address, a.RESP_SPEC_FULL_NAME_CODE, a.RESP_SPEC_EMAIL_ADDRESS, a.serial_num, a.SUPPORT_YEAR, a.appl_id, a.ADMIN_PHS_ORG_CODE 
FROM Grant_Contacts_PD_GS a inner join
(SELECT distinct a.serial_num, a.ADMIN_PHS_ORG_CODE, max(a.SUPPORT_YEAR) as LatestCompetingFy, MAX(ACTION_FY) as ActionFy, MAX(ISNULL(SUFFIX_CODE,'A0')) as SUFFIX_CODE --a.appl_type_code --, a.suffix_code
FROM      Grant_Contacts_PD_GS A 
WHERE     a.appl_type_code IN ('1', '2', '5', '7', '9')   
group by a.serial_num, a.ADMIN_PHS_ORG_CODE ) latest on a.serial_num = Latest.serial_num and a.ADMIN_PHS_ORG_CODE = Latest.ADMIN_PHS_ORG_CODE and Latest.LatestCompetingFy = a.SUPPORT_YEAR and Latest.ActionFy = a.ACTION_FY and latest.SUFFIX_CODE = isnull(a.SUFFIX_CODE, 'A0') 
where  a.appl_type_code IN ('1', '2', '5', '7', '9')


GO


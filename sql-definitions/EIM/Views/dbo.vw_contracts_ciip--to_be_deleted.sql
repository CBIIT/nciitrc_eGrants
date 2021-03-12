SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create view [dbo].[vw_contracts_ciip--to_be_deleted] AS 
SELECT     CIIP_ID AS ciip_contract_id, piid, AWARD_NUM, FISCAL_YEAR, SERIAL_NUM, CASE WHEN serial_num < 10000 THEN RIGHT(CONVERT(varchar, 
                      fiscal_year, 101), 1) + RIGHT('0000' + CONVERT(varchar, serial_num), 4) WHEN serial_num > 10000 THEN RIGHT(CONVERT(varchar, fiscal_year, 101), 
                      1) + substring(CONVERT(varchar, serial_num), 1, 2) + substring(CONVERT(varchar, serial_num), 4, 2) ELSE RIGHT(CONVERT(varchar, fiscal_year, 101), 
                      1) + CONVERT(varchar, serial_num) END AS contract_number, 
                      CASE WHEN serial_num > 10000 THEN activity_code + '-' + admin_phs_org + '-' + RIGHT(CONVERT(varchar, fiscal_year, 101), 1) 
                      + substring(CONVERT(varchar, serial_num), 1, 2) + substring(CONVERT(varchar, serial_num), 4, 2) 
                      WHEN serial_num < 10000 THEN activity_code + '-' + admin_phs_org + '-' + RIGHT(CONVERT(varchar, fiscal_year, 101), 1) 
                      + RIGHT('0000' + CONVERT(varchar, serial_num), 4) END AS full_contract_number, CASE WHEN substring(award_num, 0, 4) 
                      = 'HHS' THEN award_num ELSE NULL END AS hhs, CASE WHEN CHARINDEX('-', adb_num) = 4 AND CHARINDEX('-', adb_num, 5) = 7 AND 
                      substring(adb_num, 1, 3) NOT IN ('263') THEN adb_num ELSE NULL END AS adb_num, CASE WHEN project_end_date IS NULL OR
                      project_end_date <
                          (SELECT     '20' + CONVERT(char(6), getdate(), 12)) THEN 'y' ELSE 'n' END AS close_out, ACTIVITY_CODE, ADMIN_PHS_ORG AS contract_type, 
                      RFP_NUMBER AS rfp, UPPER(CONTRACTOR_NAME) AS institution, UPPER(SPECIALIST_NAME) AS specialist_name, PROJECT_START_DATE, 
                      PROJECT_END_DATE, CREATE_DATE, CONTRACTOR_TIN, LATEST_SIGNED_MOD_NUM
FROM         OPENQUERY(CIIP, 
                      'select c.id AS ciip_id,c.award_num,c.activity_code,c.admin_phs_org,c.rfp_number,c.serial_num,
c.contractor_name,c.specialist_name, adb_num, latest_signed_mod_num, contractor_tin,
to_char(c.fiscal_year, ''yyyy'') as fiscal_year, 
to_char(c.project_end_date, ''yyyymmdd'') as project_end_date,
to_char(c.project_end_date, ''yyyymmdd'') as project_start_date,
to_char(create_date, ''yyyymmdd'') as create_date,piid
from contract_modifications_t m, contracts_vw c 
where c.id = m.con_id (+)and c.latest_signed_mod_num = m.mod_num(+)and c.serial_num is not null and c.id 
in(select max(id) from contracts_vw group by fiscal_year,serial_num,activity_code,admin_phs_org)')
                       AS ciip
WHERE     (ADB_NUM IS NULL) OR
                      (SUBSTRING(ADB_NUM, 1, 2) NOT IN ('MQ', 'FQ')) AND (SUBSTRING(ADB_NUM, 4, 2) NOT IN ('MQ', 'FQ')) AND (SUBSTRING(ADB_NUM, 5, 2) 
                      NOT IN ('MQ', 'FQ')) AND (SUBSTRING(ADB_NUM, 6, 2) NOT IN ('MQ', 'FQ'))
GO


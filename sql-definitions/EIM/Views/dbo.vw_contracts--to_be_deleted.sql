SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_contracts--to_be_deleted]
AS

SELECT     dbo.contracts.contract_id, dbo.contracts.ciip_id, dbo.contracts.fiscal_year, dbo.contracts.contract_type, dbo.contracts.activity_code, 
                      CASE WHEN contract_number < 10000 THEN RIGHT('00000' + CONVERT(varchar, contract_number), 5) ELSE CONVERT(varchar, contract_number) 
                      END AS contract_number, CASE WHEN fiscal_year > 2006 THEN RIGHT(CONVERT(varchar, fiscal_year), 2) + RIGHT('00000' + CONVERT(varchar, 
                      contract_number), 5) END AS control_number, CASE WHEN contract_number < 10000 THEN upper(activity_code) + '-' + upper(contract_type) 
                      + '-' + RIGHT('00000' + CONVERT(varchar, contract_number), 5) ELSE upper(activity_code) + '-' + upper(contract_type) + '-' + CONVERT(varchar, 
                      contract_number) END AS full_contract_number, CASE WHEN contract_number < 10000 THEN upper(activity_code) + '-' + upper(contract_type) 
                      + '-' + RIGHT('00000' + CONVERT(varchar, contract_number), 5) ELSE upper(activity_code) + '-' + upper(contract_type) + '-' + CONVERT(varchar, 
                      contract_number) END AS adb, CASE WHEN PATINDEX('%:%', institution) > 0 THEN substring(institution, 0, PATINDEX('%:%', institution)) 
                      ELSE institution END AS institution, RTRIM(dbo.contracts.hhs) AS hhs, dbo.contracts.rfp, dbo.vw_people.person_name, dbo.vw_people.userid, 
                      dbo.contracts.specialist_id AS person_id, dbo.contracts.specialist_name, dbo.contracts.specialist_id, CONVERT(varchar, dbo.contracts.created_date, 
                      101) AS created_date, dbo.contracts.created_by_person_id, CONVERT(varchar, dbo.contracts.last_change_date, 101) AS last_change_date, 
                      CONVERT(varchar, dbo.contracts.project_end_date, 101) AS project_end_date, CASE WHEN project_end_date IS NULL OR
                      project_end_date < getdate() THEN 'y' ELSE 'n' END AS close_out, dbo.contracts.document_created, dbo.contracts.folder_created, 
                      dbo.contracts.profile_id, dbo.contracts.piid
FROM         dbo.contracts LEFT OUTER JOIN
                      dbo.vw_people ON dbo.contracts.specialist_id = dbo.vw_people.person_id
WHERE     (dbo.contracts.disabled_date IS NULL)


GO


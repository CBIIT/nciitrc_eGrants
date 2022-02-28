SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE    VIEW [dbo].[vw_grants]
AS
SELECT     dbo.grants.grant_id, dbo.grants.admin_phs_org_code, dbo.grants.serial_num, dbo.grants.mechanism_code, 
                      dbo.grants.admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, dbo.grants.serial_num), 6) AS grant_num, 
                      dbo.grants.former_admin_phs_org_code, dbo.grants.former_serial_num, dbo.grants.former_admin_phs_org_code + RIGHT('00000' + CONVERT(varchar,
                       dbo.grants.former_serial_num), 6) AS former_grant_num, dbo.grants.future_admin_phs_org_code, dbo.grants.future_serial_num, 
                      dbo.grants.paperless, dbo.appls.person_id, dbo.grants.future_admin_phs_org_code + RIGHT('00000' + CONVERT(varchar, 
                      dbo.grants.future_serial_num), 6) AS future_grant_num, dbo.appls.org_name, dbo.appls.project_title, dbo.appls.active_grant_flag, dbo.appls.fy, 

                      --dbo.appls.prog_class_code, 	  
					  (SELECT TOP 1 prog_class_code
					   FROM      Grant_Contacts_PD_GS
					   WHERE     SERIAL_NUM = grants.SERIAL_NUM and ADMIN_PHS_ORG_CODE = grants.ADMIN_PHS_ORG_CODE
					   ORDER BY SUPPORT_YEAR DESC, ACTION_FY DESC, SUFFIX_CODE DESC) as prog_class_code,

					  dbo.appls.last_name, dbo.appls.first_name, dbo.appls.mi_name, ISNULL(dbo.appls.first_name + ' ', '') 
                      + ISNULL(dbo.appls.mi_name + ' ', '') + dbo.appls.last_name AS pi_name, ISNULL(dbo.grants.is_tobacco, 0) AS is_tobacco, 
                      dbo.grants.to_be_destroyed, CASE WHEN grant_close_date IS NOT NULL THEN 'Yes' ELSE 'No' END AS closed_out, 
                      CASE WHEN stop_sign = 'yes' AND dbo.grants.admin_phs_org_code IN ('CA', 'ES') THEN 'Yes' ELSE 'No' END AS stop_sign, 
                      dbo.fn_grant_tracked(dbo.grants.grant_id) AS paper_file, 
					  dbo.fn_package_used(dbo.grants.grant_id, 'award') AS award_package, 
                      dbo.fn_package_used(dbo.grants.grant_id, 'application') AS application_package, 
					  dbo.fn_package_used(dbo.grants.grant_id, 'correspondence') AS correspondence_package, 
					  dbo.fn_package_used(dbo.grants.grant_id, 'closeout') AS closeout_package, 
					  dbo.fn_grant_funded(dbo.grants.grant_id) AS is_funded, 
					  dbo.grants.grant_close_date, 
					  /*ISNULL(dbo.grants.arra_flag, 'n') AS arra_flag, ISNULL(dbo.grants.fda_flag, 'n') AS fda_flag,*/
                      dbo.fn_get_org_flag_url(dbo.appls.org_name) as org_sv_url,	--added by leon 4/18/2016
                      CASE WHEN dbo.grants.admin_phs_org_code='CA' and dbo.fn_grant_admin_supp (grants.grant_id)>0 THEN 1 ELSE 0 END AS adm_supp,

 					  --dbo.appls.pd_full_name as current_pd_name,dbo.appls.pd_email_address as current_pd_email_address,


					  (SELECT TOP 1 pd_full_name
					   FROM      Grant_Contacts_PD_GS
					   WHERE     SERIAL_NUM = grants.SERIAL_NUM and ADMIN_PHS_ORG_CODE = grants.ADMIN_PHS_ORG_CODE
					   ORDER BY SUPPORT_YEAR DESC, ACTION_FY DESC, SUFFIX_CODE DESC) as current_pd_name,

  					  (SELECT TOP 1 pd_email_address
					   FROM      Grant_Contacts_PD_GS 
					   WHERE     SERIAL_NUM = grants.SERIAL_NUM and ADMIN_PHS_ORG_CODE = grants.ADMIN_PHS_ORG_CODE
					   ORDER BY SUPPORT_YEAR DESC, ACTION_FY DESC, SUFFIX_CODE DESC) as current_pd_email_address,


					  dbo.appls.first_name+' '+dbo.appls.last_name as current_pi_name,dbo.appls.pi_email_addr as current_pi_email_address,

					  (SELECT TOP 1  RESP_SPEC_FULL_NAME_CODE
					   FROM      Grant_Contacts_PD_GS 
					   WHERE     SERIAL_NUM = grants.SERIAL_NUM and ADMIN_PHS_ORG_CODE = grants.ADMIN_PHS_ORG_CODE
                       ORDER BY  SUPPORT_YEAR DESC, ACTION_FY DESC, SUFFIX_CODE DESC) as current_spec_name,

			           (SELECT TOP 1 RESP_SPEC_EMAIL_ADDRESS 
						FROM   Grant_Contacts_PD_GS 
						WHERE  SERIAL_NUM = grants.SERIAL_NUM and ADMIN_PHS_ORG_CODE = grants.ADMIN_PHS_ORG_CODE
						ORDER BY SUPPORT_YEAR DESC, ACTION_FY DESC, SUFFIX_CODE DESC) as current_spec_email_address,

					  CASE WHEN dbo.appls.bo_email_address IS NULL THEN 
					   (SELECT TOP 1 BO_EMAIL_ADDRESS FROM APPLS WHERE grant_id=dbo.grants.grant_id AND BO_EMAIL_ADDRESS IS NOT NULL 
					   order by fy DESC,support_year DESC,suffix_code DESC) ELSE dbo.appls.bo_email_address END AS current_bo_email_address,
					  
					  CASE WHEN MS_flag.MSFlag_cnt=0 or MS_flag.MSFlag_cnt is null THEN 'n' ELSE 'y' END AS MS_flag,
					  CASE WHEN OD_flag.ODFlag_cnt=0 or OD_flag.ODFlag_cnt is null THEN 'n' ELSE 'y' END AS OD_flag,
					  CASE WHEN STP_flag.STPFlag_cnt=0 or STP_flag.STPFlag_cnt is null THEN 'n' ELSE 'y' END AS STP_flag,
					  CASE WHEN FDA_flag.FDAFlag_cnt=0 or FDA_flag.FDAFlag_cnt is null THEN 'n' ELSE 'y' END AS FDA_flag,
					  CASE WHEN ARRA_flag.ARRAFlag_cnt=0 or ARRA_flag.ARRAFlag_cnt is null THEN 'n' ELSE 'y' END AS ARRA_flag,
					  CASE WHEN DS_flag.DSFlag_cnt=0 or DS_flag.DSFlag_cnt is null THEN 'n' ELSE 'y' END AS DS_flag


FROM         dbo.grants LEFT OUTER JOIN
                      dbo.appls ON dbo.grants.grant_id = dbo.appls.grant_id AND dbo.fn_grant_latest_appl(dbo.grants.grant_id) = dbo.appls.appl_id
 
		  LEFT OUTER JOIN (SELECT grant_id,count(*) AS MSFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='MS' and flag_application='A'
			GROUP BY grant_id) as MS_flag ON dbo.grants.grant_id=MS_flag.grant_id and dbo.grants.admin_phs_org_code='CA'

		  LEFT OUTER JOIN(SELECT grant_id,count(*) AS ODFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='OD' and flag_application='A'
			GROUP BY grant_id) as OD_flag ON dbo.grants.grant_id=OD_flag.grant_id and dbo.grants.admin_phs_org_code='CA'

		  LEFT OUTER JOIN(SELECT grant_id,count(*) AS STPFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='STP' and flag_application='G' AND end_dt IS NULL
			GROUP BY grant_id) as STP_flag ON dbo.grants.grant_id=STP_flag.grant_id and dbo.grants.admin_phs_org_code='CA'

		  LEFT OUTER JOIN(SELECT grant_id,count(*) AS FDAFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='FDA' and flag_application='B' AND end_dt IS NULL
			GROUP BY grant_id) as FDA_flag ON dbo.grants.grant_id=FDA_flag.grant_id and dbo.grants.admin_phs_org_code='CA'

		  LEFT OUTER JOIN(SELECT grant_id,count(*) AS ARRAFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='ARRA' and flag_application='G'
			GROUP BY grant_id) as ARRA_flag ON dbo.grants.grant_id=ARRA_flag.grant_id and dbo.grants.admin_phs_org_code='CA'

		  LEFT OUTER JOIN(SELECT grant_id,count(*) AS DSFlag_cnt FROM dbo.Grants_Flag_Construct where flag_type='DS' and flag_application='A'
			GROUP BY grant_id) as DS_flag ON dbo.grants.grant_id=DS_flag.grant_id and dbo.grants.admin_phs_org_code='CA'


GO


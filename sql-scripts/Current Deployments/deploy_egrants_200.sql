USE [EIM]
GO

/****** Object:  Table [dbo].[EnvUrl]    Script Date: 3/6/2022 10:09:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('[dbo].[EnvUrl]', 'U') IS NOT NULL 
	DROP table [dbo].[EnvUrl]

CREATE TABLE [dbo].[EnvUrl](
	[ServerName] [varchar](100) NULL,
	[Name] [varchar](100) NULL,
	[Url] [varchar](500) NULL,
 CONSTRAINT [uq_EnvUrl] UNIQUE NONCLUSTERED 
(
	[ServerName] ASC,
	[Name] ASC,
	[Url] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO  [dbo].[EnvUrl]
(ServerName, Name, Url
)
VALUES
('NCIDB-D387-V\MSSQLEGRANTSD',	'apps_dev_era',	'https://apps.era.nih.gov/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'appsprd_era_piAppDetail','http://appsprd.era.nih.gov/eraservices/piAppDetails/genericStatus.do?actionRole=nonPI&applID='),
('NCIDB-D387-V\MSSQLEGRANTSD',	'appsprd_era_viewdoc',	'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=GSR&parameter='),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_data_nci',	'https://egrants-data.nci.nih.gov'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_data_nci_%',	'https://egrants-data.nci.nih.gov/%'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_dev_impac',	'https://i2e-dev.'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_dev_web',	'https://egrants-web-dev.nih.gov'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_funded_modify',	'https://egrants-data.nci.nih.gov/funded/nci/modify/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_funded_pdf',	'https://egrants-data.nci.nih.gov/funded/nci/nms/pdf/pdf_multipage/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_funded2_convert_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/convert%'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_funded2_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/main/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_funding_upload',	'https://egrants-data.nci.nih.gov/funded/nci/funding/upload/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_unfunded_pdf', 'https://web-grants.nci.nih.gov/unfunded/pdf1/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'egrants_web_img_nih',	'https://egrants-web-dev.'),
('NCIDB-D387-V\MSSQLEGRANTSD',	'i2e_docviewer_openEnotify',	'https://i2e.nci.nih.gov/documentviewer/openENotificationHtml.action?applId='),
('NCIDB-D387-V\MSSQLEGRANTSD',	'i2e_docviewer_viewdoc',	'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='),
('NCIDB-D387-V\MSSQLEGRANTSD',	'nci_dev_reportserver',	'https://ncidb-d387-v.nci.nih.gov/ReportServer_MSSQLEGRANTSD/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	's2s_era_docservice',	'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'),
('NCIDB-D387-V\MSSQLEGRANTSD',	's2server_era_dev',	'https://s2s.era.nih.gov/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'apps_prd_era',	'https://apps.era.nih.gov/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'appsprd_era_piAppDetail','http://appsprd.era.nih.gov/eraservices/piAppDetails/genericStatus.do?actionRole=nonPI&applID='),
('NCIDB-P391-V\MSSQLEGRANTSP',	'appsprd_era_viewdoc',	'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=GSR&parameter='),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_data_nci',	'https://egrants-data.nci.nih.gov'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_data_nci_%',	'https://egrants-data.nci.nih.gov/%'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_funded_modify',	'https://egrants-data.nci.nih.gov/funded/nci/modify/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_funded_pdf',	'https://egrants-data.nci.nih.gov/funded/nci/nms/pdf/pdf_multipage/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_funded2_convert_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/convert%'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_funded2_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/main/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_funding_upload',	'https://egrants-data.nci.nih.gov/funded/nci/funding/upload/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_image_prod',	'https://egrants.'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_prod_impac',	'https://i2e.'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_prod_web',	'https://egrants.nih.gov'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'egrants_unfunded_pdf',	'https://web-grants.nci.nih.gov/unfunded/pdf1/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	'i2e_docviewer_openEnotify',	'https://i2e.nci.nih.gov/documentviewer/openENotificationHtml.action?applId='),
('NCIDB-P391-V\MSSQLEGRANTSP',	'i2e_docviewer_viewdoc',	'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='),
('NCIDB-P391-V\MSSQLEGRANTSP',	'nci_prd_reportserver',	'http://ncidb-p391-v.nci.nih.gov/ReportServer/Pages/ReportViewer.aspx?/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	's2s_era_docservice',	'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'),
('NCIDB-P391-V\MSSQLEGRANTSP',	's2server_era_prd',	'https://s2s.era.nih.gov/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'apps_test_era',	'https://apps.era.nih.gov/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'appsprd_era_piAppDetail','http://appsprd.era.nih.gov/eraservices/piAppDetails/genericStatus.do?actionRole=nonPI&applID='),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'appsprd_era_viewdoc',	'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=GSR&parameter='),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_data_nci',	'https://egrants-data.nci.nih.gov'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_data_nci_%',	'https://egrants-data.nci.nih.gov/%'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_funded_modify',	'https://egrants-data.nci.nih.gov/funded/nci/modify/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_funded_pdf',	'https://egrants-data.nci.nih.gov/funded/nci/nms/pdf/pdf_multipage/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_funded2_convert_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/convert%'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_funded2_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/main/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_funding_upload',	'https://egrants-data.nci.nih.gov/funded/nci/funding/upload/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_test_impac',	'https://i2e-test.'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_test_web',	'https://egrants-web-test.nih.gov'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_unfunded_pdf',	'https://web-grants.nci.nih.gov/unfunded/pdf1/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'egrants_web_img_test',	'https://egrants-web-test.'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'i2e_docviewer_openEnotify',	'https://i2e.nci.nih.gov/documentviewer/openENotificationHtml.action?applId='),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'i2e_docviewer_viewdoc',	'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	'nci_test_reportserver',	'https://ncidb-q389-v.nci.nih.gov/ReportServer_MSSQLEGRANTSQ/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	's2s_era_docservice',	'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'),
('NCIDB-Q389-V\MSSQLEGRANTSQ',	's2server_era_test',	'https://s2s.era.nih.gov/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'apps_stg_era',	'https://apps.era.nih.gov/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'appsprd_era_piAppDetail','http://appsprd.era.nih.gov/eraservices/piAppDetails/genericStatus.do?actionRole=nonPI&applID='),
('NCIDB-S390-V\MSSQLEGRANTSS',	'appsprd_era_viewdoc',	'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=GSR&parameter='),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_data_nci',	'https://egrants-data.nci.nih.gov'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_data_nci_%',	'https://egrants-data.nci.nih.gov/%'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_funded_modify',	'https://egrants-data.nci.nih.gov/funded/nci/modify/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_funded_pdf',	'https://egrants-data.nci.nih.gov/funded/nci/nms/pdf/pdf_multipage/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_funded2_convert_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/convert%'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_funded2_pdf',	'https://egrants-data.nci.nih.gov/funded2/nci/main/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_funding_upload',	'https://egrants-data.nci.nih.gov/funded/nci/funding/upload/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_stg_impac',	'https://i2e-stage.'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_stg_web',	'https://egrants-web-stage.nih.gov'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_unfunded_pdf',	'https://web-grants.nci.nih.gov/unfunded/pdf1/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'egrants_web_img_stg',	'https://egrants-web-stage.'),
('NCIDB-S390-V\MSSQLEGRANTSS',	'i2e_docviewer_openEnotify',	'https://i2e.nci.nih.gov/documentviewer/openENotificationHtml.action?applId='),
('NCIDB-S390-V\MSSQLEGRANTSS',	'i2e_docviewer_viewdoc',	'https://i2e.nci.nih.gov/documentviewer/viewDocument.action?applId='),
('NCIDB-S390-V\MSSQLEGRANTSS',	'nci_stg_reportserver',	     'https://ncidb-s390-v.nci.nih.gov/ReportServer/Pages/ReportViewer.aspx?/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	's2s_era_docservice',	'https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'),
('NCIDB-S390-V\MSSQLEGRANTSS',	's2server_era_stg',	'https://s2s.era.nih.gov/')

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO

/****** Object:  Table [dbo].[Org_Categories]    Script Date: 3/4/2022 11:46:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF COL_LENGTH ('dbo.Org_Categories','comments_required') IS NULL
BEGIN
		ALTER TABLE [dbo].[Org_Categories]
		ADD	[comments_required] [bit] NULL,
		[active] [bit] NULL
end

GO

-- Checked
IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('Site Visit') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 1, icon_path = NULL, Flag_period = 12, comments_required = NULL, active = 0
	WHERE doctype_name = 'Site Visit'
	
ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('Site Visit', 1, NULL, 12, NULL, 0)

-- Checked
IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('New Organization') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 0, icon_path = NULL, Flag_period = NULL, comments_required =NULL, active = 0
	WHERE doctype_name = 'New Organization'

ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('New Organization', 0, NULL, NULL, NULL, 0)

-- Checked
IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('Archives') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 0, icon_path = NULL, Flag_period = NULL, comments_required =NULL, active = 1
	WHERE doctype_name = 'Archives'
	
ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('Archives', 0, NULL, NULL, NULL, 1)


-- Checked
IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('Test Document') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 0, icon_path = NULL, Flag_period = NULL, comments_required =NULL, active = 1
	WHERE doctype_name = 'Test Document'
	
ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('Test Document', 0, NULL, NULL, NULL, 1)

-- Checked
IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('Follow-Up') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 1, icon_path = NULL, Flag_period = 36, comments_required =1, active = 1
	WHERE doctype_name = 'Follow-Up'
	
ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('Follow-Up',1, NULL, 36, 1, 1)

IF exists (SELECT 1 FROM dbo.Org_Categories
WHERE doctype_name  in ('Organization Document') )
	UPDATE dbo.Org_Categories 
	SET tobe_flagged = 0, icon_path = NULL, Flag_period = 0, comments_required =1, active = 1
	WHERE doctype_name = 'Organization Document'
	
ELSE
	INSERT INTO dbo.Org_Categories
	(doctype_name, tobe_flagged, icon_path, Flag_period, comments_required, active)
	VALUES
	('Organization Document',0, NULL, 0, 1, 1)

SET QUOTED_IDENTIFIER ON
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------
USE [EIM]
GO

/****** Object:  Table [dbo].[Org_Categories]    Script Date: 3/4/2022 11:46:25 AM ******/
SET ANSI_NULLS ON
GO

IF COL_LENGTH ('dbo.Org_Document','comments') IS NULL
BEGIN
		ALTER TABLE dbo.Org_Document
		ADD	comments varchar(256) NULL
end

IF COL_LENGTH ('dbo.Org_Document','updated_date') IS NULL
BEGIN
		ALTER TABLE dbo.Org_Document
		ADD	updated_date smalldatetime NULL
end

IF COL_LENGTH ('dbo.Org_Document','updated_by_person_id') IS NULL
BEGIN
		ALTER TABLE dbo.Org_Document
		ADD	updated_by_person_id smalldatetime NULL
end

SET QUOTED_IDENTIFIER ON
GO
IF COL_LENGTH ('dbo.Org_Document','comments') IS NULL
BEGIN
		ALTER TABLE dbo.Org_Document
		ADD	comments varchar(256) NULL
end

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_get_org_flag_url]    Script Date: 3/6/2022 10:13:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

  IF NOT EXISTS (SELECT *
               FROM   sys.objects
               WHERE  object_id = OBJECT_ID(N'[dbo].[fn_get_org_flag_url]')
                      AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
       EXEC('CREATE FUNCTION [dbo].[fn_get_org_flag_url] () RETURNS VARCHAR(MAX) AS BEGIN RETURN 0 END')
  GO

ALTER FUNCTION [dbo].[fn_get_org_flag_url] (@org_name varchar(200))

RETURNS varchar(100) AS 

BEGIN 

RETURN 
(
select top 1 url from vw_org_document v
where org_name =@org_name and convert(date,end_date) >= convert(date,GETDATE()) AND v.category_name = 'Site Visit'
ORDER BY end_date DESC 	
)

END


GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_get_env_url]    Script Date: 3/6/2022 10:13:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

  IF NOT EXISTS (SELECT *
               FROM   sys.objects
               WHERE  object_id = OBJECT_ID(N'[dbo].[fn_get_env_url]')
                      AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
       EXEC('CREATE FUNCTION [dbo].[fn_get_env_url]() RETURNS VARCHAR(MAX) AS BEGIN RETURN 0 END')
  GO

ALTER FUNCTION [dbo].[fn_get_env_url] (@name varchar(max))

RETURNS varchar(max) 
BEGIN 

DECLARE @url varchar(800)

select @url=url from [dbo].[EnvUrl]	
where ServerName = @@SERVERNAME
and name  = @name

RETURN @url

END

GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_get_local_image_server]    Script Date: 3/4/2022 4:07:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

  IF NOT EXISTS (SELECT *
               FROM   sys.objects
               WHERE  object_id = OBJECT_ID(N'[dbo].[fn_get_local_image_server]')
                      AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' ))
       EXEC('CREATE FUNCTION [dbo].[fn_get_local_image_server]() RETURNS INT AS BEGIN RETURN 0 END')
  GO

ALTER  FUNCTION [dbo].[fn_get_local_image_server] ()

RETURNS varchar(255) AS 

BEGIN 

declare @local_image_server varchar(255)

if @@SERVERNAME = 'NCIDB-D387-V\MSSQLEGRANTSD'
	set @local_image_server= dbo.fn_get_env_url('egrants_dev_web') 

else if @@SERVERNAME = 'NCIDB-Q389-V\MSSQLEGRANTSQ'
	set @local_image_server= dbo.fn_get_env_url('egrants_test_web') 

else if 
@@SERVERNAME = 'NCIDB-S390-V\MSSQLEGRANTSS'
	set @local_image_server= dbo.fn_get_env_url('egrants_stg_web') 

else if
@@SERVERNAME = 'NCIDB-S390-V\MSSQLEGRANTSS'
	set @local_image_server= dbo.fn_get_env_url('egrants_prod_web') 

RETURN @local_image_server

END

GO
-------------------------------------------------------------------------------------------------------
USE [EIM]
GO

/****** Object:  View [dbo].[vw_grants]    Script Date: 3/4/2022 11:34:15 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER     VIEW [dbo].[vw_grants]
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
					  CASE WHEN DS_flag.DSFlag_cnt=0 or DS_flag.DSFlag_cnt is null THEN 'n' ELSE 'y' END AS DS_flag,
					  CASE WHEN EXISTS(Select document_id from vw_org_document od inner join Org_Master om on od.org_id = om.org_id where category_name = 'Follow-Up' and CONVERT(date,end_date_showFlag) >= CONVERT(date,GETDATE()) and om.org_name = dbo.appls.org_name group by od.document_id) THEN 1 ELSE 0 END as Institutional_flag1,
					  CASE WHEN EXISTS(Select org_id from vw_org_document vod where vod.Org_name =  dbo.appls.org_name) THEN 1 ELSE 0 END as Institutional_flag2,
					  (Select top 1 url from vw_org_document od where category_name = 'Follow-Up' and end_date_showFlag >= GETDATE() and org_name = dbo.appls.org_name order by end_date_showFlag DESC ) as inst_flag1_url




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

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPane1' , N'SCHEMA',N'dbo', N'VIEW',N'vw_grants', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[46] 4[3] 2[33] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "grants"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 258
            End
            DisplayFlags = 280
            TopColumn = 13
         End
         Begin Table = "appls"
            Begin Extent = 
               Top = 160
               Left = 44
               Bottom = 268
               Right = 254
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_grants'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[46] 4[3] 2[33] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "grants"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 258
            End
            DisplayFlags = 280
            TopColumn = 13
         End
         Begin Table = "appls"
            Begin Extent = 
               Top = 160
               Left = 44
               Bottom = 268
               Right = 254
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_grants'
END
GO

IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(N'MS_DiagramPaneCount' , N'SCHEMA',N'dbo', N'VIEW',N'vw_grants', NULL,NULL))
	EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_grants'
ELSE
BEGIN
	EXEC sys.sp_updateextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vw_grants'
END

GO

------------------------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants]    Script Date: 3/4/2022 11:33:07 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER         PROCEDURE [dbo].[sp_web_egrants]

@str 			nvarchar(400),
@grant_id 		int,
@package 		varchar(50),
@appl_id 		int,
@current_page	int,
@browser		varchar(50),
@ic  			varchar(10),
@operator 		varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants											***/
/***	Description:searching for egrants										***/
/***	Created:	03/07/2013	Leon											***/
/***	Modified:	11/07/2013	Leon											***/
/***	Modified:   08/08/2016	Frances											***/
/***	Modified:   10/04/2016	Leon simplify and pagination					***/
/***	Modified:   01/25/2019	Leon added filters search						***/
/***	Modified:   03/08/2019	Leon added email address for PI, PD and SP		***/
/***	Modified:   03/19/2019	Leon added flag information for appl and grant	***/
/************************************************************************************************************/
SET NOCOUNT ON

--for sql
declare @type				varchar(50)
declare @sql				varchar(1000)
declare @count				int

--for user info
declare @profile_id			int
declare @profile			varchar(10)
declare @person_id			int
declare @position_id		int

--for search info
declare @search_str			varchar(800)
declare @separate		    int
declare @search_id			int

--for filter searching
declare @filter_type		int
declare @fy					int
declare @activity_code		varchar(3)
declare	@admin_phs_org_code	varchar(2)

--for pagination
declare @total_grants		int
declare @per_page			int
declare @per_tab			int
declare @start				int
declare @end				int

--find unser info
SET @operator=LOWER(@operator)
SELECT @profile_id=profile_id  FROM  profiles  WHERE  profile=@IC
SELECT @person_id= person_id FROM vw_people WHERE userid=@operator AND profile_id=@profile_id
SELECT @position_id=position_id FROM vw_people WHERE person_id=@person_id

SET @str=RTRIM(LTRIM(@str))
IF (@str='' or @str is null) and (@grant_id='' or @grant_id is null) and (@appl_id='' or @appl_id is null) Return

create table #a(appl_id int primary key)

--search by appl_id:123456 
IF (@str<>'' and @str is not null and (@str<>'qc' and @package<>'by_filters') )
SELECT @separate=PATINDEX('appl_id:%',@str)
IF @separate=1
BEGIN
SET @separate=PATINDEX('%:%',@str)
SET @count = ISNUMERIC(SUBSTRING(@str,@separate+1,LEN(@str)))
IF	@count = 1 SET @appl_id=convert(int,SUBSTRING(@str,@separate+1,LEN(@str))) 
END

--search by appl_id=123456
IF @separate<>1
SELECT @separate=PATINDEX('appl_id=%',@str)
IF @separate=1
BEGIN
SET @separate=PATINDEX('%=%',@str)
SET @count = ISNUMERIC(SUBSTRING(@str,@separate+1,LEN(@str)))
IF	@count = 1 SET @appl_id=convert(int,SUBSTRING(@str,@separate+1,LEN(@str))) 
END

---search by full_grant_num
IF @appl_id is null and (@str<>'' and @str is not null and @str<>'qc' and @package<>'by_filters') and PATINDEX('%-%',@str)>1 ---and len(@str)<=19
BEGIN
---IF RIGHT(@str,1)='+' SET @str=substring(@str,1,LEN(@str)-1)--remove last '+' for chrome
SET @appl_id=(select appl_id FROM vw_appls WHERE full_grant_num=@str)
IF @appl_id is not null SET @str=null  ----and @appl_id<>0 
END

--create appl_id or grant_id by full text searching
--IF (@str<>'' and @str is not null and @str<>'qc' and @package<>'by_filters') 
--BEGIN
--SET @sql='insert #a (appl_id) select distinct [key] from containstable(ncieim_b..appls_txt, keywords,' + char(39) + @str + char(39) + ')'
--EXEC(@sql)
--SET @count=(select count(*) from #a)
--IF @count=1 SET @appl_id=(select appl_id from #a)
--IF @count>1 SET @grant_id=(select distinct grant_id from vw_appls where appl_id in(select appl_id from #a))
--END

---find search_type and create search string
IF @appl_id is not null and @appl_id<>0 
BEGIN
SET @str=null
SET @type='egrants_appl' 
SET @search_str='appl_id:'+ convert(varchar,@appl_id)
END

IF @grant_id is not null and @grant_id>0 
BEGIN
SET @str=null
--SET @package='all'
SET @type='egrants_grant' 
SET @search_str='grant_id:'+ convert(varchar,@grant_id)+' package:'+@package
END

IF @str<>'' and @str is not null and (@str<>'qc' and @package<>'by_filters') 
BEGIN
SET @type='egrants_str' 
SET @search_str=@str
END

IF @str<>'' and @str is not null and @str='qc' 
BEGIN
SET @type='egrants_qc' 
SET @search_str='filter:qc  user:'+ @operator 
END

IF @str<>'' and @str is not null and @package='by_filters'
BEGIN
SET @type='egrants_filters' 
END

---create table to save all grants
CREATE TABLE #t(id int IDENTITY (1, 1) NOT NULL, grant_id int,serial_num int)

---create table to save search data**/
DECLARE  @g table(grant_id int primary key)
DECLARE  @a table(appl_id int primary key)
DECLARE  @d table(document_id int primary key, appl_id int)
--DECLARE  @d table(document_id int primary key)

--set page number and per_page for pagenation with egrants_str or egrants_qc or egrants_filters
IF @type='egrants_str' or @type='egrants_qc' or @type='egrants_filters' 
BEGIN
IF (@current_page is null or @current_page='' or @current_page=0) SET @current_page=1
IF @str='qc'SET @per_page=5 
IF @str<>'qc' SET @per_page=20
SET @end = @current_page * @per_page
SET @start = @end - @per_page + 1
END

IF @type<>'egrants_filters'
BEGIN
SET @search_id=(SELECT search_id FROM searches WHERE search_string=@search_str)
IF @search_id IS NULL
BEGIN
INSERT searches(search_string) SELECT @search_str
SET @search_id=@@IDENTITY
END
INSERT queries(search_id,execution_time,ic,searched_by,page,browser_type)
SELECT @search_id,null,UPPER(@IC),@operator,ISNULL(@current_page, null),@browser
END

--go to load data
if @type='egrants_qc' GOTO egrants_qc
if @type='egrants_str' GOTO egrants_str
if @type='egrants_appl' GOTO egrants_appl
if @type='egrants_grant' GOTO egrants_grant
if @type='egrants_filters' GOTO egrants_filters

-------------------
egrants_filters:

SET @sql='INSERT #t(grant_id, serial_num) '+ @str
EXEC(@sql)

--for pagination
SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end --ORDER BY id
END
ELSE 
BEGIN
INSERT @g(grant_id)SELECT grant_id FROM #t --ORDER BY id
END

----INSERT @g(grant_id) SELECT top 10 grant_id FROM #t ORDER BY id

----insert appl_id with @g
INSERT @a 
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id 
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1 --display new created appl_id

----clean up search if there is no appls return
SET @count=(select count(*) from @a)
IF @count=0 
BEGIN
delete from @g
END

GOTO OUTPUT
--------------------
egrants_str:

SET @str=LTRIM(RTRIM(dbo.fn_str_decode(@str)))
SET @str=dbo.fn_str(@str)

SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.doc_count>0 and a.appl_id=c.[key] order by serial_num'
---SET @sql='insert @t(grant_id, serial_num) select distinct grant_id, serial_num from containstable(ncieim_b..appls_txt,keywords,' + char(39) + @str + char(39) + ') c, vw_appls a where a.appl_id=c.[key] and a.admin_phs_org_code='+char(39)+'CA'+char(39)+' and a.closed_out='+char(39)+'no'+char(39)+ 'order by serial_num'
EXEC(@sql)

--for pagination
SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end ORDER BY serial_num
END
ELSE 
BEGIN
INSERT @g(grant_id)SELECT grant_id FROM #t ORDER BY serial_num
END

--insert appl_id with @g
INSERT @a 
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id 
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1 --display new created appl_id

--clean up search if there is no appls return
SET @count=(select count(*) from @a)
IF @count=0 
BEGIN
delete from @g
return
END
ELSE 

GOTO OUTPUT
--------------------
egrants_grant:

INSERT @g(grant_id) SELECT @grant_id

---insert appls by grant_id
INSERT @a 
SELECT DISTINCT appl_id FROM vw_appls_used_bygrant vg WHERE vg.grant_id=@grant_id
UNION
SELECT DISTINCT appl_id FROM vw_appls a WHERE a.grant_id=@grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

IF @package is null or @package ='' SET @package ='All'

---insert all documents by grant_id
IF @package ='All' or @package ='all'		
BEGIN
INSERT @d(document_id, appl_id) 
SELECT document_id, appl_id FROM egrants WHERE grant_id=@grant_id
--UNION
--SELECT distinct document_id, appl_id FROM vw_funding WHERE grant_id=@grant_id
--add doc from vw_funding for the appl without any documents in egrants by Leon 05/11/2019
INSERT @d(document_id, appl_id) SELECT distinct document_id, appl_id FROM vw_funding WHERE grant_id=@grant_id and appl_id not in(select appl_id from @d)
END

---insert documents by flag type
IF @package<>'All' and @package<>'all'
BEGIN
INSERT #a(appl_id) select distinct appl_id from Grants_Flag_Construct where flag_type=@package and grant_id=@grant_id
INSERT @d(document_id, appl_id) SELECT document_id, appl_id FROM egrants WHERE appl_id in(select appl_id from #a)
END
 
GOTO OUTPUT
--------------------
egrants_appl:

IF (@appl_id<>'' or @appl_id is not null) ---GOTO foot
BEGIN

INSERT @d(document_id, appl_id) 
SELECT document_id, appl_id FROM egrants WHERE appl_id=@appl_id
UNION
SELECT document_id, appl_id FROM vw_funding WHERE appl_id=@appl_id  --add in by leon 12/21/2016

INSERT @g(grant_id) SELECT grant_id FROM vw_appls WHERE appl_id = @appl_id

INSERT @a(appl_id)
SELECT DISTINCT appl_id FROM @g AS t, vw_appls_used_bygrant vg WHERE vg.grant_id=t.grant_id
UNION
SELECT DISTINCT appl_id FROM @g AS t, vw_appls a WHERE a.grant_id=t.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

---IF (SELECT COUNT(*) FROM @d)=0 AND (SELECT COUNT(*) FROM @a)=0 GOTO foot

END

GOTO OUTPUT
--------------------
egrants_qc:

SET @sql='insert #t(grant_id, serial_num) select distinct grant_id, serial_num FROM egrants WHERE grant_id is not null and qc_date is not null and appl_id is not null and parent_id is null and qc_person_id='+convert(varchar,@person_id) + ' order by serial_num desc'
EXEC(@sql)

SET @total_grants=(SELECT count(*)FROM #t)
IF @total_grants>@per_page 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t WHERE id between @start and @end ---ORDER BY serial_num DESC
END 
ELSE 
BEGIN
INSERT @g(grant_id) SELECT grant_id FROM #t ---ORDER BY serial_num DESC
END

INSERT @d(document_id, appl_id) SELECT document_id, appl_id FROM egrants e, @g g  ---DISTINCT 
WHERE e.grant_id=g.grant_id and qc_person_id=@person_id and qc_date is not null ---and appl_id is not null and parent_id is null

INSERT @a(appl_id)
SELECT appl_id FROM vw_appls_used_bygrant AS vg, @g AS g WHERE vg.grant_id=g.grant_id ---DISTINCT
UNION 
SELECT appl_id FROM vw_appls AS a, @g AS g WHERE a.grant_id=g.grant_id and loaded_date>convert(varchar,getdate(),101) and appl_id<1---display new created appl_id

GOTO OUTPUT
------------------
OUTPUT:

SELECT
1		as tag, 
0		as parent,
g.grant_id, 
RIGHT('00000' + CONVERT(varchar,serial_num), 6) as serial_num,
admin_phs_org_code,
former_grant_num,
dbo.fn_get_latest_full_grant_num(g.grant_id) as latest_full_grant_num,
dbo.fn_get_all_activity_code(g.grant_id) as all_activity_code,
CASE 
	WHEN len(project_title)<=60 
	THEN UPPER(project_title)
	ELSE UPPER(substring(project_title,0,60))+'...' 
END as project_title, 
org_name,			--dbo.fn_clean_characters(org_name) as org_name,                                       
pi_name,			--dbo.fn_clean_characters(pi_name) as pi_name, 
UPPER(current_pi_name) as current_pi_name,
current_pi_email_address,
UPPER(current_pd_name) as current_pd_name,
current_pd_email_address,
UPPER(current_spec_name) as current_spec_name,
current_spec_email_address,
current_bo_email_address,
prog_class_code,
org_sv_url as sv_url,		---dbo.fn_get_org_flag_url(org_name) as sv_url,
ARRA_flag as arra_flag,
FDA_flag as fda_flag,
STP_flag as stop_flag,
MS_flag	as ms_flag,
OD_flag as od_flag,
DS_flag as ds_flag,
adm_supp,
g.institutional_flag1,
g.institutional_flag2,
g.inst_flag1_url,
--applsLayer              
null			as appl_id,
null			as full_grant_num,
null			as support_year,
null			as project_title,
null			as appl_type_code,
null			as deleted_by_impac,
null			as doc_count,       ---pi_name,org_name,
null			as closeout_notcount,
null			as competing,
null            as fsr_count, 
null			as frc_destroyed, 
null			as appl_fda_flag,
null			as appl_ms_flag,
null			as appl_od_flag,
null            as appl_ds_flag,
null			as closeout_flag,
null			as irppr_id,
null            as can_add_doc,
null			as can_add_funding, 
---docsLayer
null			as docs_count
FROM @g AS t inner join vw_grants g on t.grant_id=g.grant_id  

UNION ALL

SELECT
2, 
1,
grant_id,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
CAST(0 as bit),
CAST(0 as bit),
null,
--applsLayer
a.appl_id,
full_grant_num,
support_year_suffix,
dbo.fn_clean_characters(UPPER(project_title)),
appl_type_code,
deleted_by_impac,
dbo.fn_doc_count(a.appl_id),					----dbo.fn_clean_characters(pi_name),
dbo.fn_appl_CloseOut_NotCount(a.appl_id),
--CASE
--WHEN (a.appl_id=@appl_id) or (grant_id=@grant_id and @package='all') or ( grant_id=@grant_id and (dbo.fn_applid_package(a.appl_id, @package))>= 1) or ( @str='qc' and dbo.fn_appl_with_qc(a.appl_id, @person_id)>=1) THEN 1   ---or (dbo.fn_applid_package(@appl_id, @package) = 1)
--ELSE 0
--END,
competing,
dbo.fn_appl_fsr_count(a.appl_id),
frc_destroyed,
FDA_flag,
MS_flag,		---dbo.fn_flag_A(a.appl_id,'ms'),
OD_flag,
DS_flag,
dbo.fn_show_closeout_flag(a.appl_id),
dbo.fn_get_irppr_id(a.appl_id),
CASE
WHEN @position_id >= 2 and frc_destroyed=0 and deleted_by_impac='n' 
THEN 'y'
ELSE 'n'
END,
CASE
WHEN @IC='NCI' and @position_id>=5 and admin_phs_org_code='CA'  
THEN 'y'
ELSE 'n'
END,
--docsLayer
null
FROM @a AS t,vw_appls a WHERE t.appl_id=a.appl_id ---order by support_year desc

UNION ALL	--add grant documents

SELECT
3,
2,
null,			---grant_id, 
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
null,
CAST(0 as bit),
CAST(0 as bit),
null,
--applsLayer
t.appl_id,
null,			---dbo.fn_appl_full_grant_num(t.appl_id),
null,
null,
null,
null,
null,
null,
null,
null,
CASE 
WHEN dbo.fn_appl_frc_destroyed(t.appl_id)=1 THEN 1
ELSE 0
END,
null,
null,
null,
null,
null,
null,
null,
null,
--docsLayer
count(t.document_id)
--FROM @d AS t, egrants d WHERE t.document_id=d.document_id and t.appl_id IS NOT NULL group by t.appl_id
--ORDER BY tag,grant_id,support_year desc  ----,appl_id  
FROM @d AS t, vw_appls a WHERE t.appl_id = a.appl_id group by t.appl_id
ORDER BY tag,grant_id,support_year desc  ----added by Leon 5/11/2019 

SET ANSI_NULLS OFF

GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------



-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO

/****** Object:  View [dbo].[vw_org_document]    Script Date: 3/4/2022 11:35:44 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[vw_org_document]
AS
SELECT     dbo.Org_Document.document_id, dbo.Org_Categories.doctype_name AS category_name, dbo.Org_Categories.active as active,
               dbo.Org_Document.doctype_id AS category_id, dbo.Org_Master.Org_name, 
                      dbo.Org_Document.org_id, CONVERT(varchar, dbo.Org_Document.created_date, 101) AS created_date, dbo.Org_Document.start_date_ShowFlag, CONVERT(varchar, dbo.Org_Document.start_date_ShowFlag, 
                      101) AS start_date, dbo.Org_Document.end_date_showFlag, CONVERT(varchar, dbo.Org_Document.end_date_showFlag, 101) AS end_date, dbo.Org_Categories.tobe_flagged, dbo.Org_Document.file_type, 
                      dbo.Org_Document.url,dbo.Org_Document.created_by_person_id,dbo.Org_document.comments,
					  people.person_name AS created_by
FROM         dbo.Org_Document INNER JOIN
                      dbo.Org_Categories ON dbo.Org_Document.doctype_id = dbo.Org_Categories.doctype_id INNER JOIN 
                      dbo.people ON dbo.Org_Document.created_by_person_id = dbo.people.person_id LEFT OUTER JOIN
                      dbo.Org_Master ON dbo.Org_Document.org_id = dbo.Org_Master.org_id
WHERE     (dbo.Org_Document.disabled_date IS NULL)


GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_institutional_file_create]    Script Date: 3/4/2022 11:37:05 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE OR ALTER   PROCEDURE [dbo].[sp_web_egrants_institutional_file_create]

@org_id				int,
@category_id		int,
@file_type			varchar(5),
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50),
@comments			varchar(256),
@document_id		varchar(10) OUTPUT

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_institutional_file_create					***/
/***	Description:create org files										***/
/***	Created:	01/10/2017	Leon	create it for MVC						***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@person_id		int,
@profile_id		int,
@doc_id			int,
@category_name	varchar(100),
@file_location	varchar(200),
@tobe_flag char(1)


/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

SET @file_location='/data/funded/nci/institutional/'
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))
select @category_name= doctype_name , @tobe_flag = isNull(tobe_flagged,0) from Org_Categories where doctype_id=@category_id

/*
IF @category_name<>'Site Visit' 
BEGIN
SET @start_date=null
SET @end_date=null
END
*/

IF @tobe_flag<>'1'
BEGIN
SET @start_date=null
SET @end_date=null
END


----create new document 
INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,end_date_showFlag,comments)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),ISNULL(@end_date,null),@comments

SET @doc_id=@@IDENTITY

UPDATE dbo.Org_Document SET url=@file_location + convert(varchar,@doc_id) + '.' + @file_type WHERE document_id=@doc_id

SELECT @document_id=convert(varchar, @doc_id) 

RETURN

GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------


USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_institutional_file_find_org]    Script Date: 3/4/2022 11:38:15 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_web_egrants_institutional_file_find_org]

@org_id	int = 0,
@org_name varchar(300) = ''

AS

BEGIN
--02/27/2022 BSHELL  optimized SQL to return Org Data
  Select om.org_id, UPPER(om.Org_name) as Org_Name, om.index_id, docs.created_by, docs.created_date, docs.end_date, docs.sv_url from org_master om left join 
  (Select m.org_id, max_end_date as end_date, v.created_by, v.created_date, dbo.fn_get_local_image_server() + v.url as sv_url
   FROM 
   (Select [org_id], max([end_date]) as max_end_date from vw_org_document where category_id = 2 group by org_id) m 
	inner join vw_org_document v on m.org_id = v.org_id and v.end_date = m.max_end_date) docs on om.org_id = docs.org_id 
	where (not(@org_id = 0) and om.org_id = @org_id) or (@org_id = 0 and not(@org_name = '') and om.Org_name = @org_name) 
	order by Org_Name

RETURN
END
GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_institutional_file_update]    Script Date: 3/4/2022 11:38:39 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE OR ALTER   PROCEDURE [dbo].[sp_web_egrants_institutional_file_update]

@category_id		int,
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50),
@comments			varchar(256),
@document_id		varchar(10) 

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_institutional_file_update					***/
/***	Description: update org files											***/
/***	Created:	03/02/20	Madhu		created it for MVC					***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@person_id		int,
@profile_id		int,
@doc_id			int,
@category_name	varchar(100),
@file_location	varchar(200),
@tobe_flag char(1),
@comments_req  bit


/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

SET @file_location='/data/funded/nci/institutional/'
select @category_name= doctype_name , @tobe_flag = isNull(tobe_flagged,0), @comments_req = comments_required from Org_Categories where doctype_id=@category_id

IF @tobe_flag<>'1'
BEGIN
SET @start_date=null
SET @end_date=null
END

IF @comments_req <> 1
BEGIN
SET @comments = null
END


----Update document 
UPDATE dbo.Org_Document
SET doctype_id = @category_id,
updated_date = getdate(),
updated_by_person_id = @person_id,
start_date_ShowFlag=ISNULL(@start_date,null),
end_date_showFlag=ISNULL(@end_date,null),
comments = @comments
where
document_id = @document_id

RETURN
GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_inst_files_disable_doc]    Script Date: 3/4/2022 11:39:53 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE OR ALTER   PROCEDURE [dbo].[sp_web_egrants_inst_files_disable_doc]
(
@doc_id		int,
@user_id varchar(50) = null,
@person_id	int = 0

)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_disable_doc     			***/
/***	Description:files disable doc 										***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON
declare @pperson_id int

-------------------
if (@person_id is null)
BEGIN
Select @pperson_id = p.person_id  from people p where p.userid = @user_id 
END
ELSE
set @pperson_id = @person_id

-- disable_doc:

update dbo.Org_Document set disabled_date=GETDATE(), disabled_by_person_id=@pperson_id where document_id=@doc_id	

RETURN

GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_inst_files_search_orgs]    Script Date: 3/4/2022 11:40:13 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

create or alter   PROCEDURE [dbo].[sp_web_egrants_inst_files_search_orgs]

@str varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_search_orgs				***/
/***	Description:search, dispaly or edit files							***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON

-------------------
-- search_orgs:

--display org by search string
SET @str=LTRIM(rtrim(@str))
SET @str=REPLACE(@str, '     ',' ')---reducing space
SET @str=REPLACE(@str, '   ',' ')---reducing space
SET @str=REPLACE(@str, '  ',' ')---reducing space


--SELECT org_id, UPPER(Org_name)AS org_name,dbo.fn_get_org_flag_url(Org_name) as sv_doc_url
--FROM dbo.Org_Master 
--WHERE org_name like '%'+@str+'%'
--ORDER BY Org_name



-- no comments reqd
--SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
--FROM dbo.Org_Master
--WHERE org_name like '%'+@str+'%' 
--UNION
--SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, url
--FROM vw_org_document as v, dbo.Org_Master as o
--WHERE o.org_name like '%'+@str+'%' and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
--Order by Org_name


Select
   om.org_id,
   UPPER(om.Org_name) as Org_Name,
   om.index_id,
   svdocs.created_by as svcreated_by,
   svdocs.created_date as svcreated_date,
   svdocs.end_date as svend_date,
   svdocs.sv_url,
   fudocs.created_by as fucreated_by,
   fudocs.created_date as fucreated_date,
   fudocs.end_date as fuend_date,
   fudocs.fu_url,
   CASE WHEN NOT anyorgdocs.anydoc is null THEN CAST(1 as bit) else Cast(0 as bit) END as anyorgdoc
from
   org_master om 
   -- Left join to bring the documents of type ??(2 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as sv_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Site Visit'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
		
      )
      svdocs 
      on om.org_id = svdocs.org_id and svdocs.RowNum = 1
-- Left join to bring the documents of type ??(5 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as fu_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Follow-Up'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			
      )
      fudocs 
      on om.org_id = fudocs.org_id and fudocs.RowNum = 1
	  -- Left join to bring the documents of type ??(5 here)
   left join
   (
	Select max(document_id) as anydoc,
	org_id 
	from vw_org_document vod
	inner join Org_Categories cat on vod.category_id = cat.doctype_id
	where (cat.tobe_flagged = 1 AND CONVERT(date,end_date) >= CONVERT(date,GETDATE())) OR (1=1) 
	group by org_id

	) anyorgdocs on om.org_id = anyorgdocs.org_id

where om.org_name like '%'+@str+'%' 
order by
   Org_Name

RETURN
GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_inst_files_show_docs]    Script Date: 3/4/2022 11:40:41 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_web_egrants_inst_files_show_docs]
@org_id	int

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_show_docs					***/
/***	Description:show docs files											***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON
-------------------
-- show_docs:

-- comments reqd
SELECT org_id,org_name,document_id,category_name, url,[start_date],end_date,created_date, comments
FROM dbo.vw_Org_Document 
WHERE org_id=@org_id 
ORDER BY CONVERT(datetime,created_date) DESC

RETURN
GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_inst_files_show_orgs]    Script Date: 3/4/2022 11:41:01 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER  PROCEDURE [dbo].[sp_web_egrants_inst_files_show_orgs]

@index_id	int

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_show_orgs					***/
/***	Description:Show Files												***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON

-------------------
-- search_orgs:

Select
   om.org_id,
   UPPER(om.Org_name) as Org_Name,
   om.index_id,
   svdocs.created_by as svcreated_by,
   svdocs.created_date as svcreated_date,
   svdocs.end_date as svend_date,
   svdocs.sv_url,
   fudocs.created_by as fucreated_by,
   fudocs.created_date as fucreated_date,
   fudocs.end_date as fuend_date,
   fudocs.fu_url,
   CASE WHEN NOT anyorgdocs.anydoc is null THEN CAST(1 as bit) else Cast(0 as bit) END as anyorgdoc
from
   org_master om 
   -- Left join to bring the documents of type ??(2 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as sv_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Site Visit'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
		
      )
      svdocs 
      on om.org_id = svdocs.org_id and svdocs.RowNum = 1
-- Left join to bring the documents of type ??(5 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as fu_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Follow-Up'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			
      )
      fudocs 
      on om.org_id = fudocs.org_id and fudocs.RowNum = 1
	  -- Left join to bring the documents of type ??(5 here)
   left join
   (
	Select max(document_id) as anydoc,
	org_id 
	from vw_org_document vod
	inner join Org_Categories cat on vod.category_id = cat.doctype_id
	where (cat.tobe_flagged = 1 AND CONVERT(date,end_date) >= CONVERT(date,GETDATE())) OR (1=1) 
	group by org_id

	) anyorgdocs on om.org_id = anyorgdocs.org_id

where
   om.index_id = @index_id 
   and dbo.fn_get_org_doc_count(om.org_id) > 0 
order by
   Org_Name

RETURN
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_inst_files_upload_doc]    Script Date: 3/4/2022 11:41:26 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER  PROCEDURE [dbo].[sp_web_egrants_inst_files_upload_doc]
(
@org_id				int,
@doc_id				int,
@category_id		int,
@file_type			varchar(5),
@start_date			varchar(10),
@end_date			varchar(10),
@comments           varchar(256)
)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:[sp_web_egrants_inst_files_upload_doc]     			***/
/***	Description:Files Upload											***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON
DECLARE @document_id	int,
@person_id	int


--------------------------------------
--Add comments for insert
----update new document 
INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,
end_date_showFlag,comments
)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),
	   ISNULL(@end_date,null),@comments

SELECT  @document_id=@@IDENTITY

RETURN
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------


USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_institutional_files]    Script Date: 3/4/2022 11:38:59 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_web_egrants_institutional_files]

@act  				varchar(20),
@str  				varchar(50),
@index_id			int,
@org_id				int,
@doc_id				int,
@category_id		int,
@file_type			varchar(5),
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_web_egrants_institutional_files					***/
/***	Description:search, dispaly or edit files							***/
/***	Created:	03/09/2016	Leon										***/
/***	Modified:	03/09/2016	Leon										***/
/***	Modified:	12/07/2016	Leon for MVC								***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@document_id	int,
@xmlout		varchar(max),
@X		Xml,
@person_id	int,
@count		int,
@profile_id	int

/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

/**set default act**/
if @act ='show_orgs' goto show_orgs
if @act ='search_orgs' goto search_orgs
if @act ='show_docs' goto show_docs
if @act ='disable_doc' goto disable_doc
if @act ='upload_doc' goto upload_doc
---------------------
show_orgs:

--display org by index_id
--SELECT org_id, UPPER(Org_name)AS org_name,dbo.fn_get_org_flag_url(Org_name) as sv_doc_url
--FROM dbo.Org_Master
--WHERE index_id=@index_id and dbo.fn_get_org_doc_count(org_id)>0
--ORDER BY Org_name

--Madhu - This is the old Definition
--SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
--FROM dbo.Org_Master
--WHERE index_id=@index_id and dbo.fn_get_org_doc_count(org_id)>0
--UNION
--SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, (select dbo.fn_get_local_image_server()) +url as sv_url
--FROM vw_org_document as v, dbo.Org_Master as o
--WHERE o.index_id=@index_id and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
--Order by Org_name

--02/27/2022 BSHELL  optomized SQL to return Org Data

-- Moving the following code to a new Proc . [sp_web_egrants_inst_files_show_orgs]
/*
declare @index_id int
set @index_id = 1

-- no comments here 
  Select om.org_id, UPPER(om.Org_name) as Org_Name, om.index_id, docs.created_by, docs.created_date, docs.end_date, docs.sv_url 
  from org_master om left join 
  (Select m.org_id, max_end_date as end_date, v.created_by, v.created_date, dbo.fn_get_local_image_server() + v.url as sv_url
   FROM 
   (Select [org_id], max([end_date]) as max_end_date from vw_org_document where category_id = 2 group by org_id) m 
	inner join vw_org_document v on m.org_id = v.org_id and v.end_date = m.max_end_date) docs on om.org_id = docs.org_id 
	where om.index_id = @index_id and dbo.fn_get_org_doc_count(om.org_id)>0
	order by Org_Name
*/
RETURN exec dbo.sp_web_egrants_inst_files_show_orgs @index_id

----------------------
search_orgs:

/* The following code has been moved to sp_web_egrants_inst_files_show_orgs
--display org by search string
SET @str=LTRIM(rtrim(@str))
SET @str=REPLACE(@str, '     ',' ')---reducing space
SET @str=REPLACE(@str, '   ',' ')---reducing space
SET @str=REPLACE(@str, '  ',' ')---reducing space


--SELECT org_id, UPPER(Org_name)AS org_name,dbo.fn_get_org_flag_url(Org_name) as sv_doc_url
--FROM dbo.Org_Master 
--WHERE org_name like '%'+@str+'%'
--ORDER BY Org_name
-- no comments reqd
SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
FROM dbo.Org_Master
WHERE org_name like '%'+@str+'%' 
UNION
SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, url
FROM vw_org_document as v, dbo.Org_Master as o
WHERE o.org_name like '%'+@str+'%' and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
Order by Org_name
*/

RETURN exec dbo.sp_web_egrants_inst_files_search_orgs @str

------------------------
show_docs:
-- comments reqd
/*SELECT org_id,org_name,document_id,category_name, url,[start_date],end_date,created_date
FROM dbo.vw_Org_Document 
WHERE org_id=@org_id 
*/
RETURN exec dbo.sp_web_egrants_inst_files_show_docs @org_id
-------------------------
disable_doc:


RETURN
-------------------------
upload_doc:

--Add comments for insert
----update new document 
/*
INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,
end_date_showFlag--,comments
)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),ISNULL(@end_date,null)

SELECT  @document_id=@@IDENTITY
*/

/*RETURN dbo.sp_web_egrants_inst_files_upload_doc @org_id, @doc_id, @category_id, @file_type,
@start_date,
@end_date
*/
----update url
--UPDATE dbo.Org_Document 
--SET url='/data/funded/nci/institutional/'+convert(varchar,@document_id)+'.'+@file_type
--WHERE document_id=@document_id

------return info
--SET @X = (
--SELECT document_id AS doc_id,url
--FROM dbo.Org_Document AS new_doc
--WHERE document_id=@document_id 
--FOR XML AUTO, TYPE, ELEMENTS
--)

----return xml file***/
--select @xmlout = cast(@X as varchar(max))
--select @xmlout

--GOTO load_org
DECLARE @comments varchar(256)
SET @comments = null
RETURN exec dbo.sp_web_egrants_inst_files_upload_doc @org_id, @doc_id, @category_id, @file_type, @start_date, @end_date, @comments
                                                      
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------
/*
Missing Index Details from SQLQuery26.sql - NCIDB-Q389-V\MSSQLEGRANTSQ,53500.EIM (NIH\shellba (57))
The Query Processor estimates that implementing the following index could improve the query cost by 22.1327%.
*/


USE [EIM]
GO

DROP INDEX IF EXISTS  Docs_Index_by_disabled
ON dbo.documents;
GO

CREATE NONCLUSTERED INDEX Docs_Index_by_disabled
ON [dbo].[documents] ([disabled_date])
INCLUDE ([appl_id])
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

/*
The Query Processor estimates that implementing the following index could improve the query cost by 17.0442%.
*/

USE [EIM]
GO

DROP INDEX IF EXISTS  IDX_Grant_PD_GD
ON [dbo].[Grant_Contacts_PD_GS]
GO

CREATE NONCLUSTERED INDEX IDX_Grant_PD_GD
ON [dbo].[Grant_Contacts_PD_GS] ([SERIAL_NUM],[ADMIN_PHS_ORG_CODE])
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------
USE [EIM]
GO

DROP INDEX IF EXISTS  IDX_ORG_MASTER
ON [dbo].[Org_Master]
GO

CREATE NONCLUSTERED INDEX IDX_ORG_MASTER
ON [dbo].[Org_Master] ([Org_name])
GO

-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

USE [EIM]
GO
DROP INDEX IF EXISTS  IDX_IMPP_CLOSEOUT
ON [dbo].[IMPP_CloseOut_Notification_All]
GO


CREATE NONCLUSTERED INDEX IDX_IMPP_CLOSEOUT
ON [dbo].[IMPP_CloseOut_Notification_All] ([appl_id])

GO
-----------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------

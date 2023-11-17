-- docservice_once_applid

if NOT exists (select * from EnvUrl where name like 'docservice_once_applid' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'docservice_once_applid', 'docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_once_applid' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'docservice_once_applid', 'docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_once_applid' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'docservice_once_applid', 'docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_once_applid' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'docservice_once_applid', 'docservice/dataservices/document/once/applId/')
END



-- docservice_data_keyid

if NOT exists (select * from EnvUrl where name like 'docservice_data_keyid' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'docservice_data_keyid', 'docservice/dataservices/document/once/keyId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_data_keyid' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'docservice_data_keyid', 'docservice/dataservices/document/once/keyId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_data_keyid' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'docservice_data_keyid', 'docservice/dataservices/document/once/keyId/')
END

if NOT exists (select * from EnvUrl where name like 'docservice_data_keyid' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'docservice_data_keyid', 'docservice/dataservices/document/once/keyId/')
END



-- s2server_era_nih

if NOT exists (select * from EnvUrl where name like 's2server_era_nih' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 's2server_era_nih', 'https://services.internal.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 's2server_era_nih' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 's2server_era_nih', 'https://services.internal.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 's2server_era_nih' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 's2server_era_nih', 'https://services.internal.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 's2server_era_nih' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 's2server_era_nih', 'https://services.internal.era.nih.gov/')
END


-- apps_era_nih

if NOT exists (select * from EnvUrl where name like 'apps_era_nih' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'apps_era_nih', 'https://apps.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 'apps_era_nih' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'apps_era_nih', 'https://apps.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 'apps_era_nih' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'apps_era_nih', 'https://apps.era.nih.gov/')
END

if NOT exists (select * from EnvUrl where name like 'apps_era_nih' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'apps_era_nih', 'https://apps.era.nih.gov/')
END



-- egrants_img

if NOT exists (select * from EnvUrl where name like 'egrants_img_dev' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'egrants_img_dev', 'https://egrants-web-dev.')
END

if NOT exists (select * from EnvUrl where name like 'egrants_img_test' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'egrants_img_test', 'https://egrants-web-test.')
END

if NOT exists (select * from EnvUrl where name like 'egrants_img_stg' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'egrants_img_stg', 'https://egrants-web-stage.')
END

if NOT exists (select * from EnvUrl where name like 'egrants_img_prod' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'egrants_img_prod', 'https://egrants.')
END



--data funded main

if NOT exists (select * from EnvUrl where name like 'data_funded2_main' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'data_funded2_main', 'data/funded2/nci/main/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'data_funded2_main', 'data/funded2/nci/main/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'data_funded2_main', 'data/funded2/nci/main/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'data_funded2_main', 'data/funded2/nci/main/')
END

		


--data funded main 1

if NOT exists (select * from EnvUrl where name like 'data_funded2_main1' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 'data_funded2_main1', 'data/funded2/nci/main1/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main1' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 'data_funded2_main1', 'data/funded2/nci/main1/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main1' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 'data_funded2_main1', 'data/funded2/nci/main1/')
END

if NOT exists (select * from EnvUrl where name like 'data_funded2_main1' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 'data_funded2_main1', 'data/funded2/nci/main1/')
END



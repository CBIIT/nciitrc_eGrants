-- s2server_eranih_doc

if NOT exists (select * from EnvUrl where name like 's2server_eranih_doc' and ServerName = 'NCIDB-D387-V\MSSQLEGRANTSD')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-D387-V\MSSQLEGRANTSD', 's2server_eranih_doc', 'https://services.internal.era.nih.gov/docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 's2server_eranih_doc' and ServerName = 'NCIDB-Q389-V\MSSQLEGRANTSQ')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-Q389-V\MSSQLEGRANTSQ', 's2server_eranih_doc', 'https://services.internal.era.nih.gov/docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 's2server_eranih_doc' and ServerName = 'NCIDB-S390-V\MSSQLEGRANTSS')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-S390-V\MSSQLEGRANTSS', 's2server_eranih_doc', 'https://services.internal.era.nih.gov/docservice/dataservices/document/once/applId/')
END

if NOT exists (select * from EnvUrl where name like 's2server_eranih_doc' and ServerName = 'NCIDB-P391-V\MSSQLEGRANTSP')
BEGIN
	INSERT INTO EnvURL (ServerName, Name, Url) Values
	('NCIDB-P391-V\MSSQLEGRANTSP', 's2server_eranih_doc', 'https://services.internal.era.nih.gov/docservice/dataservices/document/once/applId/')
END


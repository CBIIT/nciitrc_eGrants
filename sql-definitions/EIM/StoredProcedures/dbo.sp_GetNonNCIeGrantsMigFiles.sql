SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_GetNonNCIeGrantsMigFiles] 
AS
BEGIN
DECLARE @cmd varchar(4000)
DECLARE @Dt varchar(20)
DECLARE @stmp varchar(50)
declare @Query varchar(2000)

set @stmp=getdate()
set @stmp=replace(@stmp,' ','')
set @stmp=replace(@stmp,':','_')

-- NIEHS
DELETE dbo.temp_NIEHS_migration
insert into dbo.temp_NIEHS_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=6  
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --19067
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIEHSMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIEHS_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIEHS_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIEHSMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIEHS_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIEHS_FILES_Restore__' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

-- NIBIB
DELETE dbo.temp_NIBIB_migration
insert into dbo.temp_NIBIB_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=2 --NIBIB  
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --45583
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIBIBMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIBIB_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIBIB_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIBIBMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIBIB_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIBIB_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd


-- NLM
DELETE dbo.temp_NLM_migration
insert into dbo.temp_NLM_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=7 --NLM  
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NLMMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NLM_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NLM_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NLMMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NLM_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NLM_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

-- NIGMS
DELETE dbo.temp_NIGMS_migration
insert into dbo.temp_NIGMS_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=3 --NIGMS  
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIGMSMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIGMS_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIGMS_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIGMSMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIGMS_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIGMS_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

-- NIDDK
DELETE dbo.temp_NIDDK_migration
insert into dbo.temp_NIDDK_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=4 --NIDDK 
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIDDKMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIDDK_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIDDK_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIDDKMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIDDK_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIDDK_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

-- NHLBI
DELETE dbo.temp_NHLBI_migration
insert into dbo.temp_NHLBI_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=5 --NHLBI
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NHLBIMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NHLBI_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NHLBI_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NHLBIMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NHLBI_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NHLBI_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd


-- NICHD
DELETE dbo.temp_NICHD_migration
insert into dbo.temp_NICHD_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=8 --NICHD
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NICHDMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NICHD_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NICHD_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NICHDMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NICHD_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NICHD_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

-- NIAID
DELETE dbo.temp_NIAID_migration
insert into dbo.temp_NIAID_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=9 --NIAID
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIAIDMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIAID_migration'
print @query
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIAID_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
print @cmd
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIAIDMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIAID_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIAID_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd


-- NIMH
DELETE dbo.temp_NIMH_migration
insert into dbo.temp_NIMH_migration
select full_grant_num,dbo.fn_cleanspecial_char(category_name),document_id
,full_grant_num +'_'+ dbo.fn_cleanspecial_char(category_name)+'_'+convert(varchar,document_id,101)+'.'+convert(varchar,file_type,101) COLLATE SQL_Latin1_General_CP1_CI_AS as zpflnm
,replace(url,'https://egrants-data.nci.nih.gov','/egrants') as uxflnm
from egrants
where profile_id=10 --NIMH
and disabled_date is null
and url like 'https://egrants-data.nci.nih.gov/%' --2004
order by 1,2,3

SET @query='Select ''mv '' +uxflnm+ '' /egrants/NIMHMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''') from EIM.dbo.temp_NIMH_migration'
print @query
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIMH_FILES_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
print @cmd
EXEC master..xp_cmdshell @cmd
--To Restore
SET @query='Select ''mv /egrants/NIMHMIG/final/''+replace(right(zipflnm,len(zipflnm)-1),'' '','''')+'' ''+uxflnm from EIM.dbo.temp_NIMH_migration'
SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\NIMH_FILES_Restore_' +  @stmp + '.sh -c -T -S ' + @@SERVERNAME;
EXEC master..xp_cmdshell @cmd

END

GO


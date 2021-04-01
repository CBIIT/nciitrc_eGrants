SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
--to be deleted from system (Imran 8/21/2012)
CREATE PROCEDURE [dbo].[sp_egrants_maint_SpreadSheet_Conversion]

AS

BEGIN

DECLARE @sql 		varchar(4000)
DECLARE @sqlawdappl varchar(4000)
DECLARE @cmd 		varchar(4000)
DECLARE @cmdawdappl varchar(4000)
DECLARE @Dt			 varchar(20)
DECLARE @linkServer		varchar(10)
DECLARE @stmp 		varchar(50)

--SELECT @Dt=convert(varchar(22),last_run_date,101) from scripts where script='ObligatedSS'
--SET @Dt= char(39) + @Dt + char(39)
--SET @sql = 'SELECT APPL_ID FROM GM_ACTION_QUEUE_VW WHERE appl_status_group_code=''''A'''' AND ADMIN_PHS_ORG_CODE=''''CA'''' AND TO_DATE(obligation_date,''''DD-MON-RR'''') > TO_DATE(''' + @Dt +''',''''MM/DD/YYYY'''')'
--SET @sql = 'SELECT APPL_ID FROM GM_ACTION_QUEUE_VW WHERE appl_status_group_code=''''A'''' AND ADMIN_PHS_ORG_CODE=''''CA'''' AND TO_DATE(obligation_date,''''DD-MON-RR'''') between TO_DATE(''''09/01/2009'''',''''MM/DD/YYYY'''') and TO_DATE(''''09/28/2009'''',''''MM/DD/YYYY'''')'

--5/25/2011: Imran: Create Bundle info for new awarded appl
--1/15/2013 Commenting out the following code as this is not needed
--Exec eim.dbo.sp_CreateBundle

--5/26/2011:Imran: We have noticed some spread sheets drop off the normal 
--conversion process,AND SHOWUP IN BUNDLE lets CONVERT THEM
--1/15/2013 Commenting out the following code as this is not needed
--EXEC eim.dbo.sp_LO_SpreadSheet_Conversion

--Get all spreadsheet who is bundled to be converted into pdf.

/*Imran : PIV Migration change 6/7/2014*/
/*
SET @cmd='Insert Temp_Obligated_SpreadSheet(command,shellcommand,document_id)'
SET @cmd=@cmd+' Select replace(d.url,''https://egrants-data.nci.nih.gov/'','''') as command
,''mv '' + replace(d.url,''https://egrants-data.nci.nih.gov/'',''/egrants/'') + '' /egrants/scripts/obligated_ss/'' as shellcommand
,d.document_id from documents d 
where d.parent_id is not null and d.category_id=6 and d.file_type LIKE ''xls%'' and d.profile_id=1 AND d.disabled_date is null '
--print @cmd
*/

SET @cmd='Insert Temp_Obligated_SpreadSheet(command,shellcommand,document_id)'
SET @cmd=@cmd+' Select replace(d.url,''https://egrants-data.nci.nih.gov/'','''') as command
,''mv '' + replace(d.url,''/data/'',''/egrants/'') + '' /egrants/scripts/obligated_ss/'' as shellcommand
,d.document_id from documents d 
where d.parent_id is not null and d.category_id=6 and d.file_type LIKE ''xls%'' and d.profile_id=1 AND d.disabled_date is null '
--print @cmd

DELETE Temp_Obligated_SpreadSheet

EXEC (@cmd)

SET @cmd=''
SET @stmp=getdate()
SET @stmp=replace(@stmp,' ','')
SET @stmp=replace(@stmp,':','_')
--SET @cmd='bcp "select shellcommand from eim..Temp_Obligated_SpreadSheet order by document_id Desc " queryout \\NCIDBPRD1\util2\scripts\ListAwarded_' +  @stmp + '.sh -c -T'
--SET @cmd='bcp "select shellcommand from eim..Temp_Obligated_SpreadSheet order by document_id Desc " queryout \\NCIDBPRD1\util\scripts\ListAwarded_' +  @stmp + '.sh -c -T'
SET @cmd='bcp "select shellcommand from eim..Temp_Obligated_SpreadSheet order by document_id Desc " queryout \\NCIDB-P133-V\util\scripts\ListAwarded_' +  @stmp + '.sh -c -T'
EXEC master..xp_cmdshell @cmd

--UPDATE scripts SET last_run_date=getdate() WHERE script='ObligatedSS'

END


GO


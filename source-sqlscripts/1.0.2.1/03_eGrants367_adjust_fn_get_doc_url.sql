USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_get_doc_url]    Script Date: 9/6/2023 1:22:57 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER   FUNCTION [dbo].[fn_get_doc_url] (@document_id int, @IC varchar(5))

RETURNS varchar(800) AS 

BEGIN 

declare 
@local_image_server    varchar(100),
@impac_image_server                varchar(100),
@url                                                               varchar(800),
@doc_url                                                      varchar(800),
@impac_doc_type                                      varchar(10),
@created_by                                                varchar(20),
@created_date                             datetime,
@full_grant_num                                        varchar(20),
@fsr_id                                                          int,
@nga_rpt_seq_num                    int,
@appl_id                                                      int,
@nga_id                                                                      int,
@server_name   varchar(255),
@s2server_name varchar(255),
@sql_report_server varchar(255),
@era_server varchar(255)



set @server_name = @@SERVERNAME
set @IC=LOWER(@IC)

if @server_name = 'NCIDB-D387-V\MSSQLEGRANTSD'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_dev')+@IC+'.nih.gov/'   -- 'https://egrants-web-dev.'+@IC+'.nih.gov/'
set @impac_image_server=[dbo].[fn_get_env_url]('egrants_dev_impac')+@IC+'.nih.gov/' --   'https://i2e-dev.'+@IC+'.nih.gov/'
set @s2server_name =[dbo].[fn_get_env_url]('s2server_era_nih')   -- 'https://s2s.era.nih.gov/'
set @sql_report_server = 'https://ncidb-d387-v.nci.nih.gov/ReportServer_MSSQLEGRANTSD/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')  -- https://apps.era.nih.gov/'
END

IF @server_name = 'NCIDB-Q389-V\MSSQLEGRANTSQ'
BEGIN
set @local_image_server=  [dbo].[fn_get_env_url]('egrants_img_test')+@IC+'.nih.gov/' -- 'https://egrants-web-test.'+@IC+'.nih.gov/'   
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_test_impac')+@IC+'.nih.gov/' -- 'https://i2e-test.'+@IC+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih') -- 'https://s2s.era.nih.gov/'
set @sql_report_server = 'https://ncidb-q389-v.nci.nih.gov/ReportServer_MSSQLEGRANTSQ/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')   -- 'https://apps.era.nih.gov/'

END

IF @server_name = 'NCIDB-S390-V\MSSQLEGRANTSS'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_stg')++@IC+'.nih.gov/'      --'https://egrants-web-stage.'+@IC+'.nih.gov/'
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_stg_impac')+@IC+'.nih.gov/'      -- 'https://i2e-stage.'+@IC+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih') -- 'https://s2s.era.nih.gov/'
set @sql_report_server = 'https://ncidb-s390-v.nci.nih.gov/ReportServer_MSSQLEGRANTSS/'
set @era_server =[dbo].[fn_get_env_url]('apps_era_nih') --  'https://apps.era.nih.gov/'

END

IF @server_name = 'NCIDB-P391-V\MSSQLEGRANTSP'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_prod')+ @IC+'.nih.gov/'         --  'https://egrants.'+@IC+'.nih.gov/'
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_prod_impac')+@IC+'.nih.gov/'		-- 'https://i2e.'+@IC+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih')    -- 'https://s2s.era.nih.gov/'
set @sql_report_server = 'http://ncidb-p391-v.nci.nih.gov/ReportServer/Pages/ReportViewer.aspx?/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')   -- 'https://apps.era.nih.gov/'
END

SELECT 
@url=[url],
@appl_id=appl_id, 
@nga_id=nga_id,
@impac_doc_type=impac_doc_type,
@created_by=created_by, 
@created_date=created_date,
@full_grant_num=full_grant_num,
@nga_rpt_seq_num=nga_rpt_seq_num
FROM egrants 
WHERE document_id=@document_id

IF @url is not null 
BEGIN
              IF (substring(@url,1,5)='data/') SET @doc_url= @local_image_server + @url 
              IF (substring(@url,1,5)='/data') SET @doc_url= @local_image_server + SUBSTRING(@url,2,1000)
              IF @url like @s2server_name +'%' SET @doc_url = @url
              IF (select PATINDEX('%documentviewer%',@url))>0 SET @doc_url = @url
END

IF @url is null
BEGIN

--IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @IC='nci') SET @doc_url=@s2server_name+'docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type
IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @IC='nci') SET @doc_url=@s2server_name+ dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type
--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007' and @IC='nci') SET @doc_url=@s2server_name+'docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + 'NGA'
IF (@impac_doc_type='ENG' and @created_date<'04/10/2007' and @IC='nci') SET @doc_url=@s2server_name+ dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + 'NGA'

--IF (@impac_doc_type='ENG' and @IC='nci') SET @doc_url=@s2server_name+'docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type


--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @doc_url='https://businessobjects-sg-dev.nci.nih.gov/BOE/OpenDocument/1211012223/CrystalReports/viewrpt.cwr?apspassword=egrants1234&id=30939&apsuser=egrants&apsauthtype=secEnterprise&prompt0='+convert(varchar,@nga_id)+'&promptOnRefresh=1&wid=9b60a3fc9d06464e'
--                                                                                         NGAReports/getNGAfromNGAid&ngaid=677076
--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @doc_url= @sql_report_server+'NGAReports/getNGAfromNGAid&ngaid='+convert(varchar,@nga_id)

----PRAM Documents,MYP Documents,Final Invention Statement eAddition
--IF @impac_doc_type in('FSR','AWS','PRM','MYP','CLD','WBR','PRACPC','PRANCE','PRACOV') SET @doc_url=@s2server_name + 'docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type
IF @impac_doc_type in('FSR','AWS','PRM','MYP','CLD','WBR','PRACPC','PRANCE','PRACOV') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type

----Application File,NOA,Final Invention Statement,FPA Documents,FRAM Closeout Documents,Progress Final,JIT, PRAM Documents,Summary Statement,
--IF @impac_doc_type in ('IGI','FIS','FPA','FRM','FPR','JIT','MPR','SS','FRPPR','IRPPR','IPA') SET @doc_url=@s2server_name + 'docservice/dataservices/document/once/applId/' + convert(varchar,@appl_id) + '/' + @impac_doc_type
IF @impac_doc_type in ('IGI','DMS','FIS','FPA','FRM','FPR','JIT','MPR','SS','FRPPR','IRPPR','IPA') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_once_applid') + convert(varchar,@appl_id) + '/' + @impac_doc_type

---Greensheet PGM, Greensheet Spec,Greensheet DMC
IF @impac_doc_type in('DM','PGM','SPEC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&EXTERNAL=TRUE&APPL_ID=' + CONVERT(varchar, @appl_id)+ '&GRANT_ID='+@full_grant_num ---COLLATE DATABASE_DEFAULT 

----NEW PRAM, IRAM
--IF @impac_doc_type in('PRM_NEW','IRAM','FRM_NEW') SET @doc_url=@s2server_name + 'docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/CORAM'
IF @impac_doc_type in('PRM_NEW','IRAM','FRM_NEW') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/CORAM'

-- NEW 
IF @impac_doc_type in ('PRAM') SET @doc_url=@era_server + 'docservice/DocService/GetDocument?docType=RAM&keyId=' + convert(varchar,@nga_rpt_seq_num)

---Greensheet Rev
IF @impac_doc_type in('REV') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'

---Program Revision Greensheet 
IF @impac_doc_type in('PGMREV') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'

---Greensheet ARC
IF @impac_doc_type in('ARC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)

---for era doc only
IF @impac_doc_type='FSR'
BEGIN
	select @fsr_id=(select max(fsr_id) from impp_fsrs_all where appl_id=@appl_id) ---else set @fsr_id=null
--	set @doc_url=@s2server_name + 'docservice/dataservices/document/once/keyId/'+convert(varchar,@fsr_id) +'/'+ @impac_doc_type
	set @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') +convert(varchar,@fsr_id) +'/'+ @impac_doc_type
END

END

RETURN @doc_url

END



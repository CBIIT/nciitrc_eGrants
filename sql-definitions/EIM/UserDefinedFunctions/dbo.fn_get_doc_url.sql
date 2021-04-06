SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE   FUNCTION [dbo].[fn_get_doc_url] (@document_id int, @IC varchar(5))

RETURNS varchar(800) AS 

BEGIN 

declare 
@locall_image_server	varchar(100),
@impac_image_server		varchar(100),
@url					varchar(800),
@doc_url				varchar(800),
@impac_doc_type			varchar(10),
@created_by				varchar(20),
@created_date			datetime,
@full_grant_num			varchar(20),
@fsr_id					int,
@nga_rpt_seq_num		int,
@appl_id				int,
@nga_id					int

set @IC=LOWER(@IC)
set @locall_image_server='https://egrants-web-dev.'+@IC+'.nih.gov/'
set @impac_image_server='https://i2e-dev.'+@IC+'.nih.gov/'

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
	IF (substring(@url,1,5)='data/' or substring(@url,1,5)='/data') SET @doc_url= @locall_image_server + @url 
	IF (substring(@url,1,23)='https://s2s.era.nih.gov') SET @doc_url = @url
	IF (select PATINDEX('%documentviewer%',@url))>0 SET @doc_url = @url
END

IF @url is null
BEGIN

IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @IC='nci') SET @doc_url='https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type

--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @doc_url='https://businessobjects-sg-dev.nci.nih.gov/BOE/OpenDocument/1211012223/CrystalReports/viewrpt.cwr?apspassword=egrants1234&id=30939&apsuser=egrants&apsauthtype=secEnterprise&prompt0='+convert(varchar,@nga_id)+'&promptOnRefresh=1&wid=9b60a3fc9d06464e'
IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @doc_url='https://ncidb-d201-v.nci.nih.gov/ReportServer_MSSQLEGRANTSP/Pages/ReportViewer.aspx?%2fNGAReports%2fgetNGAfromNGAid&rs:Command=Render&ngaid='+convert(varchar,@nga_id)

----PRAM Documents,MYP Documents,Final Invention Statement eAddition
IF @impac_doc_type in('FSR','AWS','PRM','MYP','CLD','WBR','PRACPC','PRANCE','PRACOV') SET @doc_url='https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/' + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type

----Application File,NOA,Final Invention Statement,FPA Documents,FRAM Closeout Documents,Progress Final,JIT, PRAM Documents,Summary Statement,
IF @impac_doc_type in ('IGI','FIS','FPA','FRM','FPR','JIT','MPR','SS','FRPPR','IRPPR','IPA') SET @doc_url='https://s2s.era.nih.gov/docservice/dataservices/document/once/applId/' + convert(varchar,@appl_id) + '/' + @impac_doc_type

---Greensheet PGM, Greensheet Spec,Greensheet DMC
IF @impac_doc_type in('DM','PGM','SPEC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&EXTERNAL=TRUE&APPL_ID=' + CONVERT(varchar, @appl_id)+ '&GRANT_ID='+@full_grant_num ---COLLATE DATABASE_DEFAULT 

---Greensheet Rev
IF @impac_doc_type in('REV') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'

---Greensheet ARC
IF @impac_doc_type in('ARC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)

---for era doc only
IF @impac_doc_type='FSR'
BEGIN
	select @fsr_id=(select max(fsr_id) from impp_fsrs_all where appl_id=@appl_id) ---else set @fsr_id=null
	set @doc_url='https://s2s.era.nih.gov/docservice/dataservices/document/once/keyId/'+convert(varchar,@fsr_id) +'/'+ @impac_doc_type
END

END

RETURN @doc_url

END

GO


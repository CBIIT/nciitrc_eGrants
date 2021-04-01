SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_egrants_impac_doc_url_tobedeleted]

@image_server	varchar(100),
@docid			int,
@IC				varchar(5)
/************************************************************************************************************/
/***																***/
/***  Procedure Name:  sp_y2013_egrants_doc_url_nci					***/
/***  Description:  find the url by document_id for NCI				***/
/***  Created:		01/28/2012  Leon								***/
/***  Modified:		04/25/2015  Leon				   				***/
/***  Modified:		02/09/2012 Imran FPR and FIS					***/
/***  Modified:		02/07/2014 Leon return nga_rpt_seq_num			***/
/***  Modified:		09/03/2015 Leon return rul						***/
/************************************************************************************************************/

AS

declare 
@url				varchar(800),
@impac_doc_type		varchar(10),
@created_by			varchar(20),
@created_date		datetime,
@full_grant_num		varchar(20),
@fsr_id				int,
@nga_rpt_seq_num	int,
@appl_id			int,
@nga_id				int

SET @IC=LOWER(@IC)

SELECT 
@appl_id=appl_id, 
@nga_id=nga_id,
@impac_doc_type=impac_doc_type,
@created_by=created_by, 
@created_date=created_date,
@full_grant_num=full_grant_num,
@nga_rpt_seq_num=nga_rpt_seq_num
FROM egrants WHERE document_id=@docid

if @impac_doc_type='FSR' select @fsr_id=(select max(fsr_id) from impp_fsrs_all where appl_id=@appl_id) else set @fsr_id=null

IF @created_by='impac' 
BEGIN
----NGA
IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @IC='nci') SET @url=@image_server+'documentviewer/viewEgrantDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar,@nga_rpt_seq_num)

IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @IC<>'nci') SET @url='https://egrants.'+@IC+'.nih.gov/documentviewer/viewEgrantDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar, @nga_rpt_seq_num) 
--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @url='https://businessobjects-sg.nci.nih.gov:8443/BOE/CrystalReports/viewrpt.cwr?id=30939&promptOnRefresh=1&apsuser=egrants&apspassword=egrants1234&apsauthtype=secEnterprise&prompt0='+convert(varchar,@nga_id)+'&promptOnRefresh=1&wid=9b60a3fc9d06464e'
--IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @url='https://businessobjects-sg.nci.nih.gov/BOE/OpenDocument/1211012223/CrystalReports/viewrpt.cwr?apspassword=egrants1234&id=209441&apsuser=egrants&apsauthtype=secEnterprise&prompt0='+convert(varchar,@nga_id)+'&promptOnRefresh=1&wid=9b60a3fc9d06464e'
IF (@impac_doc_type='ENG' and @created_date<'04/10/2007') SET @url='https://ncidb-p232-v.nci.nih.gov/ReportServer_MSSQLEGRANTSP/Pages/ReportViewer.aspx?%2fNGAReports%2fgetNGAfromNGAid&rs:Command=Render&ngaid='+convert(varchar,@nga_id)
--https://ncidb-p232-v.nci.nih.gov/ReportServer_MSSQLEGRANTSP/Pages/ReportViewer.aspx?%2fNGAReports%2fgetNGAfromNGAid&rs:Command=Render&ngaid=554647
----Financial Report (fsr) 
IF @impac_doc_type='FSR' SET @url=@image_server+'documentviewer/viewDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&fsrId='+convert(varchar, @fsr_id)

---Award Worksheet Report for nci
--IF (@impac_doc_type='AWS' and @created_date>'07/23/2010' and @IC='nci') SET @url=@image_server+'documentviewer/viewEgrantDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar,@nga_rpt_seq_num)

--Award Worksheet Report for no nci
--IF (@impac_doc_type='AWS' and @created_date>'07/23/2010' and @IC<>'nci') SET @url='https://egrants.'+@IC+'.nih.gov/documentviewer/viewEgrantDocument.action?applId=' +convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar,@nga_rpt_seq_num)

---Award Worksheet Report for nci
IF (@impac_doc_type='AWS' and @IC='nci') SET @url=@image_server+'documentviewer/viewEgrantDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar,@nga_rpt_seq_num)

--Award Worksheet Report for no nci
IF (@impac_doc_type='AWS' and @IC<>'nci') SET @url='https://egrants.'+@IC+'.nih.gov/documentviewer/viewEgrantDocument.action?applId=' +convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&rptSeqNum='+CONVERT(varchar,@nga_rpt_seq_num)

----PRAM Documents,MYP Documents,Final Invention Statement eAddition
IF @impac_doc_type in('PRM','MYP','CLD','WBR','PRACPC','PRANCE','PRACOV') SET @url=@image_server+'documentviewer/viewEgrantDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type+'&keyId='+CONVERT(varchar,@nga_rpt_seq_num)

----Application File,NOA,Final Invention Statement,FPA Documents,FRAM Closeout Documents,Progress Final,JIT, PRAM Documents,Summary Statement,
IF @impac_doc_type in ('IGI','FIS','FPA','FRM','FPR','JIT','MPR','SS','FRPPR','IRPPR','IPA') SET @url=@image_server+'documentviewer/viewDocument.action?applId='+convert(varchar,@appl_id)+'&docType='+@impac_doc_type

---Greensheet PGM, Greensheet Spec,Greensheet DMC
IF @impac_doc_type in('DM','PGM','SPEC') SET @url=@image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&EXTERNAL=TRUE&APPL_ID=' + CONVERT(varchar, @appl_id)+ '&GRANT_ID='+@full_grant_num ---COLLATE DATABASE_DEFAULT 

---Greensheet Rev
IF @impac_doc_type in('REV') SET @url=@image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'

END	
ELSE
BEGIN
SET @url=@image_server
END

--select  @impac_doc_type,@created_date, @nga_rpt_seq_num, @url,@ic
SELECT @url as url


GO


SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_egrants_doc_url_restore_tobedeleted]

@DocID int

AS
/************************************************************************************************************/
/***									 											***/
/***	Procedure Name:  sp_y2013_egrants_url_restore									***/
/***	Description:	restore url by document_id after file uploaded false		***/
/***	Created:	05/21/2004	Dan													***/
/***	Modified:	03/25/2013	Leon												***/
/***																				***/
/************************************************************************************************************/
SET NOCOUNT ON

declare @CreatedBy 	varchar(8)
declare @FType 	varchar(4)
declare @URL 		varchar(300)
declare @Category 	varchar (50)
declare @ApplID 	int


SELECT @CreatedBy=created_by, @FType=file_type, @Category=category_name, @ApplID=appl_id FROM egrants WHERE document_id=@DocID

SET @url=
CASE 
WHEN @Category='Application File' and @CreatedBy in ('impac', 'efile') 
THEN NULL--'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=IGI&parameter='  + convert(varchar, @ApplID)

WHEN @Category='Summary Statement' and @CreatedBy='impac' 
THEN NULL--'http://appsprd.era.nih.gov/eraservices/docservice/viewDocument.do?docType=SSP&parameter=' + convert(varchar,@ApplID)

WHEN @Category = 'Financial Report' and @CreatedBy in ('impac', 'efile') 
THEN NULL

WHEN @Category = 'JIT Info' and @CreatedBy in ('impac', 'efile') 
THEN NULL

WHEN @DocID between 23316776 and 23999999 and @CreatedBy not in ('impac', 'efile') 
/*Imran : PIV Migration change 6/7/2014*/
--THEN 'https://egrants-data.nci.nih.gov/funded2/nci/main1/' + convert(varchar,@DocID) + '.' + @FType
THEN 'data/funded2/nci/main1/' + convert(varchar,@DocID) + '.' + @FType

WHEN @DocID>=24000000 and @CreatedBy not in ('impac', 'efile') 
/*Imran : PIV Migration change 6/7/2014*/
--THEN 'https://egrants-data.nci.nih.gov/funded2/nci/main/' + convert(varchar,@DocID) + '.' + @FType
THEN 'data/funded2/nci/main/' + convert(varchar,@DocID) + '.' + @FType

--added by leon 6/30/2017
WHEN @Category='No Cost Extension' and @CreatedBy='CA ERA NOTIFICATIONS'
THEN  'data/funded2/nci/main1/' + convert(varchar,@DocID) + '.txt' 

ELSE
'cannot restore'
END

IF @url<>'cannot restore'
UPDATE documents 
SET url=@url,qc_date=null			
WHERE document_id=@DocID
GO


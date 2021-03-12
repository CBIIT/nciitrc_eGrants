SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_egrants_doc_url_restore]

@DocID int

AS
/************************************************************************************************************/
/***									 											***/
/***	Procedure Name:  sp_web_egrants_url_restore									***/
/***	Description:	restore url by document_id after file uploaded false		***/
/***	Created:	05/21/2004	Dan													***/
/***	Modified:	03/25/2013	Leon												***/
/***	Modified:	06/30/2017	Leon												***/
/***	Modified:	05/15/2019	Leon add supplement doc restore function			***/
/************************************************************************************************************/
SET NOCOUNT ON

declare @CreatedBy 			varchar(8)
declare @FType 				varchar(4)
declare @url 				varchar(300)
declare @Category 			varchar (50)
declare @ApplID 			int
declare @supp_backup_url	varchar(300)

--added by Leon 5/15/2019
set @supp_backup_url= (select doc_url from IMPP_Admin_Supplements_WIP where moved_date is not null and movedto_document_id = @DocID)

SELECT @CreatedBy=created_by, @FType=file_type, @Category=category_name, @ApplID=appl_id 
FROM egrants WHERE document_id=@DocID

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
THEN 'data/funded2/nci/main1/' + convert(varchar,@DocID) + '.' + @FType

WHEN @DocID>=24000000 and @CreatedBy not in ('impac', 'efile') and @supp_backup_url is null
THEN 'data/funded2/nci/main/' + convert(varchar,@DocID) + '.' + @FType

--added by leon 5/15/2019
WHEN @DocID>=24000000 and @supp_backup_url is not null
THEN @supp_backup_url

--added by leon 6/30/2017
WHEN @Category='No Cost Extension' and @CreatedBy='CA ERA NOTIFICATIONS'
THEN 'data/funded2/nci/main1/' + convert(varchar,@DocID) + '.txt' 

ELSE
'cannot restore'
END

IF @url<>'cannot restore'
UPDATE documents 
SET url=@url,qc_date=null, file_modified_by_person_id=null,file_modified_date=null			
WHERE document_id=@DocID

--to update file type after supplement document restored (added by Leon 5/15/2019)
IF @supp_backup_url is not null
BEGIN
set @FType = (select file_type from IMPP_Admin_Supplements_WIP where movedto_document_id=@DocID and moved_date is not null )
update documents set file_type=@FType where document_id=@DocID 
END
GO


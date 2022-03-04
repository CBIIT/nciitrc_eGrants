SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE    PROCEDURE [dbo].[sp_web_egrants_inst_files_upload_doc]
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
/***	Procedure Name:sp_web_egrants_inst_files_disable_doc     			***/
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

print 'sp_web_egrants_inst_files_upload_doc'

SELECT  @document_id=@@IDENTITY

RETURN

GO


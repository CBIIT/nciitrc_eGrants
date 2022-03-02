SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



Create    PROCEDURE [dbo].[sp_web_egrants_inst_files_disable_doc]
(
@person_id	int,
@doc_id		int
)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_disable_doc     			***/
/***	Description:files disable doc 										***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON

-------------------
-- disable_doc:

update dbo.Org_Document set disabled_date=GETDATE(), disabled_by_person_id=@person_id where document_id=@doc_id	

RETURN


GO


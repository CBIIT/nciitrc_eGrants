SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON

create PROCEDURE [dbo].[sp_web_egrants_doc_error]

@error		varchar(500),
@docid		int,
@ic 		varchar(10),
@operator	varchar(50)


AS
/************************************************************************************************************/
/***									 							***/
/***	Procedure Name:  sp_web_egrants_doc_error					***/
/***	Description:	document error report						***/
/***	Created:	12/01/2004		Leon							***/
/***	Modified:	03/27/2013		Leon							***/
/***	Modified:	09/01/2016		Frances  Update for MVC			***/
/************************************************************************************************************/

SET NOCOUNT ON

---find userinfo
DECLARE @person_id 	int
SET @person_id = (SELECT person_id FROM people WHERE userid=@operator)

IF @error is not null and @error<>''
BEGIN
UPDATE documents 
SET problem_msg=@error, problem_reported_by_person_id=@person_id, qc_reason='Error', qc_date=getdate(), stored_date=null	
WHERE document_id=@docid

/**insert document transaction information**/
---EXEC sp_web_egrants_doc_transaction @docid, @operator, 'error_reported', @error
END

GO


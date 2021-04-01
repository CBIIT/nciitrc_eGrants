SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
create PROCEDURE [dbo].[sp_y2013_egrants_doc_transaction_tobedeleted]

@document_id		int,
@operator 			varchar(50),
@action				varchar(20),
@description		varchar(100)=null

AS
/************************************************************************************************************/
/***									 								***/
/***	Procedure Name: sp_2013_egrants_doc_transaction					***/
/***	Description:	record doc transaction history					***/
/***	Created:	05/27/2005	Leon									***/
/***	Modified:	03/27/2013	Leon									***/
/***																	***/
/************************************************************************************************************/

SET NOCOUNT ON

INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description) 
SELECT @document_id, @operator, @action, full_grant_num, category_name, document_date, @description	
FROM egrants 
WHERE document_id=@document_id
GO


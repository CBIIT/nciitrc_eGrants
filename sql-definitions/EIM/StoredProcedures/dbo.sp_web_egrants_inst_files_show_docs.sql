SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE   PROCEDURE [dbo].[sp_web_egrants_inst_files_show_docs]
@org_id	int

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_show_docs					***/
/***	Description:show docs files											***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON
-------------------
-- show_docs:

-- comments reqd
SELECT org_id,org_name,document_id,category_name, url,[start_date],end_date,created_date, comments
FROM dbo.vw_Org_Document 
WHERE org_id=@org_id 


RETURN

GO


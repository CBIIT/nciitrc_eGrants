SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF


CREATE PROCEDURE [dbo].[sp_web_egrants_funding_docs]

@act 		varchar(50),
@serial_num	int,
@fy			int,
@ic			varchar(10),
@Operator	varchar(50)

AS
/****************************************************************************************************************/
/***									 								***/
/***	Procedure Name: sp_web_egrants_funding_docs						***/
/***	Description:return master funding documents						***/
/***	Created:	12/22/2009	Leon									***/
/***	Modified:	11/11/2013	Leon									***/
/***	Simplified:	10/16/2014	Leon									***/
/***	Simplified:	09/23/2016	Leon	modified for MVC				***/
/****************************************************************************************************************/

SET NOCOUNT ON

IF @act='view_search' and (@serial_num is not null and @serial_num>0) GOTO view_search
IF @act='view_all'	GOTO view_all
IF @act='view_arra'	GOTO view_arra
IF @act='view_edit'	GOTO view_edit

-------------------------------------------------------
view_all:

---return all funding documents
SELECT 
document_id, 
category_id, 
category_name,
document_fy,
doc_label, 
url, 
isnull(arra_flag,'n') as arra_flag, 
created_date, 
serial_num,
null as full_grant_num,
0 as appl_id  
FROM vw_funding_master 
WHERE document_fy = @fy

RETURN
----------------------------------------------------------
view_arra:

--return arra funding documents with arra flag
SELECT 
document_id, 
category_id, 
category_name,
document_fy,
doc_label, 
url, 
isnull(arra_flag,'n') as arra_flag, 
created_date, 
serial_num,
null as full_grant_num,
0 as appl_id 
FROM vw_funding_master 
WHERE document_fy = @fy and arra_flag='y' 

RETURN
--------------------------------------------------------------
view_search:

--return searching funding documents with searching serial num
SELECT 
document_id, 
category_id, 
category_name, 
document_fy,
doc_label, 
url, 
document_fy,
isnull(arra_flag,'n') as arra_flag, 
created_date, 
serial_num,
null as full_grant_num,
0 as appl_id 
FROM vw_funding_master 
WHERE document_fy = @fy and serial_num=@serial_num

RETURN
-------------------------------------------------------------------
view_edit:

--return funding documents to edit
SELECT 
document_id,
f.category_id,
CASE
WHEN f.sub_string is not null and f.sub_string <>'' THEN
f.category_name+"_"+ f.sub_string
ELSE f.category_name
END as category_name,
f.document_fy,
null as doc_label,
f.url,
isnull(f.arra_flag,'n') as arra_flag, 
convert(varchar, f.created_date,101) as created_date,
f.serial_num,
f.full_grant_num,
f.appl_id 
FROM vw_funding as f,vw_appls a
WHERE f.appl_id=a.appl_id and stored_date is null and f.disabled_date is null and f.document_fy=@fy


GO


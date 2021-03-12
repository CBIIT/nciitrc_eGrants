SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_egrants_doc_modify]

@act 			varchar(50),
@appl_id 		int,
@category_id 	int,
@sub_category	varchar(35),
@doc_date 		varchar(10),
@docid_str		varchar(800),
@file_type		varchar(5),
--@file_location	varchar(200),
--@specialist_id 	int,
@ic				varchar(10),
@operator		varchar(50)

AS
/************************************************************************************************************/
/***									 											***/
/***	Procedure Name: sp_egrants_doc_modify										***/
/***	Description:create document													***/
/***	Created:	10/27/2003	Leon												***/
/***	Modified:	11/12/2013	Leon add store_all & delete_all						***/
/***	Modified:	05/23/2017	Leon add @sub_category								***/
/***	Modified:	08/15/2017	Leon add @specialist_id								***/
/***	Modified:	10/15/2018	Leon remove @specialist_id							***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id			smallint,
@person_id			int,
@specialist_name	varchar(100),
@doc_id				int,
@count				int,
@sql				varchar(850),
@url		 		varchar(300)

/***find the profile_id and person_id**/
SET @profile_id =(SELECT profile_id FROM profiles WHERE [profile]=@ic) 
SET @person_id = (SELECT person_id FROM vw_people WHERE userid=@operator) 

IF @act='to store all' or @act='to delete all'
BEGIN
CREATE TABLE #doc (doc_id int)
SET @sql='INSERT #doc SELECT document_id FROM documents WHERE document_id in (' + @docid_str + ')'
EXEC (@sql)
END 
ELSE 
BEGIN
SET @doc_id =convert(int,@docid_str)
END 

IF @act='to_upload' GOTO to_upload
IF @act='to_update' GOTO to_update
IF @act='to store' GOTO to_store
IF @act='to delete' GOTO to_delete
IF @act='to store all' GOTO to_store_all
IF @act='to delete all' GOTO to_delete_all
IF @act='to restore' GOTO to_restore
--IF @act='to_route' GOTO to_route
------------------------------------------
to_upload:

SET @url='data/funded/nci/modify/'
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))

--to modify document index after image was upload and return new url
UPDATE documents 
SET url=REPLACE(@url+ convert(varchar,documents.document_id) + '.' +@file_type,' ',''), file_type=@file_type, file_modified_date=getdate(), file_modified_by_person_id=@person_id, qc_date=getdate(), qc_person_id=null,qc_reason='Change',profile_id=@profile_id, stored_date=null, stored_by_person_id=null
FROM documents
WHERE document_id=@doc_id

---insert document transaction information
EXEC sp_web_egrants_doc_transaction @doc_id,@Operator,'image_modified', null

RETURN 
-----------------------------------------------------------
--modify document index
to_update:	

IF @sub_category='null' or @sub_category='undefined' or @sub_category='' or @sub_category is null 
BEGIN
SET @sub_category = null
UPDATE documents SET sub_category_name = null WHERE document_id = convert(int,@doc_id)
END

--to update document's index only
SET @sql='UPDATE documents SET ' 
IF @appl_id IS NOT NULL and @appl_id <> '' SET @sql=@sql+'appl_id='+convert(varchar,@appl_id)+', parent_id=null, '
IF @category_id IS NOT NULL and @category_id <> '' SET @sql=@sql+'category_id=' + convert(varchar,@category_id)+', '
IF @sub_category IS NOT NULL and @sub_category<>'' SET @sql=@sql+'sub_category_name='+char(39)+@sub_category+char(39)+', '
IF @doc_date IS NOT NULL and @doc_date<>'' SET @sql=@sql+'document_date=' +char(39)+convert(varchar,@doc_date)+char(39)+', '

SET @sql=@sql+'qc_reason='+char(39)+'Change'+char(39)+',  qc_person_id=null , qc_date='+char(39)+convert(varchar,getdate())+char(39)+', '
SET @sql=@sql+'modified_date='+char(39)+convert(varchar,getdate())+char(39)+', index_modified_by_person_id='+convert(varchar,@person_id)+', ' 
SET @sql=@sql+'profile_id=' +convert(varchar,@profile_id)+', stored_date=null, stored_by_person_id=null '  
SET @sql=@sql+'WHERE document_id='+ convert(varchar,@doc_id)

--print @sql
EXEC (@sql)

---insert document transaction information
EXEC sp_web_egrants_doc_transaction @doc_id, @operator, 'index_modified', null		

--IF @specialist_id IS NOT NULL and @specialist_id<>'' and @specialist_id<>0 GOTO to_route
--IF @ToStore='yes' GOTO to_store								

RETURN
----------------------------------------------------------
to_store:

---to store document
UPDATE documents SET stored_date = getdate(), stored_by_person_id=@person_id, qc_date=null WHERE document_id=@doc_id

--insert document transaction information 
EXEC sp_web_egrants_doc_transaction @doc_id, @operator,'stored', null	

RETURN
--------------------------------------------------------
to_delete:

---to disable document
UPDATE documents SET disabled_date=getdate(), disabled_by_person_id=@person_id, qc_date=null, qc_person_id=null 
WHERE document_id=@doc_id

---insert document transaction information
EXEC sp_web_egrants_doc_transaction @doc_id, @operator,'disabled', null		

RETURN  
------------------------------------------------------------
to_store_all:

SET @count=(SELECT count(*) FROM #doc)

---to store all documents
UPDATE documents 
SET stored_date=getdate(),stored_by_person_id=@person_id,qc_date=null ---, qc_person_id=null,problem_msg=null, problem_reported_by_person_id=null,profile_id=@profile_id  
FROM  documents d, #doc  
WHERE d.document_id=#doc.doc_id ---and d.appl_id is not null 

---insert document transaction information
INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description) 
SELECT e.document_id, @operator, 'stored', full_grant_num, category_name, document_date, null
FROM  egrants e, #doc  
WHERE e.document_id=#doc.doc_id

RETURN
--------------------------------------------------------------
to_delete_all:

SET @count=(SELECT count(*) FROM #doc)

--to delete all documents
UPDATE documents 
SET disabled_date=getdate(),disabled_by_person_id=@person_id,qc_date=null,qc_person_id=null 
FROM  documents d, #doc  
WHERE d.document_id=#doc.doc_id ---and d.appl_id is not null 

--insert document transaction information
INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description) 
SELECT e.document_id, @operator, 'deleted', full_grant_num, category_name, document_date, null
FROM  egrants e, #doc  
WHERE e.document_id=#doc.doc_id

RETURN
-------------------------------------------------------
to_restore:

---to restore document url
EXEC sp_web_egrants_doc_url_restore @doc_id 

---insert document transaction information
EXEC sp_web_egrants_doc_transaction @doc_id, @Operator,'url_restored', null	

RETURN	
-------------------------------------------------------
--to_route:

--UPDATE documents 
--SET qc_person_id=@specialist_id, qc_date=getdate(), stored_date=null, stored_by_person_id=null 
--WHERE document_id =@doc_id

--SELECT @specialist_name ='to '+ person_name FROM people WHERE person_id=@specialist_id

--/**insert document transaction information**/
--EXEC sp_web_egrants_doc_transaction @doc_id, @operator, 'routed', @specialist_name

--RETURN
------------------------------------------------------------

GO


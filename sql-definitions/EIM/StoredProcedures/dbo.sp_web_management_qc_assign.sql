SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_management_qc_assign]

@act			varchar(50),
@person_id		int,
@qc_person_id	int,
@qc_reason		varchar(50),
@percent		int,
@ic				varchar(10),
@operator 		varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_web_egrants_qc_assign							***/
/***	Description:	assign documents to qc or to specialist				***/
/***	Created:	03/21/2014	Leon										***/
/***	Modified:	03/21/2014	Leon										***/
/***	QC bypass not applicable to this proc hareesh 4/9/14				***/
/***	Modified:	07/13/2016	Leon modified for MVC						***/
/************************************************************************************************************/

SET NOCOUNT ON

DECLARE 
@count			int,
@profile_id		smallint,
@person_name	varchar(50),
@qc_person_name	varchar(20),
@sql			varchar(500)

SET @profile_id=(select profile_id FROM profiles WHERE profile=@ic)

--IF @act is null or @act='' RETURN
IF @act='to_remove' GOTO to_remove
IF @act='to_assign' GOTO to_assign
IF @act='to_route' GOTO to_route
---------------
to_assign:

SET @count=(select COUNT(*) from quality_control where qc_reason=@qc_reason and person_id=@qc_person_id)

IF @count=0 
BEGIN
INSERT quality_control (qc_reason, person_id, effort) SELECT @qc_reason, @qc_person_id,100
END

RETURN
-------------------
to_remove:

DELETE FROM quality_control WHERE qc_reason=@qc_reason and person_id=@qc_person_id

RETURN 
----------------------
to_route:

/** find all documents to route**/
CREATE TABLE #doc (doc_id int)
SET @sql='insert #doc select top ' + convert(varchar,@percent)+ ' percent document_id from documents 
where profile_id='+convert(varchar,@profile_id)+' and qc_date is not null and parent_id is null and stored_date is null 
and qc_person_id='+convert(varchar,@person_id)+ ' order by qc_date'
EXEC (@sql)

--route documents
UPDATE documents SET qc_person_id=@qc_person_id, qc_date=getdate() WHERE document_id in (SELECT doc_id FROM #doc)

--insert document transaction info
SELECT @person_name=ISNULL(person_name,userid) FROM vw_people WHERE person_id=@person_id
SELECT @qc_person_name =ISNULL(person_name, userid) FROM vw_people WHERE person_id=@qc_person_id	

INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description) 
SELECT e.document_id, @operator, 'routed', full_grant_num, category_name, document_date, 'from '+ @person_name + ' to ' + @qc_person_name
FROM egrants e, #doc  WHERE e.document_id=#doc.doc_id

RETURN
--------------------------------------------------------------------
to_auto:

INSERT quality_control (qc_reason, person_id, effort)  SELECT @qc_reason, @qc_person_id,100 

RETURN


GO


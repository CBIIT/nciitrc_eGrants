SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE sp_egrants_reindex_modified

AS

set nocount on

declare @CDocID 		int
declare @t table(document_id int primary key)

insert @t
select document_id 
from documents 
where appl_id in (select appl_id from appls_ciip) or stamp>(select last_reindexed_doc from performance)

update performance
set last_reindexed_doc=(select max(stamp) from documents)

declare cur CURSOR for
select dt.document_id from ncieim_b..documents_text dt,@t t
where t.document_id=dt.document_id 
and (keywords<>dbo.fn_doc_keywords(dt.document_id)  OR keywords is NULL)

OPEN cur

FETCH NEXT FROM cur INTO @CDocID

WHILE @@Fetch_Status=0
BEGIN

EXEC sp_egrants_doc_index_modify @CDocID

FETCH NEXT FROM cur
INTO @CDocID

END

CLOSE cur
DEALLOCATE cur
GO


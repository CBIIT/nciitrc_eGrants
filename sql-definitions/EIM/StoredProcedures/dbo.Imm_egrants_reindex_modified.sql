SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[Imm_egrants_reindex_modified]

AS

set nocount on

declare @CDocID int
declare @tot int
declare @i int
declare @srl int
set @i=0

Truncate table imm_docs1
----select count(*) from imm_docs1--5999838

insert imm_docs1
select document_id 
from documents (nolock)
where disabled_date is null and (appl_id in (select appl_id from appls_ciip) or stamp>(select last_reindexed_doc from performance))


update performance
set last_reindexed_doc=(select max(stamp) from documents)

Truncate table imm_docs2
--select count(*) from imm_docs2 --372880

insert into Imm_docs2
select dt.document_id 
from ncieim_b..documents_text dt (nolock), imm_docs1 d 
where d.document_id=dt.document_id
and (dt.keywords<>dbo.fn_doc_keywords(dt.document_id)  OR dt.keywords is NULL)

--select count(*) from ncieim_b..documents_text where keywords is null

select @tot=max(id) from imm_docs2
--set @tot=100000

--declare cur CURSOR for  --424897
--select document_id from imm_docs3

--select top 100 a.document_id from imm_docs2 a, ncieim_b..documents_text b
--where a.document_id=b.document_id and b.keywords is null


--OPEN cur
--FETCH NEXT FROM cur INTO @CDocID
--WHILE @@Fetch_Status=0
while (@i < @tot)   --229k
BEGIN
	select @srl=id, @CDocID=document_id from imm_docs2 where id=@i
	
	EXEC sp_egrants_doc_index_modify @CDocID
	SET @i = @i + 1
	print cast(@i as varchar(10)) + ' OF ' + cast(@tot as varchar(10))
	
--FETCH NEXT FROM cur INTO @CDocID

END

--CLOSE cur
--DEALLOCATE cur
GO


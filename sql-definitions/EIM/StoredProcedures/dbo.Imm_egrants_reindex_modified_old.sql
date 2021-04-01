SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[Imm_egrants_reindex_modified_old]
AS

SET NOCOUNT ON

declare @CDocID int  --3898194  3898377  3899097
declare @tot int
declare @i int
declare @srl int
set @i=0

--Truncate table imm_docs1   --6143594
----select count(*) from imm_docs2--5999838 --6143594

--insert imm_docs1
--select document_id 
--from documents 
--where disabled_date is null and (appl_id in (select appl_id from appls_ciip) or stamp>(select last_reindexed_doc from performance))

--print 'Inserted into Imm_docs1=' + cast(@@ROWCOUNT as varchar)

--update performance
--set last_reindexed_doc=(select max(stamp) from documents)

--Truncate table imm_docs2
--select top 10* from imm_docs2 order by id desc--4948732

--insert into Imm_docs2
--select dt.document_id 
----select COUNT(*) --4372616
--from ncieim_b..documents_text dt, imm_docs1 d
--where d.document_id=dt.document_id
--and (dt.keywords<>dbo.fn_doc_keywords(dt.document_id)  OR dt.keywords is NULL)
--print 'Inserted into Imm_docs2=' + cast(@@ROWCOUNT as varchar)
----select count(*) from ncieim_b..documents_text where keywords is null

select @tot=max(id) from imm_docs2
select @i=4470695--3899097--3898061--3847600--3789471--3028767 --1259657--726856--584065  --min(id) from imm_docs2 --584065 
select @tot, @i
--select max(id) from imm_docs2
--select min(id) from imm_docs2
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
	If (select COUNT(*) from ncieim_b..documents_text where keywords<>dbo.fn_doc_keywords(@CDocID) and document_id=@CDocID) > 0 
	begin
	--select COUNT(*) from ncieim_b..documents_text where keywords<>dbo.fn_doc_keywords(35465477) and document_id=35465477
	EXEC sp_egrants_doc_index_modify @CDocID
	print 'KW Updated ' + cast(@i as varchar(10)) + ' OF ' + cast(@tot as varchar(10))
	end
	Delete imm_docs2 where id=@i
	print 'Deleted '+cast(@i as varchar(10)) + ' OF ' + cast(@tot as varchar(10))	
	SET @i = @i + 1
--FETCH NEXT FROM cur INTO @CDocID

END

--CLOSE cur
--DEALLOCATE cur


GO


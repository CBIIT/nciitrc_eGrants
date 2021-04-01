SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE [dbo].[sp_Create_Bundle]
AS
-- =============================================================================
-- Author:		Imran Omair
-- Create date:		7/23/2012
-- Modification: 8/20/2012
--				  8/22/2012
--  Modification: 8/23/2012 Added JIT Info
--				: 9/24/2013 Made bundle document non-qc-eable as per crystal's instructions (added qc_person_id = null, qc_person_date = null)
-- =============================================================================
declare @applid int
declare @ngacnt int
declare @ngadocdate datetime
declare @ngadocid int  
declare @aid int
declare @did int
declare @ddate smalldatetime
declare @cid int
declare c_BundleNew cursor Static Local for 
	select distinct appl_id from eim.dbo.temp_bundle_new
	
	
	delete temp_bundle_new
	
	insert into temp_bundle_new(appl_id,document_id,category_id,document_date)
	select distinct appl_id,document_id,category_id,document_date 
	from documents where 
	category_id in (select category_id from bundle_categories where package_id='award' ) --and category_id<>1
	and appl_id in (select distinct appl_id from documents where category_id=1 and document_date > '5/11/2012' and appl_id>0)
	and disabled_date is null
	and profile_id=1
	and document_date is not null
	and document_id not in (select distinct parent_id from egrants where parent_id is not null)
	and parent_id is null
	order by appl_id,document_date desc

	
	--select distinct  appl_id from eim.dbo.bundles_doc where wip_stage=1 and appl_id in (7616774,8064011,8034058,8089435,8090464,8011075,8041734,8089450,8036116,8081098,8019059)
	open  c_BundleNew 
	fetch next from c_BundleNew into @applid
    while @@fetch_status = 0 
    begin
		Set @ngacnt=0	
		Set @ngadocid=Null	   
		Select @ngacnt=count(*) from eim.dbo.temp_bundle_new where appl_id=@applid and category_id=1		
		If @ngacnt = 0 Begin
			Delete eim.dbo.temp_bundle_new where appl_id=@applid
			Print 'Deleted for appl_id=' + convert(varchar(10),@applid)
			End
		--else If @ngacnt = 1 Begin
		--	--Get Nga Doc ID
		--	Select @ngadocid=document_id from eim.dbo.temp_bundle_new where appl_id=@applid and category_id=1		
		--	Update eim.dbo.temp_bundle_new
		--		Set parent_id=@ngadocid
		--		Where appl_id=@applid and category_id<>1 and 
		--End
		else If @ngacnt > 0 Begin
			
			declare c_BundleMultipleNGA cursor Static Local for 
				select appl_id,document_id,document_date,category_id from eim.dbo.temp_bundle_new 
					where appl_id=@applid		
					order by document_date desc
			Open c_BundleMultipleNGA
			Fetch Next from c_BundleMultipleNGA into @aid,@did,@ddate,@cid
			Set @ngadocid=Null
			While @@fetch_status = 0 
			Begin
				If @cid=1 
					Set @ngadocid=@did
				else If @ngadocid is not null 
					Update eim.dbo.temp_bundle_new Set parent_id=@ngadocid	Where document_id=@did and appl_id=@aid					
					
				Fetch Next from c_BundleMultipleNGA into @aid,@did,@ddate,@cid
			End 
		    close  c_BundleMultipleNGA 
			deallocate  c_BundleMultipleNGA 
		
		End 
		
		fetch next from c_BundleNew into @applid
		Set @ngacnt = 0
    End 
    close  c_BundleNew 
	deallocate  c_BundleNew 

	-- Now Update documents table for bundle from temp table.
	update documents 
	set documents.parent_id=temp_bundle_new.parent_id, qc_person_id = null, qc_date = null
	from temp_bundle_new 
	where documents.document_id=temp_bundle_new.document_id
		and temp_bundle_new.parent_id is not null
	

GO


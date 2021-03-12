SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_egrants_maint_single_applid_w_to_s]

/************************************************************************************************************/
/***									 		***/
/***	Procedure Name:  sp_egrants_maint_single_applid_w_to_s			***/
/***	Description:	check all appl suffix_code  with 'w'				***/
/***	Created:	06/19/2008	Leon						***/
/***	Modified:	06/23/2008	Leon						***/
/***											***/
/************************************************************************************************************/

AS

SET NOCOUNT ON

declare 
@sql				varchar(4000),
@appl_id			int,
@index				int,
@count				int,
@doc_count			int,
@folder_count			int,
@full_grant_num_impac 	varchar(50)

--find and insert appls suffix_code with 'w'
insert into appls_w_to_s (appl_id, full_grant_num_eim,load_date)
select appl_id, full_grant_num, getdate() 
from vw_appls 
where  appl_id>0 and appl_id not in(select appl_id from appls_w_to_s) and PATINDEX('%w%',suffix_code)>0 

--check if  the appl is still in IMPAC
declare appl_w cursor for 
select  appl_id
from appls_w_to_s 
where act is null and (appl_in_impac is null or appl_in_impac='yes')

open  appl_w
fetch next from appl_w into @appl_id

while @@fetch_status = 0 
begin
/*--commented by hareeshj on 5/4/16 5:10pm--*/
--set @sql='update appls_w_to_s set full_grant_num_impac=(select full_grant_num from openquery(IMPAC,'   + char(39) + 'select FULL_GRANT_NUM from pv_grant_pi where appl_id=' + char(39) + char(39) +convert(varchar,@appl_id) + char(39) + char(39) + char(39) +') ) where appl_id=' +convert(varchar,@appl_id) 	

/*--commented on hareeshj on 8/12/16 5:10pm--*/
/*--added by hareeshj on 5/4/16 5:10pm--*/
--set @sql='update appls_w_to_s set full_grant_num_impac=(select full_grant_num from openquery(IMPAC,'   + char(39) + 'select FULL_GRANT_NUM from pva_grant_pi_mv where appl_id=' + char(39) + char(39) +convert(varchar,@appl_id) + char(39) + char(39) + char(39) +') ) where appl_id=' +convert(varchar,@appl_id) 	

/*--added by hareeshj on 8/12/16 12:30pm--*/
set @sql='update appls_w_to_s set full_grant_num_impac=(select full_grant_num from openquery(IMPAC,'   + char(39) + 'select FULL_GRANT_NUM from appls_mv where appl_id=' + char(39) + char(39) +convert(varchar,@appl_id) + char(39) + char(39) + char(39) +') ) where appl_id=' +convert(varchar,@appl_id) 	
	exec (@sql)

	set @full_grant_num_impac=(select full_grant_num_impac from  appls_w_to_s where appl_id=@appl_id)
	if @full_grant_num_impac is not null	
	begin	--this appl is still  in IMPAC and should be keep
		update appls_w_to_s set appl_in_impac ='yes' where appl_id=@appl_id
	end
	else	--this appl_id  is no longer in IMPAC and should be deleted
	begin
		update appls_w_to_s set appl_in_impac ='no', full_grant_num_impac=null  where appl_id=@appl_id

		/**check document table**/
		select @doc_count=count(*) from documents where appl_id=@appl_id
		if @doc_count=0
		begin
			update appls_w_to_s set act ='deleted', act_date=getdate() where appl_id=@appl_id
			delete from appls where  appl_id=@appl_id	
		end
		else
		begin
			update appls_w_to_s set act ='to check document table', act_date=getdate() where appl_id=@appl_id
		end
		set @doc_count=null

		/**check folder = table**/
		select @folder_count=count(*) from folder_appls where appl_id=@appl_id
		if @folder_count=0
		begin
			update appls_w_to_s set act ='deleted', act_date=getdate() where appl_id=@appl_id
			delete from appls where  appl_id=@appl_id	
		end
		else
		begin
			update appls_w_to_s set act ='to check folder_appls table', act_date=getdate() where appl_id=@appl_id
		end
		set @folder_count=null
	end

	--clean up memories
	set @sql=null
	set @full_grant_num_impac=null
fetch next from appl_w  into @appl_id
end
close  appl_w
deallocate appl_w

/**set flag for the appl that has been switched from w to s by IMPAC**/
update appls_w_to_s set act='switched from w to s by IMPAC', act_date=getdate()
where id in (select a.id from appls_w_to_s a, appls_w_to_s b 
where a.id=b.id 
and (a.appl_in_impac='yes' and b.appl_in_impac='yes')
and (a.act is null and b.act is null) 
and (a.full_grant_num_impac is not null and b.full_grant_num_impac is not null)
and a.full_grant_num_eim <> b.full_grant_num_impac
)

GO


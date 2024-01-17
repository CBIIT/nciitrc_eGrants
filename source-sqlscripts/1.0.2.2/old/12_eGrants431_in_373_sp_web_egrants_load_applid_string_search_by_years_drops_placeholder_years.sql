USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_load_applid_string]    Script Date: 12/14/2023 2:44:50 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_load_applid_string]

@grant_id		int,
@flag_type		varchar(20)=null,
@years 			varchar(800)=null

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_create_applid_string						***/
/***	Description:create appl_id string by user selected years or flag_type	***/
/***	Created:	01/10/2019	Leon											***/
/***	Modified:	03/26/2019	Leon											***/
/************************************************************************************************************/
SET NOCOUNT ON

declare @appl_list	varchar(800)
declare @sql		varchar(800)
declare @RowNum		int
declare @MaxRow		int
declare @appl_id	int

CREATE TABLE #a(id int IDENTITY (1, 1) NOT NULL, appl_id int)

---return all appls
IF (@flag_type is null or @flag_type='') and (@years is null or @years='' or @years='all' or @years='All')
BEGIN
SET @sql='insert #a select appl_id from vw_appls 
where grant_id='+convert(varchar, @grant_id)+'and doc_count<>0 
order by support_year desc, suffix_code desc'
END

---return selected appls by years=number
IF (@flag_type is null or @flag_type='') and (@years is not null and @years<>'' and @years<>'All' and @years<>'all' and ISNUMERIC(convert(int, @years))=1) ---and Len(@years)<=2 
BEGIN
-- MLH used to do a doc count > 0 here, removed for eGrants-373
SET @sql='insert #a select distinct appl_id from vw_appls 
where grant_id = '+convert(varchar,@grant_id) + ' and support_year in(
select distinct top '+ convert(varchar,@years)+' support_year from vw_appls 
where grant_id = '+convert(varchar,@grant_id) +' order by support_year desc)'
END

--return selected appls by years=appl_id
IF (@flag_type is null or @flag_type='') and (@years is not null and @years<>'' and @years<>'All' and @years<>'all' and Len(@years)>2) 
BEGIN
SET @sql='insert #a select appl_id from vw_appls where grant_id='+convert(varchar, @grant_id)+' and 
appl_id in ('+convert(varchar,@years)+') order by support_year desc, suffix_code desc'
END

--return selected appls by flag type
IF @flag_type is not null and @flag_type<>'' 
BEGIN
SET @sql='insert #a select distinct appl_id from Grants_Flag_Construct 
where grant_id='+convert(varchar, @grant_id)+' and flag_type='+char(39)+convert(varchar, @flag_type)+char(39)
END

--print @sql
exec (@sql)
--select * from #a

set @appl_list=''
set @RowNum=1
set @MaxRow = (select max(id ) From #a )		--get total number of records
WHILE @RowNum < (@MaxRow +1 )                       --loop until no more records
BEGIN   
	set @appl_id=(select appl_id from #a where id = @RowNum )
	--print convert(varchar,@appl_id)
	set @appl_list=@appl_list + convert(varchar, @appl_id)+','
	set @RowNum = @RowNum + 1                          
END

set @appl_list=SUBSTRING(@appl_list,0,len(@appl_list))
select @appl_list



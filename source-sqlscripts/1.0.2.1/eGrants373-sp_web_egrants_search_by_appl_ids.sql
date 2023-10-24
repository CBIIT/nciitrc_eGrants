USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_search_by_appl_ids]    Script Date: 10/23/2023 3:28:35 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE OR ALTER PROCEDURE [dbo].[sp_web_egrants_search_by_appl_ids]

@appl_ids 		varchar(max) = null,
@search_type	varchar(50) = null,
@category_list	varchar(500) = null,
@ic  			varchar(10),
@operator 		varchar(50)

AS
/************************************************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_search_by_appl_id							***/
/***	Description:show documents by appl_id									***/
/***	Created:	03/06/2019	Leon											***/
/************************************************************************************************************/
SET NOCOUNT ON

--for user info
declare @profile_id			int
declare @profile			varchar(10)
declare @person_id			int
declare @position_id		int
declare @sql				varchar(800)
declare @count				int

--------------------------------------------- Modified --------------------------------
	IF OBJECT_ID('tempdb..#SearchByApplIds') IS NOT NULL DROP TABLE #SearchByApplIds
	IF OBJECT_ID('tempdb..#SearchByApplIdsResults') IS NOT NULL DROP TABLE #SearchByApplIdsResults	-- all formatted results
	

			create table #SearchByApplIdsResults(
			ic varchar(100),
			grant_id int,
			appl_id int,
			unknown varchar(1000),
			document_id int,
			document_date date,
			document_name varchar(500),
			doc_date datetime,
			category_id int,
			category_name  varchar(500),
			sub_category_name  varchar(500),
			created_by varchar(500),
			created_date date,
			modified_by  varchar(500),
			modified_date date,
			file_modified_by varchar(500),
			file_modified_date date,
			problem_msg varchar(max),
			problem_reported_by varchar(500),
			page_count int,
			fsr_count int,
			attachment_count int,
			closeout_notcount int,
			frc_destroyed int,
			url  varchar(max),
			qc_date date,
			qc_reason varchar(500),
			qc_person_id int,
			qc_person_name varchar(500),
			can_qc varchar(500),
			can_upload varchar(500),
			can_modify_index varchar(500),
			can_delete varchar(500),
			can_restore varchar(500),
			can_store varchar(500)
		)

	SELECT value 
	into #SearchByApplIds 
	FROM STRING_SPLIT(@appl_ids, ',')
	--FROM STRING_SPLIT('1,2,3', ',')

	declare @appl_id int

	while exists (select * from #SearchByApplIds)
	begin

		declare @count2 int
		select @count2 = count(*) from #SearchByApplIdsResults
		print( CONCAT('size so far of #SearchByApplIdsResults1: ', @count2) )

		select @appl_id = (select top 1 [value]
						   from #SearchByApplIds
						   )

		print( CONCAT('Running this for applId: ', @appl_id) )


		-- cleanup
		IF OBJECT_ID('tempdb..#d') IS NOT NULL DROP TABLE #d
		--IF OBJECT_ID('tempdb..#c') IS NOT NULL DROP TABLE #c


		--find unser info
		SET @operator=LOWER(@operator)
		SELECT @profile_id=profile_id FROM profiles WHERE profile=@IC
		SELECT @person_id= person_id FROM vw_people WHERE userid=@operator AND profile_id=@profile_id
		SELECT @position_id=position_id FROM vw_people WHERE person_id=@person_id

		CREATE TABLE #d (document_id int primary key)
		--CREATE TABLE #c (category_id int primary key)

		--return qc document only with appl_id
		IF @search_type='by_qc' or @search_type='by_page' GOTO by_qc

		--return all categories with appl_id such as appl_id:1234567
		IF @search_type ='by_str' and (@category_list is null or @category_list='') GOTO by_applid

		--return all categories with appl_id
		IF @category_list is not null and @category_list = 'All' or @category_list = 'all' GOTO by_cats_all

		--return selected category with appl_id
		IF @category_list is not null and @category_list <> 'All' and @category_list <>'all' GOTO by_cats

		--return all categories with appl_id
		IF @search_type is null and @category_list is null GOTO by_applid
		----------------------
		by_applid:

		SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id)
		EXEC (@sql)

		GOTO OUTPUT_ALL
		-----------------------
		by_cats:---return selected categories

		SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id) + ' and category_id in ('+ @category_list+')'
		EXEC (@sql)

		GOTO OUTPUT
		---------------------------
		by_cats_all:---return all categories

		SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id)
		EXEC (@sql)

		GOTO OUTPUT_ALL
		-----------------------
		by_qc:

		SET @sql='insert #d (document_id) select document_id from egrants where appl_id='+ convert(varchar,@appl_id) + ' and qc_person_id=' + convert(varchar,@person_id) + ' and qc_date is not null'
		EXEC (@sql)

		GOTO OUTPUT
		--------------------------
		OUTPUT: ---return documents without funding documents

		INSERT INTO #SearchByApplIdsResults
		SELECT
		ic,
		grant_id,
		appl_id,
		full_grant_num,
		document_id,
		convert(varchar,document_date,101) as document_date,
		document_name,
		document_date as doc_date,
		category_id,
		category_name,
		isnull(sub_category_name, null) as sub_category_name,
		created_by,
		convert(varchar,created_date,101) as created_date,
		modified_by,
		CONVERT(VARCHAR(20), modified_date, 100) as modified_date, --take seconds out
		file_modified_by,
		CONVERT(VARCHAR(20), file_modified_date, 100) as file_modified_date, --take seconds out
		problem_msg,
		problem_reported_by,
		page_count,
		CASE
		WHEN category_name='Financial Report' or category_name='FFR' THEN dbo.fn_appl_fsr_count(appl_id)
		ELSE 0
		END as fsr_count,
		attachment_count,
		dbo.fn_appl_CloseOut_NotCount (@appl_id) as closeout_notcount,
		dbo.fn_appl_frc_destroyed(@appl_id) as frc_destroyed,
		dbo.fn_get_doc_url(document_id, @ic) as url,	
		convert(varchar, qc_date, 101) as qc_date,
		qc_reason,
		qc_person_id,
		qc_person_name,
		CASE WHEN category_name in('Greensheet DMC','Greensheet PGM','Greensheet SPEC') THEN 'n'
		ELSE 'y' END AS can_qc,
		CASE WHEN can_upload='y' and  	(@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_upload,
		CASE WHEN can_modify_index='y' and (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_modify_index,
		CASE WHEN can_delete='y' and  	(@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_delete,
		CASE WHEN can_restore='y' and   (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_restore,
		CASE WHEN can_store='y' and 	(@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_store
		FROM egrants WHERE appl_id=@appl_id and document_id in (select document_id from #d)

		return
		-------------------------------------------
		OUTPUT_ALL: ---return documents with funding documents

		INSERT INTO #SearchByApplIdsResults
		SELECT
		ic,
		grant_id,
		appl_id,
		full_grant_num collate SQL_Latin1_General_CP1_CI_AS,
		document_id,
		convert(varchar,document_date,101) as document_date,
		document_name,
		document_date as doc_date,
		category_id,
		category_name,
		isnull(sub_category_name, null) as sub_category_name,
		created_by,
		convert(varchar,created_date,101) as created_date,
		modified_by,
		CONVERT(VARCHAR(20), modified_date, 100) as modified_date, --take seconds out
		file_modified_by,
		CONVERT(VARCHAR(20), file_modified_date, 100) as file_modified_date, --take seconds out
		problem_msg,
		problem_reported_by,
		page_count,
		CASE
		WHEN category_name='Financial Report' or category_name='FFR' THEN dbo.fn_appl_fsr_count(appl_id)
		ELSE 0
		END as fsr_count,
		attachment_count,
		dbo.fn_appl_CloseOut_NotCount (@appl_id) as closeout_notcount,
		dbo.fn_appl_frc_destroyed(@appl_id) as frc_destroyed,
		dbo.fn_get_doc_url(document_id, @ic) as url,	
		convert(varchar, qc_date, 101) as qc_date,
		qc_reason,
		qc_person_id,
		qc_person_name,
		CASE WHEN category_name in('Greensheet DMC','Greensheet PGM','Greensheet SPEC') THEN 'n'
		ELSE 'y' END AS can_qc,
		CASE WHEN can_upload='y' and (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_upload,
		CASE WHEN can_modify_index='y' and (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_modify_index,
		CASE WHEN can_delete='y' and  (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_delete,
		CASE WHEN can_restore='y' and (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_restore,
		CASE WHEN can_store='y' and (@position_id>1 and ic=@IC) THEN 'y' ELSE 'n' END AS can_store
		FROM egrants WHERE appl_id=@appl_id and document_id in (select document_id from #d)

		UNION ALL 

		SELECT * FROM fn_egrants_web_funding_documents(@appl_id) 

		UNION ALL 

		SELECT * FROM fn_egrants_web_notification_documents(@appl_id) 

		--return

			--declare @count int
		--select @count = count(*) from #SearchByApplIdsResults
		--print( CONCAT('size so far of #SearchByApplIdsResults2: ', @count) )

				--declare @count2 int
		select @count2 = count(*) from #SearchByApplIdsResults
		print( CONCAT('size of #SearchByApplIdsResults1 at end: ', @count2) )

		delete #SearchByApplIds
		where [value] = @appl_id

	end

	select * from #SearchByApplIdsResults

--/****** Object:  StoredProcedure [dbo].[sp_web_egrants_head]    Script Date: 10/6/2017 10:14:19 AM ******/
--SET ANSI_NULLS OFF

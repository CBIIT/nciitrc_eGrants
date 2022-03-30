SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON



CREATE   PROCEDURE [dbo].[sp_web_egrants_supplement]

@act 			varchar(50),
@grant_id		int,
@support_year 	smallint,
@suffix_code	varchar(4),
@docid_str		varchar(800),
@former_applid	int,
@ic				varchar(10),
@Operator		varchar(50)

AS
/************************************************************************************************************/
/***									 							***/
/***	Procedure Name: sp_web_egrants_supp_load					***/
/***	Description:load supp to egrants							***/
/***	Created:	10/27/2015	Leon								***/
/***	Modified:	12/16/2015	Leon								***/
/***	Simplified:	12/21/2015	Leon								***/
/***	Modified:	07/28/2016	Leon								***/
/***	Modified:	07/28/2016	Leon modified it to MVC				***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE
@profile_id		smallint,
@person_id		int,
@separate		int,
@count			int,
@appl_id		int,
@supp_appl_id	int,
@category_id	int,
@appl_type_code tinyint,
@activity_code 	char(3),
@serial_num		int,
@impac_doc_type	varchar(5),
@doc_id			int,
@document_id	int,
@impacid		int,
@sql			varchar(800),
@accession_number int

--@locall_image_server varchar(100)

/***find the profile_id**/
SET @ic=LOWER(@ic)
--SET @locall_image_server='https://egrants-web-test.'+@IC+'.nih.gov'

SELECT @profile_id=profile_id FROM profiles WHERE profile=@ic 
SELECT @impacid=person_id FROM people WHERE person_name='impac'

/***find the operator's person_id***/
SELECT @person_id = person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id

--find serial num, grant_id, former_appl_id
SET @serial_num=(select serial_num from grants where grant_id=@grant_id)

CREATE TABLE #doc (doc_id int)--to insert adm_supp_wip_id 
IF @act='to_move' or @act='to_pay' ---and @docid_str is not null 658,641,452
BEGIN
SET @sql='INSERT #doc SELECT adm_supp_wip_id from dbo.IMPP_Admin_Supplements_WIP where adm_supp_wip_id in ('+@docid_str+')'
END

IF @act='to_view' 
BEGIN
SET @sql='INSERT #doc SELECT adm_supp_wip_id from dbo.IMPP_Admin_Supplements_WIP where serial_num='+char(39)+CONVERT(varchar,@serial_num)+char(39)+' and moved_date is null'
END

IF @act='to_history'
BEGIN
SET @sql='INSERT #doc SELECT adm_supp_wip_id from dbo.IMPP_Admin_Supplements_WIP where serial_num='+char(39)+CONVERT(varchar,@serial_num)+char(39)
END

EXEC(@sql)---to insert adm_supp_wip_id 

--run by act
IF @act='to_view' or @act='to_history' GOTO to_view
IF @act='to_move' GOTO to_move
IF @act='to_pay' GOTO to_pay
--------------------------------------------------------
to_view:

SELECT 
1 as tag,
@grant_id as grant_id,
admin_phs_org_code,
serial_num,
null as id,
null as full_grant_num,
null as supp_appl_id,
null as support_year,
null as suffix_code,
null as former_num, 
null as former_appl_id,
null as submitted_date,
null as date_of_submitted,
null as category_name,
null as sub_category_name,
null as [status],
null as url,
null as moved_date,
null as moved_by,
null as accession_number

FROM grants AS [grant]
WHERE grant_id=@grant_id

UNION 

SELECT 
2,
@grant_id,
null,
null,
adm_supp_wip_id,
full_grant_num,
Supp_appl_id,
Support_year,
Suffix_code,
Former_num,
Former_appl_id,
submitted_date,
CONVERT(varchar(20), submitted_date),
dbo.fn_get_category_name(category_id),
sub_category_name,
dbo.get_admin_supp_latest_action(serial_num,supp_appl_id),
doc_url,
--CASE 
--WHEN SUBSTRING(url,1,5)='data/' THEN @locall_image_server +'/'+[url]
--WHEN SUBSTRING(url,1,5)='/data' THEN @locall_image_server +[url]
--ELSE [url] END,
CASE WHEN moved_date is not null THEN CONVERT(varchar,moved_date,101) 
ELSE null END,
CASE WHEN moved_by is not null THEN dbo.fn_get_person_name(moved_by) 
ELSE null END,
accession_number
FROM dbo.IMPP_Admin_Supplements_WIP  
WHERE Serial_num=@serial_num and adm_supp_wip_id in (select doc_id from #doc )
ORDER BY submitted_date DESC

RETURN
-------------------------------------------------------------------------
--to create supp documents by parent appl_id
to_pay:

---SET @former_appl_id=(select distinct max(former_appl_id) from IMPP_Admin_Supplements_WIP where adm_supp_wip_id in(select doc_id from #doc))
SET @support_year=(select support_year from appls where appl_id=@former_applid)
SET @suffix_code=(select ISNULL(suffix_code,null)from appls where appl_id=@former_applid)
SET @appl_id=@former_applid

GOTO to_load
---------------------------------------------------------------------------
--to move supp documents by appl_id
to_move:

declare @applid int

--to find appl_type_code and activity_code
--SET @former_appl_id =(select distinct max(former_appl_id) from IMPP_Admin_Supplements_WIP where adm_supp_wip_id in(select doc_id from #doc))
SET @appl_type_code = 3 
SET @activity_code =(SELECT activity_code FROM appls WHERE appl_id=@former_applid)

--to find created supplement appl_id
SET @count=(select count(appl_id) from appls where grant_id=@grant_id and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year and suffix_code=ISNULL(@suffix_code,null))

IF @count=1 SET @appl_id=(select appl_id from appls where grant_id=@grant_id and activity_code=@activity_code and appl_type_code=@appl_type_code and support_year=@support_year and suffix_code=ISNULL(@suffix_code,null))
ELSE
BEGIN			--@Supp_appl_id is not existing yet and create new appl_id
SELECT @applid=min(appl_id)-FLOOR(10000 * RAND()) FROM appls
INSERT appls(appl_id,grant_id,appl_type_code,activity_code,support_year,suffix_code)
SELECT @applid,@grant_id,@appl_type_code,@activity_code,@support_year,ISNULL(@suffix_code,null)
SET @appl_id=@applid
END				--end create new appl_id

GOTO to_load--load documents to existing appl_id
----------------
to_load:--load doc to documents table

--create cursor to load data
DECLARE load_doc CURSOR FOR select doc_id from #doc
OPEN load_doc
FETCH NEXT FROM load_doc into @doc_id 
While @@FETCH_STATUS = 0
BEGIN
	--create new document 
	INSERT documents(appl_id,url,category_id,sub_category_name, document_date,file_type,created_date,created_by_person_id, stored_date,stored_by_person_id,profile_id)
	SELECT @appl_id,doc_url,category_id,ISNULL(sub_category_name,null),Submitted_date,file_type,GETDATE(),@person_id,GETDATE(),@person_id,@profile_id
	FROM IMPP_Admin_Supplements_WIP 
	WHERE adm_supp_wip_id=@doc_id

	--to get document_id
	SELECT @document_id = @@IDENTITY
	
	--to update IMPP_Admin_Supplements_WIP table
	UPDATE IMPP_Admin_Supplements_WIP 
	SET Support_year=@support_year,Suffix_code=ISNULL(@suffix_code,null),moved_by=@person_id,moved_date=GETDATE(), movedto_appl_id=@appl_id,movedto_document_id=@document_id
	FROM IMPP_Admin_Supplements_WIP 
	WHERE adm_supp_wip_id=@doc_id
	
	--to load next one
	fetch next from load_doc into @doc_id
END

close load_doc
deallocate load_doc

---return view
GOTO to_view

RETURN

GO


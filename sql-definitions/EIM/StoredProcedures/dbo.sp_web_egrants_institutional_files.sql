SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE    PROCEDURE [dbo].[sp_web_egrants_institutional_files]

@act  				varchar(20),
@str  				varchar(50),
@index_id			int,
@org_id				int,
@doc_id				int,
@category_id		int,
@file_type			varchar(5),
@start_date			varchar(10),
@end_date			varchar(10),
@ic  				varchar(10),
@operator 			varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_org_files										***/
/***	Description:search, dispaly or edit files							***/
/***	Created:	03/09/2016	Leon										***/
/***	Modified:	03/09/2016	Leon										***/
/***	Modified:	12/07/2016	Leon for MVC								***/
/************************************************************************************************************/
SET NOCOUNT ON

DECLARE 
@document_id	int,
@xmlout		varchar(max),
@X		Xml,
@person_id	int,
@count		int,
@profile_id	int

/** find user info***/
SET @profile_id=(select profile_id from profiles where [profile]=@ic)
SET @person_id=(SELECT person_id FROM vw_people WHERE userid=@Operator and profile_id=@profile_id)

/**set default act**/
if @act ='show_orgs' goto show_orgs
if @act ='search_orgs' goto search_orgs
if @act ='show_docs' goto show_docs
if @act ='disable_doc' goto disable_doc
if @act ='upload_doc' goto upload_doc
---------------------
show_orgs:

--display org by index_id
--SELECT org_id, UPPER(Org_name)AS org_name,dbo.fn_get_org_flag_url(Org_name) as sv_doc_url
--FROM dbo.Org_Master
--WHERE index_id=@index_id and dbo.fn_get_org_doc_count(org_id)>0
--ORDER BY Org_name

--Madhu - This is the old Definition
--SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
--FROM dbo.Org_Master
--WHERE index_id=@index_id and dbo.fn_get_org_doc_count(org_id)>0
--UNION
--SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, (select dbo.fn_get_local_image_server()) +url as sv_url
--FROM vw_org_document as v, dbo.Org_Master as o
--WHERE o.index_id=@index_id and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
--Order by Org_name

--02/27/2022 BSHELL  optomized SQL to return Org Data

-- Moving the following code to a new Proc . [sp_web_egrants_inst_files_show_orgs]
/*
declare @index_id int
set @index_id = 1

-- no comments here 
  Select om.org_id, UPPER(om.Org_name) as Org_Name, om.index_id, docs.created_by, docs.created_date, docs.end_date, docs.sv_url 
  from org_master om left join 
  (Select m.org_id, max_end_date as end_date, v.created_by, v.created_date, dbo.fn_get_local_image_server() + v.url as sv_url
   FROM 
   (Select [org_id], max([end_date]) as max_end_date from vw_org_document where category_id = 2 group by org_id) m 
	inner join vw_org_document v on m.org_id = v.org_id and v.end_date = m.max_end_date) docs on om.org_id = docs.org_id 
	where om.index_id = @index_id and dbo.fn_get_org_doc_count(om.org_id)>0
	order by Org_Name
*/
print 'in show_orgs:'
RETURN exec dbo.sp_web_egrants_inst_files_show_orgs @index_id

----------------------
search_orgs:

/* The following code has been moved to sp_web_egrants_inst_files_show_orgs
--display org by search string
SET @str=LTRIM(rtrim(@str))
SET @str=REPLACE(@str, '     ',' ')---reducing space
SET @str=REPLACE(@str, '   ',' ')---reducing space
SET @str=REPLACE(@str, '  ',' ')---reducing space


--SELECT org_id, UPPER(Org_name)AS org_name,dbo.fn_get_org_flag_url(Org_name) as sv_doc_url
--FROM dbo.Org_Master 
--WHERE org_name like '%'+@str+'%'
--ORDER BY Org_name
-- no comments reqd
SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
FROM dbo.Org_Master
WHERE org_name like '%'+@str+'%' 
UNION
SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, url
FROM vw_org_document as v, dbo.Org_Master as o
WHERE o.org_name like '%'+@str+'%' and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
Order by Org_name
*/

print 'search_orgs:'
RETURN exec dbo.sp_web_egrants_inst_files_search_orgs @str

------------------------
show_docs:
-- comments reqd
/*SELECT org_id,org_name,document_id,category_name, url,[start_date],end_date,created_date
FROM dbo.vw_Org_Document 
WHERE org_id=@org_id 
*/
print 'show_docs'
RETURN exec dbo.sp_web_egrants_inst_files_show_docs @org_id
-------------------------
disable_doc:


RETURN
-------------------------
upload_doc:

--Add comments for insert
----update new document 
/*
INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,
end_date_showFlag--,comments
)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),ISNULL(@end_date,null)

SELECT  @document_id=@@IDENTITY
*/

/*RETURN dbo.sp_web_egrants_inst_files_upload_doc @org_id, @doc_id, @category_id, @file_type,
@start_date,
@end_date
*/
----update url
--UPDATE dbo.Org_Document 
--SET url='/data/funded/nci/institutional/'+convert(varchar,@document_id)+'.'+@file_type
--WHERE document_id=@document_id

------return info
--SET @X = (
--SELECT document_id AS doc_id,url
--FROM dbo.Org_Document AS new_doc
--WHERE document_id=@document_id 
--FOR XML AUTO, TYPE, ELEMENTS
--)

----return xml file***/
--select @xmlout = cast(@X as varchar(max))
--select @xmlout

--GOTO load_org

RETURN exec dbo.sp_web_egrants_inst_files_upload_doc @org_id, @doc_id, @category_id, @file_type, @start_date, @end_date
                                                      


GO


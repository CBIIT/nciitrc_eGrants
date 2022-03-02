SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE    PROCEDURE [dbo].[sp_web_egrants_inst_files_show_orgs]

@index_id			int

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name:sp_web_egrants_inst_files_search_orgs				***/
/***	Description:search, dispaly or edit files							***/
/***	Created:	03/01/2022	Madhu										***/
/************************************************************************************************************/
SET NOCOUNT ON

-------------------
-- search_orgs:

set @index_id = 1

-- no need for comments here 
  Select om.org_id, UPPER(om.Org_name) as Org_Name, om.index_id, docs.created_by, docs.created_date, docs.end_date, docs.sv_url 
  from org_master om left join 
  (Select m.org_id, max_end_date as end_date, v.created_by, v.created_date, dbo.fn_get_local_image_server() + v.url as sv_url
   FROM 
   (Select [org_id], max([end_date]) as max_end_date from vw_org_document where category_id = 2 group by org_id) m 
	inner join vw_org_document v on m.org_id = v.org_id and v.end_date = m.max_end_date) docs on om.org_id = docs.org_id 
	where om.index_id = @index_id and dbo.fn_get_org_doc_count(om.org_id)>0
	order by Org_Name

print '[sp_web_egrants_inst_files_show_orgs2]'
RETURN


GO


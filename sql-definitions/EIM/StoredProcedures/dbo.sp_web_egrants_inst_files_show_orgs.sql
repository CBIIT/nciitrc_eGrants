SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE     PROCEDURE [dbo].[sp_web_egrants_inst_files_show_orgs]

@index_id	int

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

Select
   om.org_id,
   UPPER(om.Org_name) as Org_Name,
   om.index_id,
   svdocs.created_by as svcreated_by,
   svdocs.created_date as svcreated_date,
   svdocs.end_date as svend_date,
   svdocs.sv_url,
   fudocs.created_by as fucreated_by,
   fudocs.created_date as fucreated_date,
   fudocs.end_date as fuend_date,
   fudocs.fu_url,
   odocs.created_by as odcreated_by,
   odocs.created_date odcreated_date,
   odocs.end_date as odend_date,
   odocs.od_url 
from
   org_master om 
   -- Left join to bring the documents of type ??(2 here)
   left join
      (
         Select
            m.org_id,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as sv_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date,
				  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function
				  
               from
                  vw_org_document 
               where
                  category_id = 2 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			   and v.category_id = m.category_id
      )
      svdocs 
      on om.org_id = svdocs.org_id 
-- Left join to bring the documents of type ??(5 here)
   left join
      (
         Select
            m.org_id,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as fu_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date,
  				  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document 
               where
                  category_id = 5 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			   and v.category_id = m.category_id
      )
      fudocs 
      on om.org_id = fudocs.org_id 
	  -- Left join to bring the documents of type ??(5 here)
   left join
      (
         Select
            m.org_id,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as od_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date, 
				  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document 
               where
                  category_id = 6 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			   and v.category_id = m.category_id
      )
      odocs 
      on om.org_id = odocs.org_id 
where
   om.index_id = @index_id 
   and dbo.fn_get_org_doc_count(om.org_id) > 0 
order by
   Org_Name

RETURN

GO


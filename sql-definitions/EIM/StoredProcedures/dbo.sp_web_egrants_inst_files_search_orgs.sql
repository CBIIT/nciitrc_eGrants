SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create     PROCEDURE [dbo].[sp_web_egrants_inst_files_search_orgs]

@str varchar(50)

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
--SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
--FROM dbo.Org_Master
--WHERE org_name like '%'+@str+'%' 
--UNION
--SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, url
--FROM vw_org_document as v, dbo.Org_Master as o
--WHERE o.org_name like '%'+@str+'%' and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
--Order by Org_name


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
   CASE WHEN NOT anyorgdocs.anydoc is null THEN CAST(1 as bit) else Cast(0 as bit) END as anyorgdoc
from
   org_master om 
   -- Left join to bring the documents of type ??(2 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as sv_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Site Visit'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
		
      )
      svdocs 
      on om.org_id = svdocs.org_id and svdocs.RowNum = 1
-- Left join to bring the documents of type ??(5 here)
   left join
      (
         Select 
            m.org_id,
			ROW_NUMBER() OVER(PARTITION BY m.org_id ORDER BY v.document_id DESC) as RowNum,
            max_end_date as end_date,
            v.created_by,
            v.created_date,
            dbo.fn_get_local_image_server() + v.url as fu_url 
         FROM
            (
               Select
                  [org_id],
                  max([end_date]) as max_end_date
  				--  max(category_id) as category_id -- doesn't matter all are same but need the aggregate function

               from
                  vw_org_document  org_doc
				inner join Org_Categories org_cat on org_cat.doctype_id = org_doc.category_id 
               where
                  category_name = 'Follow-Up'
				  and CONVERT(date,end_date) >= CONVERT(date,GETDATE()) 
               group by
                  org_id
            )
            m 
            inner join
               vw_org_document v 
               on m.org_id = v.org_id 
               and v.end_date = m.max_end_date
			
      )
      fudocs 
      on om.org_id = fudocs.org_id and fudocs.RowNum = 1
	  -- Left join to bring the documents of type ??(5 here)
   left join
   (
	Select max(document_id) as anydoc,
	org_id 
	from vw_org_document vod
	inner join Org_Categories cat on vod.category_id = cat.doctype_id
	where (cat.tobe_flagged = 1 AND CONVERT(date,end_date) >= CONVERT(date,GETDATE())) OR (1=1) 
	group by org_id

	) anyorgdocs on om.org_id = anyorgdocs.org_id

where om.org_name like '%'+@str+'%' 
order by
   Org_Name

RETURN

GO


SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE     PROCEDURE [dbo].[sp_web_egrants_institutional_file_find_org]

@org_id	int = 0,
@org_name varchar(300) = ''

AS

BEGIN
--02/27/2022 BSHELL  optimized SQL to return Org Data
  Select om.org_id, UPPER(om.Org_name) as Org_Name, om.index_id, docs.created_by, docs.created_date, docs.end_date, docs.sv_url from org_master om left join 
  (Select m.org_id, max_end_date as end_date, v.created_by, v.created_date, dbo.fn_get_local_image_server() + v.url as sv_url
   FROM 
   (Select [org_id], max([end_date]) as max_end_date from vw_org_document where category_id = 2 group by org_id) m 
	inner join vw_org_document v on m.org_id = v.org_id and v.end_date = m.max_end_date) docs on om.org_id = docs.org_id 
	where (not(@org_id = 0) and om.org_id = @org_id) or (@org_id = 0 and not(@org_name = '') and om.Org_name = @org_name) 
	order by Org_Name

RETURN
END

GO


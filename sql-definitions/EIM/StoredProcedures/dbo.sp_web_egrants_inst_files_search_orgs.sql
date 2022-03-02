SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE      PROCEDURE [dbo].[sp_web_egrants_inst_files_search_orgs]

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
SELECT 1 as tag, org_id, UPPER(Org_name)AS org_name, null as created_by,null as created_date, null as end_date, null as sv_url
FROM dbo.Org_Master
WHERE org_name like '%'+@str+'%' 
UNION
SELECT 2 as tag, v.org_id, o.Org_name, created_by, v.created_date, end_date, url
FROM vw_org_document as v, dbo.Org_Master as o
WHERE o.org_name like '%'+@str+'%' and o.org_id=v.org_id and tobe_flagged=1 and end_date = (select dbo.fn_get_org_max_end_date(o.org_id))
Order by Org_name

print '[sp_web_egrants_inst_files_search_orgs]'

RETURN


GO


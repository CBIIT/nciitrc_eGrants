SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE view [dbo].[vw_appls_arra] 
AS
--changed ciip2 to ciip on 6/2/13 4:52pm by hareesh

SELECT a.appl_id, g.grant_id, admin_phs_org_code 
FROM appls a INNER JOIN grants g ON a.grant_id=g.grant_id INNER JOIN
openquery(CIIP,
'select appl_id from GM_ACTION_QUEUE_VW where arra_grant_flag=''Y'' UNION
select appl_id from cans_mv c inner join awd_fundings_mv a on a.can=c.can where cfda_code=''701'''
) t

ON a.appl_id=t.appl_id

UNION

SELECT appl_id, grant_id, admin_phs_org_code
FROM egrants
WHERE category_name IN ('Recovery Act')
and ISNULL(appl_status_group_descrip,'') not in ('NRFC','Withdrawn','Terminated','Cancelled')

GO


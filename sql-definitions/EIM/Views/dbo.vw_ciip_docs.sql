SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE VIEW [dbo].[vw_ciip_docs]

AS

SELECT ciip.appl_id, ciip.spec_form_submitted_date, ciip.pgm_form_submitted_date 
FROM OPENQUERY(CIIP, 'select a.appl_id, g.spec_form_submitted_date, g.pgm_form_submitted_date 
					  from form_appl_status_vw a, form_grant_vw g 
					  where a.appl_id=g.appl_id') ciip INNER JOIN dbo.appls ON ciip.appl_id = dbo.appls.appl_id


/**
SELECT ciip.APPL_ID FROM
OPENQUERY(CIIP, 'select appl_id from form_appl_status_vw') ciip INNER JOIN
dbo.appls ON ciip.APPL_ID = dbo.appls.appl_id
**/


GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_script_spread
AS
SELECT     dbo.egrants.document_id, dbo.egrants.created_date, 'cp ' + dbo.egrants.unix_file + ' ' + '/egrants/funded2/nci/merge/spread_xls/' + CONVERT(varchar, 
                      dbo.egrants.appl_id) + '.xls' AS command
FROM         dbo.egrants INNER JOIN
                      dbo.scripts ON dbo.egrants.created_date > dbo.scripts.last_run_date
WHERE     (dbo.egrants.category_name = 'Spread Sheet') AND (dbo.egrants.file_type = 'xls') AND (dbo.scripts.script = 'Spread')

GO


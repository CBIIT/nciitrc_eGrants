SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW dbo.vw_appls_fake
AS
SELECT     dbo.appls.appl_id
FROM         dbo.appls LEFT OUTER JOIN
                      dbo.vw_all_appls_used ON dbo.appls.appl_id = dbo.vw_all_appls_used.appl_id
WHERE     (dbo.vw_all_appls_used.appl_id IS NULL) AND (dbo.appls.appl_id < 0)

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE VIEW [dbo].[vw_adm_menu_assignment]
AS
SELECT     dbo.vw_people.person_id AS person_id, dbo.vw_people.person_name, dbo.vw_people.userid, dbo.adm_menu.menu_id, dbo.adm_menu.menu_title, 
                      dbo.adm_menu.menu_url, dbo.adm_menu.menu_hover, dbo.adm_menu.menu_help_url,dbo.adm_menu.menu_action
FROM         dbo.adm_menu INNER JOIN
                      dbo.adm_menu_assignment ON dbo.adm_menu.menu_id = dbo.adm_menu_assignment.menu_id INNER JOIN
                      dbo.vw_people ON dbo.adm_menu_assignment.person_id = dbo.vw_people.person_id
WHERE dbo.adm_menu_assignment.end_date is null





GO


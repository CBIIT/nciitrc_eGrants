SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE VIEW [dbo].[vw_people_all_staging]
AS
SELECT person_id, CASE WHEN application_type = 'cft' THEN position_id ELSE 0 END AS cft, 
               CASE WHEN application_type = 'econ' THEN position_id ELSE 0 END AS econ, 
               CASE WHEN application_type = 'egrants' THEN position_id ELSE 0 END AS egrants, 
               CASE WHEN application_type = 'gft' THEN position_id ELSE 0 END AS gft, 
               CASE WHEN application_type = 'mgt' THEN position_id ELSE 0 END AS mgt
FROM  dbo.egrants_access
--WHERE end_date IS NULL
GROUP BY person_id, application_type, position_id
GO


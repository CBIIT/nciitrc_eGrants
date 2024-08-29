USE [EIM]
GO

/****** Object:  View [dbo].[vw_people]    Script Date: 8/27/2024 9:57:59 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER VIEW [dbo].[vw_service_accounts]
AS
SELECT 
	dbo.people.person_id
      ,dbo.people.[person_name]
      ,dbo.people.[userid]
      ,dbo.people.[profile_id]
      ,dbo.people.[position_id]
      ,dbo.people.[type]
      ,dbo.people.[active]
      ,dbo.people.[application_type]
      ,dbo.people.[team_id]
      ,dbo.people.[cft]
      ,dbo.people.[econ]
      ,dbo.people.[gft]
      ,dbo.people.[mgt]
      ,dbo.people.[egrants]
      ,dbo.people.[admin]
      ,dbo.people.[docman]
      ,dbo.people.[start_date]
      ,dbo.people.[end_date]
      ,dbo.people.[created_by]
      ,dbo.people.[created_date]
      ,dbo.people.[last_updated_by]
      ,dbo.people.[last_updated_date]
      ,dbo.people.[first_name]
      ,dbo.people.[middle_initial]
      ,dbo.people.[last_name]
      ,dbo.people.[email]
      ,dbo.people.[ic_coordinator_id]
      ,dbo.people.[is_coordinator]
      ,dbo.people.[iccoord]
      ,dbo.people.[phone_number]
      ,dbo.people.[dashboard]
      ,dbo.people.[last_login_date]
FROM  
	dbo.people
WHERE 
	person_name not like '%, %' and person_name not like '%,%'






GO



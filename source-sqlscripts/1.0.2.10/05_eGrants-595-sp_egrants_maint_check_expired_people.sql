USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_check_expired_people]    Script Date: 8/9/2024 3:16:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[sp_egrants_maint_check_expired_people]
AS
	PRINT '----------- Deactivating Users Who Have Not Recently Logged In ----------'
	PRINT 'Setting these people to Active = 0 and flagging for notification to OGA'
	
	DECLARE @sql varchar(max)
	SET @sql='select * from people WHERE last_login_date < DateAdd(day, -120, GETDATE()) AND Active=1'
	EXEC(@sql)
	PRINT(@sql)
	
	PRINT('flagging for notification to OGA ...')
	INSERT INTO [people_for_oga_to_disable] (person_id, sent_to_oga_date)
	SELECT person_id, null
	FROM [dbo].[people]
	WHERE last_login_date is not null AND last_login_date < DateAdd(day, -120, GETDATE()) AND active=1 AND person_id not in (
			select person_id from vw_service_accounts
	)
	PRINT('done')
	
	PRINT('Setting to Active = 0 ...')
	UPDATE [dbo].[people]
	SET active = 0
	WHERE last_login_date is not null AND last_login_date < DateAdd(day, -120, GETDATE()) AND active=1 AND person_id not in (
			select person_id from vw_service_accounts
	)
	PRINT('done')
	
	
	
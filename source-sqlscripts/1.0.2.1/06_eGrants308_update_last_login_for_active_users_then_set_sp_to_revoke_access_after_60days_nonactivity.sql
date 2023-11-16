USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_check_expired_people]    Script Date: 9/20/2023 10:34:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

	PRINT '----------- reset last login date for inactive people ----------' 
	UPDATE [dbo].[people]
	SET last_login_date = GETDATE()
	WHERE active = 1
	
GO


	
GO

PRINT '----------- Adjusting [dbo].[sp_egrants_maint_check_expired_people] to 120 day expiration ----------'

GO

ALTER   PROCEDURE [dbo].[sp_egrants_maint_check_expired_people]
AS
	PRINT '----------- Deactivating Users Who Have Not Recently Logged In ----------'
	PRINT 'Setting these people to Active = 0'
	
	DECLARE @sql varchar(max)
	SET @sql='select * from people WHERE last_login_date < DateAdd(day, -120, GETDATE()) AND Active=1'
	EXEC(@sql)
	PRINT(@sql)

	UPDATE [dbo].[people]
	SET active = 0
	WHERE last_login_date is not null AND last_login_date < DateAdd(day, -120, GETDATE()) AND active=1;
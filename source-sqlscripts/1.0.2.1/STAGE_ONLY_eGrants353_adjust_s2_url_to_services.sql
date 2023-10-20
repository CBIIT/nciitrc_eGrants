print('Checking for potential cleanup ...')

IF EXISTS (
	SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = '[dbo]' 
                 AND  TABLE_NAME = 'temp_micah_envurl_test_table'
	)
	BEGIN
		PRINT('temp_micah_envurl_test_table test table still exists ... cleaning it up') 
		DROP table temp_micah_envurl_test_table
		PRINT('removed.') 
	END
ELSE
	BEGIN
		PRINT('temp_micah_envurl_test_table was already cleaned up') 
	END
	
print('Cleanup complete.')

----------------------------------------------------

print('OK, actually converting the data.')

IF EXISTS
(
	-- old https://s2s.era.nih.gov/
	select * from [dbo].[EnvUrl] where URL like '%s2%'
)
	BEGIN
		PRINT('old s2s url with s2s.era.nih.gov discovered in table [EnvUrl]') 
		UPDATE [dbo].[EnvUrl] SET url=REPLACE(url,'https://s2s.era.nih.gov/','https://services.internal.stage.era.nih.gov')
			where URL like '%s2%'
		PRINT('updated to services.internal.era.nih.gov') 
	END
ELSE
	BEGIN
		PRINT('[dbo].[EnvUrl] was already already pointing everything to services.internal.stage.era.nih.gov') 
	END



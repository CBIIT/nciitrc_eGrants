print('Checking for potential cleanup ...')

IF EXISTS
(
	select OBJECT_ID (N'temp_micah_envurl_test_table', N'U')
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
		-- first "SPEC, "
		UPDATE [dbo].[EnvUrl] SET url=REPLACE(url,'https://s2s.era.nih.gov/','https://services.internal.dev.era.nih.gov/')
			where URL like '%s2%'
		-- second "SPEC"
		--UPDATE temp_micah_email_test_table SET email_cc=REPLACE(email_cc,'SPEC','')
		PRINT('updated to services.internal.era.nih.gov') 
	END
ELSE
	BEGIN
		PRINT('[dbo].[EnvUrl] was already already pointing everything to services.internal.era.nih.gov') 
	END



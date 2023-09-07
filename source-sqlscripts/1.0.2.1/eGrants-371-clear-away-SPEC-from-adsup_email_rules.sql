
print('Clearing out the old SPEC references from adsup_email_rules ...')

-- do this first !
UPDATE adsup_email_rules SET email_cc=REPLACE(email_cc,'SPEC, ','')
-- second "SPEC"
UPDATE adsup_email_rules SET email_cc=REPLACE(email_cc,'SPEC','')

-- clear away temporary table I used to test
IF OBJECT_ID (N'temp_micah_email_test_table', N'U') IS NOT NULL
	BEGIN
		PRINT('temp_micah_email_test_table test table still exists ... cleaning it up') 
		DROP table temp_micah_email_test_table
		PRINT('removed.') 
	END
ELSE
	BEGIN
		PRINT('temp_micah_email_test_table was already cleaned up') 
	END
	
print('Done.')
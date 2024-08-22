-- create table for emails that should be sent to OGA to be disabled
PRINT('checking for existence of table for emails that should be sent to OGA to be disabled ...')
PRINT('dbo.people_for_oga_to_disable')
IF EXISTS
(
	SELECT * 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE --TABLE_SCHEMA = 'TheSchema' AND
        TABLE_NAME = 'people_for_oga_to_disable'
)
	BEGIN
		PRINT('people_for_oga_to_disable stage table already exists') 
	END
ELSE
	BEGIN
		PRINT('people_for_oga_to_disable does not yet exist') 
		PRINT('adding ...')
			CREATE TABLE [dbo].[people_for_oga_to_disable](
				[person_id] [int] NOT NULL FOREIGN KEY REFERENCES people(person_id),
				[sent_to_oga_date] [datetime] NULL
			);
		PRINT('created new table people_for_oga_to_disable')
	END
PRINT('Done.')



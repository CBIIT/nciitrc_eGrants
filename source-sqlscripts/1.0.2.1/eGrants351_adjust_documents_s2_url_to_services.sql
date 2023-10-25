DECLARE @retVal int

SELECT @retVal = COUNT(*) 
FROM documents
WHERE url like 'https://s2s%'

print(concat('sum of old URLs in this DB is : ', @retVal))

if ( @retVal > 0)
	BEGIN
		print('found some old URLs ... cleaning up')
		update documents set URL= REPLACE(url,'https://s2s.era.nih.gov/','https://services.internal.era.nih.gov/')
			where URL like '%s2%'
	END
ELSE
	BEGIN
		print('didnt find any old s2s URLS so not updating anything')
	END
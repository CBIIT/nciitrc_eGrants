DECLARE @retVal int

SELECT @retVal = COUNT(*) 
FROM IMPP_Admin_Supplements_WIP
WHERE doc_url like '%s2s%'

print(concat('sum of old URLs in this DB is : ', @retVal)) -- currently 0

if ( @retVal > 0)
	BEGIN
		print('found some old URLs ... cleaning up')
		update IMPP_Admin_Supplements_WIP set doc_url= REPLACE(doc_url,'https://s2s.era.nih.gov/','https://services.internal.era.nih.gov/')
			where doc_url like '%s2s%'
	END
ELSE
	BEGIN
		print('didnt find any old s2s URLS so not updating anything')
	END
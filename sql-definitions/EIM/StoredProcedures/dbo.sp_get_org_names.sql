SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_get_org_names]
AS

BEGIN
-----------------------------------------
--for last_upd_date is NOT null
-----------------------------------------
--DROP table temp1
--DROP table temp2
--DROP table temp3
DROP table temp4

SELECT * 
INTO temp4
FROM OPENQUERY (IRDB, 'select external_org_id, org_name, created_date, last_upd_date
from external_orgs_mv
where org_name is not null')

--SELECT org_name, COUNT(*) cnt
--FROM temp4
--GROUP BY org_name
--HAVING COUNT(*) >1
--ORDER BY cnt DESC

DROP table temp5

SELECT org_name, MAX(CONVERT(varchar(23), last_upd_date, 121)) last_upd_date
INTO temp5
FROM temp4
WHERE last_upd_date IS NOT NULL
GROUP BY org_name
ORDER BY org_name 

--SELECT org_name, COUNT(*) cnt
--FROM temp5
--GROUP BY org_name
--HAVING COUNT(*) >1
--ORDER BY cnt DESC

--SELECT * FROM org_master--99583

INSERT INTO org_master (org_name)
SELECT ltrim(rtrim(org_name)) FROM temp5
WHERE ltrim(rtrim(org_name)) COLLATE database_default NOT IN (SELECT org_name FROM dbo.Org_Master)

--SELECT org_name, COUNT(*)
--FROM dbo.Org_Master
--GROUP BY Org_name
--HAVING COUNT(*) > 1
-----------------------------------------
--for last_upd_date IS null
-----------------------------------------
DROP table temp6

SELECT org_name, MAX(CONVERT(varchar(23), last_upd_date, 121)) last_upd_date
INTO temp6
FROM temp4
WHERE last_upd_date IS NULL
GROUP BY org_name
ORDER BY org_name 

--SELECT org_name, COUNT(*) cnt
--FROM temp6
--GROUP BY org_name
--HAVING COUNT(*) >1
--ORDER BY cnt DESC

INSERT INTO org_master (org_name)
SELECT ltrim(rtrim(org_name)) FROM temp5
WHERE ltrim(rtrim(org_name)) COLLATE database_default NOT IN (SELECT org_name FROM dbo.Org_Master)

--SELECT org_name, COUNT(*)
--FROM dbo.Org_Master
--GROUP BY Org_name
--HAVING COUNT(*) > 1

---update index for org_master table(added by leon 4/1/2016)
update Org_Master
set Org_Master.index_id=Org_Index.index_id
from Org_Index
where Org_Master.index_id is null and LEFT(org_name,1)=Org_Index.org_index

update Org_Master set index_id=1 where index_id is null

END

GO


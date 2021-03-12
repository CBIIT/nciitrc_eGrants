SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON










CREATE VIEW [dbo].[vw_grants_construct_fy_Mechanism_Admincode_serialNum] 
WITH SCHEMABINDING 
AS
SELECT   a.fy, g.serial_num, a.activity_code, g.admin_phs_org_code,
-- COALESCE(REPLICATE('0', 5 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, 
--g.serial_num), REPLICATE('0', 6 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, 
--g.serial_num)) as serial_num2,
--REPLICATE('0', 5 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, 
----g.serial_num) AS serial_num5,
--REPLICATE('0', 5 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, 
--g.serial_num) AS serial_num2,
CASE 
WHEN LEN(CONVERT(varchar, g.serial_num))=3 OR  LEN(CONVERT(varchar, g.serial_num))=4  THEN RIGHT('0000' + CONVERT(varchar, g.serial_num), 6) 
WHEN LEN(CONVERT(varchar, g.serial_num))=4 OR LEN(CONVERT(varchar, g.serial_num))=5 THEN RIGHT('0000' + CONVERT(varchar, g.serial_num), 6) 
WHEN LEN(CONVERT(varchar, g.serial_num))=4 OR LEN(CONVERT(varchar, g.serial_num))=5 THEN RIGHT('00000' + CONVERT(varchar, g.serial_num), 6) 

--WHEN LEN(CONVERT(varchar, g.serial_num))=3 THEN RIGHT('0000' + CONVERT(varchar, g.serial_num), 4) 
--WHEN LEN(CONVERT(varchar, g.serial_num))=4 THEN RIGHT('0000' + CONVERT(varchar, g.serial_num), 5) 
--WHEN LEN(CONVERT(varchar, g.serial_num))=4 THEN RIGHT('0000' + CONVERT(varchar, g.serial_num), 6) 

--WHEN LEN(CONVERT(varchar, g.serial_num))=5 THEN RIGHT('00000' + CONVERT(varchar, g.serial_num), 6) 
--WHEN LEN(CONVERT(varchar, g.serial_num))=4 THEN REPLICATE('0', 5 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, g.serial_num) 
 --WHEN LEN(CONVERT(varchar, g.serial_num))=4  THEN COALESCE(REPLICATE('0', 5 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, g.serial_num), REPLICATE('0', 6 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, g.serial_num))
--WHEN LEN(CONVERT(varchar, g.serial_num))=6 THEN substring(g.serial_num, patindex('%[^0]%',g.serial_num), 6)


--WHEN  LEN(CONVERT(varchar, g.serial_num))=6 THEN REPLICATE('0', 6 - LEN(CONVERT(varchar, g.serial_num))) + CONVERT(varchar, g.serial_num) 

--ELSE g.serial_num
END AS serial_num2,

 --RIGHT('0000' + CONVERT(varchar, g.serial_num), 5)


  COUNT_BIG(*) as cnt 
  FROM dbo.grants g
  INNER JOIN dbo.appls a ON g.grant_id  = a.grant_id 
	      WHERE FY IS NOT NULL AND  serial_num is not null and activity_code is not null and admin_phs_org_code is not null
		  GROUP BY fy, serial_num, activity_code, admin_phs_org_code

GO


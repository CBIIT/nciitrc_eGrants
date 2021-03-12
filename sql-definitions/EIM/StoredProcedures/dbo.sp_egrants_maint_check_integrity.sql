SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE sp_egrants_maint_check_integrity

AS

/*

1. NGA - many
2. Summary Statement 1
3. Application File  1
4. Financial Report 1
5. Greensheet PGM 1
6. Greensheet SPEC 1
7. JIT Info 1 problem URL
8. Change ?
9. Progress Final 1 problem URL
10. Final Invention Statement 1 problem URL

*/
--sample check
SELECT appl_id, count(*)
FROM egrants
WHERE category_name='Final Invention Statement' and created_by='impac'
GROUP BY appl_id
HAVING count(*) >1
GO


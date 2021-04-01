SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE VIEW dbo.vw_impp_fsrs_latest
AS

SELECT * FROM impp_fsrs_all
WHERE appl_id in (SELECT appl_id FROM impp_fsrs_all GROUP BY appl_id HAVING COUNT(appl_id)=1)
UNION
SELECT * FROM impp_fsrs_all
WHERE fsr_id IN(SELECT MAX(fsr_id) FROM impp_fsrs_all GROUP BY appl_id HAVING COUNT(appl_id)>1)














GO


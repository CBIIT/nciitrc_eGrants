SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE Grants_Released_Report
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from OPENQUERY(CIIP,'SELECT RESP_SPEC_FULL_NAME_CODE,ACTION_FY, 
			OCT_CNT AS "Oct Cnt", OCT_REL as "Oct Rel",
			Nov_CNT AS "Nov Cnt", NOV_REL as "Nov Rel",
			Dec_CNT AS "Dec Cnt", DEC_REL as "Dec Rel",
			JAN_CNT AS "Jan Cnt", JAN_REL as "Jan Rel",
			FEB_CNT AS "Feb Cnt", FEB_REL as "Feb Rel",
			MAR_CNT AS "Mar Cnt", MAR_REL as "Mar Rel",
			APR_CNT AS "Apr Cnt", APR_REL as "Apr Rel",
			MAY_CNT AS "May Cnt", MAY_REL as "May Rel",
			JUN_CNT AS "Jun Cnt", JUN_REL as "Jun Rel",
			JUL_CNT AS "Jul Cnt", JUL_REL as "Jul Rel",
			AUG_CNT AS "Aug Cnt", AUG_REL as "Aug Rel",
			SEP_CNT AS "Sep Cnt", SEP_REL as "Sep Rel",
			TOTAL_CNT AS "Total Count", TOTAL_REL as "Total Released",0,TOTL_WRKLD as "Total Workload"
			FROM OGA_OVERALL_WORKLOAD order by RESP_SPEC_FULL_NAME_CODE')

END

GO


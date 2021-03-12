SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_web_egrants_dashboard]

@act		varchar(20),
@idstr		varchar(20),
@ic			varchar(10),
@operator	varchar(50)

AS
/************************************************************************************************************/
/***									 							***/
/***	Procedure Name:  sp_web_egrants_dashboard					***/
/***	Description:egrants dashboard								***/
/***	Created:	08/12/2016		Leon							***/
/***	Modified:	09/15/2016		Leon							***/
/***	Modified:	09/29/2016		Leon	add new grants			***/
/***	Modified:	12/22/2016		Leon	simplified	for MVC		***/
/************************************************************************************************************/

SET NOCOUNT ON

DECLARE @person_id 			int
DECLARE @profile_id			int
DECLARE @separate			int
DECLARE @person_name		varchar(50)
DECLARE @first_name			varchar(20)
DECLARE @sql				varchar(800)
DECLARE @count				int
DECLARE @total_selections	int
--DECLARE @xmlout				varchar(max)
--DECLARE @X					Xml

DECLARE @USERID				varchar(50)
SET @USERID=@operator
--SET @USERID='OMAIRI'
--SET @USERID='BROWNELD'
--BROWNELD
--FISHERB
--GASTLEYK
--BIRKENJG
--BOUDJEDAJ

/**find profile_id **/
SET @profile_id=(select profile_id FROM profiles WHERE profile=@ic)
SET @person_id = (SELECT person_id FROM vw_people WHERE userid=@operator)

IF @act='set_assignment' GOTO set_assignment
IF @act='get_assignment' GOTO get_assignment

RETURN
--------------------
set_assignment:

CREATE TABLE #assignment(widget_id int)
SET @sql='INSERT #assignment SELECT WIDGET_ID FROM dbo.DB_Widget_Master WHERE WIDGET_ID in (' + @idstr + ')'
EXEC (@sql)

--DISABLE ALL PAST ASSIGNMENT
UPDATE dbo.DB_WIDGET_ASSIGNMENT 
SET end_date = GETDATE()
WHERE userid=@operator AND person_id=@person_id AND end_date IS NULL

--INSERT NEW WIDGET SELECTION TO ASSIGNMENT TABLE
INSERT dbo.DB_WIDGET_ASSIGNMENT(person_id,userid,widget_id,sortorder,start_date)
SELECT @person_id,@operator,w.widget_id,w.widget_id,GETDATE()
FROM #assignment a, DB_Widget_Master w
WHERE w.widget_id=a.widget_id

RETURN
-----------------------------------
get_assignment:

SELECT widget_id,widget_title,dbo.fn_get_widget_assigment(@USERID,widget_id) as selected 
FROM dbo.DB_Widget_Master 
WHERE end_date is null and widget_title<>'Audit Report'
UNION ALL
SELECT widget_id,widget_title,dbo.fn_get_widget_assigment(@USERID,widget_id) as selected 
FROM dbo.DB_Widget_Master 
WHERE end_date is null and widget_title='Audit Report' and @USERID in (select userid from people where position_id=8 or person_name='Jones, Robert')
ORDER BY widget_title

RETURN


GO


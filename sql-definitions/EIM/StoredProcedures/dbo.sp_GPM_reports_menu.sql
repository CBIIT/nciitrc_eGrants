SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_GPM_reports_menu]

@ic  			varchar(10),
@operator 		varchar(50)
/************************************************************************************************************/
/***                                                             ***/
/***  Procedure Name:  sp_GPM_reports_menu                       ***/
/***  Description:    dispaly all  GPM reports list               ***/
/***  Created:    09/21/2011  Dan                                 ***/
/***  Modified:   09/27/2011  Leon                                ***/
/***                                                              ***/
/************************************************************************************************************/

AS

declare
@user_name		varchar(50),
@profile_id		smallint,
@person_id		int,
@position_id	int,
@count			int,
@description	varchar(150)

/***find the profile_id**/
SELECT @profile_id=profile_id FROM profiles WHERE profile=@ic 

/***find the operator's person_id***/
SELECT @person_id =person_id FROM people WHERE userid=@operator

/***find the operator's @position_id***/
SELECT @position_id =position_id FROM people WHERE person_id=@person_id

/**check the permission**/
IF (select count(*) FROM GPM_Report_User WHERE user_id=@operator)<>1
BEGIN
SET @description='You do not have the permission to view this report.'
SELECT @description AS error from performance AS error_message FOR XML AUTO, ELEMENTS
END
ELSE
BEGIN
SELECT
1		AS tag, 
null		AS parent,
null		[reports!1!reports!element], 
null		[report!2!id!element], 
null		[report!2!name!element],		
null		[report!2!description!element]

UNION 

SELECT
2, 
1,
null,
rep_id,
name,
description

FROM  GPM_Reports ORDER BY [report!2!id!element]
FOR XML EXPLICIT
END

GO


SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_web_egrants_funding_doc_edit]

@act 			varchar(50),
@appl_id		int,
@document_id	int,
--@fy			int,
@ic				varchar(10),
@Operator		varchar(50)

AS
/**********************************************************************/
/***									 							***/
/***	Procedure Name: sp_web_egrants_funding_doc_edit				***/
/***	Description:edit funding document							***/
/***	Created:	02/06/2009	Leon								***/
/***	Modified:	11/22/2013	Leon								***/
/***	Modified:	11/22/2013	Leon 								***/
/***	Simplified:	10/15/2014	Leon 								***/
/***	Modified:	09/23/2016	Leon 	modified for MVC			***/
/**********************************************************************/

SET NOCOUNT ON

declare 
@profile_id 	int,
@person_id 		int,
@xmlout			varchar(max),
@X				Xml

/***find the profile_id**/
SELECT @profile_id=profile_id FROM profiles WHERE profile=LOWER(@ic)

/***find the operator's person_id***/
SELECT @person_id =person_id FROM people WHERE userid=@Operator and profile_id=@profile_id

--IF @act='' or @act is null or @act='to_qc' GOTO head
IF @act='to_delete' GOTO to_delete
IF @act='to_store' GOTO to_store
IF @act='to_edit' GOTO to_edit
------------------------------------------------------
to_edit:

UPDATE funding_documents SET stored_date=null, stored_by_person_id=null WHERE document_id=@document_id 

RETURN
-------------------------------------------------------
to_delete:

UPDATE funding_appls SET disabled_date=getdate(), disabled_by_person_id=@person_id 
WHERE document_id=@document_id and appl_id=@appl_id

RETURN
-------------------------------------------------------------
to_store:

UPDATE funding_documents SET stored_date=getdate(), stored_by_person_id=@person_id WHERE document_id=@document_id 

RETURN
-----------------------------------------------------------
GO


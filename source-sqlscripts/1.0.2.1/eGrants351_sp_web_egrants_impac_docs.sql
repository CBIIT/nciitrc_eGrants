USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_web_egrants_impac_docs]    Script Date: 10/25/2023 9:58:25 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_web_egrants_impac_docs]

@act			varchar(20),
@appl_id		int

AS
/***********************************************************************/
/***																***/
/***	Procedure Name: sp_y2013_egrants_impac_docs					***/
/***	Description:	find the impac docs by act					***/
/***	Created:	03/01/2013	Leon								***/
/***	Modified:	05/14/2015	Leon								***/
/***	Modified:	07/20/2015	Leon add rejection in fsr			***/
/***	Modified:	03/20/2017	Leon modified for MVC				***/
/***	Modified:	08/08/2018	Leon modified url					***/
/**********************************************************************/

SET NOCOUNT ON

declare @url			varchar(800)
declare @fsr_id			int
declare @impac_doc_type		varchar(10)

IF @act='fsr' GOTO fsr
IF @act='closeout' GOTO closeout
-----------------------
closeout:

SELECT
1 as tag,
appl_id, 
full_grant_num, 
null as fsr_seq_num,
null as accepted_date,
null as category_name,
null as created_date,
null as url
FROM vw_appls 
WHERE appl_id=@appl_id

UNION

SELECT
2,
null,
null,
null,
null,
Notification_Name,
convert(varchar,Created_date, 101), 
'run javascript to open Notification' 
FROM IMPP_CloseOut_Notification_All 
WHERE appl_id=@appl_id

RETURN
----------------------------
fsr:

set @impac_doc_type	='FSR'
set @url='https://services.internal.era.nih.gov/docservice/dataservices/document/once/keyId/'

SELECT
1	 as tag,
appl_id , 
full_grant_num, 
null as fsr_seq_num,
null as accepted_date,
null as category_name,
null as created_date,
null as url
FROM vw_appls
WHERE appl_id=@appl_id

UNION

SELECT
2,
null,
null,
fsr_seq_num,
convert(varchar,accepted_date,101), 
'Financial Report',
null,
@url + convert(varchar,fsr_id) +'/'+ @impac_doc_type
FROM impp_fsrs_all 
WHERE appl_id=@appl_id

UNION

SELECT 
3,
null,
null,
null,
null,
Notification_Name,
convert(varchar,Created_date, 101), 
'run javascript to open Notification' 
FROM dbo.IMPP_FFR_Notification_All 
WHERE appl_id=@appl_id

RETURN
-------------------------------

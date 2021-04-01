SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF

create PROCEDURE [dbo].[sp_web_egrants_head]

@ic			varchar(5),
@operator 	varchar(50)

AS
/**********************************************************************/
/***									 							***/
/***	Procedure Name:sp_web_egrants_head							***/
/***	Description:searching data for egrants head					***/
/***	Created:	01/16/2014	Leon								***/
/***	Modified:	01/27/2014	Leon								***/
/***	Modified:	02/21/2014	Leon								***/
/**********************************************************************/
SET NOCOUNT ON

declare @qc_cout int
declare @qc		 int

set @qc_cout = (select COUNT(qc_person_id) from egrants
where qc_date is not null and parent_id is null and qc_person_id=(select person_id from vw_people where userid=@operator))

if (@qc_cout =0 or @qc_cout is null) set @qc=0 else set @qc=1

select person_name,person_id,userid,position_id,application_type,can_egrants,can_mgt,can_docman,can_cft,can_admin ,@qc as 'can_qc',can_egrants_upload
from vw_people 
where userid=@operator


GO


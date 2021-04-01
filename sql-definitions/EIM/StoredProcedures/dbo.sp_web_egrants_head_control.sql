SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

create PROCEDURE [dbo].[sp_web_egrants_head_control]
 
@ic varchar(5),
@operator varchar(50)

AS
/******************************************************************************************/
/***												    								***/
/***	Procedure Name:sp_y2016_web_egrants_head_control					    		***/
/***	Description:searching user gloabl data for egrants head					    	***/
/***	Created:    06/10/2016	Frances													***/
/***	Modified:   06/13/2016  Frances  												***/
/***																					***/
/******************************************************************************************/
SET NOCOUNT ON

declare @qc_count	int
declare @qc			varchar(1)
declare @daydiff	int
declare @person_id	int
declare @qc_date    smalldatetime

--get person_id by operator
set @person_id = (select person_id from vw_people where userid=@operator)

--find how many docs to qc and how many days to qc
SELECT @qc_date=MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null and stored_date is null
IF @qc_date IS NOT NULL
BEGIN
SELECT @daydiff=DATEDIFF(day, getdate(), @qc_date)-1
END

select person_name,person_id,userid,position_id,application_type,can_egrants,can_mgt,can_docman,can_cft,can_admin,can_egrants_upload,@qc as 'can_qc',@daydiff as qc_days
from vw_people
where userid=@operator and [profile]=@ic and application_type='egrants'


GO


SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF



CREATE   PROCEDURE [dbo].[sp_web_egrants_user_type_check]

@ic						varchar(10),
@Operator				varchar(20),
@user_application_type	varchar(2) output

AS
-- =============================================
-- Author:	Leon
-- Create date: 2/22/2018
-- Description:	If User user is validate for egrants or docman
-- =============================================

declare @count			int
declare @docmancount	int

set @count=(select count(*) from vw_people where userid=@Operator and application_type='egrants')
IF @count=1 SET @user_application_type='e' ELSE set @user_application_type=null

--set @docmancount=(select count(*) from docman..vw_people where userid=@Operator and application_type='econtracts')
--IF @docmancount=1 and @count=1 SET @user_application_type=@user_application_type +'d' 
--IF @docmancount=1 and (@count is null or @count='') SET @user_application_type='d' 

--set @count=(select count(*) from vw_people where userid=@Operator and application_type='egrants')
--IF @count=1 SET @user_application_type='e' ELSE set @user_application_type=null

--set @count=(select count(*) from docman..vw_people where userid=@Operator and application_type='econtracts')
--IF @count=1 SET @user_application_type=@user_application_type +'d' 

select @user_application_type

return

GO


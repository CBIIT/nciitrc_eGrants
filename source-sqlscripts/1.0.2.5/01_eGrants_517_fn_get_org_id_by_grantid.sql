USE [EIM]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_get_org_id_by_applid_specific_year]    Script Date: 5/31/2024 1:13:26 PM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE OR ALTER        FUNCTION [dbo].[fn_get_org_id_by_grantid] (@grant_id int)
  
RETURNS int AS  

BEGIN

declare @pi_org_id  varchar(92)
set @pi_org_id=(
	select org_id from vw_grants vg
	inner join Org_Master om on vg.org_name = om.Org_name
	where vg.grant_id=@grant_id
)

RETURN @pi_org_id

END


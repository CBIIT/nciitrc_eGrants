SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE  FUNCTION [dbo].[fn_get_local_image_server] ()

RETURNS varchar(255) AS 

BEGIN 

declare @local_image_server varchar(255)

if @@SERVERNAME = 'NCIDB-D387-V\MSSQLEGRANTSD'
	set @local_image_server= dbo.fn_get_env_url('egrants_dev_web') 

else if @@SERVERNAME = 'NCIDB-Q389-V\MSSQLEGRANTSQ'
	set @local_image_server= dbo.fn_get_env_url('egrants_test_web') 

else if 
@@SERVERNAME = 'NCIDB-S390-V\MSSQLEGRANTSS'
	set @local_image_server= dbo.fn_get_env_url('egrants_stg_web') 

else if
@@SERVERNAME = 'NCIDB-S390-V\MSSQLEGRANTSS'
	set @local_image_server= dbo.fn_get_env_url('egrants_prod_web') 

RETURN @local_image_server

END


GO


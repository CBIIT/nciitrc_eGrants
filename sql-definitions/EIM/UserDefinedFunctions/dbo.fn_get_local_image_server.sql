SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF


CREATE       FUNCTION [dbo].[fn_get_local_image_server] ()

RETURNS varchar(255) AS 

BEGIN 

declare 
@locall_image_server   varchar(255),
@server_name varchar(255)


set @server_name = @@SERVERNAME

if @server_name = 'NCIDB-D387-V\MSSQLEGRANTSD'  --'NCIDB-Q356-V\MSSQLEGRANTSQ', 'NCIDB-P232-V\MSSQLEGRANTSP' ,'NCIDB-S222-V\MSSQLEGRANTSS'
set @locall_image_server='https://egrants-web-dev.nih.gov'


IF @server_name = 'NCIDB-Q356-V\MSSQLEGRANTSQ'
set @locall_image_server='https://egrants-web-test.nih.gov'


IF @server_name = 'NCIDB-S222-V\MSSQLEGRANTSS'
set @locall_image_server='https://egrants-web-stage.nih.gov'


IF @server_name = 'NCIDB-P232-V\MSSQLEGRANTSP'
set @locall_image_server='https://egrants.nih.gov'

RETURN @locall_image_server

END

GO


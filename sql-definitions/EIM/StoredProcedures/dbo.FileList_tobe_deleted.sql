SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
Create Procedure FileList_tobe_deleted
AS
BEGIN

-- =============================================================================
-- Author:		Imran Omair
-- Create date:		5/26/2011
-- Description:	IMPLEMENTATION OF RETENTION POLICY
-- 1/9/2013
-- =============================================================================
declare @query varchar(2000)
declare @cmd varchar(4000)
DECLARE @stmp 		varchar(50)
SET @stmp=getdate()
SET @stmp=replace(@stmp,' ','')
SET @stmp=replace(@stmp,':','_')
		
	SET @query='select ''mv '' + replace(b.url,''https://egrants-data.nci.nih.gov/'',''/egrants/'') + '' /egrants/scripts/obligated_ss/'' as shellcommand'
	SET @query=@query + ' from eim.dbo.bundles_doc a, eim.dbo.documents b'
	SET @query=@query + ' where a.appl_id=b.appl_id and a.document_id=b.document_id and a.category_id=6 and b.file_type <> ''pdf'''
	
	SET @Cmd= 'bcp "' + @query + '" queryout d:\util\scripts\ListAwardedLO_' +  @stmp + '.sh -c -T'
	print @cmd
	EXEC master..xp_cmdshell @cmd
			
END 

GO


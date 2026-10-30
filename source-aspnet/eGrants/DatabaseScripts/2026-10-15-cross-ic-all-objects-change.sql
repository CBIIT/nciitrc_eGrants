USE [EIM]
GO
SET NOCOUNT ON
GO

/*
Cross-IC change script aligned to the .NET app changes.

Key updates:
- Adds optional cross-IC context params to stored procedures.
- Normalizes effective IC to authority IC for cross-IC scope.
- Resolves acting person by userid first so non-NCI users still map correctly.
- Allows specialist baseline row-actions across IC when access scope is cross-IC specialist.
- Fixes fn_get_doc_url authority host derivation so viewer IC no longer breaks links.
*/

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_doc_create]

@ApplID             int,
@CategoryID         int,
@SubCategory        varchar(35)=null,
@DocDate            smalldatetime,
@file_type          varchar(5),
@Operator           varchar(50),
@ic                 varchar(10),
@DocumentID         int OUTPUT,
@access_scope       varchar(50)=null,
@authority_ic       varchar(10)=null

AS
SET NOCOUNT ON

DECLARE
@profile_id         smallint,
@person_id          int,
@url                varchar(300),
@effective_ic       varchar(10)

SET @url='data/funded2/nci/main/'
SET @Operator = LOWER(@Operator)
SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), NULLIF(@ic,''), @ic))

IF @access_scope='CrossIcSpecialistBaseline'
BEGIN
    SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), 'nci'))
END

SET @ic = @effective_ic

SELECT @profile_id=profile_id FROM profiles WHERE profile=@ic

SELECT TOP 1 @person_id = person_id
FROM vw_people
WHERE userid=@Operator
  AND (@profile_id IS NULL OR profile_id=@profile_id)

IF @person_id IS NULL
BEGIN
    SELECT TOP 1 @person_id = person_id
    FROM vw_people
    WHERE userid=@Operator
END

SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))

INSERT documents(appl_id,category_id,sub_category_name,document_date,file_type,created_date,created_by_person_id,qc_date,profile_id,qc_reason)
SELECT @ApplID,@CategoryID,ISNULL(@SubCategory,null),@DocDate, @file_type, getdate(), @person_id, getdate(), @profile_id, 'LocalScan'

SELECT @DocumentID = @@IDENTITY
UPDATE documents SET url=REPLACE(@url + convert(varchar,@DocumentID) + '.' + @file_type,' ', '') WHERE document_id=@DocumentID

EXEC sp_web_egrants_doc_transaction @DocumentID, @Operator,'created', null

SELECT @DocumentID
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_doc_modify]

@act                varchar(50),
@appl_id            int,
@category_id        int,
@sub_category       varchar(35),
@doc_date           varchar(10),
@docid_str          varchar(800),
@file_type          varchar(5),
@ic                 varchar(10),
@operator           varchar(50),
@access_scope       varchar(50)=null,
@authority_ic       varchar(10)=null

AS
SET NOCOUNT ON

DECLARE
@profile_id         smallint,
@person_id          int,
@specialist_name    varchar(100),
@doc_id             int,
@count              int,
@sql                varchar(850),
@url                varchar(300),
@effective_ic       varchar(10)

SET @operator = LOWER(@operator)
SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), NULLIF(@ic,''), @ic))

IF @access_scope='CrossIcSpecialistBaseline'
BEGIN
    SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), 'nci'))
END

SET @ic = @effective_ic

SET @profile_id =(SELECT profile_id FROM profiles WHERE [profile]=@ic)

SELECT TOP 1 @person_id = person_id
FROM vw_people
WHERE userid=@operator
  AND (@profile_id IS NULL OR profile_id=@profile_id)

IF @person_id IS NULL
BEGIN
    SELECT TOP 1 @person_id = person_id
    FROM vw_people
    WHERE userid=@operator
END

IF @act='to store all' or @act='to delete all'
BEGIN
CREATE TABLE #doc (doc_id int)
SET @sql='INSERT #doc SELECT document_id FROM documents WHERE document_id in (' + @docid_str + ')'
EXEC (@sql)
END
ELSE
BEGIN
SET @doc_id =convert(int,@docid_str)
END

IF @act='to_upload' GOTO to_upload
IF @act='to_update' GOTO to_update
IF @act='to store' GOTO to_store
IF @act='to delete' GOTO to_delete
IF @act='to store all' GOTO to_store_all
IF @act='to delete all' GOTO to_delete_all
IF @act='to restore' GOTO to_restore

to_upload:

SET @url='data/funded/nci/modify/'
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))

UPDATE documents
SET url=REPLACE(@url+ convert(varchar,documents.document_id) + '.' +@file_type,' ',''), file_type=@file_type, file_modified_date=getdate(), file_modified_by_person_id=@person_id, qc_date=getdate(), qc_person_id=null,qc_reason='Change',profile_id=@profile_id, stored_date=null, stored_by_person_id=null
FROM documents
WHERE document_id=@doc_id

EXEC sp_web_egrants_doc_transaction @doc_id,@Operator,'image_modified', null

RETURN

to_update:

IF @sub_category='null' or @sub_category='undefined' or @sub_category='' or @sub_category is null
BEGIN
SET @sub_category = null
UPDATE documents SET sub_category_name = null WHERE document_id = convert(int,@doc_id)
END

SET @sql='UPDATE documents SET '
IF @appl_id IS NOT NULL and @appl_id <> '' SET @sql=@sql+'appl_id='+convert(varchar,@appl_id)+', parent_id=null, '
IF @category_id IS NOT NULL and @category_id <> '' SET @sql=@sql+'category_id=' + convert(varchar,@category_id)+', '
IF @sub_category IS NOT NULL and @sub_category<>'' SET @sql=@sql+'sub_category_name='+char(39)+@sub_category+char(39)+', '
IF @doc_date IS NOT NULL and @doc_date<>'' SET @sql=@sql+'document_date=' +char(39)+convert(varchar,@doc_date)+char(39)+', '

SET @sql=@sql+'qc_reason='+char(39)+'Change'+char(39)+',  qc_person_id=null , qc_date='+char(39)+convert(varchar,getdate())+char(39)+', '
SET @sql=@sql+'modified_date='+char(39)+convert(varchar,getdate())+char(39)+', index_modified_by_person_id='+convert(varchar,@person_id)+', '
SET @sql=@sql+'profile_id=' +convert(varchar,@profile_id)+', stored_date=null, stored_by_person_id=null '
SET @sql=@sql+'WHERE document_id='+ convert(varchar,@doc_id)

EXEC (@sql)

EXEC sp_web_egrants_doc_transaction @doc_id, @operator, 'index_modified', null

RETURN

to_store:

UPDATE documents SET stored_date = getdate(), stored_by_person_id=@person_id, qc_date=null WHERE document_id=@doc_id
EXEC sp_web_egrants_doc_transaction @doc_id, @operator,'stored', null
RETURN

to_delete:

UPDATE documents SET disabled_date=getdate(), disabled_by_person_id=@person_id, qc_date=null, qc_person_id=null
WHERE document_id=@doc_id
EXEC sp_web_egrants_doc_transaction @doc_id, @operator,'disabled', null
RETURN

to_store_all:

SET @count=(SELECT count(*) FROM #doc)

UPDATE documents
SET stored_date=getdate(),stored_by_person_id=@person_id,qc_date=null
FROM  documents d, #doc
WHERE d.document_id=#doc.doc_id

INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description)
SELECT e.document_id, @operator, 'stored', full_grant_num, category_name, document_date, null
FROM  egrants e, #doc
WHERE e.document_id=#doc.doc_id

RETURN

to_delete_all:

SET @count=(SELECT count(*) FROM #doc)

UPDATE documents
SET disabled_date=getdate(),disabled_by_person_id=@person_id,qc_date=null,qc_person_id=null
FROM  documents d, #doc
WHERE d.document_id=#doc.doc_id

INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description)
SELECT e.document_id, @operator, 'deleted', full_grant_num, category_name, document_date, null
FROM  egrants e, #doc
WHERE e.document_id=#doc.doc_id

RETURN

to_restore:

EXEC sp_web_egrants_doc_url_restore @doc_id
EXEC sp_web_egrants_doc_transaction @doc_id, @Operator,'url_restored', null
RETURN
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_institutional_file_create]

@org_id             int,
@category_id        int,
@file_type          varchar(5),
@start_date         varchar(10),
@end_date           varchar(10),
@ic                 varchar(10),
@operator           varchar(50),
@comments           varchar(256),
@document_id        varchar(10) OUTPUT,
@access_scope       varchar(50)=null,
@authority_ic       varchar(10)=null

AS
SET NOCOUNT ON

DECLARE
@person_id          int,
@profile_id         int,
@doc_id             int,
@category_name      varchar(100),
@file_location      varchar(200),
@tobe_flag          char(1),
@effective_ic       varchar(10)

SET @operator = LOWER(@operator)
SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), NULLIF(@ic,''), @ic))

IF @access_scope='CrossIcSpecialistBaseline'
BEGIN
    SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), 'nci'))
END

SET @ic = @effective_ic

SET @profile_id=(select profile_id from profiles where [profile]=@ic)

SELECT TOP 1 @person_id = person_id
FROM vw_people
WHERE userid=@Operator
  AND (@profile_id IS NULL OR profile_id=@profile_id)

IF @person_id IS NULL
BEGIN
    SELECT TOP 1 @person_id = person_id
    FROM vw_people
    WHERE userid=@Operator
END

SET @file_location='/data/funded/nci/institutional/'
SET @file_type=SUBSTRING(@file_type,2,LEN(@file_type))
select @category_name= doctype_name , @tobe_flag = isNull(tobe_flagged,0) from Org_Categories where doctype_id=@category_id

IF @tobe_flag<>'1'
BEGIN
SET @start_date=null
SET @end_date=null
END

INSERT dbo.Org_Document(org_id,doctype_id,file_type,url,created_date,created_by_person_id,start_date_ShowFlag,end_date_showFlag,comments)
SELECT @org_id,@category_id,@file_type,'to be updated',getdate(),@person_id,ISNULL(@start_date,null),ISNULL(@end_date,null),@comments

SET @doc_id=@@IDENTITY

UPDATE dbo.Org_Document SET url=@file_location + convert(varchar,@doc_id) + '.' + @file_type WHERE document_id=@doc_id

SELECT @document_id=convert(varchar, @doc_id)

RETURN
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_institutional_file_update]

@category_id        int,
@start_date         varchar(10),
@end_date           varchar(10),
@ic                 varchar(10),
@operator           varchar(50),
@comments           varchar(256),
@document_id        varchar(10),
@access_scope       varchar(50)=null,
@authority_ic       varchar(10)=null

AS
SET NOCOUNT ON

DECLARE
@person_id          int,
@profile_id         int,
@doc_id             int,
@category_name      varchar(100),
@file_location      varchar(200),
@tobe_flag          char(1),
@comments_req       bit,
@effective_ic       varchar(10)

SET @operator = LOWER(@operator)
SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), NULLIF(@ic,''), @ic))

IF @access_scope='CrossIcSpecialistBaseline'
BEGIN
    SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), 'nci'))
END

SET @ic = @effective_ic

SET @profile_id=(select profile_id from profiles where [profile]=@ic)

SELECT TOP 1 @person_id = person_id
FROM vw_people
WHERE userid=@Operator
  AND (@profile_id IS NULL OR profile_id=@profile_id)

IF @person_id IS NULL
BEGIN
    SELECT TOP 1 @person_id = person_id
    FROM vw_people
    WHERE userid=@Operator
END

SET @file_location='/data/funded/nci/institutional/'
select @category_name= doctype_name , @tobe_flag = isNull(tobe_flagged,0), @comments_req = comments_required from Org_Categories where doctype_id=@category_id

IF @tobe_flag<>'1'
BEGIN
SET @start_date=null
SET @end_date=null
END

IF @comments_req <> 1
BEGIN
SET @comments = null
END

UPDATE dbo.Org_Document
SET doctype_id = @category_id,
updated_date = getdate(),
updated_by_person_id = @person_id,
start_date_ShowFlag=ISNULL(@start_date,null),
end_date_showFlag=ISNULL(@end_date,null),
comments = @comments
where
document_id = @document_id

RETURN
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[sp_web_egrants_search_by_appl_id]

@appl_id            int,
@search_type        varchar(50) = null,
@category_list      varchar(500) = null,
@ic                 varchar(10),
@operator           varchar(50),
@access_scope       varchar(50)=null,
@authority_ic       varchar(10)=null

AS
SET NOCOUNT ON

DECLARE @profile_id         int
DECLARE @profile            varchar(10)
DECLARE @person_id          int
DECLARE @position_id        int
DECLARE @sql                varchar(800)
DECLARE @count              int
DECLARE @effective_ic       varchar(10)
DECLARE @cross_specialist   bit

SET @cross_specialist = CASE WHEN @access_scope='CrossIcSpecialistBaseline' THEN 1 ELSE 0 END
SET @operator = LOWER(@operator)
SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), NULLIF(@ic,''), @ic))

IF @access_scope IN ('CrossIcProgramBaseline','CrossIcSpecialistBaseline')
BEGIN
    SET @effective_ic = LOWER(COALESCE(NULLIF(@authority_ic,''), 'nci'))
END

SET @ic = @effective_ic

SELECT @profile_id=profile_id FROM profiles WHERE profile=@IC

SELECT TOP 1 @person_id = person_id
FROM vw_people
WHERE userid=@operator
  AND (@profile_id IS NULL OR profile_id=@profile_id)

IF @person_id IS NULL
BEGIN
    SELECT TOP 1 @person_id = person_id
    FROM vw_people
    WHERE userid=@operator
END

SELECT @position_id=position_id FROM vw_people WHERE person_id=@person_id
IF @position_id IS NULL SET @position_id = 1

CREATE TABLE #d (document_id int primary key)
CREATE TABLE #c (category_id int primary key)

IF @search_type='by_qc' or @search_type='by_page' GOTO by_qc
IF @search_type ='by_str' and (@category_list is null or @category_list='') GOTO by_applid
IF @category_list is not null and @category_list = 'All' or @category_list = 'all' GOTO by_cats_all
IF @category_list is not null and @category_list <> 'All' and @category_list <>'all' GOTO by_cats
IF @search_type is null and @category_list is null GOTO by_applid

by_applid:
SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id)
EXEC (@sql)
GOTO OUTPUT_ALL

by_cats:
SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id) + ' and category_id in ('+ @category_list+')'
EXEC (@sql)
GOTO OUTPUT

by_cats_all:
SET @sql='insert #d (document_id) select document_id from egrants where appl_id=' + convert(varchar,@appl_id)
EXEC (@sql)
GOTO OUTPUT_ALL

by_qc:
SET @sql='insert #d (document_id) select document_id from egrants where appl_id='+ convert(varchar,@appl_id) + ' and qc_person_id=' + convert(varchar,@person_id) + ' and qc_date is not null'
EXEC (@sql)
GOTO OUTPUT

OUTPUT:
SELECT
ic,
grant_id,
appl_id,
full_grant_num,
document_id,
convert(varchar,document_date,101) as document_date,
document_name,
document_date as doc_date,
category_id,
category_name,
isnull(sub_category_name, null) as sub_category_name,
created_by,
convert(varchar,created_date,101) as created_date,
modified_by,
CONVERT(VARCHAR(20), modified_date, 100) as modified_date,
file_modified_by,
CONVERT(VARCHAR(20), file_modified_date, 100) as file_modified_date,
problem_msg,
problem_reported_by,
page_count,
CASE
WHEN category_name='Financial Report' or category_name='FFR' THEN dbo.fn_appl_fsr_count(appl_id)
ELSE 0
END as fsr_count,
attachment_count,
dbo.fn_appl_CloseOut_NotCount (@appl_id) as closeout_notcount,
dbo.fn_appl_frc_destroyed(@appl_id) as frc_destroyed,
dbo.fn_get_doc_url(document_id, @ic) as url,
convert(varchar, qc_date, 101) as qc_date,
qc_reason,
qc_person_id,
qc_person_name,
CASE WHEN category_name in('Greensheet DMC','Greensheet PGM','Greensheet SPEC') THEN 'n'
ELSE 'y' END AS can_qc,
CASE WHEN can_upload='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_upload,
CASE WHEN can_modify_index='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_modify_index,
CASE WHEN can_delete='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_delete,
CASE WHEN can_restore='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_restore,
CASE WHEN can_store='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_store
FROM egrants WHERE appl_id=@appl_id and document_id in (select document_id from #d)

return

OUTPUT_ALL:
SELECT
ic,
grant_id,
appl_id,
full_grant_num collate SQL_Latin1_General_CP1_CI_AS,
document_id,
convert(varchar,document_date,101) as document_date,
document_name,
document_date as doc_date,
category_id,
category_name,
isnull(sub_category_name, null) as sub_category_name,
created_by,
convert(varchar,created_date,101) as created_date,
modified_by,
CONVERT(VARCHAR(20), modified_date, 100) as modified_date,
file_modified_by,
CONVERT(VARCHAR(20), file_modified_date, 100) as file_modified_date,
problem_msg,
problem_reported_by,
page_count,
CASE
WHEN category_name='Financial Report' or category_name='FFR' THEN dbo.fn_appl_fsr_count(appl_id)
ELSE 0
END as fsr_count,
attachment_count,
dbo.fn_appl_CloseOut_NotCount (@appl_id) as closeout_notcount,
dbo.fn_appl_frc_destroyed(@appl_id) as frc_destroyed,
dbo.fn_get_doc_url(document_id, @ic) as url,
convert(varchar, qc_date, 101) as qc_date,
qc_reason,
qc_person_id,
qc_person_name,
CASE WHEN category_name in('Greensheet DMC','Greensheet PGM','Greensheet SPEC') THEN 'n'
ELSE 'y' END AS can_qc,
CASE WHEN can_upload='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_upload,
CASE WHEN can_modify_index='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_modify_index,
CASE WHEN can_delete='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_delete,
CASE WHEN can_restore='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_restore,
CASE WHEN can_store='y' and (@position_id>1 and (ic=@IC OR @cross_specialist=1)) THEN 'y' ELSE 'n' END AS can_store
FROM egrants WHERE appl_id=@appl_id and document_id in (select document_id from #d)

UNION ALL

SELECT * FROM fn_egrants_web_funding_documents(@appl_id)

UNION ALL

SELECT * FROM fn_egrants_web_notification_documents(@appl_id)

return
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER FUNCTION [dbo].[fn_get_doc_url] (@document_id int, @IC varchar(5))

RETURNS varchar(800) AS 

BEGIN 

declare 
@local_image_server    varchar(100),
@impac_image_server                varchar(100),
@url                                                               varchar(800),
@doc_url                                                      varchar(800),
@impac_doc_type                                      varchar(10),
@created_by                                                varchar(20),
@created_date                             datetime,
@full_grant_num                                        varchar(20),
@fsr_id                                                          int,
@nga_rpt_seq_num                    int,
@appl_id                                                      int,
@nga_id                                                                      int,
@server_name   varchar(255),
@s2server_name varchar(255),
@sql_report_server varchar(255),
@era_server varchar(255),
@authority_ic varchar(5)

set @server_name = @@SERVERNAME
set @IC=LOWER(@IC)
set @authority_ic='nci'

if @server_name = 'NCIDB-D387-V\MSSQLEGRANTSD'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_dev')+@authority_ic+'.nih.gov/'
set @impac_image_server=[dbo].[fn_get_env_url]('egrants_dev_impac')+@authority_ic+'.nih.gov/'
set @s2server_name =[dbo].[fn_get_env_url]('s2server_era_nih')
set @sql_report_server = 'https://ncidb-d387-v.nci.nih.gov/ReportServer_MSSQLEGRANTSD/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')
END

IF @server_name = 'NCIDB-Q389-V\MSSQLEGRANTSQ'
BEGIN
set @local_image_server=  [dbo].[fn_get_env_url]('egrants_img_test')+@authority_ic+'.nih.gov/'
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_test_impac')+@authority_ic+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih')
set @sql_report_server = 'https://ncidb-q389-v.nci.nih.gov/ReportServer_MSSQLEGRANTSQ/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')

END

IF @server_name = 'NCIDB-S390-V\MSSQLEGRANTSS'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_stg')+@authority_ic+'.nih.gov/'
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_stg_impac')+@authority_ic+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih')
set @sql_report_server = 'https://ncidb-s390-v.nci.nih.gov/ReportServer_MSSQLEGRANTSS/'
set @era_server =[dbo].[fn_get_env_url]('apps_era_nih')

END

IF @server_name = 'NCIDB-P391-V\MSSQLEGRANTSP'
BEGIN
set @local_image_server=[dbo].[fn_get_env_url]('egrants_img_prod')+ @authority_ic+'.nih.gov/'
set @impac_image_server= [dbo].[fn_get_env_url]('egrants_prod_impac')+@authority_ic+'.nih.gov/'
set @s2server_name = [dbo].[fn_get_env_url]('s2server_era_nih')
set @sql_report_server = 'http://ncidb-p391-v.nci.nih.gov/ReportServer/Pages/ReportViewer.aspx?/'
set @era_server = [dbo].[fn_get_env_url]('apps_era_nih')
END

SELECT 
@url=[url],
@appl_id=appl_id, 
@nga_id=nga_id,
@impac_doc_type=impac_doc_type,
@created_by=created_by, 
@created_date=created_date,
@full_grant_num=full_grant_num,
@nga_rpt_seq_num=nga_rpt_seq_num
FROM egrants 
WHERE document_id=@document_id

IF @url is not null 
BEGIN
              IF (substring(@url,1,5)='data/') SET @doc_url= @local_image_server + @url 
              IF (substring(@url,1,5)='/data') SET @doc_url= @local_image_server + SUBSTRING(@url,2,1000)
              IF @url like @s2server_name +'%' SET @doc_url = @url
              IF (select PATINDEX('%documentviewer%',@url))>0 SET @doc_url = @url
              IF @url like 'http%' AND @doc_url is null SET @doc_url = @url
END

IF @url is null
BEGIN
IF (@impac_doc_type='ENG' and @created_date>'04/10/2007' and @authority_ic='nci') SET @doc_url=@s2server_name+ dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type
IF (@impac_doc_type='ENG' and @created_date<'04/10/2007' and @authority_ic='nci') SET @doc_url=@s2server_name+ dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + 'NGA'
IF @impac_doc_type in('FSR','AWS','PRM','MYP','CLD','WBR','PRACPC','PRANCE','PRACOV') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/' + @impac_doc_type
IF @impac_doc_type in ('IGI','DMS','FIS','FPA','FRM','FPR','JIT','MPR','SS','FRPPR','IRPPR','IPA') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_once_applid') + convert(varchar,@appl_id) + '/' + @impac_doc_type
IF @impac_doc_type in('DM','PGM','SPEC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&EXTERNAL=TRUE&APPL_ID=' + CONVERT(varchar, @appl_id)+ '&GRANT_ID='+@full_grant_num
IF @impac_doc_type in('PRM_NEW','IRAM','FRM_NEW') SET @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') + CONVERT(varchar,@nga_rpt_seq_num) + '/CORAM'
IF @impac_doc_type in ('PRAM') SET @doc_url=@era_server + 'docservice/DocService/GetDocument?docType=RAM&keyId=' + convert(varchar,@nga_rpt_seq_num)
IF @impac_doc_type in('REV') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'
IF @impac_doc_type in('PGMREV') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)+ '&ORACLE_ID=NCIGAB'
IF @impac_doc_type in('ARC') SET @doc_url=@impac_image_server+'greensheets/retrievegreensheet.do?GS_GROUP_TYPE='+ @impac_doc_type +'&AGT_ID=' + CONVERT(varchar, @nga_rpt_seq_num)
IF @impac_doc_type='FSR'
BEGIN
    select @fsr_id=(select max(fsr_id) from impp_fsrs_all where appl_id=@appl_id)
    set @doc_url=@s2server_name + dbo.fn_get_env_url('docservice_data_keyid') +convert(varchar,@fsr_id) +'/'+ @impac_doc_type
END

END

RETURN @doc_url

END
GO

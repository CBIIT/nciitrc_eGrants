SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON

create PROCEDURE [dbo].[sp_web_management_doc_transaction_report]

@transaction_type	varchar(20),		
@startdate			varchar(10),
@enddate			varchar(10),
@date_range			varchar(15),
@person_id			int,
--@doc_id			int,
@ic					varchar(10),
@operator			varchar(50)

 AS
/******************************************************************************************************************/
/***									 																		***/
/***	Procedure Name:sp_web_egrants_doc_transaction_report													***/
/***	Description:Find documents procession info by operator name, time rank  or style.						***/
/***	Created:	03/07/2014	Leon																			***/
/***	Modified:	03/07/2014	Leon																			***/
/***	Modified:	07/18/2016	Leon modified it for MVC														***/
/***	Modified:	07/22/2016	Leon modified it for MVC														***/
/******************************************************************************************************************/

SET NOCOUNT ON

DECLARE
@profile_id			smallint,
@person_name		varchar(50),
@total				int,
@day 				int,
@date 				int,
@days 				int,
@month				int,
@year 				int,
@today  			smalldatetime,
@start_date			date,
@end_date			date

SET @profile_id=(select profile_id from profiles  where profile =@ic)
SET @person_name=(select person_name from vw_people where person_id=@person_id)

DECLARE @report table(transaction_type varchar(20), document_id int,full_grant_num varchar(50),category_name varchar(50),person_name varchar(50),url varchar(200), transaction_date varchar(10))

--to get start_data e and end_date
IF @date_range is not null and @date_range<>'' GOTO get_date 
Else 
BEGIN
IF @startdate is not null and @startdate<>'' SET @start_date=CONVERT(date, @startdate)
IF @enddate is not null and @enddate<>'' SET @end_date=CONVERT(date, @enddate)
GOTO show_report
END
--------------------------------
get_date:--find searching date range

/**today **/
SET @today =getdate()
SET @day=DAY(@today)
SET @month=MONTH(@today)
SET @date =DATEPART(dw, @today)

/**today **/
IF @date_range='Today'
BEGIN
SET @start_date=convert(date,@today) 
SET @end_date=dateadd(d,1,@start_date)
END

/**this week **/
IF @date_range='This_Week'
BEGIN
SET @start_date=dateadd(d,-@date+1,@today)
SET @end_date=dateadd(d,6,@start_date)
END

/*last tweek**/
IF @date_range='Last_Week'
BEGIN
SET @start_date=dateadd(d,-@date-6,@today)
SET @end_date=dateadd(d,6,@start_date)
END

/** this month**/
IF @date_range='This_Month'
BEGIN
SET @start_date=dateadd(d,-@day+1,@today)
SET @end_date=dateadd(d,-@day,dateadd(m,1,@today))
END

/**last month**/
IF @date_range='Last_Month'
BEGIN
SET @start_date=dateadd(d,-@day+1,@today)
SET @start_date=dateadd(m,-1,@start_date)
SET @end_date=dateadd(d,-@day,@today)
END

GOTO show_report
------------------------------------
show_report:

IF @transaction_type='all' GOTO search_all
IF @transaction_type='index modified' GOTO index_modified
IF @transaction_type='created' GOTO created
IF @transaction_type='deleted' GOTO deleted
IF @transaction_type='image modified' GOTO image_modified
IF @transaction_type='stored' GOTO stored
--IF @transaction_type='to_restore' GOTO to_restore
----------------------------------------------
search_all:

DECLARE @return_message	varchar(800)
SET @return_message=''

DECLARE 
@created_total	int, 
@uploaded_total int, 
@modified_total int, 
@deleted_total	int,
@stored_total	int

--@created_pages int, 
--@uploaded_pages int, 
--@deleted_pages int 
--@stored_pages int, 

SET @created_total=ISNULL((SELECT count(document_id)  FROM documents WHERE  created_by_person_id=@person_id and (created_date>@start_date and created_date<@end_date)),0)
SET @uploaded_total=ISNULL((SELECT count(document_id) FROM documents WHERE file_modified_by_person_id=@person_id and (file_modified_date>@start_date and file_modified_date<@end_date)),0)
SET @modified_total=ISNULL((SELECT count(document_id) FROM documents WHERE  index_modified_by_person_id=@person_id and (modified_date>@start_date and modified_date<@end_date)),0)
SET @stored_total=ISNULL((SELECT count(document_id) FROM documents WHERE  stored_by_person_id=@person_id and (stored_date>@start_date and stored_date<@end_date)),0)
SET @deleted_total=ISNULL((SELECT count(document_id) FROM documents WHERE disabled_by_person_id=@person_id and (disabled_date>@start_date and disabled_date<@end_date)),0)

--SET @created_pages=ISNULL((SELECT sum(page_count)  FROM documents WHERE  page_count is not null and created_by_person_id=@person_id and (created_date>@start_date and created_date<@end_date)),0)
--SET @uploaded_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and file_modified_by_person_id=@person_id and (file_modified_date>@start_date and file_modified_date<@end_date)),0)
--SET @stored_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and stored_by_person_id=@person_id and (stored_date>@start_date and stored_date<@end_date)),0)
--SET @deleted_pages=ISNULL((SELECT sum(page_count) FROM documents WHERE  page_count is not null and disabled_by_person_id=@person_id and (disabled_date>@start_date and disabled_date<@end_date)),0)

IF @created_total<>0 or @uploaded_total<>0 or @modified_total<>0 or @deleted_total<>0 or @stored_total<>0
BEGIN

SET @return_message='From '+ convert(varchar,@start_date)+' to '+convert(varchar,@end_date)+', '+@person_name+ ' has'

IF @created_total>0 SET @return_message=@return_message+ ' created '+ convert(varchar,@created_total) +' files,'
IF @uploaded_total>0 SET @return_message=@return_message+ ' uploaded '+ convert(varchar,@uploaded_total) +' files,' 
IF @modified_total>0 SET @return_message=@return_message+ ' modified index for '+ convert(varchar,@modified_total) +' files,'
IF @deleted_total>0 SET @return_message=@return_message+ ' deleted '+ convert(varchar,@deleted_total) +' files,'
IF @stored_total>0 SET @return_message=@return_message+ ' stored '+ convert(varchar,@stored_total) +' files'

END

SELECT @return_message as return_notice From performance as return_message

RETURN
-------------------------------------
created:

IF @person_id=0  ---for ic only
BEGIN	

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type, document_id,full_grant_num,category_name,'IMPAC',dbo.fn_get_doc_url(d.document_id,@ic), convert(varchar,d.created_date,101) 
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (d.created_by_person_id is not null and d.created_by_person_id=p.person_id) and  (d.created_date >@start_date and  d.created_date<@end_date )

END
ELSE
BEGIN		---for personal

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type,document_id,full_grant_num,category_name,ISNULL(person_name, userid),dbo.fn_get_doc_url(d.document_id,@ic), convert(varchar,d.created_date,101) 
FROM documents d, vw_appls a, categories c, people p
WHERE d.appl_id=a.appl_id and d.category_id=c.category_id and d.profile_id=@profile_id and d.created_by_person_id=@person_id and d.created_by_person_id=p.person_id and (d.created_date>@start_date and d.created_date<@end_date)

END

SELECT * FROM @report order by category_name

RETURN
-------------------------------------
index_modified:

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type, document_id, full_grant_num, category_name,ISNULL(person_name, userid),dbo.fn_get_doc_url(d.document_id,@ic),convert(varchar,d.modified_date,101) 
FROM documents d, vw_appls a, categories c, people p
WHERE  d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and d.index_modified_by_person_id=@person_id and d.index_modified_by_person_id=p.person_id and (d.modified_date>@start_date and d.modified_date<@end_date)

SELECT * FROM @report order by category_name

RETURN
-------------------------------------
deleted:

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type, document_id,full_grant_num,category_name,ISNULL(person_name, userid),dbo.fn_get_doc_url(d.document_id,@ic),convert(varchar,d.disabled_date,101)
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and d.disabled_by_person_id=@person_id and d.disabled_by_person_id=p.person_id and (d.disabled_date>@start_date and d.disabled_date<@end_date)

SELECT * FROM @report order by category_name

RETURN
-------------------------------------
image_modified:

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type,document_id,full_grant_num,category_name,ISNULL(person_name, userid),dbo.fn_get_doc_url(d.document_id,@ic),convert(varchar,file_modified_date,101)	
FROM documents d, vw_appls a, categories c, people p
WHERE d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and file_modified_by_person_id=@person_id and  file_modified_by_person_id=p.person_id and (file_modified_date>@start_date and file_modified_date<@end_date) 


SELECT * FROM @report order by category_name

RETURN
-------------------------------------
stored:

INSERT @report(transaction_type,document_id,full_grant_num,category_name,person_name,url, transaction_date)
SELECT @transaction_type, document_id,full_grant_num,category_name,ISNULL(person_name, userid),dbo.fn_get_doc_url(d.document_id,@ic), convert(varchar,stored_date,101)	
FROM documents d,  vw_appls a, categories c, people p
WHERE  d.profile_id=@profile_id and d.appl_id=a.appl_id and d.category_id=c.category_id and (stored_by_person_id=@person_id and stored_by_person_id=p.person_id) and  (stored_date>@start_date and stored_date<@end_date) 

SELECT * FROM @report order by category_name

RETURN
-------------------------------------
--to_restore:

--restore deleted documen
--UPDATE documents SET  disabled_date=null, disabled_by_person_id=null WHERE document_id=@doc_id

--RETURN


GO


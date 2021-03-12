SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE PROCEDURE [dbo].[sp_y2013_egrants_qc_assignment_tobedeleted]

@act			varchar(50),
@person_id		int,
@qc_person_id	int,
@qc_reason		varchar(50),
@percent		int,
@inst			varchar(10),
@operator 		varchar(50)

AS
/************************************************************************************************************/
/***									 									***/
/***	Procedure Name: sp_y2013_egrants_assignment							***/
/***	Description:	assign documents to qc or to specialist				***/
/***	Created:	06/16/2005	Leon										***/
/***	Modified:	03/28/2013	Leon										***/
/***	QC bypass not applicable to this proc hareesh 4/9/14				***/
/***	simplified	10/08/14	Leon										***/
/************************************************************************************************************/

SET NOCOUNT ON

DECLARE 
@count				int,
@profile_id			smallint,
@person_name		varchar(50),
@qc_person_name		varchar(20),
@sql				varchar(500),
@xmlout				varchar(max),
@X					Xml

SELECT @profile_id=profile_id FROM profiles WHERE profile=@inst	

IF @act is null or @act=''  GOTO header

/**
IF @act='auto'
BEGIN
SET @qc_reason='Error'
GOTO to_auto
END
**/

IF @act='remove'
GOTO to_remove

IF @act='add'
GOTO to_add

IF @act='route'
GOTO to_route

----------------------
to_auto:

INSERT quality_control (qc_reason, person_id, effort)  SELECT @qc_reason, @qc_person_id,100 

GOTO header

RETURN
---------------------
to_remove:

DELETE FROM quality_control WHERE qc_reason=@qc_reason and person_id=@person_id

GOTO header

RETURN 
----------------------
to_add:

INSERT quality_control (qc_reason, person_id, effort)  SELECT @qc_reason, @qc_person_id,100

GOTO header

RETURN
----------------------
to_route:

/** find all documents to route**/
CREATE TABLE #doc (doc_id int)
SET @sql='insert #doc select top ' + convert(varchar,@percent)+ ' percent document_id from egrants where qc_date is not null and stored_date is null and qc_reason is not null and parent_id is null and qc_person_id='+convert(varchar,@person_id)+ ' order by qc_date'
EXEC (@sql)

--route documents
UPDATE documents SET qc_person_id=@qc_person_id, qc_date=getdate() WHERE document_id in (SELECT doc_id FROM #doc)

--insert document transaction info
SELECT @person_name=ISNULL(person_name,userid) FROM vw_people WHERE person_id=@person_id
SELECT @qc_person_name =ISNULL(person_name, userid) FROM vw_people WHERE person_id=@qc_person_id	

INSERT documents_transactions (document_id, operator, action_type, full_grant_num, category_name, document_date, description) 
SELECT e.document_id, @operator, 'routed', full_grant_num, category_name, document_date, 'from '+ @person_name + ' to ' + @qc_person_name
FROM egrants e, #doc  WHERE e.document_id=#doc.doc_id

GOTO header

RETURN
--------------------------------------------------------------------
header:

/**return current assignments info**/
SET @X = (
SELECT
qc_reason,
person_id,
person_name,
userid 
FROM vw_quality_control  as qc_person
WHERE profile_id=@profile_id and (qc_reason is not null and qc_reason <>'')
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/**current qc report**/
SET @X = (
SELECT
count(*) as qc_count,
qc_person_id,
ISNULL(qc_person_name, qc_userid) AS qc_person_name,
AVG(datediff(d,qc_date,getdate())) as qc_date
FROM vw_people v LEFT OUTER JOIN egrants as qc_report ON qc_report.qc_person_id=v.person_id
WHERE qc_date is not null and  qc_person_id is not null and qc_reason is not null and stored_date is null and parent_id IS null
GROUP BY qc_person_id,qc_person_name,qc_userid 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/****return qc_reason info**/
SET @X = (
SELECT
DISTINCT
qc_reason 
FROM vw_quality_control as qc_reason
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/****return user with documents to qc**/
SET @X = (
SELECT
person_id,
person_name
FROM vw_people as qc_person
WHERE person_id in(
SELECT distinct qc_person_id FROM egrants WHERE qc_date is not null and stored_date is null and profile_id=@profile_id and parent_id is null)
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/****return all user data**/
SET @X = (
SELECT
person_id,
ISNULL(person_name, userid) AS person_name
FROM vw_people as person
WHERE profile_id=@profile_id and application_type='egrants' and position_id>=2 and person_name not in(
'NCI OGA PROGRESS REPORT',
'CA ERA NOTIFICATIONS',
'nciogastage',
'system'
)
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout

/** find user info***/
--declare @qc_type		int				--commented by hareesh on 12/22/2014 5:30pm
--declare @qc_date		smalldatetime	--commented by hareesh on 12/22/2014 5:30pm
--declare @daydiff 		int				--commented by hareesh on 12/22/2014 5:30pm

declare @first_name		varchar(50)
declare @separate		int

SET @person_id=(select person_id from vw_people where userid=@operator)


/*--commented by hareesh on 12/22/2014 5:30pm
SELECT @qc_date=MIN(qc_date) FROM egrants WHERE qc_person_id=@person_id and qc_date is not null and parent_id is null
IF @qc_date IS NOT NULL
BEGIN
SELECT @daydiff=DATEDIFF(day, getdate(), @qc_date)-1
END

SET @qc_type=(select qc_type from profiles AS qc_type where profile_id=@profile_id)--  for xml auto, elements
*/

---find user info
--return user's info
SET @person_id=(select person_id from vw_people where userid=@operator)
SET @count=(select COUNT(*) from vw_people WHERE userid=@operator and application_type='egrants' and can_egrants_upload='y' ) 

IF @count=1
BEGIN
SET @X = (
SELECT *, convert(varchar,getdate(),101) as today
FROM vw_people  as user_info
WHERE person_id=@person_id
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
END
ELSE
BEGIN
SELECT null
END

--set @person_name=(select person_name from vw_people where person_id=@person_id)
--IF @person_name<>'' and @person_name is not null 
--BEGIN
--select  @separate=PATINDEX('%,%', @person_name)
--select  @first_name=LEFT(@person_name,@separate-1 )
--END 

--added by hareesh on 12/22/2014 5:30pm
--return user's info
--SELECT person_id, person_id, person_name,ISNULL(@first_name, NULL) as first_name,userid,profile_id,[profile] as ic,
--admin_phs_org_code,position_id,convert(varchar,getdate(),101) as today,can_egrants,can_egrants_upload,can_cft,can_mgt,can_docman,can_admin
--FROM vw_people 
--WHERE userid=@operator and application_type='egrants' and can_egrants_upload='y'  


/*--commented by hareesh on 12/22/2014 5:30pm
SET @X = (
SELECT person_id, person_name,ISNULL(@first_name, NULL)as first_name,userid,profile_id, 
[profile] as ic,@qc_type AS qc_type,@daydiff as qc_days,application_type AS user_type,
position_id,can_egrants,can_econ_upload,can_cft,can_mgt,can_docman,can_admin
FROM vw_people AS user_info 
WHERE userid=@operator 
FOR XML AUTO, TYPE, ELEMENTS
)
select @xmlout = cast(@X as varchar(max))
select @xmlout
*/

GO


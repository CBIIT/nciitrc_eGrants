SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE PROCEDURE [dbo].[sp_egrants_route]

AS

--EXEC sp_egrants_mail

/**all of the documents created by system shouldn't go to qc
UPDATE documents 
SET qc_date=null, qc_person_id=null, qc_reason=null, created_by_person_id=1899
FROM documents 
WHERE profile_id=1 and qc_date is not null and qc_person_id is not null and qc_reason ='Email' and category_id=315 and created_by_person_id is null
**/

--all of the documents belong to L40 & L30 shouldn't go to qc
UPDATE documents 
SET stored_date= getdate(), stored_by_person_id=created_by_person_id, qc_date=null, qc_reason=null
FROM documents 
WHERE profile_id=1 and qc_date is not null and qc_person_id is null and appl_id in (select appl_id from appls where activity_code in('L30','L40'))

---for NIDDK and NIBIB, user who created the document will be also to QC it except Fax, Error and email
UPDATE documents 
SET qc_person_id=created_by_person_id WHERE document_id in(
SELECT document_id  
FROM egrants 
WHERE profile_id in(2,4) and stored_date is null and  qc_person_id is null and qc_date is not null and qc_reason not in('Fax','Error','Email') and created_by_person_id not in(530, 510,1987,1899)
)

---for NIAID and all IC who doesn't  want to sent new documents to QC it except Fax, Error and email
UPDATE documents 
SET qc_person_id=null, qc_date=null WHERE document_id in(
SELECT document_id 
FROM egrants 
WHERE profile_id=9 and stored_date is null and disabled_date is null and qc_date is not null and  qc_reason not in('Fax','Error','Email') ---and created_by_person_id not in(530, 510,1987)
)

--get all quality control data
CREATE TABLE #q (profile_id int, qc_reason varchar(20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS, person_id int, doc_count int)
INSERT #q
SELECT q.profile_id,q.qc_reason,q.person_id,count(document_id)
FROM vw_quality_control q LEFT OUTER JOIN egrants d ON q.profile_id=d.profile_id and q.person_id=d.qc_person_id and q.qc_reason=d.qc_reason and d.qc_date is not null 
GROUP BY q.profile_id,q.qc_reason,q.person_id

--get the QC person who has less doc count to QC fro each qc_reason in each IC 
CREATE TABLE  #p (profile_id int,qc_reason varchar(20) COLLATE SQL_Latin1_General_Pref_CP1_CI_AS,person_id int,doc_count int)
INSERT #p
SELECT #q.profile_id, #q.qc_reason, #q.person_id,#q.doc_count 
FROM #q INNER JOIN (select profile_id, qc_reason, min(doc_count) doc_count FROM  #q GROUP BY profile_id,qc_reason)m
ON #q.profile_id=m.profile_id and #q.qc_reason=m.qc_reason and #q.doc_count=m.doc_count
ORDER BY  #q.profile_id,#q.qc_reason,#q.doc_count asc

--update documents table
UPDATE documents
SET qc_person_id=p.person_id
FROM #p p, documents d
WHERE d.profile_id=p.profile_id and d.qc_reason=p.qc_reason and d.qc_date is not null and d.disabled_date is null and d.qc_person_id is null and parent_id is null

--set up arra_flag

/*
update grants set arra_flag='y'
WHERE grant_id IN
(
select distinct grant_id from appls where arra_flag='y'
UNION 
select distinct grant_id from egrants where category_name='Recovery Act'
)
and ISNULL(arra_flag,'n')!='y'
*/

GO


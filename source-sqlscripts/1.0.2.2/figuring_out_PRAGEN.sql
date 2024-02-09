select * from categories where category_name like '%pr%'

select * from categories where impac_doc_type_code like '%pr%'
select * from categories where impac_doc_type_code like '%pragen%'
select * from categories where category_id = 382	-- PRACOV

select * from categories where category_id in (67, 382,634)	-- might be different in other environments

select package from categories group by package

-- this gets nothing
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id ')  

-- pa_requests_t
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_t pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id ')  

-- pa_requests_t, removing doc_type_code from WHERE, displaying d.doc_type_code
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id, doc_type_code FROM OPENQUERY(IRDB, '
select pa.appl_id, d.doc_type_code, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_t pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id ')  

-- join PA_REQUESTS_T.KEY_ID (to what ..?)
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id, doc_type_code FROM OPENQUERY(IRDB, '
select pa.appl_id, d.doc_type_code, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_t pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id ')  

-- oh snap, did I need that pa_requests_mv table ?
-- reset and pull in pa_requests_t
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history, pa_requests_t t
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  

-- ok, what is in that new t table ?
SELECT * FROM OPENQUERY(IRDB, '
select t.* from doc_available_mv d, pa_requests_mv pa, pa_history_mv history, pa_requests_t t
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  

-- t contains PA_REQUEST_ID, APPL_ID, REQUEST_TYPE_ID, PA_REQUEST_STATUS_CODE, CREATED_DATE, CREATOR_ID, LAST_UPD_DATE, LAST_UPD_ID

-- do something with key ID ?
-- this gets no records
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv
history, pa_requests_t t
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id
and t.appl_id = HISTORY.PA_HISTORY_ID')  

-- projecting t.pa_request_id
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id, PA_REQUEST_ID FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID, t.PA_REQUEST_ID
from doc_available_mv d, pa_requests_mv pa, pa_history_mv history, pa_requests_t t
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  



-- this is where David just said to replace PRACOV with PRAGEN
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
WHERE pa.request_type_id =5 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id ')  

-- changed request_type_id to 10 .. gets back stuff ... this looks all spaghetti
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id ')  

-- do I even need to join pa_requests_t ?

-- David says yes, really

--SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
--select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID
--from doc_available_mv d, pa_requests_mv pa, pa_history_mv history, pa_requests_t pa
--WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB''
--and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id ')  

SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID from doc_available_mv d, pa_requests_mv pa, pa_history_mv history, pa_requests_t pa
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN'' and d.doc_key_id = pa.pa_request_id  and t.appl_id = pa.appl_id')  

-- this works
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID
from doc_available_mv d, pa_requests_mv pa,pa_history_mv history, pa_requests_t t
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN''
and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  

-- adding to projection
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID, t.pa_request_id
from doc_available_mv d, pa_requests_mv pa,pa_history_mv history, pa_requests_t t
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN''
and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  

-- this is where David says:
-- You query PA_REQUESTS_T with the Appl ID to fetch the PA_REQUEST_ID.  You take that PA_REQUEST_ID to join to DOC_AVAILABLE and join by DOC_KEY_ID

-- ERROR
SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID
from doc_available_mv d, pa_requests_mv pa,pa_history_mv history, pa_requests_t t, DOC_AVAILABLE da
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN''
and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id and da.doc_key_id = t.pa_request_id')  

SELECT APPL_ID, DOCUMENT_DATE,keyId,Request_id FROM OPENQUERY(IRDB, '
select pa.appl_id, history.action_date as DOCUMENT_DATE,HISTORY.PA_HISTORY_ID as keyId,HISTORY.REQUEST_ID
from doc_available_mv d, pa_requests_mv pa,pa_history_mv history, pa_requests_t t, DOC_AVAILABLE da
WHERE pa.request_type_id =10 and pa.pa_request_id = history.request_id and history.action_code=''SUB'' and d.doc_type_code =''PRAGEN''
and d.doc_key_id = pa.pa_request_id and t.appl_id = pa.appl_id')  




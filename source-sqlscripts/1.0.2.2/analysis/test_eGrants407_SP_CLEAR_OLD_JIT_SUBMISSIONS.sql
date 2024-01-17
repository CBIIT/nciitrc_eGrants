
select TOP 100 * from documents where appl_id = 10239114

select TOP 100 * from documents where document_id = 41643741 for json PATH

select TOP 100 * from documents where document_id = 41643741

select TOP 100 * from documents where qc_reason is not null

select * from categories where category_name like '%FFR%'
	-- category name is FFR in categories table but sub_category_name is FFR Rejection


--exec SP_CREATE_EGRANTS_DOCUMENT_NEW 
--@DOCID VARCHAR(10), --If there is a doc id and an appl_id this means to replace existing doc
--@CAT VARCHAR(100),	--Category Name
--@APPID VARCHAR(10),--
--@PROFILEID smallint,
--@DD VARCHAR(10),	--Document Date is email recieved date
--@UID VARCHAR(50),	--Sender user id
--@FT VARCHAR(5),		--
--@QCFLAG VARCHAR(3),		-- its something like 'yes'
--@SUB VARCHAR(35)

-- note: I dont know how to test QC flag ... try yes ?
	-- in VBscript this is called 'move to QC' and it gets set to 'yes' if applid is 0 or something is OFF about it

exec SP_CREATE_EGRANTS_DOCUMENT_NEW  41643741, 'FFR', 10239114, 1, '2023-02-28T00:00:00', 'caeranotifications', 'txt', 'no', 'FFR Rejection'

select TOP 100 * from documents where document_id = 41643741
--Success	42090855

select * from categories where category_name like '%era%'

select TOP 100 * from documents where document_id = 41643741

insert into Documents (url, appl_id, stamp, category_id, file_type, profile_id, document_date, created_date, added_date, disabled_date, created_by_person_id,
	disabled_by_person_id, uid, is_destroyed, sub_category_name)
	VALUES ('data/funded2/nci/main/999test1.txt', 10239114, '0x000000002B9C92FF', 481, 'txt', 1, GETDATE(), GETDATE(), GETDATE(), 3319, 3319, 'test_uid', 'testX', 0, 'JIT Submitted')
	--Cannot insert an explicit value into a timestamp column. Use INSERT with a column list to exclude the timestamp column, or insert a DEFAULT into the timestamp column.
	-- stamp is the timestamp column ... so try leaving that out ...

insert into Documents (url, appl_id, category_id, file_type, profile_id, document_date, created_date, added_date, disabled_date, created_by_person_id,
	disabled_by_person_id, [uid], is_destroyed, sub_category_name)
	VALUES ('data/funded2/nci/main/999test1.txt', 10239114, 481, 'txt', 1, GETDATE(), GETDATE(), GETDATE(), GETDATE(), 3319, 3319, 'test_uid',  0, 'JIT Submitted')

insert into Documents (url, appl_id, category_id, file_type, profile_id, document_date, created_date, added_date, disabled_date, created_by_person_id,
	disabled_by_person_id, [uid], is_destroyed, sub_category_name)
	VALUES ('data/funded2/nci/main/999test2.txt', 10239114, 481, 'txt', 1, GETDATE(), GETDATE(), GETDATE(), GETDATE(), 3319, 3319, 'test_uid',  0, 'JIT Submitted')

	select * from documents where appl_id = 10239114 and sub_category_name like '%JIT_Submitted'
	--42090859
	--42090860

	-- now try deleting one ... pass one of the doc_ids
	exec SP_CLEAR_OLD_JIT_SUBMISSIONS 42090859

	--It deleted the old one

	--clean up
	delete from documents where document_id = 42090859
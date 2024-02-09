/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [document_id]
      ,[url]
      ,[page_count]
      ,[appl_id]
      ,[stamp]
      ,[category_id]
      ,[document_size]
      ,[problem_msg]
      ,[file_type]
      ,[profile_id]
      ,[corrupted]
      ,[alias]
      ,[inventoried]
      ,[qc_reason]
      ,[document_date]
      ,[created_date]
      ,[modified_date]
      ,[added_date]
      ,[stored_date]
      ,[file_modified_date]
      ,[qc_date]
      ,[disabled_date]
      ,[qc_person_id]
      ,[created_by_person_id]
      ,[stored_by_person_id]
      ,[index_modified_by_person_id]
      ,[file_modified_by_person_id]
      ,[disabled_by_person_id]
      ,[problem_reported_by_person_id]
      ,[nga_id]
      ,[external_upload_id]
      ,[mail_upload_id]
      ,[status_id]
      ,[uid]
      ,[aws_id]
      ,[impp_doc_id]
      ,[processed_date]
      ,[nga_rpt_seq_num]
      ,[is_destroyed]
      ,[control_id]
      ,[parent_id]
      ,[sub_category_name]
  FROM [EIM].[dbo].[documents]
  where sub_category_name like '%JIT%'
  group by sub_category_name


  select * from categories where category_name like '%otificatio%'
	--481 'eRA Notification'

	-- derive this query somehow from this  "exec SP_CREATE_EGRANTS_DOCUMENT_NEW '"&documentid&"','"&category&"','"&applid&"','"&profileid&"','"&docdt&"',
	--	'"&v_SenderID&"','"&V_flType&"','"&movetoqc&"','"&subcat&"'"

	select TOP 100 g.grant_id, d.appl_id, c.category_name, d.sub_category_name
		from grants g
		inner join appls a on a.grant_id = g.grant_id
		inner join documents d on d.appl_id = a.appl_id
		inner join categories c on c.category_id = d.category_id
		where --c.category_name = 'eRA Notification'
			--AND 
			d.sub_category_name like '%JIT%'

	-- use grant example Id of 1544613
	select TOP 100 d.document_id, g.grant_id, d.appl_id, c.category_name, d.sub_category_name
		from grants g
		inner join appls a on a.grant_id = g.grant_id
		inner join documents d on d.appl_id = a.appl_id
		inner join categories c on c.category_id = d.category_id
		where --c.category_name = 'eRA Notification'
			--AND 
			g.grant_id = 1544613
			and
			--d.sub_category_name like '%JIT%Sub'
			--and
			d.document_id != 38406708

	-- if d is getting deleted, rearrange this query around
		select d.*
		from documents d
		inner join appls a on d.appl_id = a.appl_id
		inner join grants g on a.grant_id = g.grant_id
		inner join categories c on d.category_id = c.category_id
		where g.grant_id = 1544613 and
			c.category_name = 'eRA Notification'
			and
			d.sub_category_name like '%JIT%Sub'
			and
			d.document_id != 38406708

		-- so if I pass 38406708 to SP_CLEAR_OLD_JIT_SUBMISSIONS, it shouldn't actually delete anything
		--EXEC SP_CLEAR_OLD_JIT_SUBMISSIONS 38406708

	-- passing in grant ID and document ID :
	delete d
		from documents d
		inner join appls a on d.appl_id = a.appl_id
		inner join grants g on a.grant_id = g.grant_id
		inner join categories c on d.category_id = c.category_id
		where g.grant_id = 1544613 and
			c.category_name = 'eRA Notification'
			and
			d.sub_category_name like '%JIT%Sub'
			and
			d.document_id != 38406708


	DECLARE @DOCID VARCHAR(10)
	SET @DOCID = 38406708
	DECLARE @GrantId int
	select @GrantId = a.grant_id
		from appls a
		inner join documents d on d.appl_id = a.appl_id
		where d.document_id = @DOCID 
	print(CONCAT('grantId = ',@GrantId))

  /****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP (1000) [sub_category_name]
  FROM [EIM].[dbo].[documents]
  where sub_category_name like '%JIT%'
  group by sub_category_name







  select TOP 1000 sub_category_name from eGrants e where e.sub_category_name like '%JIT%'
    group by sub_category_name





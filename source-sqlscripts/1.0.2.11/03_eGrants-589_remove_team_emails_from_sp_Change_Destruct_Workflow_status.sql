USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_Change_Destruct_Workflow_status]    Script Date: 9/9/2024 10:53:46 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

/**********************************************************************************************************
**									 					
**	Procedure Name: [sp_Change_Destruct_Workflow_status]
**	Description: This process creates entry in dbo.IMPAC_DESTRUCT_WORKFLOW_EMAIL and change status by changing data in dbo.IMPAC_DESTRUCTED_APPL
**		Three Parameters : WorkFlow_id which is an year, submitter_id,reserved word 'next' or 'Previous'
		Next will move this process forward and Previous will move this to backward. Means an administrator  can reseubmit this workflow to a person already submitted (previous one).
		The Normal course is forward.
**	Usage : 	sp_Change_Destruct_Workflow_status 2014 408 'Next'

---APPL_STATUS_GRP_DESCRIP 
--0.Excluded or Retained (for various reasons) from deletion either by a user or by a proc
--1.Waiting : waiting for approval or Just downloaded from impac
-- Status 2 through 4 has been reserved for the user workflow.
--5.System will locate all the documents and create a command file to delete on file server then mark DB deleted
--6.System will FTP command file to sile server and show a wait status
--7.FTP process will FTP a result file from file server that will have Applid and document id which is been deleted Then system will change status to 7
--Deletion Complete : Deletion of documents completed, Links are unlinked, notice on website created etc....
--Note: Can there be three stages and same admin is assigned as admin to all stages?
--Answer No : If OGA want this to be handled by one person, then no need to create three steps of checking, one stage one person.
--It violates the basic principle of checking and double checking by various person
***********************************************************************************************************/

ALTER PROCEDURE [dbo].[sp_Change_Destruct_Workflow_status]

@workflow_id			int,  --year
@Admin_id				int,	--submitter
@Direction				varchar(30)  -- Possible values could be Next or Previous

AS
SET NOCOUNT ON

DECLARE
@current_status_code	smallint,
@current_sequence_code	smallint,
@next_status_code		smallint,
@next_sequence_code		smallint,
@next_admin_id			int,
@next_admin_email		varchar(100),
@next_admin_name		varchar(50),
@current_Admin_name		varchar(50),
@email NVARCHAR(100),
@Returncode int, 
@body1 nvarchar (800)


--THERE MUST NOT BE DUPS FOR GIVEN PARAMETER 
SELECT @current_status_code=DESTRUCT_PROCESS_STATUS_CODE, @current_sequence_code=WorkFlow_sequence 
FROM dbo.IMPAC_Destruction_Process_WorkFlow
WHERE ID=@workflow_id and Admin_start_date <= GETDATE() and Admin_end_date is null AND  ADMIN_ID=@Admin_id

IF (@Direction='Next')
BEGIN
  IF EXISTS(SELECT * FROM dbo.IMPAC_Destruction_Process_WorkFlow WHERE WorkFlow_sequence > @current_sequence_code and ID=@workflow_id and Admin_start_date <= GETDATE() and Admin_end_date is null)
  BEGIN
	set @next_sequence_code= @current_sequence_code + 1		--Re Open to the Next in the Sequence 
	
	SELECT @next_status_code=DESTRUCT_PROCESS_STATUS_CODE,@next_admin_id=Admin_ID FROM dbo.IMPAC_Destruction_Process_WorkFlow
	WHERE ID=@workflow_id and Admin_start_date <= GETDATE() and Admin_end_date is null AND WorkFlow_sequence=@next_sequence_code	

	SELECT @next_admin_email=EMAIL, @next_admin_name=PERSON_NAME FROM people WHERE person_id=@next_admin_id AND profile_id=1 AND end_date IS NULL AND start_date IS NOT NULL
	
	INSERT dbo.IMPAC_DESTRUCT_WORKFLOW_EMAIL(workflow_submitter,workflow_submit_date,route_to_email,route_to_person_name)
	VALUES(@Admin_id,GETDATE(),@next_admin_email,@next_admin_name)
	
	UPDATE dbo.IMPAC_DESTRUCTED_APPL SET DESTRUCT_PROCESS_STATUS_CODE=@next_status_code
	WHERE YEAR(dbo.IMPAC_DESTRUCTED_APPL.EGRANTS_CREATED_DATE)=@workflow_id
	AND dbo.IMPAC_DESTRUCTED_APPL.DESTRUCT_PROCESS_STATUS_CODE=@current_status_code

	SELECT @current_Admin_name=PERSON_NAME FROM people WHERE person_id=@Admin_id AND profile_id=1 AND end_date IS NULL AND start_date IS NOT NULL
	
	SET @body1 = '<b>DO NOT REPLY --> System generated Email.</b><br><br>Dear '+@next_admin_name+',<br><br>'+@current_Admin_name+' has completed excluding grants from ImpacII Annual Cleanup list. Now the entire Data set is here for your review.<br>
		You can further Exclude grants for eGrants Cleanup process by marking them under various exceptions and/or submit them <br>
		to the next person in workflow. You can also reject this exclusion and submit it back to '+@current_Admin_name+' for further exclusion.<br><br>'
	
	--print @body1

	begin try
	EXEC @Returncode = msdb.dbo.sp_send_dbmail
    @profile_name = 'DBMailProfile',
    @recipients = @next_admin_email,
    @blind_copy_recipients ='egrantsdevs@mail.nih.gov',
    @body_format ='HTML',
    @importance ='High',
    @subject = 'eDRAM submission of Retained grants List',
    @body = @body1 ;
	end try
	begin catch
		select @@ERROR as Err
	end catch
		
  END
END
ELSE
  BEGIN
  IF EXISTS(SELECT * FROM dbo.IMPAC_Destruction_Process_WorkFlow WHERE WorkFlow_sequence < @current_sequence_code and ID=@workflow_id and Admin_start_date <= GETDATE() and Admin_end_date is null)
  BEGIN
	set @next_sequence_code= @current_sequence_code - 1		--Re Open to the previous Admin		
	
	SELECT @next_status_code=DESTRUCT_PROCESS_STATUS_CODE,@next_admin_id=Admin_ID FROM dbo.IMPAC_Destruction_Process_WorkFlow
	WHERE ID=@workflow_id and Admin_start_date <= GETDATE() and Admin_end_date is null AND WorkFlow_sequence=@next_sequence_code	

	SELECT @next_admin_email=EMAIL, @next_admin_name=PERSON_NAME FROM people WHERE person_id=@next_admin_id AND profile_id=1 AND end_date IS NULL AND start_date IS NOT NULL
	
	INSERT dbo.IMPAC_DESTRUCT_WORKFLOW_EMAIL(workflow_submitter,workflow_submit_date,route_to_email,route_to_person_name)
	VALUES(@Admin_id,GETDATE(),@next_admin_email,@next_admin_name)
	
	UPDATE dbo.IMPAC_DESTRUCTED_APPL SET DESTRUCT_PROCESS_STATUS_CODE=@next_status_code
	WHERE YEAR(dbo.IMPAC_DESTRUCTED_APPL.EGRANTS_CREATED_DATE)=@workflow_id
	AND dbo.IMPAC_DESTRUCTED_APPL.DESTRUCT_PROCESS_STATUS_CODE=@current_status_code
	
	SELECT @current_Admin_name=PERSON_NAME FROM people WHERE person_id=@Admin_id AND profile_id=1 AND end_date IS NULL AND start_date IS NOT NULL
	IF (@next_sequence_code=1)
		SET @body1='<b>DO NOT REPLY --> System generated Email.</b><br><br>Dear '+@next_admin_name+',<br><br>'+@current_Admin_name+' has re-submitted back the grants (from ImpacII Annual Cleanup list) for you to re-consider the exclusion or retention <br>
			you have created. Now the entire Data set is here for your review again.<br>You can further Exclude grants for eGrants Cleanup process by marking them under various exceptions and/or submit them again<br>
			to the next person in workflow.<br>'
	ELSE
		SET @body1 = '<b>DO NOT REPLY --> System generated Email.</b><br><br>Dear '+@next_admin_name+',<br><br> '+@current_Admin_name+' has re-submitted back the grants (from ImpacII Annual Cleanup list) for you to re-consider the exclusion or retention <br>
			you have created. Now the entire Data set is here for your review again.<br>You can further Exclude grants for eGrants Cleanup process by marking them under various exceptions and/or submit them again<br>
			to the next person in workflow. You can also reject this exclusion and submit it back to previous user in workflow for further review or correction.<br>'

	--print @body1

	Begin try
	EXEC @Returncode= msdb.dbo.sp_send_dbmail
    @profile_name = 'DBMailProfile',
    @recipients = @next_admin_email,  
    @blind_copy_recipients ='egrantsdevs@mail.nih.gov',   
    @body_format ='HTML',
    @importance ='High',
    @subject = 'eDRAM Rejection of Retained grants List',
    @body = @body1  ;
    end try
    Begin catch
		Select @@ERROR as err
	End catch
  END
END


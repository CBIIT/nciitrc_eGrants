SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE PROCEDURE [dbo].[sp_test_adm_Files]
AS


SET NOCOUNT ON;
BEGIN
declare 
@Categoryid int,
@CreatedbyPersonId int,
@CreatedByPersonEmail int,
@CreatedByPersonIdProfileId int,
@CreatedByEmailProfileId int,
@Documentid int

    SELECT @Categoryid=CATEGORY_ID FROM categories 
    
    SELECT @CreatedbyPersonId=PERSON_ID,@CreatedByPersonIdProfileId=profile_id FROM PEOPLE WHERE USERID = 'test'
    --SELECT @CreatedByPersonEmail=PERSON_ID,@CreatedByEmailProfileId=profile_id FROM PEOPLE WHERE USERID = 'email'
	
	INSERT documents(appl_id,document_date,file_type,category_id,created_date,created_by_person_id,profile_id, uid)
	values(10100202, 'test', 'email' ,@Categoryid,GETDATE(),ISNULL(@CreatedbyPersonId,@CreatedByPersonEmail),ISNULL(@CreatedByPersonIdProfileId,@CreatedByEmailProfileId),ISNULL('test','email') )
	SET @Documentid=@@identity	

	UPDATE documents SET url='data/funded2/nci/main/'+CAST(@Documentid AS VARCHAR)+'.'
	WHERE document_id=@Documentid
	
	UPDATE documents SET url='data/funded2/nci/main/'+CAST(@Documentid AS VARCHAR)+'.'
	WHERE document_id=909087687686876
	
	SELECT @Documentid as ABC
	
END


GO


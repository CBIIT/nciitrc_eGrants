USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[SP_CLEAR_OLD_JIT_SUBMISSIONS]    Script Date: 12/1/2023 10:37 AM ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



-- =============================================
-- Author:		copied from <Imran Omair>'s SP_CREATE_EGRANTS_DOCUMENT_NEW
-- Create date: <12/1/2023>

-- Can only be one ‘eRA Notification: JIT Submitted’ document for each grant number

-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[SP_CLEAR_OLD_JIT_SUBMISSIONS]

@DOCID VARCHAR(10)

AS
	SET NOCOUNT ON;
BEGIN
	DECLARE @GrantId int

	select @GrantId = a.grant_id
		from appls a
		inner join documents d on d.appl_id = a.appl_id
		where d.document_id = @DOCID 

	delete d
	--select d.*
		from documents d
		inner join appls a on d.appl_id = a.appl_id
		inner join grants g on a.grant_id = g.grant_id
		inner join categories c on d.category_id = c.category_id
		where g.grant_id = @GrantId and
			c.category_name = 'eRA Notification'
			and
			d.sub_category_name like '%JIT%Submitted%'
			--d.sub_category_name like '%JIT%Sub'
			and
			d.document_id != @DOCID

END



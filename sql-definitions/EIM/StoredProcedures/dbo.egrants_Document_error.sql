SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =============================================
-- Author:		Joel Friedman
-- Create date: 05/08/2013
-- Description:	Automate Report error for selected docuiment
-- =============================================
CREATE PROCEDURE [dbo].[egrants_Document_error]
	-- Add the parameters for the stored procedure here
	@doc_id int, 
	@reason	varchar(250)
AS
BEGIN

	SET NOCOUNT ON;

	declare @qc_id	int,
			@ic		int
	
	select @ic=profile_id from documents where document_id=@doc_id
	
	select @qc_id = person_id from vw_quality_control where qc_reason='Error' and @ic=profile_id
	
    update documents  
    SET qc_person_id=@qc_id,
    qc_reason='Error',
    qc_date = getdate(),
    stored_date = null,
    problem_reported_by_person_id = 1875,	-- egrants
    problem_msg = @reason
	WHERE document_id = @doc_id
    
END

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =====================================================================================================
-- Author:			Joel Friedman
-- Create date:		05/15/2013
-- Description:		Create a list of missing pFR. PFR is a local file to eGrants this is not an 
--					ImpacII document or any other link. In document table for a PFR if the url is 
--					missing/blank/null this means we do not have document in our system.
-- =====================================================================================================
CREATE PROCEDURE sp_Create_Missing_Document_List 
	@category_id	int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare	@impac	int
				
	select @impac = person_id from people where person_name ='impac'
	
    select appl_id from documents where category_id=@category_id and url is null and created_by_person_id <>  @impac
    
END

GO


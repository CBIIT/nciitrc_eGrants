USE [EIM]
GO


-- new PRAGEN category
PRINT('checking to see if PRAGEN category exists ...')
IF EXISTS
(
    SELECT *
    FROM [dbo].[categories]
    WHERE category_name like '%Prior Approval%' and category_name != 'Prior Approvals'
)
	BEGIN
		PRINT('PRAGEN category already exists') 
		-- update here if there is a name change, etc
	END
ELSE
	BEGIN
		PRINT('does not exist') 
		INSERT INTO [dbo].[categories] (category_name, package, created_date, created_by_person_id, impac_doc_type_code, modified_date, modified_by_person_id, can_upload, input_type, input_constraint)
			VALUES 					   ('Prior Approval', 'Award', GetDate(), null, 'PRAGEN', GetDate(), null, 'Yes', 'T', 0)
		PRINT('inserted new type PRAGEN')
	END

-- create staging table for new category
PRINT('checking to see if temp staging table for doc category (impp_PriorApproval) PRAGEN exists ...')
IF EXISTS
(
	SELECT * 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE --TABLE_SCHEMA = 'TheSchema' AND
        TABLE_NAME = 'impp_PriorApproval'
)
	BEGIN
		PRINT('impp_PriorApproval stage table already exists') 
	END
ELSE
	BEGIN
		PRINT('does not exist') 
		
		CREATE TABLE [dbo].[impp_PriorApproval](
			[appl_id] [int] NOT NULL,
			[document_date] [smalldatetime] NULL,
			[KeyId] [int] NULL,
			[Request_id] [int] NULL
		) ON [PRIMARY]
		
		PRINT('created new table impp_PriorApproval')
	END






GO


USE [EIM]
GO


-- new RPPR category
PRINT('checking to see if new RPPR category exists ...')
IF EXISTS
(
    SELECT *
    FROM [dbo].[categories]
    WHERE category_name = 'RPPR'
)
	BEGIN
		PRINT('RPPR category already exists') 
		-- update here if there is a name change, etc
	END
ELSE
	BEGIN
		PRINT('does not exist') 
		INSERT INTO [dbo].[categories] (category_name, package, created_date, created_by_person_id, impac_doc_type_code, modified_date, modified_by_person_id, can_upload, input_type, input_constraint)
			VALUES 					   ('RPPR', null, GetDate(), null, null, GetDate(), null, 'Yes', null, 0)
		PRINT('inserted new type RPPR')
	END
	
	
PRINT('creating IC link to new category, RPPR ...')

IF EXISTS(	
			SELECT * FROM categories c
			inner join dbo.categories_ic ic on c.category_id=ic.category_id
			WHERE c.category_name = 'RPPR'
)
	BEGIN
		PRINT('IC link to new category, RPPR already exists') 
	END
ELSE
	BEGIN
		PRINT('does not exist') 
			INSERT INTO dbo.categories_ic (category_id, ic)
			select TOP 1 category_id, 'NCI' from categories where category_name = 'RPPR'
		PRINT('inserted new IC link to new category, RPPR')
	END
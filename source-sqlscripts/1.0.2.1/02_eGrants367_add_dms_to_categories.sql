USE [EIM]
GO


-- new DMS category
PRINT('checking to see if DMS category exists ...')
IF EXISTS
(
    SELECT *
    FROM [dbo].[categories]
    WHERE category_name like '%Data Management and Sharing%'
)
	BEGIN
		PRINT('dms category already exists') 
	END
ELSE
	BEGIN
		PRINT('does not exist') 
		INSERT INTO [dbo].[categories] (category_name, package, created_date, created_by_person_id, impac_doc_type_code, modified_date, modified_by_person_id, can_upload, input_type, input_constraint)
			VALUES 					   ('Data Management and Sharing (DMS) Plan', 'Application', GetDate(), null, 'DMS', GetDate(), null, 'Yes', 'T', 0)
		PRINT('inserted new type DMS')
	END

-- create staging table for new category
PRINT('checking to see if temp staging table for doc category (impp_dms_plans_NEW_t) DMS exists ...')
IF EXISTS
(
	SELECT * 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE --TABLE_SCHEMA = 'TheSchema' AND
        TABLE_NAME = 'impp_dms_plans_NEW_t'
)
	BEGIN
		PRINT('impp_dms_plans_NEW_t stage table already exists') 
	END
ELSE
	BEGIN
		PRINT('does not exist') 
			CREATE TABLE [dbo].[impp_dms_plans_NEW_t](
				[appl_id] [int] NOT NULL,
				[accession_num] [int] NULL,
				[total_pages_num] [int] NULL,
				[appl_received_date] [datetime] NULL,
			 CONSTRAINT [PK_impp_dms_plans_NEW_t] PRIMARY KEY CLUSTERED 
			(
				[appl_id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
			) ON [PRIMARY]
		PRINT('created new table impp_dms_plans_NEW_t')
	END






GO


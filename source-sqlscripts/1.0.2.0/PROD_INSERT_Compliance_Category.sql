USE [EIM]
GO

  INSERT INTO [dbo].[categories]
           ([category_name]
           ,[created_date]
           ,[created_by_person_id]
           ,[can_upload]
           ,[input_type]
           ,[input_constraint])
     VALUES
           ('Compliance',
           GETDATE()
           ,1899
           ,'Yes'
           ,'T'
           ,0)

 INSERT INTO [dbo].[categories_ic]
           ([category_id]
           ,[ic]
           ,[added_date]
           ,[added_by_person_id])
     VALUES
           (675,
            'NCI'
           ,GETDATE()
           ,1899)

GO

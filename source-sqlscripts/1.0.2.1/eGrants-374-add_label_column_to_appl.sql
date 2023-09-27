IF EXISTS(SELECT 1 FROM sys.columns 
    WHERE Name = N'label'
    AND Object_ID = Object_ID(N'[EIM].[dbo].[appls]'))
BEGIN
	print('label column on appls exists')
END
ELSE
BEGIN
	print('label column on appls does not exist')
	ALTER TABLE [EIM].[dbo].[appls]
	ADD label varchar(10);
END

SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
--USAGE: exec BUNDLE_Upload 
CREATE PROCEDURE [dbo].[SP_INSERT_NEW_PA]

@pa				varchar(10)
AS
BEGIN
	 INSERT dbo.adsup_pa_master(PA,Created_by_person_id,Created_date)
	 VALUES(@pa,1899,GETDATE())

END

GO


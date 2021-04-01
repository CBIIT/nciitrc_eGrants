SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON




CREATE PROCEDURE [dbo].[sp_egrants_batchftp]
@profileID smallint,
@xml text

AS
BEGIN

DECLARE @MID int
DECLARE @idoc int

SELECT @MID=max(document_id) from documents
EXEC sp_xml_preparedocument @idoc OUTPUT, @xml



--INSERT documents
--(appl_id, category_id, document_date, created_date, created_by_person_id, profile_id,file_type)

SELECT appl_id, 40 as category_id, document_date, getdate() as created_date
, 1884 created_by_person_id, @profileID profile_id, 'pdf' file_type

, grant_num, file_path, category_name
INTO temp_ftpbatch




FROM       OPENXML (@idoc, '/root/document',2)
WITH 	(appl_id varchar(50), category_name varchar(255),document_date smalldatetime, file_path varchar(255),grant_num varchar(20)) x

--INNER JOIN appls a
--ON x.appl_id=convert(varchar,a.appl_id)

RETURN


-----------DONT NEED THIS PIECE----------------

--ALTERNATIVE data source from a temp table
--SELECT x.appl_id,category_name, document_date, file_path, grant_num into temp_nia2

/*

INSERT documents
(appl_id, category_id, document_date, created_date, created_by_person_id, profile_id,file_type)


SELECT x.appl_id, 40, document_date, getdate(), 1884, @profileID, 'pdf'
from temp_nia2 x inner join appls a on x.appl_id=convert(varchar,a.appl_id)

*/

------------------------------------------

UPDATE documents
--SET url='https://egrants-data.nci.nih.gov/funded/nci/main2/' + 
--SET url='/data/funded/nci/main2/'  comment out by leon 5/16/2016
SET url='data/funded/nci/main2/' + 
convert(varchar,document_id) + '.pdf'
WHERE url is null and document_id>@MID and created_by_person_id=1884



EXEC sp_xml_removedocument @idoc


SELECT 'mv /egrants/unfunded/efdb2/' + convert(varchar,appl_id) + '.pdf ' + 
unix_file
FROM egrants
WHERE document_id>@MID and created_by_person_id=1884



END




GO


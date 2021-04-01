SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

CREATE procedure [dbo].[sp_egrants_dir]
as

DECLARE @t table([file_name] varchar(255), name_id int, in_egrants bit default 0 )

INSERT @t([file_name])
select * FROM 
openquery(pdfinfo,'select * from staging_convert_out.txt') source


UPDATE @t
SET name_id=replace([file_name],'.pdf','')
where ISNUMERIC(replace([file_name],'.pdf',''))=1

UPDATE @t set in_egrants=1
FROM @t t INNER JOIN egrants
ON t.name_id=egrants.document_id

update documents
--set url='https://egrants-data.nci.nih.gov/funded2/nci/main12/' + 
--set url='/data/funded2/nci/main12/' + comment out by leon 5/16/2016
set url='data/funded2/nci/main12/' + 
convert(varchar, document_id) + '.pdf',
file_type='pdf',
file_modified_by_person_id=1899,
file_modified_date=getdate()
from documents inner join @t t ON
t.name_id=documents.document_id and in_egrants=1

return

GO


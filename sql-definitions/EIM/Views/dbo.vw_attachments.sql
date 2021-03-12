SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER OFF

CREATE VIEW [dbo].[vw_attachments]
AS 
select 
a.*,
/*Imran : PIV Migration change 6/7/2014*/
--'https://egrants-data.nci.nih.gov/funded2/nci/main4/' + 'a'  + RIGHT('000000' + convert(varchar, attachment_id),6) + '.' + a.file_type as url
'/data/funded2/nci/main4/' + 'a'  + RIGHT('000000' + convert(varchar, attachment_id),6) + '.' + a.file_type as url
FROM 
attachments a INNER JOIN documents d ON
d.document_id=a.document_id




GO


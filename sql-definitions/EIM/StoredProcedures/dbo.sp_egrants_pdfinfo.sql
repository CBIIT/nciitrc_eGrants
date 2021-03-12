SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE sp_egrants_pdfinfo

@xml_str text

AS

DECLARE @idoc int


EXEC sp_xml_preparedocument @idoc OUTPUT, @xml_str

SELECT *
FROM       OPENXML (@idoc, '/Root/File',2)
          WITH 	(FileName varchar(255), Pages smallint, Producer varchar(255), Optimized varchar(3))

EXEC sp_xml_removedocument @idoc
GO


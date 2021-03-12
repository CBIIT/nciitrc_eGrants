SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
create function dbo.getColumnSize (@typeName SYSNAME, @max_length INT, @precision INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT CASE @typeName
        WHEN 'tinyint'          THEN 1
        WHEN 'smallint'         THEN 2
        WHEN 'int'              THEN 4
        WHEN 'bigint'           THEN 8
        WHEN 'numeric'          THEN ((@precision - 1)/2) + 1
        WHEN 'decimal'          THEN ((@precision - 1)/2) + 1
        WHEN 'real'             THEN 4
        WHEN 'float'            THEN CASE WHEN @precision <=24 THEN 4 ELSE 8 END
        WHEN 'money'            THEN 8
        WHEN 'smallmoney'       THEN 4
        WHEN 'time'             THEN 5
        WHEN 'timestamp'        THEN 5
        WHEN 'date'             THEN 3
        WHEN 'smalldatetime'    THEN 4
        WHEN 'datetime'         THEN 8
        WHEN 'datetime2'        THEN 8
        WHEN 'datetimeoffset'   THEN 10
        WHEN 'char'             THEN @max_length
        WHEN 'varchar'          THEN @max_length + 2
        WHEN 'nchar'            THEN @max_length
        WHEN 'nvarchar'         THEN @max_length + 2
        WHEN 'binary'           THEN @max_length
        WHEN 'varbinary'        THEN @max_length + 2
        WHEN 'bit'              THEN 0.125
    END)
END
GO


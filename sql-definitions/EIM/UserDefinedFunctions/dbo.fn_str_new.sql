SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION [dbo].[fn_str_new]  (@str varchar(300))

RETURNS varchar(300)

AS
BEGIN 
declare @i smallint

set @Str=' ' + @Str + ' '

--separators

set @Str=replace(@str,',', ' ')
set @Str=replace(@str,')', ' ')
set @Str=replace(@str,'(', ' ')
set @Str=replace(@str,'/', ' ')
set @Str=replace(@str,'&', ' ')
set @Str=replace(@str,'.', ' ')

set @Str=replace(@str,'[', ' ')
set @Str=replace(@str,']', ' ')

set @Str=replace(@str,' and ', ' ')
set @Str=replace(@str,' or ', ' ')
set @Str=replace(@str,' near ', ' ')
set @Str=replace(@str,' of ', ' ')
set @Str=replace(@str,' other ', ' ')
set @Str=replace(@str,' the ', ' ')
set @Str=replace(@str,' in ', ' ')
set @Str=replace(@str,' how ', ' ')
set @Str=replace(@str,' for ', ' ')
set @Str=replace(@str,' under ', ' ')
set @Str=replace(@str,' at ', ' ')


set @Str=replace(@str,' see ', ' ')
set @Str=replace(@str,' also ', ' ')
set @Str=replace(@str,' to ', ' ')
set @Str=replace(@str,' with ', ' ')

set @Str=replace(@str,' from ', ' ')
set @Str=replace(@str,' page ', ' ')


--Common field names

set @Str=replace(@str,' YEAR ', ' ')
set @Str=replace(@str,' RFA ', ' ')
set @Str=replace(@str,' PAR ', ' ')
set @Str=replace(@str,' NCAB ', ' ')

--get rid of all the special characters

--set @Str=replace(@str, CHAR(9), ' ')	-- Horizontal tab 
--set @Str=replace(@str, CHAR(10), ' ')	-- New Line
--set @Str=replace(@str, CHAR(11), ' ')	-- Vertical Tab  
--set @Str=replace(@str, CHAR(12), ' ')	-- New Page 
--set @Str=replace(@str, CHAR(13), ' ')	-- Carriage Return  

set @i=0
while @i<=31
begin
set @Str=replace(@str,char(@i),' ')

set @i=@i+1
end

--get rid of all the 1-character words

set @i=65
while @i<=90
begin
set @Str=replace(@str,' ' + char(@i) + ' ',' ')

set @i=@i+1
end


--make sure only one space between words
set @Str=rtrim(ltrim(@str))

set @i=1
while @i<=5
begin
set @str=replace(@str,'  ', ' ')
set @str=replace(@str,'%%', '%')
set @str=replace(@str,'--', '-')
set @str=replace(@str,'==', '=')

set @i=@i+1
end


--------------------------------------------------

set @Str=replace(@str,char(39), char(39) + char(39))
set @Str=replace(@str,'"', '')

--set @str=replace(@str,' ',' and ')

ret:
return @str



END














GO


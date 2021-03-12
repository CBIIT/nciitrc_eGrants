SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION Word_count 
	( @InputString VARCHAR(4000) ) 
RETURNS INT
AS
BEGIN
	--declare @InputString VARCHAR(4000)
	DECLARE @Index          INT
	DECLARE @Char           CHAR(1)
	DECLARE @PrevChar       CHAR(1)
	DECLARE @WordCount      INT

	SET @Index = 1
	SET @WordCount = 0
	--set @InputString = 'a b c    d   e'
	WHILE @Index <= LEN(@InputString)
		BEGIN
			SET @Char     = SUBSTRING(@InputString, @Index, 1)
			SET @PrevChar = CASE WHEN @Index = 1 THEN ' '
								 ELSE SUBSTRING(@InputString, @Index - 1, 1)
							END

			IF @PrevChar = ' ' AND @Char != ' '
				SET @WordCount = @WordCount + 1

			SET @Index = @Index + 1
		END
--select @WordCount
	RETURN @WordCount

END

GO


SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON




CREATE FUNCTION [dbo].[IMM_fn_applid_match]
(
	@strText varchar(8000)
)
RETURNS int

AS

BEGIN
 -- Declare the return variable here
DECLARE	@strT varchar ( 20 ),
		@intI  int,
		@intL  int,
		@intR  int,
		@suffix varchar(4)

	-- Remove spaces from the input string
	set @strText = replace(@strText,' ','')
	set @strText = replace(@strText,char(9),'')		-- Tab
	set @strText = replace(@strText,char(10),'')	-- Line Feed
	--set @strText = replace(@strText,char(13),'')	-- Carriage Return 
	set @suffix = null

	-- nLnnLLnnnnnn-nnLnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,18)
			select @suffix = substring(@strT,16,3)
			goto FoundIt
		End

	else	-- nLnnLLnnnnn-nnLnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,11)
			select @suffix = substring(@strT,16,3)
			goto FoundIt
		End

	else	-- nLnnLLnnnn-nnLnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,10)
			select @suffix = substring(@strT,16,3)
			goto FoundIt
		End

	else	-- nLnnLLnnn-nnLnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '000' + substring(@strText,@intI+6,9)
			select @suffix = substring(@strT,16,3)
			goto FoundIt
		End

	else	-- nLnnLLnnnnnn-nnLnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,19)
			select @suffix = substring(@strT,16,4)
			goto FoundIt
		End

	else	-- nLnnLLnnnnn-nnLnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,12)
			select @suffix = substring(@strT,16,4)
			goto FoundIt
		End

	else	-- nLnnLLnnnn-nnLnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,11)
			select @suffix = substring(@strT,16,4)
			goto FoundIt
		End

	else	-- nLnnLLnnn-nnLnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '000' + substring(@strText,@intI+6,10)
			select @suffix = substring(@strT,16,4)
			goto FoundIt
		End

	else	-- nLnnLLnnnnnn-nnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,17)
			select @suffix = substring(@strT,16,2)
			goto FoundIt
		End

	else	-- nLnnLLnnnnn-nnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,10)
			select @suffix = substring(@strT,16,2)
			goto FoundIt
		End

	else	-- nLnnLLnnnn-nnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,9)
			select @suffix = substring(@strT,16,2)
			goto FoundIt
		End

	else	-- nLnnLLnnn-nnLn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '000' + substring(@strText,@intI+6,8)
			select @suffix = substring(@strT,16,2)
			goto FoundIt
		End

	else	-- nLnnLLnnnnnn-nn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,15)
			goto FoundIt
		End

	else	-- nLnnLLnnnnn-nn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,8)
			goto FoundIt
		End

	else	-- nLnnLLnnnn-nn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,7)
			goto FoundIt
		End

	else	-- nLnnLLnnn-nn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9]-[0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '000' + substring(@strText,@intI+6,6)
			goto FoundIt
		End

	else 	-- nLnnLLnnnnnn-n
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,13) + '0' + substring(@strText,@intI+13,1)
			goto FoundIt
		End

	else 	-- nLnnLLnnnnn-n
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,6) + '0' + substring(@strText,@intI+12,1)
			goto FoundIt
		End

	else 	-- nLnnLLnnnn-n
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,5) + '0' + substring(@strText,@intI+11,1)
			goto FoundIt
		End

	else 	-- nLnnLLnnn-n
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]-[0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '000' + substring(@strText,@intI+6,4) + '0' + substring(@strText,@intI+11,1)
			goto FoundIt
		End

	else 	-- nLnnLLnnnnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,12)
			goto FoundIt
		End

	else 	-- nLnnLLnnnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '0' + substring(@strText,@intI+6,5)
			goto FoundIt
		End

	else 	-- nLnnLLnnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,4)
			goto FoundIt
		End

	else 	-- nLnnLLnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,6) + '00' + substring(@strText,@intI+6,3)
			goto FoundIt
		End

	else 	-- LLnnnnnn-nnLnLn
	select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,15)
			select @suffix = substring(@strT,12,4)
			goto FoundIt
		End

	else  	-- LLnnnnn-nnLnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '0' + substring(@strText,@intI+2,12)
				select @suffix = substring(@strT,12,4)
				goto FoundIt
			End

	else  	-- LLnnnn-nnLnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '00' + substring(@strText,@intI+2,11)
				select @suffix = substring(@strT,12,4)
				goto FoundIt
			End
		
	else  	-- LLnnn-nnLnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '000' + substring(@strText,@intI+2,10) 
				select @suffix = substring(@strT,12,4)
				goto FoundIt
			End

	else 	-- LLnnnnnn-nnLn
	select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,13)
			select @suffix = substring(@strT,12,2)
			goto FoundIt
		End
		
	else  	-- LLnnnnn-nnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '0' + substring(@strText,@intI+2,10)
				select @suffix = substring(@strT,12,2)
				goto FoundIt
			End

	else  	-- LLnnnn-nnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '00' + substring(@strText,@intI+2,9)
				select @suffix = substring(@strT,12,2)
				goto FoundIt
			End
		
	else  	-- LLnnn-nnLn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9][A,S][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '000' + substring(@strText,@intI+2,8) 
				select @suffix = substring(@strT,12,2)
				goto FoundIt
			End

	else 	-- LLnnnnnn-nn
	select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,11)
			goto FoundIt
		End
		
	else  	-- LLnnnnn-nn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '0' + substring(@strText,@intI+2,8)
				goto FoundIt
			End

	else  	-- LLnnnn-nn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '00' + substring(@strText,@intI+2,7)
				goto FoundIt
			End

	else  	-- LLnnn-nn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9]-[0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '000' + substring(@strText,@intI+2,6) 
				goto FoundIt
			End

	else 	-- LLnnnnnn-n
	select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9]%', @strText)
	if @intI>0 
		Begin
			select @strT = substring(@strText,@intI,9) + '0' + substring(@strText,@intI+9,1)
			goto FoundIt
		End

	else  	-- LLnnnnn-n
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]-[0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '0' + substring(@strText,@intI+2,6) + '0' + substring(@strText,@intI+8,1)
				goto FoundIt
			End

	else  	-- LLnnnn-n
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9]-[0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '00' + substring(@strText,@intI+2,5) + '0' + substring(@strText,@intI+7,1)
				goto FoundIt
			End

	else  	-- LLnnn-n
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9]-[0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '000' + substring(@strText,@intI+2,4) +'0' + substring(@strText,@intI+7,1) 
				goto FoundIt
			End

	else 	-- LLnnnnnn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,8) 
				goto FoundIt
			End

	else  	-- LLnnnnn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '0' + substring(@strText,@intI+2,5)
				goto FoundIt
			End

	else  	-- LLnnnn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '00' + substring(@strText,@intI+2,4)
				goto FoundIt
			End

	else  	-- LLnnn
		select @intI = patindex('%[A-Z][A-Z][0-9][0-9][0-9]%', @strText) 
		if @intI>0 
			Begin
				select @strT = substring(@strText,@intI,2) + '000' + substring(@strText,@intI+2,3)
				goto FoundIt
			End

FoundIt:
	Select @intL=3
	select @intR = null

	--		nLnnLLnnnnnn
	select @intI = patindex('%[0-9][A-Z][0-9][0-9][A-Z][A-Z][0-9][0-9][0-9][0-9][0-9][0-9]%', @strT)
	If @intI > 0
		Begin
			Select @intL = @intL + 4
		End
	
	if isnumeric(substring(@strT,@intL,6))=1		-- True
		Begin
			if @suffix is not null
				Begin
					select @intR = (SELECT TOP 1 appl_id from dbo.vw_appls 
							WHERE serial_num = substring(@strT,@intL,6) and  
							FULL_GRANT_NUM LIKE + '%' + @strT + '%' and suffix_code = @suffix
							order by support_year desc, suffix_code desc)
				End
			else
			if @suffix is null
				Begin
					select @intR = (SELECT TOP 1 appl_id from dbo.vw_appls 
							WHERE serial_num = substring(@strT,@intL,6) and  
							FULL_GRANT_NUM LIKE + '%' + @strT + '%' and suffix_code is null
							order by support_year desc, suffix_code desc)
				End
		End

	return @intR

END











GO


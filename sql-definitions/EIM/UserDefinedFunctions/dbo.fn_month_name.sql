SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION fn_month_name (@d smalldatetime)

  
RETURNS varchar(15) AS  
BEGIN 

declare @S varchar(15)


select  @S=
case month(@d)
when 1 then 'january'
when 2 then 'february'
when 3 then 'march'
when 4 then 'april'
when 5 then 'may'
when 6 then 'june'
when 7 then 'july'
when 8 then 'august'
when 9 then 'september'
when 10 then 'october'
when 11 then 'november'
when 12 then 'december'
end


RETURN @S

END

GO


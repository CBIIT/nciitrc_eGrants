SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
CREATE FUNCTION dbo.fc_ContaRegistros_x_Criterio (
@str_TBName VARCHAR(100),
@str_Criter VARCHAR(500))
RETURNS BIGINT
AS

BEGIN
-- Objetivo   : Contar numero de registros de uma determinada tabela de acordo com o critério passado
-- Criação    : Josué Monteiro Viana - 09/07/09
/*
Exemplo:
DECLARE @count INT
SET @count = dbo.fc_ContaRegistros_x_Criterio('master.dbo.sysobjects', '')
PRINT @count
SET @count = dbo.fc_ContaRegistros_x_Criterio('crk.dbo.acao', 'where cod_acao is null')
PRINT @count
*/

DECLARE @int_objSQL INT, @int_erros INT, @int_objSelectCountResult INT,
@bint_SelectCount BIGINT, @sql NVARCHAR(2000)

EXEC @int_erros = sp_OACreate 'SQLDMO.SQLServer', @int_objSQL OUTPUT
EXEC @int_erros = sp_OASetProperty @int_objSQL, 'LoginSecure', TRUE
EXEC @int_erros = sp_OAMethod @int_objSQL, 'Connect', null, '.'

--SET @sql = 'SELECT count(*) FROM ' + @str_TBName + ' WHERE ' + @str_Criter

SET @sql = 'SELECT count(*) FROM ' + @str_TBName + ' ' + @str_Criter
SET @sql = 'ExecuteWithResults("' + @sql + '")'

EXEC @int_erros = sp_OAMethod @int_objSQL, @sql, @int_objSelectCountResult OUTPUT
EXEC @int_erros = sp_OAMethod @int_objSelectCountResult, 'GetRangeString(1, 1)', @bint_SelectCount OUT
EXEC @int_erros = sp_OADestroy @int_objSQL

-- debug info: not valid inside a fc
--if @int_erros <> 0 EXEC sp_OAGetErrorInfo @int_objSQL else print 'ok'

if @int_erros <> 0 SET @bint_SelectCount = @int_erros

RETURN @bint_SelectCount

END
GO


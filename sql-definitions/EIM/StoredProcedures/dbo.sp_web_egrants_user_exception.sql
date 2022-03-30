SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF


CREATE   PROCEDURE [dbo].[sp_web_egrants_user_exception]

@operator 		varchar(50)

AS
/**********************************************************************************/
/***									 										***/
/***	Procedure Name:sp_web_egrants_user_exception							***/
/***	Description:check ic for exception users and who ic not as nci			***/
/***	Created:	07/31/2019	Leon											***/
/**********************************************************************************/
SET NOCOUNT ON

set @operator=LOWER(@operator)
declare @count			int

--if (@operator="wilburns" or @operator="agarwalraj" or @operator="canariaca" or @operator="silkensens" or @operator="hallettkl")
if (@operator='pondma' or @operator='agarwalraj' or (@operator='strasbuj' and getdate() < '9/30/2020') or @operator = 'liuy') 
set @count = 1
else set @count=0
                             
select @count as 'count'

GO


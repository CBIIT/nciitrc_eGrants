SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF


CREATE PROCEDURE [dbo].[sp_web_egrants_load_data_autocomplete-fy-mechan-admin_to_be_deleted]  

@fy				varchar(4)=null,
--@fy				int=0,
@mechanism		varchar(3)=null,
@admincode		varchar(2)=null,
--@serialnum		int=0,
@serialnum		varchar(10)=null,
@term           varchar(10)


AS

SET NOCOUNT ON

---1=@fy
---2=@mechanism
---3=@admincode
---4=@serialnum

--select 'I'+nchar(92)+'m happpy'

declare @sql			varchar(400)
--declare @like			varchar(100)


--2
---1,Autocomplete by @fy only
IF ( @fy<>'') and (@mechanism is null or @mechanism ='') and (@admincode is null or @admincode='') and (@serialnum is null or @serialnum='')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +'  order by fy'
exec ( @sql)

END


-----2, Autocomplete by @mechanism only
--IF (@fy is null or @fy='') and (@mechanism is not null and @mechanism<>'') and (@admincode is null or @admincode='') and (@serialnum is null or @serialnum='')	
--BEGIN
--SET @sql ='select distinct top 10  activity_code from vw_grants_construct_fy_Mechanism_Admincode_serialNum where activity_code like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' order by activity_code'

--END

-----3, Autocompleteh by @admincode only
--IF (@fy is null or @fy='') and (@mechanism is null or @mechanism='') and (@admincode is not null and @admincode<>'') and (@serialnum is null or @serialnum='')	
--BEGIN
--SET @sql ='select distinct top 10  admin_phs_org_code from vw_grants_construct_fy_Mechanism_Admincode_serialNum where admin_phs_org_code like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' order by admin_phs_org_code'

--END

-----4, Autocomplete by @serialnum only
--IF (@fy is null or @fy='') and (@mechanism is null or @mechanism='') and (@admincode is null or @admincode='') and (@serialnum is not null and @serialnum<>'')	
--BEGIN
--SET @sql ='select distinct top 10  serial_num from vw_grants_construct_fy_Mechanism_Admincode_serialNum where serial_num like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' order by serial_num '

--END

-----1-2, Autocomplete by @fy and @mechanism 
IF ( @fy<>'') and (@mechanism is not null and @mechanism<>'') and (@admincode is null or @admincode='') and (@serialnum is null or @serialnum='')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like ' + nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' and activity_code=' +char(39)+ @mechanism +char(39)+' order by fy'
 exec ( @sql)
END

-------1-3, Autocomplete by @fy and @admincode
IF (@fy<>'') and (@mechanism is null or @mechanism ='') and (@admincode is not null and @admincode<>'') and (@serialnum is null or @serialnum='')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' and admin_phs_org_code=' +char(39)+ @admincode +char(39)+' order by fy'
exec (@sql)
END

-----1-4, Autocomplete by @fy and @serialnum
IF (@fy<>'') and (@mechanism is null or @mechanism ='') and (@admincode is null or @admincode='') and (@serialnum is not null and @serialnum<>'')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like  '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +' and serial_num=' +convert(varchar, @serialnum) +'  order by fy'
exec(@sql)
END

-----2-3, Autocomplete by @mechanism and @admincode
--IF (@fy is null or @fy='') and (@mechanism is not null and @mechanism<>'') and (@admincode is not null and @admincode<>'') and (@serialnum is null or @serialnum='')	
--BEGIN
--SET @sql ='select distinct top 10 fy, activity_code, admin_phs_org_code from vw_grants_construct_fy_Mechanism_Admincode_serialNum where activity_code  like '+ nchar(39)+ convert(varchar, @term) + '%' + nchar(39) +'  and admin_phs_org_code=' +char(39)+ @admincode +char(39)+' order by activity_code'
--exec(@sql)
--END

-----2-4, Autocomplete by @mechanism and @serialnum
--IF (@fy is null or @fy='') and (@mechanism is not null and @mechanism<>'') and (@admincode is null or @admincode='') and (@serialnum is not null and @serialnum<>'')	
--BEGIN
--SET @sql ='select distinct top 10  activity_code, serial_num from vw_grants_construct_fy_Mechanism_Admincode_serialNum where activity_code like ' +char(39)+ @term +char(39)+ '%'+'  and serial_num=' +convert(varchar, @serialnum) +' " order by activity_code'

--END

-----3-4, Autocomplete by @admincode and @serialnum
--IF (@fy is null or @fy='') and (@mechanism is null or @mechanism='') and (@admincode is not null and @admincode<>'') and (@serialnum is not null and @serialnum<>'')	
--BEGIN
--SET @sql ='select distinct top 10  admin_phs_org_code from vw_grants_construct_fy_Mechanism_Admincode_serialNum where admin_phs_org_code like ' +char(39)+ @term +char(39)+ '%' +' and serial_num=' +convert(varchar, @serialnum) +' order by admin_phs_org_code'

--END

---1-2-3, Autocomplete by @fy,@mechanism ,@admincode 
IF (@fy <>'') and (@mechanism is not null and @mechanism<>'') and (@admincode is not null and @admincode<>'') and (@serialnum is null or @serialnum='')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like '+ nchar(39) + convert(varchar, @term) + '%'+ nchar(39) +' and activity_code =' +char(39)+ @mechanism +char(39)+' and admin_phs_org_code=' +char(39)+ @admincode +char(39)+' order by fy'
exec(@sql)
END
  
-----1-2-4, Autocomplete by @fy, @mechanism and @serialnum
IF (@fy <>'') and (@mechanism is not null or @mechanism<>'') and (@admincode is  null or @admincode ='') and (@serialnum is not null and @serialnum<>'')	
BEGIN
SET @sql ='select distinct top 10 fy, activity_code,serial_num from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like ' +char(39)+ convert(varchar, @term) + '%'+ char(39)+' and activity_code=' +char(39)+ @mechanism +char(39)+' and serial_num=' +convert(varchar, @serialnum) +' order by fy'
exec(@sql)
END


-----1-3-4, Autocomplete by @fy, @admincode and @serialnum
IF (@fy <>'') and (@mechanism is null or @mechanism='') and (@admincode is not null and @admincode<>'') and (@serialnum is not null and @serialnum<>'')	
BEGIN
SET @sql ='select distinct top 10 fy, activity_code,admin_phs_org_code,serial_num from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like ' +nchar(39)+ convert(varchar, @term) + '%'+ nchar(39)+' and admin_phs_org_code=' +char(39)+ @admincode +char(39)+' and serial_num=' +convert(varchar, @serialnum) +' order by fy'
exec(@sql)
END

-----2-3-4, Autocomplete by @mechanism, @admincode and @serialnum
--IF (@fy is null or @fy='') and (@mechanism is not null and @mechanism<>'') and (@admincode is not null and @admincode<>'') and (@serialnum is not null and @serialnum<>'')	
--BEGIN
--SET @sql ='select distinct top 10 activity_code, admin_phs_org_code, serial_num from vw_grants_construct_fy_Mechanism_Admincode_serialNum where activity_code like ' +char(39)+ @mechanism +char(39)+ '%'+' and admin_phs_org_code=' +char(39)+ @admincode +char(39)+' and serial_num=' +convert(varchar, @serialnum) +' order by serial_num '

--END

---1-2-3-4, Autocomplete by @fy,@mechanism ,@admincode and @serialnum
IF (@fy <>'') and (@mechanism is not null and @mechanism<>'') and (@admincode is not null and @admincode<>'') and (@serialnum is not null and @serialnum<>'')	
BEGIN
SET @sql ='select distinct top 10 fy from vw_grants_construct_fy_Mechanism_Admincode_serialNum where fy like '+ nchar(39) + convert(varchar, @term) + '%'+ nchar(39) + ' and activity_code =' + char(39)+ @mechanism + char(39) +' and admin_phs_org_code=' + char(39) + @admincode + char(39) +' and serial_num=' + convert(varchar, @serialnum) +' order by fy '
exec(@sql)
END



--return SQL query
select @sql


GO


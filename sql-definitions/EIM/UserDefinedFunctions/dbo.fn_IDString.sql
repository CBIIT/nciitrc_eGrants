SET ANSI_NULLS OFF
SET QUOTED_IDENTIFIER OFF
CREATE FUNCTION fn_idstring (@FolderID int,@GrantID int)
  
RETURNS varchar(20) AS  


BEGIN 

declare @s varchar(20)
declare @BCode varchar(10)

select @BCode=bar_code from folders where folder_id=@FolderID

IF @BCode is null
return 
 (select grant_num from folders f,vw_grants g
where f.grant_id=g.grant_id and folder_id=@FolderID)


SELECT @s=grant_num from vw_grants g
where grant_id=@GrantID


SELECT @s=@s + '#' + right('00' + convert(varchar,min(support_year)),2) +'-' + right('00'+convert(varchar,max(support_year)),2)
FROM folder_appls fa,vw_appls a
WHERE a.appl_id=fa.appl_id and fa.folder_id=@FolderID and grant_id=@GrantID
GROUP BY fa.folder_id

RETURN @s

END












GO


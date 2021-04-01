SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
---************************************************
---Procedure : sp_grants_flag_ARRA
---Description: Mostly used in back end process
---Can be run on any off p[eak time. Not critical to be run peak schedule.
---Dependency : None. from out side system
---************************************************
CREATE PROCEDURE [dbo].[sp_grants_flag_ARRA] AS
Begin

DECLARE 
@SYSUSR INT,
@FLAGAPPLICATION CHAR(1),
@FLAGTYPE VARCHAR(5)

SET @FLAGTYPE='ARRA'  --HARD CODE :  BECAUSE THIS PROC IS FOR ARRA FLAG

SELECT @SYSUSR=PERSON_ID FROM People WHERE person_name='SYSTEM'
SELECT @FLAGAPPLICATION=flag_application_code FROM dbo.Grants_Flag_Master WHERE flag_type_code=@FLAGTYPE AND end_date IS NULL AND flag_gen_type='automatic'
PRINT '==>> PROC: sp_grants_flag_ARRA. STARTED BUILDING FLAG= ARRA TIME = ' + cast(GETDATE() as varchar)

update dbo.grants_flag_WIP set arra_flag='n' where arra_flag='y'
print 'RESET ARRA FLAG TO no COUNT=' + cast(@@ROWCOUNT as varchar)

update dbo.grants_flag_WIP SET arra_flag='y'
WHERE grant_id IN(select distinct grant_id from vw_appls_arra where admin_phs_org_code='CA')
AND APPL_ID IS NULL
print 'ARRA FLAG SET count=' + cast(@@ROWCOUNT as varchar)+' @ '+ cast(getdate() as varchar)


/***  disable all ARRA flag in construct if it is not in WIP, it means it has changed in ImpacII*/
UPDATE dbo.Grants_Flag_Construct
SET end_dt=GETDATE(), last_updated_by=@SYSUSR, last_updated_date=GETDATE()
where flag_type=@FLAGTYPE
and appl_id is null
and end_dt is null 
and grant_id in (select grant_id from dbo.grants_flag_WIP where arra_flag='n')
--PRINT @@
print 'DISABLED ARRA FLAG=' + cast(@@ROWCOUNT as varchar)
PRINT 'FLAG SET=' + @FLAGTYPE

/**  INSERT ALL NEW FLAGS CAME IN TODAY    **/
INSERT DBO.Grants_Flag_Construct(grant_id,flag_type,flag_application,start_dt,created_by,created_dt)
SELECT A.GRANT_ID,@FLAGTYPE,@FLAGAPPLICATION,GETDATE(),1899,GETDATE() from dbo.grants_flag_WIP A  
where A.grant_id not in (
select B.grant_id from dbo.Grants_Flag_Construct B 
where flag_type=@FLAGTYPE and b.appl_id is null and B.end_dt is null 
)
AND A.arra_flag='y'
print 'NEW FLAGS ADDED =' + cast(@@ROWCOUNT as varchar)

PRINT '==>> PROC: sp_grants_flag_ARRA. FINISHED BUILDING FLAG=ARRA TIME= ' + cast(GETDATE() as varchar)

end


GO


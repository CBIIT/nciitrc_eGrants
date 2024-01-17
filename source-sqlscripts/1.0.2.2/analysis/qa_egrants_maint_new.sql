USE [EIM]
GO
/****** Object:  StoredProcedure [dbo].[sp_egrants_maint_new]    Script Date: 12/20/2023 9:50:29 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


ALTER   PROCEDURE  [dbo].[sp_egrants_maint_new] AS

DECLARE @impacID int
SELECT @impacID=person_id from people where userid='impac' 

---DELETE appls_ciip_deleted

--delete bad appls missing non-nullable columns
DELETE appls_ciip  WHERE appl_type_code IS NULL or  activity_code IS NULL or admin_phs_org_code IS NULL or serial_num IS NULL or support_year IS NULL

--check if duplicates in source table
DELETE appls_ciip WHERE appl_id IN (select appl_id from appls_ciip group by appl_id having count(*)>1)
--------------------------------------------------
--clean up multiple serial number for nigms_dups
--3/24/2018 commented based on Leon recomendation
--EXEC sp_egrants_maint_NIGMS_multiple
---------------------------------------------------
--check all full grant number with 'w' and delete all appls no longer in IMPAC II
--EXEC sp_egrants_maint_single_applid_w_to_s
----------------------------------------------------
--add grants that do not exist yet
INSERT grants(admin_phs_org_code, serial_num)
SELECT DISTINCT appls_ciip.admin_phs_org_code, appls_ciip.serial_num		
FROM  appls_ciip LEFT OUTER JOIN  grants ON appls_ciip.admin_phs_org_code = grants.admin_phs_org_code AND appls_ciip.serial_num = grants.serial_num
WHERE grants.grant_id IS NULL

--set grant_id for source table as a comfort measure


BEGIN TRY
UPDATE appls_ciip 
SET grant_id=grants.grant_id
FROM appls_ciip, grants
WHERE appls_ciip.grant_id is null and grants.admin_phs_org_code=appls_ciip.admin_phs_org_code and grants.serial_num=appls_ciip.serial_num
print 'done with Step 1'
end try
begin catch
 print('Step1 - Error updating appls_ciip '  + ERROR_MESSAGE()) -- Changed by Madhu
end catch

--delete more duplicates
DELETE appls_ciip
FROM appls_ciip a,
(SELECT grant_id, appl_type_code, activity_code,support_year, suffix_code 
FROM appls_ciip GROUP BY grant_id, appl_type_code, activity_code,support_year, suffix_code HAVING COUNT(*)>1) s
WHERE
s.grant_id=a.grant_id and
s.appl_type_code=a.appl_type_code and
s.activity_code=a.activity_code and
s.support_year=a.support_year and
ISNULL(s.suffix_code,'')=ISNULL(a.suffix_code,'')


--delete bad former numb
DELETE appls_ciip where former_num like '% %'
--------------------------------------------------------------
--commented on 11292016 11:15am by hareeshj

--set up to_be_destoried and is_tobacco flag fro grant
--exec sp_egrants_maint_grant_flags_set_up

----------------------------------------------------------------
--fix or replace old appl_id
DECLARE @t table(appl_id int PRIMARY KEY, appl_id_alt int)

INSERT @t(appl_id, appl_id_alt)
SELECT a.appl_id, s.appl_id
FROM appls_ciip s INNER JOIN appls a ON
s.grant_id=a.grant_id and
s.appl_type_code=a.appl_type_code and
s.activity_code=a.activity_code and
s.support_year=a.support_year and
ISNULL(s.suffix_code,'')=ISNULL(a.suffix_code,'') and
s.appl_id<>a.appl_id 

--update new appl_id into appls for duplicate appls
BEGIN TRY
UPDATE appls SET new_appl_id=t.appl_id_alt FROM appls INNER JOIN @t t ON t.appl_id=appls.appl_id
print 'done with Step2'
end try
begin catch
	print('Step2 - Error updating appls '  +  ERROR_MESSAGE()) -- Changed by Madhu
end catch


--insert all replacement info
INSERT appls_replacement (appl_id_old, appl_id_new) select appl_id, new_appl_id from appls where  new_appl_id is not null

--update appls, documents and appl_folders tables with new_appl_id 
begin try
UPDATE folder_appls SET appl_id=new_appl_id FROM  folder_appls f, appls a WHERE f.appl_id=a.appl_id and a.new_appl_id in(select appl_id from appls)
print 'done with Step 3'
end try 
begin catch 
 --SELECT ERROR_MESSAGE() AS ERROR
	print('Step3 - Error updating folder_appls '  + ERROR_MESSAGE()) -- Changed by Madhu
 end catch


begin try
UPDATE documents SET appl_id=a.appl_id_new FROM documents d, appls_replacement a WHERE d.appl_id=a.appl_id_old and a.appl_id_new in(select new_appl_id from appls where new_appl_id is not null) ------a.new_appl_id in(select appl_id from appls)
print 'done with Step 4'

end try 
begin catch 
-- SELECT ERROR_MESSAGE() AS ERROR
	print('Step 4 - Error updating documents '  + ERROR_MESSAGE()) -- Changed by Madhu
 end catch
 
 /* Madhu - Commented out to loop through each appl_id */
 begin try
UPDATE appls SET appl_id=new_appl_id WHERE new_appl_id is not null and new_appl_id not in(select appl_id from appls)  ----it will cascade update documents and appl_folders tables
print 'done with step5'
end try 
begin catch 
 --SELECT ERROR_MESSAGE() AS ERROR
	print(' step5 - Error updating appls '  + ERROR_MESSAGE()) -- Changed by Madhu - 10/05
 end catch
 

Declare @currAppl_id varchar(20) = null

--update the same for funding_appls

begin try
UPDATE funding_appls SET appl_id=t.appl_id_alt 
FROM funding_appls INNER JOIN @t t ON t.appl_id=funding_appls.appl_id
print 'done with Step6'
end try
begin catch
 --SELECT ERROR_MESSAGE() AS ERROR
	print('Step6 - Error updating appls '  + ERROR_MESSAGE()) -- Changed by Madhu
end catch


--clean up old appl_id in appls table
DELETE FROM appls WHERE appl_id<>new_appl_id and new_appl_id in(select appl_id from appls)

--clean up new appl_id in appls table
begin try
UPDATE appls SET new_appl_id=null WHERE new_appl_id is not null
print 'done with Step7'
end try
begin catch
 SELECT ERROR_MESSAGE() AS ERROR
	print('Step7 - Error updating appls '  + ERROR_MESSAGE()) -- Changed by Madhu
end catch
---------------------------------------------------------------
--MISC CLEANUP

--delete exceptional grants
DELETE appls WHERE appl_id in (2137681) --these 2 have a wierd former_num 2137678,



--more EB exceptions - wrong former num for two EB years in IMPAC
begin try
UPDATE appls SET former_num='1R01NS043928-01' WHERE appl_id in (6529978,6661361)
print 'done with step8'
end try
begin catch
 SELECT ERROR_MESSAGE() AS ERROR
	print('step8 - Error updating appls '  + ERROR_MESSAGE()) -- Changed by Madhu
end catch

--------------------------------------------------
--update former grant_num info for the grants table
UPDATE grants SET
 former_admin_phs_org_code=substring(former_num,5,2),
 former_serial_num=convert(int,substring(former_num,7,6))
FROM grants g, appls_ciip a
WHERE g.admin_phs_org_code=a.admin_phs_org_code and g.serial_num=a.serial_num and former_num is not null and substring(former_num,13,1)='-' and
(g.admin_phs_org_code<>substring(former_num,5,2) or g.serial_num<>convert(int,substring(former_num,7,6)))
print 'done with step9'

--update future grant_num in the grants table
UPDATE g1 SET
future_admin_phs_org_code=g2.admin_phs_org_code,
future_serial_num=g2.serial_num
FROM grants g1, grants g2
WHERE g1.admin_phs_org_code=g2.former_admin_phs_org_code and g1.serial_num=g2.former_serial_num
print 'done with step10'


--because some grant numbers changed more than once, give priority to the EB number 
UPDATE g1 SET
future_admin_phs_org_code=g2.admin_phs_org_code,
future_serial_num=g2.serial_num
FROM grants g1,grants g2
WHERE g1.admin_phs_org_code=g2.former_admin_phs_org_code and g1.serial_num=g2.former_serial_num and g2.admin_phs_org_code='EB'
print 'done with step11'

--exceptional grant with 2 EB former nums!
UPDATE grants SET future_serial_num='2189' WHERE grant_id=390961
print 'done with step12'

---add couple of columns from CIIP
--UPDATE appls_ciip
--SET gms_user_id=c.gms_user_id, cay_code=c.cay_code
--FROM appls_ciip a INNER JOIN
--openquery(CIIP, 'select appl_id,gms_user_id, cay_code from form_grant_vw') c ON a.appl_id=c.appl_id

BEGIN TRANSACTION

UPDATE appls
SET
grant_id=s.grant_id,
appl_type_code=s.appl_type_code,
activity_code=s.activity_code,
support_year=s.support_year,
suffix_code=s.suffix_code,
last_name=s.last_name,
first_name=s.first_name,
mi_name=s.mi_name,
project_title=s.project_title,
former_num=s.former_num,
org_name=s.org_name,
rfa_pa_number=s.rfa_pa_number,
council_meeting_date=s.council_meeting_date,
prog_class_code=s.prog_class_code,
irg_code=s.irg_code,
appl_status_group_descrip=s.appl_status_group_descrip,
fy=s.fy,
appl_received_date=s.appl_received_date,
last_upd_date=s.last_upd_date,
irg_flex_code=s.irg_flex_code,
summary_statement_flag=s.summary_statement_flag,
loaded_date=getdate(),
gms_user_id=s.gms_user_id,
cay_code=s.cay_code,
pd_full_name=s.pd_full_name,
pd_email_address=s.pd_email_address,
GS_PERSON_ID=s.GS_PERSON_ID,
GS_FIRST_NAME = s.GS_FIRST_NAME,
GS_LAST_NAME=s.GS_LAST_NAME,
GS_EMAIL_ADDRESS=s.GS_EMAIL_ADDRESS,
PI_EMAIL_ADDR=s.PI_EMAIL_ADDR

FROM appls a INNER JOIN appls_ciip s ON a.appl_id=s.appl_id
print 'done with step13 - Update appls with all the misc data'

IF @@ERROR>0
BEGIN
ROLLBACK TRAN
RETURN
END
COMMIT TRAN

/*
Madhu - Taking the insert out of the transaction and implementing a loop to go through each appl_id

INSERT appls (appl_id, appl_type_code, activity_code, grant_id, support_year, suffix_code, project_title, former_num, rfa_pa_number, council_meeting_date, external_org_id, 
org_name, person_id, last_name, first_name, mi_name, prog_class_code, irg_code, appl_status_group_descrip, fy,
appl_received_date, created_date, last_upd_date, irg_flex_code, summary_statement_flag,loaded_date,
pd_full_name,pd_email_address,GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,GS_EMAIL_ADDRESS,PI_EMAIL_ADDR)

SELECT appls_ciip.APPL_ID, appls_ciip.APPL_TYPE_CODE, appls_ciip.ACTIVITY_CODE, appls_ciip.grant_id, appls_ciip.SUPPORT_YEAR, 
               appls_ciip.SUFFIX_CODE, appls_ciip.PROJECT_TITLE, appls_ciip.FORMER_NUM, appls_ciip.RFA_PA_NUMBER,
               appls_ciip.COUNCIL_MEETING_DATE, appls_ciip.EXTERNAL_ORG_ID, appls_ciip.ORG_NAME, 
               appls_ciip.PERSON_ID, appls_ciip.LAST_NAME, appls_ciip.FIRST_NAME, appls_ciip.MI_NAME, 
               appls_ciip.PROG_CLASS_CODE, appls_ciip.IRG_CODE, appls_ciip.APPL_STATUS_GROUP_DESCRIP, appls_ciip.FY, 
               appls_ciip.APPL_RECEIVED_DATE, appls_ciip.CREATED_DATE, 
               appls_ciip.LAST_UPD_DATE,appls_ciip.irg_flex_code,appls_ciip.summary_statement_flag,getdate(),
			   appls_ciip.pd_full_name,appls_ciip.pd_email_address,appls_ciip.GS_PERSON_ID,appls_ciip.GS_FIRST_NAME,appls_ciip.GS_LAST_NAME,appls_ciip.GS_EMAIL_ADDRESS,appls_ciip.PI_EMAIL_ADDR
FROM  appls_ciip LEFT OUTER JOIN appls ON appls_ciip.appl_id=appls.appl_id
WHERE appls.appl_id IS NULL
print 'done with step12'

				  
IF @@ERROR>0
BEGIN
ROLLBACK TRAN

RETURN
END
*/
----------------------------------------------------

-- Added by Madhu
DROP TABLE IF EXISTS #appl_ciip_with_identity

-- Adding an identity_column to the temp table (#appl_ciip_with_identity) to enable easier looping to insert. 
SELECT IDENTITY(INT, 1, 1) AS ID,   
		appls_ciip.APPL_ID, appls_ciip.APPL_TYPE_CODE, appls_ciip.ACTIVITY_CODE, appls_ciip.grant_id, appls_ciip.SUPPORT_YEAR, 
               appls_ciip.SUFFIX_CODE, appls_ciip.PROJECT_TITLE, appls_ciip.FORMER_NUM, appls_ciip.RFA_PA_NUMBER,
               appls_ciip.COUNCIL_MEETING_DATE, appls_ciip.EXTERNAL_ORG_ID, appls_ciip.ORG_NAME, 
               appls_ciip.PERSON_ID, appls_ciip.LAST_NAME, appls_ciip.FIRST_NAME, appls_ciip.MI_NAME, 
               appls_ciip.PROG_CLASS_CODE, appls_ciip.IRG_CODE, appls_ciip.APPL_STATUS_GROUP_DESCRIP, appls_ciip.FY, 
               appls_ciip.APPL_RECEIVED_DATE, appls_ciip.CREATED_DATE, 
               appls_ciip.LAST_UPD_DATE,appls_ciip.irg_flex_code,appls_ciip.summary_statement_flag,getdate() as to_date,
			   appls_ciip.pd_full_name,appls_ciip.pd_email_address,appls_ciip.GS_PERSON_ID,appls_ciip.GS_FIRST_NAME,appls_ciip.GS_LAST_NAME,appls_ciip.GS_EMAIL_ADDRESS,appls_ciip.PI_EMAIL_ADDR
INTO #appl_ciip_with_identity
FROM appls_ciip LEFT OUTER JOIN appls ON appls_ciip.appl_id=appls.appl_id
WHERE appls.appl_id IS NULL

print 'Done copying records from appls_ciip to #appl_ciip_with_identity'

-- Go through the records in the temp table(#appl_ciip_with_identity) and insert into appls table.
Declare @min_id_appl_ciip int = 0, @max_id_appl_ciip int = 0, @loopId_appl_ciip int = 0, 
        @temptableRecCount int  = 0, @recordsAdded int = 0

select @temptableRecCount = count(*) from #appl_ciip_with_identity
print '@temptableRecCount = ' + cast(@temptableRecCount as varchar)

IF(@temptableRecCount > 0)
BEGIN 
	select @min_id_appl_ciip = min(id), @max_id_appl_ciip = max(id) from #appl_ciip_with_identity
	print 'Starting loop = '  + cast(@min_id_appl_ciip as varchar) + ' for INSERT into appls table from #appl_ciip_with_identity'
	print '@min_id_appl_ciip  = ' + cast(@min_id_appl_ciip as varchar) + ' ---- @max_id_appl_ciip = ' + cast(@max_id_appl_ciip as varchar)
	set @loopId_appl_ciip = @min_id_appl_ciip 

	WHILE(@loopId_appl_ciip <= @max_id_appl_ciip)
	begin
--		print '      Processing @loopId_appl_ciip  = ' + cast(@loopId_appl_ciip as varchar) + ' record'

	   BEGIN TRY 
	   select  @currAppl_id = appl_id from #appl_ciip_with_identity where id = @loopId_appl_ciip
	   	SET @RecordsAdded = @RecordsAdded + 1

		INSERT appls (appl_id, appl_type_code, activity_code, grant_id, support_year, suffix_code, project_title, former_num, rfa_pa_number, council_meeting_date, external_org_id, 
					org_name, person_id, last_name, first_name, mi_name, prog_class_code, irg_code, appl_status_group_descrip, fy,
					appl_received_date, created_date, last_upd_date, irg_flex_code, summary_statement_flag,loaded_date,
					pd_full_name,pd_email_address,GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,GS_EMAIL_ADDRESS,PI_EMAIL_ADDR)

		select APPL_ID,APPL_TYPE_CODE, ACTIVITY_CODE, grant_id, SUPPORT_YEAR, SUFFIX_CODE,
				 PROJECT_TITLE, FORMER_NUM, RFA_PA_NUMBER,
				   COUNCIL_MEETING_DATE, EXTERNAL_ORG_ID, ORG_NAME,
				   PERSON_ID, LAST_NAME, FIRST_NAME, MI_NAME, 
				   PROG_CLASS_CODE, IRG_CODE, APPL_STATUS_GROUP_DESCRIP, FY, 
				   APPL_RECEIVED_DATE, CREATED_DATE, 
				   LAST_UPD_DATE,irg_flex_code,summary_statement_flag, to_date,
				   pd_full_name,pd_email_address,GS_PERSON_ID,GS_FIRST_NAME,GS_LAST_NAME,GS_EMAIL_ADDRESS,PI_EMAIL_ADDR
		from #appl_ciip_with_identity
		where id = @loopId_appl_ciip

		END TRY 
		BEGIN CATCH 
			print 'Step14 - Error Inserting to Appls table for Appl_id ' + CAST(@currAppl_id AS VARCHAR) + '     ' + ERROR_MESSAGE(); 
			INSERT INTO appls_ciip_unprocessed_appl_ids
			SELECT *, getdate(), ERROR_MESSAGE() FROM appls_ciip where appl_id = @currAppl_id
			print ' ---- Added record in appls_ciip_unprocessed_appl_ids for appl_id ' + CAST(@currAppl_id AS VARCHAR)
			SET @RecordsAdded = @RecordsAdded - 1
		END CATCH
		IF @loopId_appl_ciip = @max_id_appl_ciip
		BEGIN
		   print('Reached end of while loop')
			breaK
		END
	
	   set @loopId_appl_ciip = @loopId_appl_ciip + 1
   
	END -- END OF WHILE LOOP

	print 'done with step14 : ' + CAST(@RecordsAdded as varchar) + ' records inserted to appls table' 
END -- END OF IF 
ELSE
	BEGIN
		print 'done with step14  ---- zero records inserted to appls table' 
	END
-----------------------------END OF CODE-------------------------

---------------------------------------------
--Imran: 5/3/2019: GRAB GRANTS PD CONTACTS FROM GPMATS.
---------------------------------------------

TRUNCATE TABLE Grant_contacts_PD

	--BRING ALL PD FROM PD_DETAILS_VW ACTIVE & INACTIVE, THEN ERASE EMAILS IF PD IS INACTIVE
	INSERT INTO [dbo].[Grant_contacts_PD](PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS,INACTIVE_DATE)
	SELECT PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS,INACTIVE_DATE
	from openquery(CIIP,'SELECT DISTINCT PCC AS PROG_CLASS_CODE, PD_NAME AS PD_FULL_NAME,EMAIL_ADDRESS AS PD_EMAIL_ADDRESS ,INACTIVE_DATE
	FROM PD_DETAILS_VW WHERE PD_CODE IS NOT NULL')
	print 'done with step15'


	--ERASE EMAILS IF PD IS INACTIVE
	UPDATE Grant_contacts_PD SET PD_EMAIL_ADDRESS = NULL 
	WHERE INACTIVE_DATE IS NOT NULL
	print 'done with step16'


	print 'Disabled email for moved out PD =' + cast(@@ROWCOUNT as varchar)
	
	DELETE Grant_contacts_PD WHERE PROG_CLASS_CODE IS NULL

	--DATA CLEANUP
	--UPDATE APPLS SET APPLS.pd_full_name=NULL,APPLS.pd_email_address=NULL   --3348459  3403715

	 /*UPDATE PD_INFO FOR ALL PROG_CLASS_CODE DOWNLOADED FROM iMPACii*/
	 UPDATE A SET A.pd_full_name=B.PD_FULL_NAME, A.pd_email_address=B.PD_EMAIL_ADDRESS
	 FROM APPLS A LEFT OUTER JOIN Grant_contacts_PD B 
	 ON LEFT(A.PROG_CLASS_CODE,4)=LEFT(B.PROG_CLASS_CODE,4)
	 WHERE B.prog_class_code IS  NOT NULL  --328324/328708

	print 'PD INFO UPDATE =' + cast(@@ROWCOUNT as varchar)
	print 'done with step17'

/*   iMRAN: 6/7/2019
TRUNCATE TABLE Grant_contacts_PD

INSERT INTO [dbo].[Grant_contacts_PD](PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS)  
SELECT PROG_CLASS_CODE,PD_FULL_NAME,PD_EMAIL_ADDRESS
from openquery(CIIP,'SELECT DISTINCT ROLE_USAGE_CODE||CAY_CODE AS PROG_CLASS_CODE, PD_NAME AS PD_FULL_NAME, 
EMAIL_ADDRESS AS PD_EMAIL_ADDRESS FROM PD_ORG_VW WHERE ROLE_USAGE_CODE IS NOT NULL AND PD_END_DATE IS NULL')

print 'PD DETAIL DOWNLOADED =' + cast(@@ROWCOUNT as varchar)

DELETE Grant_contacts_PD WHERE PROG_CLASS_CODE IS NULL

 --UPDATE A SET A.pd_full_name =B.PD_FULL_NAME,  A.pd_email_address=B.PD_EMAIL_ADDRESS
 --FROM APPLS_CIIP A, Grant_contacts_PD B
 --WHERE LEFT(A.PROG_CLASS_CODE,4)=B.PROG_CLASS_CODE
 
 /*UPDATE PD_INFO FOR ALL PROG_CLASS_CODE DOWNLOADED FROM iMPACii*/
 UPDATE A SET A.pd_full_name=B.PD_FULL_NAME, A.pd_email_address=B.PD_EMAIL_ADDRESS
 FROM APPLS A LEFT OUTER JOIN Grant_contacts_PD B 
 ON LEFT(A.PROG_CLASS_CODE,4)=LEFT(B.PROG_CLASS_CODE,4)
 WHERE B.prog_class_code IS  NOT NULL  

 /*5/5/2019 : iMRAN JUST FOR RECORDING THAT i HAVE POP ONCE ALL PI EMAIL ADRESS FROM IMPAii 
 oN DAILY BASIS THIS WILL COME FROM APPLS_LOAD JOB
 */

 --UPDATE A SET A.PI_EMAIL_ADDR=B.PI_EMAIL_ADDR
 --FROM APPLS A, APPLS_CIIP_IMM B
 --WHERE A.APPL_ID=B.APPL_ID

  print 'PD INFO UPDATE =' + cast(@@ROWCOUNT as varchar)
*/
---------------------------------------------
--GRAB GRANTS CONTACTS FROM GPMATS.
---------------------------------------------
TRUNCATE TABLE Grant_contacts

INSERT INTO [dbo].[Grant_contacts]([SERIAL_NUM],[APPL_ID],[ADMIN_PHS_ORG_CODE],[SUPPORT_YEAR],[SUFFIX_CODE],[FY],[RESP_SPEC_FULL_NAME_CODE],[RESP_SPEC_EMAIL_ADDRESS],[BO_EMAIL_ADDR],[RESP_NPN_ID])  
SELECT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,BO_EMAIL_ADDR,RESP_SPEC_NPN_ID
from openquery(CIIP,'SELECT DISTINCT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,BO_EMAIL_ADDR,A.RESP_SPEC_NPN_ID
FROM GM_ACTION_QUEUE_VW A WHERE (A.SUFFIX_CODE LIKE ''%A%'' OR A.SUFFIX_CODE IS NULL) ')
	print 'done with step18'


--select * from Grant_contacts where resp_npn_id is not null   --145365
--select * from Grant_contacts where RESP_SPEC_FULL_NAME_CODE is not null   --145365
--select * from Grant_contacts where RESP_SPEC_EMAIL_ADDRESS is not null   --124129

--REMOVE EMAIL ADDRESS FOR THOSE WHO LEFT ORGANIZATION   --87662/87537
UPDATE Grant_contacts SET Grant_contacts.RESP_SPEC_EMAIL_ADDRESS = NULL
WHERE Grant_contacts.RESP_NPN_ID NOT IN 
(SELECT  epn_id FROM  openquery(CIIP,'select distinct epn_id  from NCI_PERSON_ORG_ROLES_T where ERE_CODE = ''GMSPEC'' AND END_DATE IS NULL') )  --87662

--select * from Grant_contacts where RESP_SPEC_EMAIL_ADDRESS is not null   --57703/57828
--select * from Grant_contacts where RESP_SPEC_EMAIL_ADDRESS is null and RESP_SPEC_FULL_NAME_CODE is not null   --87662
--select * from Grant_contacts where resp_npn_id=32803

print 'DOWNLOADED Grant_contacts INFO =' + cast(@@ROWCOUNT as varchar)
	print 'done with step19'

--CLEANUP OLD INFO
UPDATE APPLS SET BO_EMAIL_ADDRESS=NULL --WHERE BO_EMAIL_ADDRESS IS NOT NULL

--UPDATE BO EMAIL INTO APPLS TABLE
UPDATE A SET A.BO_EMAIL_ADDRESS=B.BO_EMAIL_ADDR
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID 
AND B.BO_EMAIL_ADDR IS NOT NULL
print 'UPDATED BO_EMAIL_ADDRESS IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)
	print 'done with step20'

--CLEANUP OLD INFO
--UPDATE APPLS SET gs_full_name=NULL,GS_EMAIL_ADDRESS=NULL WHERE (GS_EMAIL_ADDRESS IS NOT NULL OR GS_FULL_NAME IS NOT NULL)

--UPDATE SPEC FULL NAME AND EMAIL ADDRESS FROM GPMATS DATA BASED ON APPL_ID
UPDATE A SET A.gs_full_name=B.RESP_SPEC_FULL_NAME_CODE, a.GS_EMAIL_ADDRESS=b.RESP_SPEC_EMAIL_ADDRESS
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID AND isnull(A.fy,'')=isnull(B.FY,'') AND isnull(A.support_year,'')=isnull(B.SUPPORT_YEAR,'') AND isnull(A.suffix_code,'')=isnull(B.SUFFIX_CODE,'')

print 'UPDATED gs_full_name,GS_EMAIL_ADDRESS  IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)
	print 'done with step21'


/******** IMRAN: 6/7/2019
TRUNCATE TABLE Grant_contacts

INSERT INTO [dbo].[Grant_contacts]([SERIAL_NUM],[APPL_ID],[ADMIN_PHS_ORG_CODE],[SUPPORT_YEAR],[SUFFIX_CODE],[FY],[RESP_SPEC_FULL_NAME_CODE],[RESP_SPEC_EMAIL_ADDRESS],[BO_EMAIL_ADDR])  
SELECT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,BO_EMAIL_ADDR
from openquery(CIIP,'SELECT DISTINCT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,BO_EMAIL_ADDR 
FROM GM_ACTION_QUEUE_VW A 
WHERE (A.SUFFIX_CODE LIKE ''%A%'' OR A.SUFFIX_CODE IS NULL) 
AND A.RESP_SPEC_NPN_ID = (SELECT DISTINCT B.NPN_ID FROM NCI_PEOPLE_VW B WHERE B.INACTIVE_DATE IS NULL AND B.NPN_ID=A.RESP_SPEC_NPN_ID) ')  --57845

print 'DOWNLOADED Grant_contacts INFO =' + cast(@@ROWCOUNT as varchar)

--CLEANUP OLD INFO
UPDATE APPLS SET BO_EMAIL_ADDRESS=NULL --WHERE BO_EMAIL_ADDRESS IS NOT NULL

--UPDATE BO EMAIL INTO APPLS TABLE
UPDATE A SET A.BO_EMAIL_ADDRESS=B.BO_EMAIL_ADDR
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID 
AND B.BO_EMAIL_ADDR IS NOT NULL
print 'UPDATED BO_EMAIL_ADDRESS IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)  --42619

--CLEANUP OLD INFO
UPDATE APPLS SET gs_full_name=NULL,GS_EMAIL_ADDRESS=NULL

--UPDATE SPEC FULL NAME AND EMAIL ADDRESS FROM GPMATS DATA BASED ON APPL_ID
UPDATE A SET A.gs_full_name=B.RESP_SPEC_FULL_NAME_CODE, a.GS_EMAIL_ADDRESS=b.RESP_SPEC_EMAIL_ADDRESS
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID AND isnull(A.fy,'')=isnull(B.FY,'') AND isnull(A.support_year,'')=isnull(B.SUPPORT_YEAR,'') AND isnull(A.suffix_code,'')=isnull(B.SUFFIX_CODE,'')
--54151

print 'UPDATED gs_full_name,GS_EMAIL_ADDRESS  IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)
**********/
--5/11/2019:Imran commented the following to refine this data
/*
INSERT INTO [dbo].[Grant_contacts]([SERIAL_NUM],[APPL_ID],[ADMIN_PHS_ORG_CODE],[SUPPORT_YEAR],[SUFFIX_CODE],[FY],[PD_FULL_NAME],[PD_EMAIL_ADDRESS],[PI_FULL_NAME],[PI_EMAIL_ADDRESS],[PI_EMAIL_ADDRESS_FOR_LETTER],[RESP_SPEC_FULL_NAME_CODE],[RESP_SPEC_EMAIL_ADDRESS],[BO_EMAIL_ADDR])  
SELECT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,PD_FULL_NAME,PD_EMAIL_ADDRESS,PI_FULL_NAME,PI_EMAIL_ADDRESS,PI_EMAIL_ADDRESS_FOR_LETTER,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,BO_EMAIL_ADDR
from openquery(CIIP,'SELECT DISTINCT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,B.EMAIL_ADDRESS as RESP_SPEC_EMAIL_ADDRESS,
PI_FULL_NAME,PI_EMAIL_ADDRESS,PI_EMAIL_ADDRESS_FOR_LETTER,
PD_FULL_NAME,PD_EMAIL_ADDRESS,BO_EMAIL_ADDR 
FROM GM_ACTION_QUEUE_VW A LEFT OUTER JOIN NCI_PEOPLE_VW B
ON A.SPEC_NPN_ID=B.NPN_ID AND B.ACTIVE_FLAG=''Y'' ')

----from openquery(CIIP_PROD,'SELECT DISTINCT SERIAL_NUM,APPL_ID,ADMIN_PHS_ORG_CODE,SUPPORT_YEAR,SUFFIX_CODE,FY,RESP_SPEC_FULL_NAME_CODE,RESP_SPEC_EMAIL_ADDRESS,SPEC_REASSIGNED_DATE, 
----PI_FULL_NAME,PI_EMAIL_ADDRESS,PI_EMAIL_ADDRESS_FOR_LETTER,
----PD_FULL_NAME,PD_EMAIL_ADDRESS,BO_EMAIL_ADDR FROM GM_ACTION_QUEUE_VW')

print 'DOWNLOADED Grant_contacts INFO =' + cast(@@ROWCOUNT as varchar)

--UPDATE BO EMAIL INTO APPLS TABLE
UPDATE A SET A.BO_EMAIL_ADDRESS=B.BO_EMAIL_ADDR
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID 
AND B.BO_EMAIL_ADDR IS NOT NULL
print 'UPDATED BO_EMAIL_ADDRESS IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)

--UPDATE SPEC FULL NAME AND EMAIL ADDRESS FROM GPMATS DATA BASED ON APPL_ID
UPDATE A SET A.gs_full_name=B.RESP_SPEC_FULL_NAME_CODE, a.GS_EMAIL_ADDRESS=b.RESP_SPEC_EMAIL_ADDRESS
FROM APPLS A, Grant_contacts B
WHERE A.APPL_ID=B.APPL_ID  
print 'UPDATED gs_full_name,GS_EMAIL_ADDRESS  IN APPLS TABLE =' + cast(@@ROWCOUNT as varchar)
*/
/******************* Check & Update User's authenticationtication  *************/

--Things to do
--Create an email to be sent to Rob/IC C-oordinator about this action 

/******************* End of Check & Up  *************/

/**--set arra_flag at the grant level
update grants set arra_flag='n' where arra_flag='y'

update grants SET arra_flag='y'
WHERE grant_id IN
(select distinct grant_id from vw_appls_arra where admin_phs_org_code='CA')
**/
	 
print 'Completed sp_egrants_maint_new'
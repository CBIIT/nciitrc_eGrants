SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


CREATE PROCEDURE [dbo].[sp_web_egrants_user_profile_bkp_12_08]  
  
@ic			varchar(10),  
@Operator	varchar(20),  
@type		varchar(2)  
  
AS  
-- =============================================  
-- Author:  Imran Omair  
-- Create date: 5/10/2013  
-- Description: If User user is validate then return list of comma separated menu otherwise return a code "NOTVALID"  
-- =============================================  
BEGIN  
  
DECLARE @T TABLE(nam varchar(20), val varchar(100))  
  
DECLARE   
@USEREMAIL VARCHAR(100)='',  
@USRNAME VARCHAR(100),  
@USRMENU VARCHAR(200),  
@EG   VARCHAR(10)='',  
@DM   VARCHAR(10)='',  
@MGT  VARCHAR(10)='',  
@CFT  VARCHAR(20)='',  
@ADM  VARCHAR(10)='',  
@ICRD  VARCHAR(20)='',  
@DSHBRD  VARCHAR(20)='',  
@QC   VARCHAR(20)=''  
  
DECLARE   
@PERSONID INT=NULL,   
@PRFLID INT=NULL,   
@POSITIONID INT=NULL,  
@qc_cout int,  
@qc_tab  int  
  
if @type='e' goto egrants_user  
if @type='d' goto docman_user  
if @type='ed' goto both_user  
-----------------------------  
egrants_user:  
  
---find qc tab  
  
set @qc_cout = (select COUNT(qc_person_id) from egrants  
where qc_date is not null and parent_id is null and qc_person_id=(select person_id from vw_people where userid=@Operator))  
if (@qc_cout =0 or @qc_cout is null) set @qc_tab=0 else set @qc_tab=1  
  
IF EXISTS(SELECT * FROM vw_people A, profiles B WHERE A.PROFILE_ID=B.PROFILE_ID AND B.profile=@IC AND A.userid=@Operator )   --AND A.active=1
BEGIN  
 SELECT @PERSONID=PERSON_ID,@PRFLID=B.profile_id,@POSITIONID=position_id FROM people A, profiles B WHERE A.profile_id=B.profile_id AND B.profile=@IC AND A.userid=@Operator AND A.active=1 and application_type='egrants'  
   
 SELECT @USRNAME = ISNULL(FIRST_NAME,'')+' '+ISNULL(last_name,''),@USEREMAIL=email  FROM people WHERE person_id=@PERSONID  
   
 SELECT @EG=egrants, @DM=Docman, @MGT=MGT, @CFT=cft, @ADM=admin, @ICRD=iccoord, @DSHBRD=dashboard  
 FROM people  
 WHERE person_id=@PERSONID  
   
 SET @USRMENU = ''  
 --IF @EG = 1  
  --SET @USRMENU='"eGrants"'  
  --SET @USRMENU='eGrants|G' 
 IF @MGT = 1  
  --SET @USRMENU=@USRMENU+',"Management"'  
  SET @USRMENU=@USRMENU+',Management|M'  
 IF @ADM = 1  
  --SET @USRMENU=@USRMENU+',"Admin"'  
  SET @USRMENU=@USRMENU+',Admin|A'  
 --IF @ICRD = 1  
  --SET @USRMENU=@USRMENU+',"IC Coordinator"'   
  --SET @USRMENU=@USRMENU+',IC Coordinator|I'   
 IF @DSHBRD = 1  
  --SET @USRMENU=@USRMENU+',"IC Coordinator"'   
  SET @USRMENU=@USRMENU+',Dashboard|D'   
 IF @qc_tab=1  
  SET @USRMENU=@USRMENU+',QC|Q'  
 IF @DM = 1   
  --SET @USRMENU=@USRMENU+',"DocMan"'  
  --SET @USRMENU=@USRMENU+',DocMan|O'  
  
 IF @CFT = 1  
  --SET @USRMENU=@USRMENU+',"Contract File Tracking"'  
  SET @USRMENU=@USRMENU+',Contract File Tracking|C'  
   
 INSERT @T VALUES('VALIDATION','OK')  
 INSERT @T VALUES('USERID',@Operator)  
 INSERT @T VALUES('IC',@ic)  
 INSERT @T VALUES('PERSONID',@PERSONID)  
 INSERT @T VALUES('POSITIONID',@POSITIONID)  
 INSERT @T VALUES('USERNAME',@USRNAME)  
 INSERT @T VALUES('USEREMAIL',@USEREMAIL)  
 INSERT @T VALUES('MENULIST',@USRMENU)   
END  
ELSE  
BEGIN   
 INSERT @T VALUES('VALIDATION','NOTVALID')   
END  
  
SELECT * FROM @T  
  
END  
  
return  
----------------------  
docman_user:  
  
IF EXISTS(SELECT * FROM docman..vw_people A, profiles B WHERE A.PROFILE_ID=B.PROFILE_ID AND B.profile=@IC AND A.userid=@Operator )  --AND A.active=1
BEGIN  
 SELECT @PERSONID=PERSON_ID,@PRFLID=B.profile_id,@POSITIONID=econ FROM docman..people A, profiles B WHERE A.profile_id=B.profile_id AND B.profile=@IC AND A.userid=@Operator AND A.active=1 and application_type='econtracts'  
   
 SELECT @USRNAME = ISNULL(FIRST_NAME,'')+' '+ISNULL(middle_initial,'')+' '+ISNULL(last_name,''),@USEREMAIL=email FROM docman..people WHERE person_id=@PERSONID  
   
 SELECT @EG=egrants, @DM=Docman, @CFT=cft  
 FROM docman..people  
 WHERE person_id=@PERSONID  
   
 SET @USRMENU = ''  
  
 IF @DM = 1   
  --SET @USRMENU=@USRMENU+',"DocMan"'  
--  SET @USRMENU=@USRMENU+'DocMan|O'  
 IF @CFT = 1  
  --SET @USRMENU=@USRMENU+',"Contract File Tracking"'  
  SET @USRMENU=@USRMENU+',Contract File Tracking|C'  
 IF @EG = 1  
  --SET @USRMENU='"eGrants"'  
  SET @USRMENU=@USRMENU+',eGrants|G'  
   
 INSERT @T VALUES('VALIDATION','OK')  
 INSERT @T VALUES('USERID',@Operator)  
 INSERT @T VALUES('IC',@ic)  
 INSERT @T VALUES('PERSONID',@PERSONID)  
 INSERT @T VALUES('POSITIONID',@POSITIONID)  
 INSERT @T VALUES('USERNAME',@USRNAME)  
 INSERT @T VALUES('USEREMAIL',@USEREMAIL)  
 INSERT @T VALUES('MENULIST',@USRMENU)   
END  
ELSE  
BEGIN   
 INSERT @T VALUES('VALIDATION','NOTVALID')   
END  
  
SELECT * FROM @T  
  
return  
---------------  
both_user:  
  
set @qc_cout = (select COUNT(qc_person_id) from egrants  
where qc_date is not null and parent_id is null and qc_person_id=(select person_id from vw_people where userid=@Operator))  
if (@qc_cout =0 or @qc_cout is null) set @qc_tab=0 else set @qc_tab=1  
  
IF EXISTS(SELECT * FROM vw_people A, profiles B WHERE A.PROFILE_ID=B.PROFILE_ID AND B.profile=@IC AND A.userid=@Operator )  ---AND A.active=1
BEGIN  
 SELECT @PERSONID=PERSON_ID,@PRFLID=B.profile_id,@POSITIONID=position_id FROM people A, profiles B WHERE A.profile_id=B.profile_id AND B.profile=@IC AND A.userid=@Operator AND A.active=1 and application_type='egrants'  
   
 SELECT @USRNAME = ISNULL(FIRST_NAME,'')+' '+ISNULL(middle_initial,'')+' '+ISNULL(last_name,''),@USEREMAIL=email  FROM people WHERE person_id=@PERSONID  
   
 SELECT @EG=egrants, @DM=Docman, @MGT=MGT, @CFT=cft, @ADM=admin, @ICRD=iccoord, @DSHBRD=dashboard  
 FROM people  
 WHERE person_id=@PERSONID  
 IF EXISTS(SELECT * FROM docman..vw_people A, profiles B WHERE A.PROFILE_ID=B.PROFILE_ID AND B.profile=@IC AND A.userid=@Operator)  --AND A.active=1
   BEGIN   
   declare @PERSONID_D INT=NULL,  @DM_D INT=NULL, @CFT_D INT= NULL  
    SELECT @PERSONID_D=PERSON_ID FROM docman..people A, profiles B WHERE A.profile_id=B.profile_id AND B.profile=@IC AND A.userid=@Operator AND A.active=1 and application_type='econtracts'  
    SELECT @DM_D=Docman, @CFT_D=cft  
    FROM docman..people  
    WHERE person_id=@PERSONID_D   
   END 
   
 SET @USRMENU = ''  
 --IF @EG = 1  
  --SET @USRMENU='"eGrants"'  
  --SET @USRMENU='eGrants|G' 
 IF @MGT = 1  
  --SET @USRMENU=@USRMENU+',"Management"'  
  SET @USRMENU=@USRMENU+',Management|M'  
 IF @ADM = 1  
  --SET @USRMENU=@USRMENU+',"Admin"'  
  SET @USRMENU=@USRMENU+',Admin|A'  
 --IF @ICRD = 1  
  --SET @USRMENU=@USRMENU+',"IC Coordinator"'   
  --SET @USRMENU=@USRMENU+',IC Coordinator|I'   
 IF @DSHBRD = 1  
  --SET @USRMENU=@USRMENU+',"IC Coordinator"'   
  SET @USRMENU=@USRMENU+',Dashboard|D'   
 IF @qc_tab=1  
  SET @USRMENU=@USRMENU+',QC|Q'  
 IF @DM = 1   
  --SET @USRMENU=@USRMENU+',"DocMan"'  
  --SET @USRMENU=@USRMENU+',DocMan|O'  
 
 
 IF @CFT = 1  
  --SET @USRMENU=@USRMENU+',"Contract File Tracking"'  
  SET @USRMENU=@USRMENU+',Contract File Tracking|C'    
   
 INSERT @T VALUES('VALIDATION','OK')  
 INSERT @T VALUES('USERID',@Operator)  
 INSERT @T VALUES('IC',@ic)  
 INSERT @T VALUES('PERSONID',@PERSONID)  
 INSERT @T VALUES('POSITIONID',@POSITIONID)  
 INSERT @T VALUES('USERNAME',@USRNAME)  
 INSERT @T VALUES('USEREMAIL',@USEREMAIL)  
 INSERT @T VALUES('MENULIST',@USRMENU)   
END  
ELSE  
BEGIN   
 INSERT @T VALUES('VALIDATION','NOTVALID')   
END  
  
SELECT * FROM @T  
  
  
return   
----------------------  

GO


''wscript.echo "Hello"
'''''===============================================================
''''' 8/7/2017 : Imran : This app will act as main router to all theirs as well users. If an email is set to be uploaded this 
''''' should be uploaded to all tiers. Now that we have servers/loaders for all tiers. we can afford to separate apps for different tiers runn on the tier itself.
''''' eRA Notifications will be read and routed only from here the production server. Therefore forwarding to dev and stage emails respectively.
'''''
'''''===============================================================

startTimeStamp=Now	
	logDir="C:\eGrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)
	Dim ItemsProcessed	
	Dim oPkg, oConn, oRs, oRsU, cmd, dBug
	Dim OtlkApps 'as Object
	Dim objNS 'As Outlook.NameSpace
	dBug="n"
	dBugEmail = "robin.briggs@nih.gov"
	eGrantsDevEmail = "eGrantsDev@mail.nih.gov"
	eGrantsTestEmail = "eGrantsTest1@mail.nih.gov"
	eGrantsStageEmail = "eGrantsStage@mail.nih.gov"
	eFileEmail = "efile@mail.nih.gov"
	nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov"
	
	conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"


	Set oConn = CreateObject("ADODB.Connection")
	Set oRS = CreateObject("ADODB.Recordset")


	dirpath="NCI CA eRA Notifications (NIH/NCI)\Inbox\"	
	'dirpath="Public Folders - NCIOGAeGrantsProd@mail.nih.gov\All Public Folders\NCI\GAB\efile\test\"

	Call Process(dirpath,oConn,oRS)

	set oRS = Nothing
	oConn.close
	set oConn=Nothing
	set objNS=Nothing
	set OtlkApps=Nothing


    taskEndMssg=  "******* Task Completed! *******" & ItemsProcessed & " Mail Items Have Been Processed"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	
	
	Set objFS=Nothing

'==========================================	

Sub Process (dirpath,oConn,oRS)
	'wscript.echo "Hello you are in Process"
		
	Dim cmd	
	Dim sepchar 'As String
	Dim Documentid
	
	On Error Resume Next
	
	oConn.Open conStr
	Set cmd = CreateObject("ADODB.Command")     
	set cmd.ActiveConnection = oConn 	

	Const ForReading = 1, ForWriting = 2, ForAppending = 8
	'Set filesys = CreateObject("Scripting.FileSystemObject")

	'Set OtlkApps = CreateObject("Outlook.Application")
	Set OtlkApps = GetObject("","Outlook.application")
	'Set OtlkApps = GetObject(,"Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")	

	sepchar = "\"

	'Parse inputstr and Navigate to the folder
	If dirpath <> "" Then
		xArray = Split(dirpath, sepchar)
		i = 1
		Set CFolder = objNS.Folders(xArray(0))

		Do While i < UBound(xArray)
			Set CFolder = CFolder.Folders(xArray(i))
			i = i + 1
		Loop
	End If  'If dirpath <> "" Then

	Set OldFldr = CFolder.Folders("Old emails")

	itmcnt = CFolder.Items.Count
	''wscript.echo "Mail count in Inbox=" & itmcnt
	itmscncnt = 1
	itmtoprocess=0
	ItemsProcessed=0

	Do While CFolder.Items.Count > 0
		IF (dBug="y") Then
			'wscript.echo "Mail count=" & CFolder.Items.Count
		END IF
		
  		Set CItem = CFolder.Items(CFolder.Items.Count)
		itmtoprocess=itmtoprocess+1
		v_SubLine = CItem.Subject
		v_SenderID = getSenderID(CItem)

		IF ((InStr(v_SubLine,"Undeliverable: ") < 1)) Then
			v_Sender = CItem.Sender
			IF (InStr(v_SubLine,"eSNAP Received at NIH") > 0)  OR  (InStr(v_SubLine,"eRA Commons: RPPR for Grant ") > 0) Then
                           IF (InStr(v_SubLine," submitted to NIH with a Non-Compliance ") > 0) Then
				IF (InStr(v_SubLine," submitted to NIH with a Non-Compliance ") > 0) Then
						''''(1) load into eGrants
						''''---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"
						replysubj="category=eRANotification, sub=RPPR Non-Compliance, extract=1," & CItem.subject
					
						Set OutMail = CItem.Forward
						IF (dBug="n") Then
							With OutMail
								.Recipients.Add(eFileEmail)
								.Recipients.Add(eGrantsDevEmail)
								.Recipients.Add(eGrantsTestEmail)
								.Recipients.Add(eGrantsStageEmail)
								.Subject = replysubj
								.Send
								
							End With
						ELSE
							With OutMail

								.Recipients.Add(dBugEmail)
								.Recipients.Add(eGrantsDevEmail)
								.Subject = replysubj
								.Send								
							End With						
						END IF
						Set OutMail=nothing
					END IF	
			
					
					''''(2) forward to Bryan and Nicole
					Set OutMail = CItem.Forward
					IF (dBug="n") Then
						With OutMail 
							.Recipients.Add("jonesni@mail.nih.gov")
							.Recipients.Add("bakerb@mail.nih.gov")
							.Recipients.Add("edward.mikulich@nih.gov")							
							
							.Send	
						End With
					ELSE
						''wscript.echo "FOUND->"&v_SubLine
						With OutMail 
								.Recipients.Add(dBugEmail)
								.Recipients.Add(eGrantsDevEmail)
							.Send	
						End With					
					END IF
					Set OutMail=nothing
				ELSE
				'''wscript.echo "FOUND->"&v_SubLine
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail 
						.Recipients.Add("jonesni@mail.nih.gov")
						.Recipients.Add("bakerb@mail.nih.gov")
						.Recipients.Add("edward.mikulich@nih.gov")

						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail 
								.Recipients.Add(dBugEmail)
								.Recipients.Add(eGrantsDevEmail)
						.Send
					End With
				END IF
				Set OutMail=nothing
				END IF
		
			ELSEIF InStr(v_SubLine,"IC ACTION REQUIRED - Relinquishing Statement") > 0 Then
				'''wscript.echo "FOUND->"&v_SubLine
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail
						.Recipients.Add("emily.driskell@nih.gov")
						.Recipients.Add("dvellaj@mail.nih.gov")
						.Recipients.Add("edward.mikulich@nih.gov")									
						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail						
						.Recipients.Add(dBugEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Send
					End With
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_SubLine," Supplement Requested through ") > 0 Then
				'''wscript.echo "FOUND->"&v_SubLine
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail
						.Recipients.Add("NCIOGASupplements@mail.nih.gov")

						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail
						.Recipients.Add(dBugEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Send
					End With				
				END IF
				Set OutMail=nothing
			ELSEIF (InStr(v_SubLine," FCOI ") > 0  AND InStr(1,v_SubLine,"Automatic reply:") = 0) Then				
				IF  len(Trim(v_SubLine))<>0  THEN
					applid=getApplid(removespcharacters(v_SubLine),oConn)
					'''wscript.echo "FCOI => applid=" & applid
				END IF
				IF  len(Trim(applid)) <> 0  THEN	
					cmd.CommandText = "sp_getOfficersEmailForGrantNum"
					cmd.CommandType = 4						
					cmd.Parameters.Refresh
					cmd.Parameters(1).Value=applid
					cmd.Parameters(2).Value="SPEC"
					
					Set oRS = cmd.Execute
					If (oRS.BOF = True and oRS.EOF = True) Then
						SpecEmail=""
					Else	
						p_SpecEmail=oRs.Fields("Email_address_p").value
						b_SpecEmail=oRs.Fields("Email_address_b").value
					end if
					set oRS = Nothing
				END IF
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail
						.Recipients.Add("jonesni@mail.nih.gov")
						.Recipients.Add("hue.tran@nih.gov")
						.Recipients.Add("bakerb@mail.nih.gov")
						.Recipients.Add("dvellaj@mail.nih.gov")
						.Recipients.Add("agyemann@mail.nih.gov")
						.Recipients.Add("eugenia.chester@nih.gov")
						.Recipients.Add("emily.driskell@nih.gov")
						IF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) <> 0 AND p_SpecEmail <> b_SpecEmail THEN	
								.Recipients.Add(p_SpecEmail)
								.Recipients.Add(b_SpecEmail)
						ELSEIF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) <> 0 AND p_SpecEmail =  b_SpecEmail THEN	
								.Recipients.Add(p_SpecEmail)
						ELSEIF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) = 0  THEN	
								.Recipients.Add(p_SpecEmail)
						ELSEIF  len(Trim(p_SpecEmail)) = 0  AND  len(Trim(b_SpecEmail)) <> 0  THEN	
								.Recipients.Add(b_SpecEmail)
						END IF
						.Send
					End With
				ELSE					
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						IF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) <> 0 AND p_SpecEmail <> b_SpecEmail THEN
							replysubj="P="&p_SpecEmail&"B="&b_SpecEmail
						ELSEIF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) <> 0 AND p_SpecEmail =  b_SpecEmail THEN	
							replysubj="P="&p_SpecEmail
						ELSEIF  len(Trim(p_SpecEmail)) <> 0  AND  len(Trim(b_SpecEmail)) = 0  THEN	
							replysubj="P="&p_SpecEmail
						ELSEIF  len(Trim(p_SpecEmail)) = 0  AND  len(Trim(b_SpecEmail)) <> 0  THEN	
							replysubj="P="&b_SpecEmail
						END IF
						'wscript.echo "FCOI FOUND SPEC ID->"&replysubj
						.Subject = replysubj 
						.Send
						'''wscript.echo SpecEmail
					End With				
				END IF
				Set OutMail=nothing
			''''---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"	
			ELSEIF InStr(v_SubLine,"No Cost Extension Submitted") > 0 Then
				replysubj="category=eRANotification, sub=No Cost Extension, extract=1," & CItem.subject
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj
						.Send
					End With
				ELSE
					With OutMail
						''wscript.echo "FOUND->"&replysubj
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)				
						.Subject = replysubj
						.Send
					End With
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_SubLine," Change of Institution request for Grant ") > 0 Then
				replysubj=CItem.subject
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add("emily.driskell@nih.gov")
						.Recipients.Add("dvellaj@mail.nih.gov")
						.Recipients.Add("edward.mikulich@nih.gov")										
						.Subject = replysubj
						.Send
					End With
				ELSE
					''wscript.echo "Subject->"&v_SubLine
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj
						.Send
					End With				
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_Sender,"Public Access") > 0 Then
				replysubj="category=PublicAccess, extract=1, " & CItem.subject
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						''''===>STOPPING THIS UNTILL FIX IT/5/30 started
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					With OutMail
						''wscript.echo "FOUND->"&replysubj
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With				
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_SubLine,"JIT Request for Grant") > 0    Then  
					replysubj="category=JIT Info, sub=Reminder, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					IF (dBug="n") Then								
						With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
							.Subject = replysubj 
							.Send
						End With
					ELSE
						''wscript.echo "DON'T WANT THIS" & v_SubLine
						With OutMail
							.Recipients.Add(dBugEmail)	
							.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj & "URGENT ERROR"
							.Send
						End With					
					END IF
					Set OutMail=nothing								
			ELSEIF InStr(v_SubLine,"NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant") > 0 Then
			    IF (InStr(v_SubLine," R15 ") > 0) Then
				replysubj="category=eRANotification, sub=Late Progress Report, extract=1, " & CItem.subject

				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&replysubj
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With				
				END IF
				Set OutMail=nothing
			    END IF
			ELSEIF InStr(v_SubLine,"Expiring Funds") > 0  OR InStr(v_SubLine,"EXPIRING FUNDS-") > 0  Then  
	
				replysubj="category=Closeout, extract=2, " & CItem.subject
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Body=" "
						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(dBugEmail)	
						.Subject = replysubj 
						.Body=" "
						.Send
					End With				
				END IF
				Set OutMail=nothing								
			ELSEIF InStr(v_SubLine,"Prior Approval: ") > 0  Then  
				'''wscript.echo "FOUND Prior Approval_1"&v_SubLine
				Set OutMail = CItem.Forward
				IF (dBug="n") Then					
					With OutMail
						.Recipients.Add(nciGrantsPostAwardEmail)
						.Send					
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Send
					End With				
				END IF
					Set OutMail=nothing								
				''END IF
			ELSEIF InStr(v_SubLine,"FFR NOTIFICATION : REJECTED") > 0    Then  

				IF (InStr(lcASE(v_SubLine),"re: ffr notification") > 0  OR  InStr(LCase(v_SubLine),"fw: ffr notification") > 0)  Then  
					'''wscript.echo "DON'T WANT THIS" & v_SubLine
				ELSE
					replysubj="category=Notification, sub=FFR Rejection, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					
					IF (dBug="n") Then								
						With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
							.Subject = replysubj 
							.Send
						End With
					ELSE
						''wscript.echo "DON'T WANT THIS" & v_SubLine
						With OutMail
							.Recipients.Add(dBugEmail)	
							.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj 
							.Send
						End With					
					END IF
					Set OutMail=nothing								
				END IF
			ELSEIF InStr(v_SubLine,"eRA Commons: The Final RPPR - Additional Materials for Award") > 0    Then  

				IF (InStr(lcASE(v_SubLine),"re: eRA Commons: The Final RPPR ") > 0  OR  InStr(LCase(v_SubLine),"fw: eRA Commons: The Final RPPR ") > 0)  Then  
					'''wscript.echo "DON'T WANT THIS" & v_SubLine
				ELSE
					replysubj="category=FRAM: Request, sub=The Final RPPR, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					
					IF (dBug="n") Then								
						With OutMail
							'.Recipients.Add("efile@mail.nih.gov")
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						
							.Subject = replysubj 
							.Send
						End With
					ELSE
						''wscript.echo "DON'T WANT THIS" & v_SubLine
						With OutMail
							.Recipients.Add(dBugEmail)	
							.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj 
							.Send
						End With					
					END IF
					Set OutMail=nothing								
				END IF
			ELSEIF InStr(v_SubLine,"eRA Commons: PRAM for Grant") > 0    Then  

				IF (InStr(lcASE(v_SubLine),"re: eRA Commons: PRAM for Grant") > 0  OR  InStr(LCase(v_SubLine),"fw: eRA Commons: PRAM for Grant") > 0)  Then  
					'''wscript.echo "DON'T WANT THIS" & v_SubLine
				ELSE
					replysubj="category=PRAM: Requested, sub=PRAM for Grant, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					
					IF (dBug="n") Then								
						With OutMail
							'.Recipients.Add("efile@mail.nih.gov")
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						
							.Subject = replysubj 
							.Send
						End With
					ELSE
						''wscript.echo "DON'T WANT THIS" & v_SubLine
						With OutMail

							.Recipients.Add(dBugEmail)	
							.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj 
							.Send
						End With					
					END IF
					Set OutMail=nothing								
				END IF
																							  
				''If this is a PRAM Requested or a FRAM Requested
			ELSEIF InStr(v_SubLine,"FRAM Requested") > 0 OR InStr(v_SubLine, "PRAM Requested") > 0   Then  

				replysubj = ""
				
				IF InStr(v_SubLine,"FRAM Requested") > 0 Then  
					'' get the appl id from the grant number in the subject line
					IF  len(Trim(v_SubLine))<>0  THEN
						applid=getApplid(removespcharacters(v_SubLine),oConn)
					END IF
					'' set the applid, category, subcategory and the extract type to 1
					replysubj = "applid=" & applid & ", category=FRAM, sub=Request, extract=1, " & CItem.subject
				ELSEIF InStr(v_SubLine,"PRAM Requested") > 0 Then
					'' get the appl id from the grant number in the subject line
					IF  len(Trim(v_SubLine))<>0  THEN
						applid=getApplid(removespcharacters(v_SubLine),oConn)
					END IF
					replysubj = "applid=" & applid & ", category=PRAM, sub=Request, extract=1, " & CItem.subject
				END If

				Set OutMail = CItem.Forward
					
				IF (dBug="n") Then								
					With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					''wscript.echo "DON'T WANT THIS" & v_SubLine
					With OutMail

						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With					
				END IF
				Set OutMail=nothing	
			'' for ACR we want to make sure the subject has these two strings in it
			ELSEIF InStr(v_SubLine,"CHANGE_NOTICE_FOR") > 0 AND InStr(v_SubLine, "Application is withdrawn request") > 0   Then  
		
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					applid = getApplid(removespcharacters(v_SubLine),oConn)
				END IF
				
				'' set the applid, category, subcategory and the extract type to 1
				replysubj = "applid=" & applid & ", category=eRA Notification, sub=Application Withdrawn, extract=1, " & CItem.subject

				Set OutMail = CItem.Forward
				IF (dBug="n") Then								
					With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With						
				END IF
				
				Set OutMail=nothing	
				
			ELSEIF InStr(v_SubLine,"IRPPR Reminder") > 0 THEN
		
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					applid = getApplid(removespcharacters(v_SubLine),oConn)
				END IF
				
				'' set the applid, category, subcategory and the extract type to 1
				replysubj = "applid=" & applid & ", category=IRPPR, sub=Reminder, extract=1, " & CItem.subject

				Set OutMail = CItem.Forward
				IF (dBug="n") Then								
					With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With						
				END IF
				
				Set OutMail=nothing	
				
			ELSEIF InStr(v_SubLine,"FFR Reminder") > 0 AND InStr(v_SubLine, "FFR Past Due") > 0   Then  
		
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					applid = getApplid(removespcharacters(v_SubLine),oConn)
				END IF
				
				'' set the applid, category, subcategory and the extract type to 1
				replysubj = "applid=" & applid & ", category=FFR, sub=Reminder, extract=1, " & CItem.subject

				Set OutMail = CItem.Forward
				IF (dBug="n") Then								
					With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With						
				END IF
				
				Set OutMail=nothing	
				 
			ELSEIF InStr(v_SubLine,"ClinicalTrials.gov Results Reporting for Grant") > 0 THEN
		
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					lastWordInSubject = getLastWord(CItem.Subject)
					lastFourCharacters = Right(lastWordInSubject, 4)
					applid = getApplid(removespcharacters(v_SubLine),oConn)
				END IF
				
				'' set the applid, category, subcategory and the extract type to 1
				replysubj = "applid=" & applid & ", category=CT.gov, sub=Results Reporting Reminder NCT" & lastFourCharacters & " , extract=1, " & CItem.subject

				Set OutMail = CItem.Forward
				IF (dBug="n") Then								
					With OutMail
							.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Send
					End With
				ELSE
					With OutMail

						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj 
						.Send
					End With						
				END IF
				
				Set OutMail=nothing	
				
			ELSEIF InStr(lcase(v_SubLine),"closeout action required") > 0 Then  
				replysubj="category=closeout, sub=Past Due Documents Reminder, extract=1," & CItem.subject
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						.Subject = replysubj 
						.Body=" "
						.Send
					End With
				ELSE
					''wscript.echo "FOUND->"&v_SubLine
					With OutMail
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(dBugEmail)	
						.Subject = replysubj 
						.Body=" "
						.Send
					End With				
				END IF
				Set OutMail=nothing	
				
			ELSEIF InStr(v_SubLine,"Imran") > 0 Then
				''wscript.echo "FOUND Imran->"&v_SubLine		
			End If
		END IF	'''''((InStr(v_SubLine,"Undeliverable: ") < 1)) Then
			itmscncnt=itmscncnt+1
			CItem.Move(OldFldr)	
	
			processTimeStamp=Now
			If Err.Number <> 0 Then
				logMssg= "Error Occured! => EmailSender:" & v_SenderID & "; " & "Subjectline :" & v_SubLine & "; "& " Recieved Date: " & CItem.ReceivedTime 
				errorMssg= "Error Number: " & Err.Number & "," & "Error Description: " & Err.Description & ", " & "Error Source: " & Err.Source
			Else		
				logMssg= "Processed! => EmailSender:" & v_SenderID & "; " & "Subjectline :" & v_SubLine & "; " & " Recieved Date: " & CItem.ReceivedTime 
				errorMssg=""
				
			End If			
			
			Call writeLog(forAppending, logMssg, errorMssg, processTimeStamp)
			
			If Err.Number <> 0 Then
			Errmsg1="Error Occured! PROD eMailRouter vbs"
			Errmsg2=errorMssg
			Call RaiseErrortoAdmin(CItem,Errmsg1,Errmsg2)
				If((Err.Number=-2147221223) Or (Err.Source= "Microsoft Outlook")) Then
						strServiceName = "ClickToRunSvc"
						Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
						Set colListOfServices = objWMIService.ExecQuery("Select * from Win32_Service Where Name ='" & strServiceName & "'")
						For Each objService in colListOfServices
						objService.StartService()
						Next
						For Each objService in colListOfServices
						objService.StartService()
						Next
						CItem.Move(OldFldr)				
				End If
				Err.Clear
			Exit DO
			End If
			
			Err.Clear
			
			ItemsProcessed=ItemsProcessed + 1
			
			If ItemsProcessed>=50 Then
			Errmsg1="Warning! PROD eMailRouter vbs has processed 50 mail items in one instance!"			
			Errmsg2="Hello Admin, 50 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing."
			Call emailme(Errmsg1,Errmsg2)
			Exit DO	
			End If		
	
 	Loop	
	'Wend
	
	Set objNS = Nothing
	Set OtlkApps = Nothing
	
End Sub
'==========================================
'==========================================
Function removespcharacters(txt)
            txt = Replace(txt, "vbLf", "vbCrLF")
            txt = Replace(txt, ":", " ")
            txt = Replace(txt, "/", " ")
            txt = Replace(txt, "\", " ")
            txt = Replace(txt, "&", "and")
            txt = Replace(txt, ";", " ")
            txt = Replace(txt, "<", " ")
            txt = Replace(txt, ">", " ")
            txt = Replace(txt, "<<", " ")
            txt = Replace(txt, ">>", " ")
            txt = Replace(txt, "^", " ")
            txt = Replace(txt, "%", " ")
            txt = Replace(txt, "@", " ")
            txt = Replace(txt, "'", " ")
            txt = Replace(txt, " ", "")
            removespcharacters = LTrim(RTrim(txt))
End Function
'==========================================
'==========================================
Function getApplid(str,oConn)
	Dim oRsU
	Set oRsU = CreateObject("ADODB.Recordset")
		
	strSQLTextU = "select dbo.Imm_fn_applid_match( ' "& str &" ') as applid"
	''''''MsgBox  strSQLTextU

	set oRSU =  oConn.execute(strSQLTextU)				
		
	If IsNull(oRsU.Fields("applid").value) then
		getApplid=""
	else
		getApplid=oRsU.Fields("applid").value
	end if
			
	set oRsU=Nothing

End Function
'==========================================
'==========================================
Function getSenderID(CItem)

	IF CItem.SenderEmailType = "EX" Then			

			  set objSender = CItem.Sender
			  If Not (objSender Is Nothing) Then
			    set objExchUser = CItem.Sender.GetExchangeUser()
			    If Not (objExchUser Is Nothing) Then			      
			       SenderID = objExchUser.Alias
			    End If
			  End If

			IF Len(SenderID) = 0 Then
			    SenderID = GetAlias(CItem)
			END IF
		
    ElseIf CItem.SenderEmailType = "SMTP" Then
       			SenderID = CItem.SenderEmailAddress
    End If
	getSenderID=SenderID			

End Function
'==========================================
'==========================================
Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="eMailRouter-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
		set logOutput=objFS.OpenTextFile(logDir & flname,  code, True)
	
	If(errorInfo="") Then 
		logRecord= timeStamp & "  -" & vbTab & message 
	Else		
		logRecord= timeStamp & "  -" & vbTab & message & vbNewLine & vbTab & vbTab & "      -> " & errorInfo	
	End If
	
	logOutput.WriteLine(logRecord)
	logOutput.Close
			
End Function
'==========================================
Function RaiseErrortoAdmin(CItem,eRRMsg1,eRRMsg2)
	Set OutMail = CItem.Forward
	With OutMail
		.Recipients.Add("robin.briggs@nih.gov")
		.Recipients.Add("leul.ayana@nih.gov")
		.Subject = eRRMsg1 & " >>(Subj: " & CItem.Subject & ")" 
		.body=eRRMsg2 & vbCrLf & vbCrLf & CItem.body
		.Send
	End With					
	Set OutMail=nothing	
	RaiseErrortoAdmin ="Done"
End Function
'==========================================

'==========================================
Function emailme(SubjMSG,BodyMSG)
		
	Set Mitem = OtlkApps.CreateItem(olMailItem )
	With Mitem
		.To="robin.briggs@nih.gov"			
		'.CC="leul.ayana@nih.gov"	
		.Subject = SubjMSG
		.BodyFormat = 2
		.HTMLBody = " " & BodyMSG
		.Send

	End With
	Set Mitem=nothing	
	emailme = true

End Function
'==========================================
Function getLastWord(str)
	wordz  = Split(str, " ")
	numberOfWords = Ubound(wordz)
	lastWord = wordz(numberOfWords) 'Remember arrays start at index 0
    getLastWord = lastWord
End Function

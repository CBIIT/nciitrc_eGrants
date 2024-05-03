''wscript.echo "Hello"
'''''===============================================================
''''' 8/7/2017 : Imran : This app will act as main router to all theirs as well users. If an email is set to be uploaded this 
''''' should be uploaded to all tiers. Now that we have servers/loaders for all tiers. we can afford to separate apps for different tiers runn on the tier itself.
''''' eRA Notifications will be read and routed only from here the production server. Therefore forwarding to dev and stage emails respectively.
'''''
'''''===============================================================

startTimeStamp=Now	

	dBug = getConfigVal("dBug")
	Verbose = getConfigVal("Verbose")
	ShowDiagnosticIfVerbose("starting Router ...", Verbose)
	
	logDir = getConfigVal("logDir")
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)
	Dim ItemsProcessed	
	Dim oPkg, oConn, oRs, oRsU, cmd, dBug
	Dim OtlkApps 'as Object
	Dim objNS 'As Outlook.NameSpace

	dBugEmail = "leul.ayana@nih.gov"
	eGrantsDevEmail = "eGrantsDev@mail.nih.gov"
	eGrantsTestEmail = "eGrantsTest1@mail.nih.gov"
	eGrantsStageEmail = "eGrantsStage@mail.nih.gov"
	eFileEmail = "efile@mail.nih.gov"
	nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov"
	
	conStr = getConfigVal("conStr")

	Set oConn = CreateObject("ADODB.Connection")
	Set oRS = CreateObject("ADODB.Recordset")

	dirpath = getConfigVal("dirpathRouter")
	
	Call Process(dirpath,oConn,oRS)
	
	call ShowDiagnosticIfVerbose("Completed Process()", Verbose)

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
	call ShowDiagnosticIfVerbose("Hello you are in Process", Verbose)
		
	Dim cmd	
	Dim sepchar 'As String
	Dim Documentid
	
	On Error Resume Next
	
	oConn.Open conStr
	Set cmd = CreateObject("ADODB.Command")     
	set cmd.ActiveConnection = oConn 	

	call ShowDiagnosticIfVerbose("Connected", Verbose)

	Const ForReading = 1, ForWriting = 2, ForAppending = 8
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")	

	sepchar = "\"
	
	call ShowDiagnosticIfVerbose("dirpath: " & dirpath, Verbose)

	'Parse inputstr and Navigate to the folder
	If dirpath <> "" Then
		xArray = Split(dirpath, sepchar)
		i = 1
		Set CFolder = objNS.Folders(xArray(0))

		Do While i < UBound(xArray)
			call ShowDiagnosticIfVerbose("xArray " & i & " : " & xArray, Verbose)
			call ShowDiagnosticIfVerbose("xArray i " & xArray(i), Verbose)
			Set CFolder = CFolder.Folders(xArray(i))
			i = i + 1
		Loop
	End If  'If dirpath <> "" Then
	
	call ShowDiagnosticIfVerbose("Finished stepping through CFolder xarray", Verbose)

	Set OldFldr = CFolder.Folders("Old emails")	'this is what it is in other envs
	'Set OldFldr = CFolder.Folders("old")
	
	call ShowDiagnosticIfVerbose("went to Old emails", Verbose)
	call ShowDiagnosticIfVerbose("Mail count=" & CFolder.Items.Count, Verbose)
	
	itmcnt = CFolder.Items.Count
	call ShowDiagnosticIfVerbose("Mail count in Inbox=" & itmcnt, Verbose)
	itmscncnt = 1
	itmtoprocess=0
	ItemsProcessed=0
	Do While CFolder.Items.Count > 0
		call ShowDiagnosticIfVerbose("Mail count=" & CFolder.Items.Count, Verbose)
		
  		Set CItem = CFolder.Items(CFolder.Items.Count)
		itmtoprocess=itmtoprocess+1
		v_SubLine = CItem.Subject
		v_Body = CItem.Body
		call ShowDiagnosticIfVerbose("subject: " & v_SubLine , Verbose)
		
		v_SenderID = getSenderID(CItem)
		call ShowDiagnosticIfVerbose("Sender= "  & v_Sender , Verbose)
		IF ((InStr(v_SubLine,"Undeliverable: ") < 1)) Then
			v_Sender = CItem.Sender
			IF (InStr(v_SubLine,"eSNAP Received at NIH") > 0)  OR  (InStr(v_SubLine,"eRA Commons: RPPR for Grant ") > 0) Then
                           IF (InStr(v_SubLine," submitted to NIH with a Non-Compliance ") > 0) Then
				IF (InStr(v_SubLine," submitted to NIH with a Non-Compliance ") > 0) Then
						''''(1) load into eGrants
						''''---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"
						replysubj="category=eRANotification, sub=RPPR Non-Compliance, extract=1," & CItem.subject
						call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine , Verbose)
						call ShowDiagnosticIfVerbose("FOUND->"&replysubj , Verbose)
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
							'.Recipients.Add("leul.ayana@nih.gov")
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
						'.Recipients.Add("leul.ayana@nih.gov")
						.Recipients.Add("emily.driskell@nih.gov")
						.Recipients.Add("dvellaj@mail.nih.gov")
						.Recipients.Add("edward.mikulich@nih.gov")									
						.Send
					End With
				ELSE
					call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine , Verbose)
					With OutMail						
						.Recipients.Add(dBugEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Send
					End With
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_SubLine," Supplement Requested through ") > 0 Then
				call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine , Verbose)
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail
						.Recipients.Add("NCIOGASupplements@mail.nih.gov")

						.Send
					End With
				ELSE
					call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine , Verbose)
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
					call ShowDiagnosticIfVerbose("FCOI => applid=" & applid , Verbose)
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
						call ShowDiagnosticIfVerbose(" oRS.State " &  oRS.State , Verbose)
						call ShowDiagnosticIfVerbose(" oRS(0) " & oRS(0) , Verbose)
						call ShowDiagnosticIfVerbose(" oRS(1) " &  oRS(1) , Verbose)
						call ShowDiagnosticIfVerbose(" oRs.Fields(filenamenumber).value " & oRs.Fields("filenamenumber").value , Verbose)
						call ShowDiagnosticIfVerbose(" Fields(ABC).value & Data found creating document in OutDir Alias " &  oRs.Fields("ABC").value & "Data found creating document in" & OutDir & Alias , Verbose)
						p_SpecEmail=oRs.Fields("Email_address_p").value
						b_SpecEmail=oRs.Fields("Email_address_b").value
						call ShowDiagnosticIfVerbose("Return from poroc (SPEC EMAIL)=>" & p_SpecEmail , Verbose)
						call ShowDiagnosticIfVerbose("Return from poroc (BACKUP_SPEC EMAIL)=>"& b_SpecEmail , Verbose)
						call ShowDiagnosticIfVerbose(" OutDir & Alias " &  OutDir & Alias, Verbose)
						call ShowDiagnosticIfVerbose("BODY  saved as Alias="&Alias, Verbose)
					end if
					set oRS = Nothing
				END IF
				Set OutMail = CItem.Forward
				IF (dBug="n") Then				
					With OutMail
						.Recipients.Add("jonesni@mail.nih.gov")
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
						call ShowDiagnosticIfVerbose("FCOI FOUND SPEC ID->" & replysubj, Verbose)
						.Subject = replysubj 
						.Send
						call ShowDiagnosticIfVerbose("wscript.echo SpecEmail"& SpecEmail, Verbose)
					End With				
				END IF
				Set OutMail=nothing
			''''---- IMP: STRIP SPACES FROM CATEGORY NAME "ERA NOTIFICATION"	
			ELSEIF InStr(v_SubLine,"No Cost Extension Submitted") > 0 Then
				replysubj="category=eRANotification, sub=No Cost Extension, extract=1," & CItem.subject
				call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine, Verbose)
				call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
				Set OutMail = CItem.Forward
				IF (dBug="n") Then
					With OutMail
						.Recipients.Add(eFileEmail)
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(eGrantsTestEmail)
						.Recipients.Add(eGrantsStageEmail)
						''.Recipients.Add("eGrantsTest@mail.nih.gov")
						
						'''''''''''''-----------ADD THE FOLLOWING FOR DEVELOPMENT TIER	AS NEEDED BASIS					
						''.Recipients.Add("leul.ayana@nih.gov")
						.Subject = replysubj
						.Send
					End With
				ELSE
					With OutMail
						call ShowDiagnosticIfVerbose( "FOUND-> replysubj: "&replysubj, Verbose)
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						'''''''''''''-----------ADD THE FOLLOWING FOR DEVELOPMENT TIER	AS NEEDED BASIS					
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
					call ShowDiagnosticIfVerbose( "Subject->"&v_SubLine, Verbose)
					With OutMail
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
						.Subject = replysubj
						.Send
					End With				
				END IF
				Set OutMail=nothing
			ELSEIF InStr(v_Sender,"public") > 0 Then
				call ShowDiagnosticIfVerbose( "found a public access email.", Verbose)
				'' get the appl id from the grant number in the subject line
				'' example : PASC: 5U24CA213274-08 - RUDIN, CHARLES M
				'' example2 : Compliant PASC: 5R01CA258784-04 - SEN, TRIPARNA
				IF  len(Trim(v_SubLine))<>0  THEN
					call ShowDiagnosticIfVerbose( "x v_SubLine: '" & v_SubLine & "'", Verbose)

					a=Split(v_SubLine,": ")
					call ShowDiagnosticIfVerbose( "second part: '" & a(1) & "'", Verbose)
					secondPart=a(1)
					
					subCat = ""
					call ShowDiagnosticIfVerbose( "LCase(v_SubLine): '" & LCase(v_SubLine) & "'", Verbose)
					'adding a " " here so if compliant is the first part of the string it never returns 0
					IF InStr(LCase(" " & v_SubLine),"compliant") >= 1 THEN
						call ShowDiagnosticIfVerbose( "found compliant", Verbose)
						subCat = "Compliant"
					END IF
					call ShowDiagnosticIfVerbose( "subCat: '" & subCat & "'", Verbose)

					b=Split(secondPart, " - ")
					middle = b(0)

					call ShowDiagnosticIfVerbose( "isolated grant id: '" & middle & "'", Verbose)
					applid = getApplid(removespcharacters(middle),oConn)
					call ShowDiagnosticIfVerbose( "applid  '" & applid & "'", Verbose)

					replysubj="category=PublicAccess, sub=" & subCat & ", applid=" & applid & ", extract=1, " & CItem.subject

					call ShowDiagnosticIfVerbose( "dBugEmail : " & dBugEmail, Verbose)
					call ShowDiagnosticIfVerbose( "eGrantsDevEmail : " & eGrantsDevEmail, Verbose)
					call ShowDiagnosticIfVerbose( "replysubj : " & replysubj , Verbose)

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
						.Recipients.Add(dBugEmail)	
						.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj 
							.Send
						End With				
					END IF

					
					
				END IF
				call ShowDiagnosticIfVerbose( "done" , Verbose)
				Set OutMail=nothing
				call ShowDiagnosticIfVerbose( "done2" , Verbose)
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
						call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine , Verbose)
						With OutMail
							.Recipients.Add(dBugEmail)
							.Recipients.Add(eGrantsDevEmail)							
							.Subject = replysubj & "URGENT ERROR"
							.Send
						End With					
					END IF
					'Set OutMail=nothing			
			'ELSEIF InStr(v_SubLine,"JIT Documents Have Been Submitted for Grant ") > 0    Then  		' mhoover 1/2024
			'		replysubj="category=eRA Notification, sub=JIT Submitted, extract=1, " & CItem.subject
			'		Set OutMail = CItem.Forward
			'		IF (dBug="n") Then								
			'			With OutMail
			'				.Recipients.Add(eFileEmail)
			'				.Recipients.Add(eGrantsDevEmail)
			'				.Recipients.Add(eGrantsTestEmail)
			'				.Recipients.Add(eGrantsStageEmail)
			'										
			'				'.Recipients.Add("leul.ayana@nih.gov")
			'				.Subject = replysubj 
			'				.Send
			'			End With
			'		ELSE
			'			''wscript.echo "DON'T WANT THIS" & v_SubLine
			'			With OutMail
			'				.Recipients.Add(dBugEmail)	
			'				.Subject = replysubj
			'				.Send
			'			End With					
			'		END IF
					Set OutMail=nothing						
			ELSEIF InStr(v_SubLine,"NIH Automated Email: ACTION REQUIRED - Overdue Progress Report for Grant") > 0 Then
			    IF (InStr(v_SubLine," R15 ") > 0) Then
				replysubj="category=eRANotification, sub=Late Progress Report, extract=1, " & CItem.subject
				call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine , Verbose)
				call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
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
					call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
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
				call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
				'''Only attached document has to be extracted so many make Body=""
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
					call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
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
				Set OutMail = CItem.Forward
				IF (dBug="n") Then					
					With OutMail
						.Recipients.Add(nciGrantsPostAwardEmail)
						.Send					
					End With
				ELSE
					call ShowDiagnosticIfVerbose("FOUND->"&replysubj, Verbose)
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
					call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
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
						call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
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
					call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
				ELSE
					replysubj="category=FRAM: Request, sub=The Final RPPR, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					
					IF (dBug="n") Then								
						With OutMail
							'.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						
							.Subject = replysubj 
							.Send
						End With
					ELSE
						call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
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
					call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
				ELSE
					replysubj="category=PRAM: Requested, sub=PRAM for Grant, extract=1, " & CItem.subject
					Set OutMail = CItem.Forward
					
					IF (dBug="n") Then								
						With OutMail
							'.Recipients.Add(eFileEmail)
							.Recipients.Add(eGrantsDevEmail)
							.Recipients.Add(eGrantsTestEmail)
							.Recipients.Add(eGrantsStageEmail)
						
							.Subject = replysubj 
							.Send
						End With
					ELSE
						call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
						With OutMail
							.Recipients.Add(dBugEmail)	
							.Recipients.Add(eGrantsDevEmail)
							.Subject = replysubj 
							.Send
						End With					
					END IF
					Set OutMail=nothing								
				END IF
				'' If this is a PRAM Requested or a FRAM Requested
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
					call ShowDiagnosticIfVerbose("DON'T WANT THIS" & v_SubLine, Verbose)
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
					''wscript.echo "DON'T WANT THIS" & v_SubLine
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
				
			ELSEIF InStr(lcase(v_SubLine),"closeout action required") > 0 Then  
				call ShowDiagnosticIfVerbose("Hello you are closing out a thing ...", Verbose)
				
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					isolated = getNthWord(v_SubLine, 4)
					call ShowDiagnosticIfVerbose("isolated: '" & isolated & "'", Verbose)
					applid = getApplid(removespcharacters(isolated),oConn)
				END IF
				
				replysubj="category=closeout, sub=Past Due Documents Reminder, applid=" & applid & ", extract=1," & CItem.subject
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
					call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine, Verbose)
					With OutMail
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(dBugEmail)	
						.Subject = replysubj 
						.Send
					End With				
				END IF
				Set OutMail=nothing		
				call ShowDiagnosticIfVerbose("Hello you closed out a thing", Verbose)
			ELSEIF InStr(lcase(v_SubLine),"closeout program action required") > 0 Then  
				call ShowDiagnosticIfVerbose("Hello you are closing out a PROGRAM thing ...", Verbose)
				
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					isolated = getNthWord(v_SubLine, 4)
					call ShowDiagnosticIfVerbose("isolated: '" & isolated & "'", Verbose)
					applid = getApplid(removespcharacters(isolated),oConn)
				END IF
				
				replysubj="category=closeout, sub=F-RPPR Acceptance Past Due Reminder, applid=" & applid & ", extract=1," & CItem.subject
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
					call ShowDiagnosticIfVerbose("FOUND->"&v_SubLine, Verbose)
					With OutMail
						.Recipients.Add(eGrantsDevEmail)
						.Recipients.Add(dBugEmail)	
						.Subject = replysubj 
						.Send
					End With				
				END IF
				Set OutMail=nothing		
				call ShowDiagnosticIfVerbose("Hello you closed out a PROGRAM thing", Verbose)
				
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
				
			ELSEIF InStr(v_SubLine,"SBIR/STTR Foreign Risk Management") > 0 THEN
				call ShowDiagnosticIfVerbose(""handling SBIR/STTR", Verbose)
				'' example body we want to snatch the appl id from :
				'1R43CA291415-01 (10921643) has undergone SBIR/STTR risk management assessment in accordance with the SBIR and STTR Extension Act of 2022 on 04/25/2024 12:19 PM. 
		
				'' get the appl id from the grant number in the subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					applid = Split(v_Body, " ")(1)
					call ShowDiagnosticIfVerbose("applid step 1 :" & applid, Verbose)
					'should look somethin like this :		(10921643)
					applid = Replace(Replace(applid, "(", ")"), ")", "")
					call ShowDiagnosticIfVerbose("applid step 2 :" & applid, Verbose)
				END IF
				
				'' set the applid, category, subcategory and the extract type to 1
				replysubj = "applid=" & applid & ", category=Funding, sub=DCI-InTh Cleared, extract=1, " & CItem.subject
				IF InStr(v_SubLine,"Not Cleared") > 0 THEN
					replysubj = "applid=" & applid & ", category=Funding, sub=DCI-InTh Not Cleared, extract=1, " & CItem.subject
				END IF
				call ShowDiagnosticIfVerbose("replysubj :" & replysubj, Verbose)

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
				call ShowDiagnosticIfVerbose("completed SBIR", Verbose)
				
			ELSEIF InStr(v_SubLine,"Imran") > 0 Then
				call ShowDiagnosticIfVerbose("FOUND Imran->"&v_SubLine, Verbose)
			End If
			call ShowDiagnosticIfVerbose("outer loop complete.", Verbose)
		END IF
	
			itmscncnt=itmscncnt+1

			call ShowDiagnosticIfVerbose("incrementing count", Verbose)
				
			CItem.Move(OldFldr)	
			
			call ShowDiagnosticIfVerbose("CItem.Moved", Verbose)
	
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
Function ShowDiagnosticIfVerbose(Message,Verbose)
	If (Verbose="y") Then
		wscript.echo Message
		If Err.Number <> 0 Then
			wscript.echo "Error Number: " & Err.Number
		End If	
	End If
End Function
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
		.Recipients.Add("leul.ayana@nih.gov")
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
		.To="leul.ayana@nih.gov"		
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
'==========================================
Function getNthWord(str,number)
	wordz  = Split(str, " ")    
	lastWord = wordz(number - 1) 'Remember arrays start at index 0
    getNthWord = lastWord
End Function
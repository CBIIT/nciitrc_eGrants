'**********************************************************************
' Created by: Imran Omair:\
' Created Date: 10/31/2015
' Description: This dts checks efile publick folder and extract all emails, finds out egrants node, and attach email and it's attachment to egnts.
'Released date: 
'Modification description:
'Modification date:
'************************************************************************

'Notes: Omair replaced with woldezf@mail

	startTimeStamp=Now	
	logDir="C:\eGrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)
	
	Dim dirpath
	Dim oConn
	Dim ItemsProcessed
	Dim OtlkApps
	Dim objNS

	
	conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=eGrants"
	''conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=eim;Data Source=ncidb-d131-v\egrants_dev,52300\egrants_piv;Application Name=TestApp"
	''conStr = "Provider=SQLOLEDB.1;Password=Justice424!;Persist Security Info=True;User ID=egrantsuser_read;Initial Catalog=EIM;Data Source=ncidbprd,54500\mssqlprd;Application Name=egrants"
	Set oConn = CreateObject("ADODB.Connection")	
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")	
	oConn.Open conStr

	'dirpath="Public Folders - nciogastage@mail.nih.gov\All Public Folders\NCI\GAB\NCIOGASupplements\Test\"	
	dirpath="Public Folders - NCIOGAeGrantsProd@mail.nih.gov\All Public Folders\NCI\GAB\NCIOGASupplements\"	
	
	Call Process()
	
	
	oConn.close
	set oConn=Nothing	

	taskEndMssg=  "******* Task Completed! *******" & ItemsProcessed & " Mail Items Have Been Processed"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	
	
	Set objFS=Nothing

'==========================================
Sub Process()

	Dim oRS 	
	Dim CFolder
	Dim cmd	
	
	'OutDir = "E:\egrants\watch\IN\Email\Test\"
	'OutDir = "E:\egrants\watch\IN\Email\Prod\"
	'OutDir = "E:\egrants\watch\out\DEV\nciogasupplement\"
	OutDir = "C:\egrants\watch\out\"

	On Error Resume Next
	'Set OtlkApps = CreateObject("Outlook.application")
	
	''Set oRS = CreateObject("ADODB.Recordset")
	Set cmd = CreateObject("ADODB.Command")     
	set cmd.ActiveConnection = oConn	
	Set CFolder=getCurrFolder(objNS)	
	Set OldFldr = CFolder.Folders("old")
	
	ItemsProcessed = 0
	totcnt = CFolder.Items.Count
	ItemtoProcess=totcnt
	'------MAIN LOOP STARTS HERE....
	DO WHILE ItemtoProcess > 0		
			pa=" "
			fgn = " "
			applid=""
			tempapplid=""
			filenumbername=""
			profileid=1			
			em_time=""			
			v_SenderID = ""

			Set CItem = CFolder.Items(ItemtoProcess)
			v_SubLine = CItem.Subject
			v_SenderID=getSenderID(CItem)	
			''''   If Sender is nciogastage then these to forward to get uploaded only in TEMP folder. These are notification that egrants has sent them to PD
		IF Lcase(v_SenderID)="nciogastage"  THEN
				catname="Correspondence"
				IF (InStr(v_SubLine,"Change in Status") > 0) Then				
					subcatname="Supplement Status Change"
				elseIF (InStr(v_SubLine,"Admin Supplement ") > 0) Then				
					subcatname="Admin Supplement"
				elseIF (InStr(v_SubLine,"Response Required") > 0) Then				
					subcatname="Supplement Response Required"					
				elseIF (InStr(v_SubLine,"Diversity Supplement ") > 0) Then				
					subcatname="Diversity Supplement"				
				else 
					subcatname="Unknown"										
				End If

				Notification_filetype= "txt"
				''MsgBox "1==>SENDER=nciogastage"
				abc=ExtractNotificationIDElement(Trim(CItem.Body),2)
				applid=getTempApplid(abc,oConn)					
				pa=""					
					''''THERE ARE TWO CONDITION 1) IF PA IS GARBLED AND DOES NOT MATCH TEXT PATTERN THEN ERROR IT OUT AND INFORM EMILY/IMRAN
					''''' IF PA MATCHES THE PATTERN THEN ALLOTHERS RULLE WILL BE APPLIED					
		
							cmd.CommandText = "getPlaceHolder_new"
							cmd.CommandType = 4
							'cmd.CommandType = adCmdText				
							cmd.Parameters.Refresh
							cmd.Parameters(1).Value=applid
							cmd.Parameters(2).Value=pa
							cmd.Parameters(3).Value=CItem.ReceivedTime
							cmd.Parameters(4).Value=catname
							cmd.Parameters(5).Value=Notification_filetype
							cmd.Parameters(6).Value=CItem.Subject
							cmd.Parameters(7).Value=CItem.Body
							cmd.Parameters(8).Value=subcatname
					Set oRS = cmd.Execute

					If (oRS.BOF = True and oRS.EOF = True) Then
						set oRS = Nothing
						''MsgBox "Could not load OGA notification"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj="ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new to load OGA_Notification"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("woldezf@mail.nih.gov")	
							.Recipients.Add("omairi@mail.nih.gov")														
							.Subject = replysubj
							.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
							.Send
						End With
						Set OutMail=nothing	
						'Exit Do
					Else	
						''MsgBox oRS(0)
						filenumbername=oRS(0)
						set oRS = Nothing
						''MsgBox  "Return from poroc=>"& filenumbername
						Alias = filenumbername & ".txt"
						''MsgBox OutDir & Alias
						CItem.SaveAs OutDir & Alias, olTXT	
						''MsgBox "BODY  saved as Alias="&Alias		
					end if
		ELSEIF Lcase(v_SenderID)="caeranotifications"  THEN
				'or Lcase(v_SenderID)="omairi" THEN
				param=0
				beginpos=1
				catname="eRA Notification"
				IF (InStr(v_SubLine," Supplement Requested ") > 0) Then				
					subcatname="Supplement Requested"
				else
					subcatname="Unknown"
				End If

				Notification_filetype= "txt"

				''substr=v_SubLine
				'------------------FW: NIH Automated Email: IC ACTION REQUIRED - 1R15CA161634-01A1 Supplement Requested through PA-14-077
				'------------------Grab equivalent appl id from grant number in subject line			
				''-----Try to get Parent ApplId  from subject line
				IF  len(Trim(v_SubLine))<>0  THEN
					applid=getApplid(removespcharacters(v_SubLine),oConn)
				END IF
				
				''-----If not found then Try to get Parent ApplId  from email body
				IF  len(Trim(applid)) = 0  THEN
					applid=getApplid(removespcharacters(CItem.body),oConn)
				END IF

				''-----'If Still appl id is blank then send this email to administrator
				IF  len(Trim(applid)) = 0  THEN		
					'If couldn't find a proper identification, email to emily to have a look into this in Admin area
					replysubj="ERROR: Supplement could not identified"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("woldezf@mail.nih.gov")		
						.Recipients.Add("omairi@mail.nih.gov")		
						.Subject = replysubj
						.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
						.Send
		   			End With
					Set OutMail=nothing
				ELSE 
					pa=getpa(removespcharacters(v_SubLine),oConn)
				
					''''THERE ARE TWO CONDITION 1) IF PA IS GARBLED AND DOES NOT MATCH TEXT PATTERN THEN ERROR IT OUT AND INFORM EMILY/IMRAN
					''''' IF PA MATCHES THE PATTERN THEN ALLOTHERS RULLE WILL BE APPLIED				
						''Wscript.echo applid & vbTab & pa & vbTab & catname & vbTab & subcatname  
							cmd.CommandText = "getPlaceHolder_new"
							cmd.CommandType = 4
							'cmd.CommandType = adCmdText				
							cmd.Parameters.Refresh
							cmd.Parameters(1).Value=applid
							cmd.Parameters(2).Value=pa
							cmd.Parameters(3).Value=CItem.ReceivedTime
							cmd.Parameters(4).Value=catname
							cmd.Parameters(5).Value=Notification_filetype
							cmd.Parameters(6).Value=CItem.Subject
							cmd.Parameters(7).Value=CItem.Body
							cmd.Parameters(8).Value=subcatname

					Set oRS = cmd.Execute

					If (oRS.BOF = True and oRS.EOF = True) Then
						''set oRS = Nothing
						'''MsgBox "No data found Sending email to admin"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj="ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("woldezf@mail.nih.gov")	
							.Recipients.Add("omairi@mail.nih.gov")		
							.Subject = replysubj
							.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
							.Send
						End With
						Set OutMail=nothing	
						'Exit Do
					Else						
												
						filenumbername=oRS(0)		
						''MsgBox  "Return from poroc=>"& filenumbername
						Alias = filenumbername & ".txt"
						''MsgBox filenumbername & "Data found creating document in" & OutDir & Alias
						''MsgBox OutDir & Alias
						CItem.SaveAs OutDir & Alias, olTXT
						set oRS = Nothing						
						''MsgBox "BODY  saved as Alias="&Alias		
					end if

				END IF

		ELSEIF Lcase(v_SenderID)="driskelleb" or Lcase(v_SenderID)="jonesni" OR Lcase(v_SenderID)="omairi" OR Lcase(v_SenderID)="waldezf" THEN

				''''RULE TO UPOLOAD VIA EMAIL UNDER SUPPLEMENT
				''''MUST HAVE: category=correspondence, OR category=application file, 
				''''MUST HAVE: grantnumber=<<full parent grant number>>  IF FULL GRANT NUMBER IS NOT PRESENT IN SUBJECT LINE OR BODY
				''''ONE EMAIL ONE UPLOAD EITHER WITH BODY AND NO ATTACHMENT OR WITH ATTACHMENT AND NO EMAIL BODY
				''''IF THERE IS AN EMAIL BODY AND AN ATTACHENT IN EMAIL WITH  "category=application file, " in subject line only attachent wil be uploaded under category mentioned
				''''IF YOU HAVE AN EMAIL WITH BODY AND ATTACHMENT, THEN SEND TWO EMAILS ONE FOR BODY UNDER CORRESPONDENCE AND ANOTHER EMAIL WITH ATTCHMENT ONLY FOR APPLICATION FILE

				param=0
				beginpos=1
				substr=v_SubLine
				Set CAttachments = CItem.Attachments
				'''''Fetch category and Grant number
				WHILE  InStr(substr, ",") > 0
			            		pos = InStr(substr, ",")
            					V_saStr = Lcase(Mid(substr, 1, pos -1))    

					IF InStr(V_saStr,"grantnumber") > 0 Then
						fgn=Mid(V_saStr,InStr(V_saStr,"grantnumber"),pos-1)
						fgn=Extractvalue(fgn,"grantnumber")
					ELSEIF InStr(V_saStr,"category") > 0 Then
						category=Mid(V_saStr,InStr(V_saStr,"category"),pos-1)
						category=Extractvalue(category,"category")
					ELSEIF InStr(V_saStr,"applid") > 0 Then
						applid=Mid(V_saStr,InStr(V_saStr,"applid"),pos-1)
						applid=Extractvalue(applid,"applid")	
					ELSEIF InStr(V_saStr,"sub") > 0 Then
						subcat=Mid(V_saStr,InStr(V_saStr,"sub"),pos-1)
						subcat=Extractvalue(subcat,"sub")	
					End If

					beginpos=pos+1
					substr=Mid(substr, beginpos, Len(substr))    
					param=param + 1
        				WEND								
				If (Trim(Lcase(category))="") Then
					category = "correspondence"
				end If

				If (Trim(Lcase(category)) = "correspondence") and (CAttachments.Count=0) and (Len(Trim(CItem.Body)) >0) THEN
					Notification_filetype="txt"
				ELSEIF (Trim(Lcase(category)) = "application file") and (CAttachments.Count>0) THEN
					CName = removejunk(CAttachments(1).FileName)
					Notification_filetype = getFileType(CName)					
				end if
	
				''-----Try to get Parent ApplId  from subject line			
				IF (len(Trim(v_SubLine))>0) and (len(Trim(applid)) = 0) and (len(Trim(fgn)) > 0)  THEN
					applid=getApplid(removespcharacters(fgn),oConn)
				ELSEIF (len(Trim(v_SubLine))>0) and (len(Trim(applid)) = 0) and (len(Trim(fgn)) = 0)  THEN
					applid=getApplid(removespcharacters(v_SubLine),oConn)					
				END IF
				
				''-----If STILL APPL_ID not found then Try to get Parent ApplId  from email body
				IF  len(Trim(applid)) = 0  THEN
					applid=getApplid(removespcharacters(CItem.body),oConn)
				END IF

				''-----'If Still appl id is blank then send this email to administrator
				IF  len(Trim(applid)) = 0  THEN		
					'If couldn't find a proper identification, email to emily to have a look into this in Admin area
					replysubj="ERROR: GRANT NUMBER OR APPL_ID COULD BE IDENTIFIED EITHER IN SUBJECT OR EMAIL BODY"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("woldezf@mail.nih.gov")	
						.Recipients.Add("omairi@mail.nih.gov")													
						.Subject = replysubj
						.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
						.Send
		   			End With
					Set OutMail=nothing
				ELSEIF (lcase(category)="correspondence") and (len(Trim(subcat)) = 0)  THEN
					'If couldn't find a proper sub category, email to emily to have a look into this subject line.
					''RULE : if the category=correspondence then there must be a subcategory
					replysubj="INVALID SUBJECT LINE" 
					replyText = "Two parameter is Important. 1)category  2)grantnumber. If 1)category = coresspondence. You must add third parameter called sub=<<subcategoryname>>. Example category=correspondence,sub=admin supplement,grantnumber=SP30CA123456-65"
					replyText = replyText & vbNewLine & " If category=Application file, do not add third parameter sub=<<>> . Example : category=application file, grantnumber=SP30CA123456-65"
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("woldezf@mail.nih.gov")		
						.Recipients.Add("omairi@mail.nih.gov")	
						.Subject = replysubj
						.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
						.Send
		   			End With
					Set OutMail=nothing

				ELSEIF ( (len(applid) <> 0)  and (Lcase(category)="correspondence" or Lcase(category)="application file") ) THEN
						pa=""       'Dont need pa just to upload a file it is needed for a workflow
						cmd.CommandText = "getPlaceHolder_new"
						cmd.CommandType = 4
						'cmd.CommandType = adCmdText				
						cmd.Parameters.Refresh
						cmd.Parameters(1).Value=applid
						cmd.Parameters(2).Value=pa
						cmd.Parameters(3).Value=CItem.ReceivedTime
						cmd.Parameters(4).Value=category
						cmd.Parameters(5).Value=Notification_filetype
						cmd.Parameters(6).Value=""
						cmd.Parameters(7).Value=""
						cmd.Parameters(8).Value=subcat

					Set oRS = cmd.Execute

					If (oRS.BOF = True and oRS.EOF = True) Then
						set oRS = Nothing
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj="ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("woldezf@mail.nih.gov")	
							.Recipients.Add("omairi@mail.nih.gov")							
							.Subject = replysubj
							.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
							.Send
						End With
						Set OutMail=nothing	
					ELSEIF (Trim(Lcase(category)) = "correspondence") and (CAttachments.Count=0) and (Len(Trim(CItem.Body)) >0) THEN
						filenumbername=oRS(0)
						set oRS = Nothing
						Alias = filenumbername & ".txt"
						CItem.SaveAs OutDir & Alias, olTXT	
					ELSEIF (Trim(Lcase(category)) = "application file" and (CAttachments.Count>0)) THEN
						filenumbername=oRS(0)
						Alias = filenumbername & "." &Notification_filetype
						CAttachments(1).SaveAsFile (OutDir & Alias)
						set oRS = Nothing				
					end if

				END IF
		
			'''''IF v_SenderID IS NEITHER NCIOGASTAGE NOR caeranotifications NOR ExtractNotificationIDElement THIS MEANS THIS IS UNAUTHORIZED EMAIL , MOVE IT TO OLD AND INFORM ADMIN	
		ELSE 
				'' IF THIS IS A REPLY FROM PD OR PI
				abc=ExtractNotificationIDElement(Trim(CItem.Body),2)
				If  len(Trim(abc)) > 0  THEN
					''''MsgBox "Notification_id="&abc
					'''Assume this is a reply from PD check the DB for this Notification and inform Emily
					isReply=CheckIFreply(abc,v_SenderID,oConn)
					pa=getpa(removespcharacters(v_SubLine),oConn)					
					applid=getTempApplid(abc,oConn)
					catname="Correspondence"
					subcat="Supplement Response"
					Notification_filetype= "txt"
							cmd.CommandText = "getPlaceHolder_new"
							cmd.CommandType = 4
							'cmd.CommandType = adCmdText				
							cmd.Parameters.Refresh
							cmd.Parameters(1).Value=applid
							cmd.Parameters(2).Value=pa
							cmd.Parameters(3).Value=CItem.ReceivedTime
							cmd.Parameters(4).Value=catname
							cmd.Parameters(5).Value=Notification_filetype
							cmd.Parameters(6).Value=CItem.Subject
							cmd.Parameters(7).Value=CItem.Body
							cmd.Parameters(8).Value=subcat

					Set oRS = cmd.Execute

					If (oRS.BOF = True and oRS.EOF = True) Then
						set oRS = Nothing
						''MsgBox "No data found"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj="ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						Set OutMail = CItem.Forward
						With OutMail
							.Recipients.Add("woldezf@mail.nih.gov")	
							.Recipients.Add("omairi@mail.nih.gov")														
							.Subject = replysubj
							.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
							.Send
						End With
						Set OutMail=nothing	
						'Exit Do
					Else	
						''MsgBox oRS(0)
						filenumbername=oRS(0)
						set oRS = Nothing
						''MsgBox  "Return from poroc=>"& filenumbername
						Alias = filenumbername & ".txt"
						'''''MsgBox OutDir & Alias
						CItem.SaveAs OutDir & Alias, olTXT	
						''MsgBox "BODY  saved as Alias="&Alias		
					end if
					''MsgBox "2==>SENDER=caeranotifications==>db IS DONE"	
				''IF THIS EMAIL IS NOT EVEN A REPLY FROM PD/PI THEN THIS IS JUNK	
				ELSE
					'If couldn't find a proper Notification ID, this means this email from somebody else OR JUNK
					replysubj="UN Identified email: NCIOGASupplent public folder: "
					Set OutMail = CItem.Forward
			   		With OutMail
						.Recipients.Add("woldezf@mail.nih.gov")	
						.Recipients.Add("omairi@mail.nih.gov")							
						.Subject = replysubj
						.Body = replyText & vbNewLine  & vbNewLine & CItem.body			
						.Send
		   			End With
					Set OutMail=nothing
				END If

		END IF   '''''	
			Call MovetoOldFolder(CFolder,CItem)
			
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
			Errmsg1="Error Occured! PROD Add_Suppl vbs"
			Errmsg2=errorMssg
			Call RaiseErrortoAdmin(CItem,Errmsg1,Errmsg2)
			Exit DO
			End If
			
			Err.Clear
			
			ItemsProcessed=ItemsProcessed + 1
			
			If ItemsProcessed>=30 Then
			Errmsg1="Warning! PROD Add_Suppl vbs has processed 30 mail items in one instance!"			
			Errmsg2="Hello Admin, 30 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing."
			Call emailme(Errmsg1,Errmsg2)
			Exit DO	
			End If	
	
			ItemtoProcess=CFolder.Items.Count
	
	LOOP
	set oRS = Nothing
	Set objNS = Nothing
	Set OtlkApps = Nothing
	'Set OutMail = Nothing
End Sub

'==========================================
Function getCurrFolder(objNS)
	sepchar = "\"
	Dim CurFolder	

	'Parse inputstr and Navigate to the folder
	If dirpath <> "" Then
		xArray = Split(dirpath, sepchar)
		i = 1
		Set CurFolder = objNS.Folders(xArray(0))

		Do While i < UBound(xArray)
			Set CurFolder = CurFolder.Folders(xArray(i))
			i = i + 1
		Loop
	End If  'If dirpath <> "" Then
	set getCurrFolder=CurFolder	
End Function	
'==========================================
'==========================================

Function ExtractNotificationIDElement(str, n)

	'   Returns the nth element from a string,
	'   using a specified separator character

	Dim x 'As Variant
	x = Split(str, "Notification Id=")
	If n > 0 And n - 1 <= UBound(x) Then

		If  len(Trim(x(n-1))) > 10  THEN
			ExtractNotificationIDElement =onlyDigits( x(n - 1))
		else
			ExtractNotificationIDElement = x(n - 1)
		end If
	Else
		ExtractNotificationIDElement = ""
	End If
	''''''MsgBox "ExtractElement out "+ExtractNotificationIDElement
End Function
'==========================================
Function onlyDigits(s )
    Dim retval 
    retval = ""

    For i = 1 To 10
        If Mid(s, i, 1) >= "0" And Mid(s, i, 1) <= "9" Then
            retval = retval + Mid(s, i, 1)
        End If
    Next
    onlyDigits = retval

End Function
'==========================================
Function ExtractCategory(p)
	Dim catname 'As String
	Dim namevalsepa 'As String
       
	catname = "cat"
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = catname Then
			ExtractCategory = x(1)
		Else
			ExtractCategory = " "
		End If
    	Else
            		ExtractcPIID = " "
    	End If
End Function
'==========================================
Function getAttachmentsCount(objItem )
	Set CAttach = objItem.Attachments
	getAttachmentsCount = CAttach.Count
End Function
'==========================================

Function getDateTime(dt)
	getdateTime = DatePart("M", dt) & DatePart("D", dt) & DatePart("YYYY", dt) & DatePart("H", dt) & DatePart("N", dt) & DatePart("S", dt) 
End Function
'==========================================

Function removejunk(flnm)
            flnm = Replace(flnm, ":", " ")
            flnm = Replace(flnm, "/", " ")
            flnm = Replace(flnm, "\", " ")
            flnm = Replace(flnm, "&", "and")
            flnm = Replace(flnm, ";", " ")
            removejunk = LTrim(RTrim(flnm))
End Function
'==========================================

Function getFileType(flnm)
	V_flType = flnm
            While InStr(V_flType, ".") > 0	
		pos = InStr(V_flType, ".")
		V_flType = Mid(V_flType, pos + 1, Len(V_flType))
            Wend

           getFileType = V_flType
End Function
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

Function MovetoOldFolder(CFolder,CItem)
	set oldfolder = CFolder.Folders("old")
	CItem.Move(oldfolder)
	MovetoOldFolder="Done"
	''MsgBox "Moved to old folder"
End Function
'==========================================

Function getMailtoProcess(fldr,lastrun)
	itmcnt = fldr.Items.Count
	itmscncnt = 1
	itmtoprocess=0
	While itmcnt >= itmscncnt
  		Set TItem = fldr.Items(itmscncnt)
  		If (CDate(TItem.ReceivedTime) > CDate(lastrun)) Then
			itmtoprocess=itmtoprocess+1
		end if
	itmscncnt=itmscncnt+1
 	Wend
 	getMailtoProcess=itmtoprocess
End Function
'==========================================

Function correctquote(txt1)
            txt2 = Replace(txt1, "'", "''")
            correctquote = LTrim(RTrim(txt2))
End Function

'==========================================
'==========================================
Function ExtractGrantNumber(p)
	'   best try to ditermin whether fgn is an NIH Grant Number
	'   using a specified Structure
    	Dim fgnname 'As String
    	Dim namevalsepa 'As String

    	fgnname = "grantnumber"
    	namevalsepa = "="
    	x = Split(p, namevalsepa)
    	If (UBound(x) = 1) Then
    		If Trim(LCase(x(0))) = fgnname Then
            		ExtractGrantNumber = x(1)
        		Else
            		ExtractGrantNumber = " "
        		End If
    	Else
            	ExtractGrantNumber = " "
    	End If
End Function
'==========================================
Function ExtractcPIID(p)
	'   best try to ditermin whether fgn is an NIH Grant Number
	'   using a specified Structure
    	Dim scpiid 'As String
    	Dim namevalsepa 'As String
    	scpiid = "cpiid"
    	namevalsepa = "="
    	x = Split(p, namevalsepa)
    	If (UBound(x) = 1) Then
    		If Trim(LCase(x(0))) = scpiid Then
            			ExtractcPIID = x(1)
        		Else
            		    ExtractcPIID = " "
        		End If
    	Else
            		ExtractcPIID = " "
    	End If
End Function
'==========================================
Function Extractvalue(p,name)
	Dim catname 'As String
	Dim namevalsepa 'As String
       
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = name Then
			Extractvalue = x(1)
			'''''MsgBox "Extractval="&Extractvalue
		Else
			Extractvalue = " "
		End If
    	Else
            		Extractvalue = " "
    	End If
End Function
'============================================
Function CheckIFreply(abc,v_SenderID,oConn)
	Dim oRsU
	Set oRsU = CreateObject("ADODB.Recordset")
	strSQLTextU = "update dbo.adsup_Notification_email_status set reply_recieved_date=GETDATE() where Notification_id="&abc&" and email_address LIKE '"&v_SenderID&"%'"
	'strSQLTextU="EXEC DBO.adsupp_UpdateReply "&abc&","&v_SenderID
	''MsgBox  strSQLTextU
	set oRSU =  oConn.execute(strSQLTextU)				
	CheckIFreply="DONE"

	set oRsU=Nothing
End Function

'============================================
Function getTempApplid(str,oConn)
	Dim oRsU
	Set oRsU = CreateObject("ADODB.Recordset")
		
	strSQLTextU = "select appl_id from adsup_notification where id="&str
	''''''MsgBox  strSQLTextU

	set oRSU =  oConn.execute(strSQLTextU)				
		
	If IsNull(oRsU.Fields("appl_id").value) then
		getTempApplid=""
	else
		getTempApplid=oRsU.Fields("appl_id").value
	end if
			
	set oRsU=Nothing

End Function


'==========================================
Function getpa(str,oConn)
	Dim oRsU
	Set oRsU = CreateObject("ADODB.Recordset")
		
	strSQLTextU = "select dbo.fn_PA_match( ' "& str &" ') as pa"
	''''''MsgBox  strSQLTextU

	set oRSU =  oConn.execute(strSQLTextU)				
		
	If IsNull(oRsU.Fields("pa").value) then
		getpa=""
	else
		getpa=oRsU.Fields("pa").value
	end if
			
	set oRsU=Nothing

End Function
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
Function GetAlias(CItem)
	V_saStr = CItem.SenderEmailAddress
	WHILE InStr(V_saStr, "=") > 0
	
			pos = InStr(V_saStr, "=")
			V_saStr = Mid(V_saStr, pos + 1, Len(V_saStr))
	WEND
	IF Len(V_saStr) > 0 THEN
		GetAlias = V_saStr
	ELSE
		GetAlias =""
	END IF
	End Function

'==========================================

'==========================================
Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="Add-Suppl-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
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
'==========================================
Function RaiseErrortoAdmin(CItem,eRRMsg1,eRRMsg2)
	Set OutMail = CItem.Forward
	With OutMail
		.Recipients.Add("woldezf@mail.nih.gov")
		.Recipients.Add("omairi@mail.nih.gov")
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
		.To="omairi@mail.nih.gov;qians@mail.nih.gov;woldezf@mail.nih.gov"			
		'.CC="omairi@mail.nih.gov"	
		.Subject = SubjMSG
		.BodyFormat = 2
		.HTMLBody = " " & BodyMSG
		.Send

	End With
	Set Mitem=nothing	
	emailme = true

End Function
'==========================================
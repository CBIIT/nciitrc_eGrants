
'wscript.echo "Exchange_latest script is going to run!! "
 
	startTimeStamp=Now	
	logDir="C:\egrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFslog
	set objFslog=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)	
	
	Dim OtlkApps
	Dim objNS
	Dim oConn
	Dim ItemsProcessed
	''conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-D201-V.NCI.NIH.GOV\MSSQLEGRANTSD,52000;Application Name=egrants"
	conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"
	
	

  'Dim objWMIService, colProcessList
  'Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
  'Set colProcessList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'Outlook.exe'")  
 'IF(colProcessList.count>0) Then
  'For Each objProcess in colProcessList 
    'objProcess.Terminate() 
  'Next  
 'End If 
	  
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")	
	Set oConn = CreateObject("ADODB.Connection")
	
	'dirpath="Public Folders - NCIOGAeGrantsDev@mail.nih.gov\All Public Folders\NCI\GAB\eGrantsDev\test\"
	dirpath="Public Folders - NCIOGAeGrantsProd@mail.nih.gov\All Public Folders\NCI\GAB\efile\"
	OutDir = "C:\eGrants\watch\out\"
		
	Call Process(dirpath,oConn)	
	
	'set colProcessList=Nothing
	'set objWMIService=Nothing
	set oConn=Nothing
	Set objNS = Nothing
	Set OtlkApps = Nothing		
	
	taskEndMssg="******* Task Completed! *******" & ItemsProcessed & " Mail Items Have Been Processed"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	
	
'==========================================
Sub Process(dirpath,oConn)
	
	Dim CFolder	
	Dim Documentid	
	Dim objSender
	Dim objExchUser	

	'OutDir = "E:\egrants\watch\IN\Email\Test\"
	'OutDir = "E:\egrants\watch\IN\Email\Prod\"
	'OutDir = "E:\egrants\watch\out\efiletest\"
	'OutDir = "C:\eGrants\watch\out\"


	'Set OtlkApps = CreateObject("Outlook.application")
	On Error Resume Next
	set CFolder=getCurrentFolder(dirpath)	
	ItemsProcessed = 0
	totcnt = CFolder.Items.Count
	'wscript.echo "Total number of mail items to be processed are " &  totcnt
	ItemtoProcess=totcnt
	oConn.Open conStr
	
	'------MAIN LOOP TO PROCESS ALL MAIL	
	
	DO WHILE ItemtoProcess > 0

		fgn = " "
		category = " "
		documentid=""
		applid=""
		docid=""
		profileid=1
		id= " "
		docdt= " "
		cKeytxt = " "
		movetoqc="no"
		subcat=" "
		extract=" "

		Set CItem = CFolder.Items(ItemtoProcess)
		v_SubLine = CItem.Subject

		Set CAttachments = CItem.Attachments

		'---------/* Grep Userid from EX email address */
		'----------This prog will fetch emails Aliases only not the faxes so qc required will not always be yes rather it 
		'----------will eb deterministic based on filetype appl and docid provided in subject line
    		v_SenderID = ""
    		IF CItem.SenderEmailType = "EX" Then			

			  set objSender = CItem.Sender
			  If Not (objSender Is Nothing) Then
			    set objExchUser = CItem.Sender.GetExchangeUser()
			    If Not (objExchUser Is Nothing) Then			      
			       v_SenderID = objExchUser.Alias
			    End If
			  End If

			IF Len(v_SenderID) = 0 Then
			    v_SenderID = GetAlias(CItem)
			END IF
		
    		ElseIf CItem.SenderEmailType = "SMTP" Then
       			v_SenderID = CItem.SenderEmailAddress
    		End If
		'''''MsgBox "Sender email address = "&v_SenderID
		
		beginpos=1
		substr=v_SubLine
		

       		WHILE  InStr(substr, ",") > 0
				pos = InStr(substr, ",")
				V_saStr = Mid(substr, 1, pos -1)    
				IF InStr(LCase(V_saStr),"grantnumber") > 0 Then
					fgn=Mid(V_saStr,InStr(V_saStr,"grantnumber"),pos-1)
					fgn=Extractvalue(fgn,"grantnumber")
				ELSEIF InStr(LCase(V_saStr),"category") > 0 Then
					category=Mid(V_saStr,InStr(V_saStr,"category"),pos-1)
					category=Extractvalue(category,"category")
				ELSEIF InStr(LCase(V_saStr),"applid") > 0 Then
					applid=Mid(V_saStr,InStr(V_saStr,"applid"),pos-1)
					applid=Extractvalue(applid,"applid")	
				ELSEIF InStr(LCase(V_saStr),"documentdate") > 0 Then
					docdt=Mid(V_saStr,InStr(V_saStr,"documentdate"),pos-1)
					docdt=Extractvalue(docdt,"documentdate")						
				ELSEIF InStr(LCase(V_saStr),"documentid") > 0 Then
					documentid=Mid(V_saStr,InStr(V_saStr,"documentid"),pos-1)
					documentid=Extractvalue(documentid,"documentid")	
				ELSEIF InStr(LCase(V_saStr),"sub=") > 0 Then
					subcat=Mid(V_saStr,InStr(V_saStr,"sub"),pos-1)
					subcat=Extractvalue(subcat,"sub")			
				ELSEIF InStr(LCase(V_saStr),"extract") > 0 Then
					''extract= 1:body only 2:attachment only 3:extract both
					extract=Mid(V_saStr,InStr(V_saStr,"extract"),pos-1)
					extract=Extractvalue(extract,"extract")	
				End If
				beginpos=pos+1
				substr=Mid(substr, beginpos, Len(substr))   
        	WEND	
			
			V_saStr=substr
			pos = Len(V_saStr)
			IF InStr(LCase(V_saStr),"grantnumber") > 0 Then
				fgn=Mid(V_saStr,InStr(V_saStr,"grantnumber"),pos)
				fgn=Extractvalue(fgn,"grantnumber")
			ELSEIF InStr(LCase(V_saStr),"category") > 0 Then
				category=Mid(V_saStr,InStr(V_saStr,"category"),pos)
				category=Extractvalue(category,"category")
			ELSEIF InStr(LCase(V_saStr),"applid") > 0 Then
				applid=Mid(V_saStr,InStr(V_saStr,"applid"),pos)
				applid=Extractvalue(applid,"applid")						
			ELSEIF InStr(LCase(V_saStr),"documentdate") > 0 Then
				'''MsgBox "Before mid="&V_saStr
				docdt=Mid(V_saStr,InStr(V_saStr,"documentdate"),pos)
				'''MsgBox "before Extractva l " & docdt
				docdt=Extractvalue(docdt,"documentdate")	
				'''MsgBox "Docdt="&docdt					
			ELSEIF InStr(LCase(V_saStr),"documentid") > 0 Then
				documentid=Mid(V_saStr,InStr(V_saStr,"documentid"),pos)
				documentid=Extractvalue(documentid,"documentid")	
			ELSEIF InStr(LCase(V_saStr),"extract") > 0 Then
				''extract= 1:body only 2:attachment only 3:extract both
				extract=Mid(V_saStr,InStr(V_saStr,"extract"),pos)
				extract=Extractvalue(extract,"extract")	
			End If


			IF  len(Trim(applid)) = 0 and len(Trim(fgn))<>0  THEN
				applid=getApplid(removespcharacters(fgn),oConn)
			ELSEIF  len(Trim(applid)) = 0 and len(Trim(fgn))=0  THEN
				applid=getApplid(removespcharacters(v_SubLine),oConn)
				IF (len(applid)=0) THEN	
					applid=getApplid(removespcharacters(CItem.Body),oConn)
				END IF					
    		END IF

			IF Len(Trim(docdt)) =0  Then 
				docdt=CDate(CItem.ReceivedTime)
				docdt=Mid(docdt,1, InStr(docdt , " ")-1)
			end If

			'''''===  LOAD NCIOGAPROGESS INTO CORRESPONDENCE, decide category, decide to extract 1, 2 or 3
			IF Len(Trim(category)) =0  AND Trim(v_SenderID)="NCIOGAPROGESS" Then 					   
				category="Correspondence" 
				extract="1"
				movetoqc="no"
			end If


			IF extract="1" THEN    '''===Extract body only
				IF (len(applid)=0) THEN	movetoqc="yes" ELSE movetoqc="no" END IF
				If(category="PublicAccess") Then
					V_flType="pdf"
					Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)					
					IF Documentid = "" Then
						RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "")
					ELSE
						Call saveMailAsPdf(citem, Documentid)
					End If					
				Else
					V_flType="txt"				
					Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
					IF Documentid = "" Then
						RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "")
					ELSE
						Alias = Documentid & ".txt"
						''''============CItem.SaveAs OutDir & Alias, olTXT	
						set outfile = objFslog.OpenTextFile(OutDir & Alias, 8, True)
						outfile.WriteLine("From: " & CItem.SenderName)
						outfile.WriteLine("Sent: " & CItem.ReceivedTime)
						outfile.WriteLine("To: " & CItem.To)                                                                        
						outfile.WriteLine("Subject: " & CItem.Subject)					                                                                      
						If(CAttachments.Count>0) Then
							attchlist=""                                                                         
							For Each attch In CAttachments
								attchlist = attchlist & ", " & attch.FileName 
							Next
						End IF
						outfile.WriteLine("Attachments: " & attchlist )                                                                    
						outfile.WriteLine(CItem.Body)
						outfile.close
					END IF	
				End If					
										
			ELSEIF extract="2" THEN    '''===  EXTRACT ATTACHMENT  only
				IF (len(applid)=0) THEN	movetoqc="yes" ELSE movetoqc="no" END IF		
				
				IF CAttachments.Count > 0 THEN
				i = 1
				cKeytxt=""
				Do While i <= CAttachments.Count
					CName = removejunk(CAttachments(i).FileName)
					V_flType = getFileType(CName)
					movetoqc = IsQCRequired(V_flType)
				       	'/* DO NOT EXTRACT ATTCHMENTS NAME STARTS WITH ATT
						'''wscript.echo documentid & "-" & category & "-" & applid & "-" & profileid & "-" & docdt  & "-" & v_SenderID & "-" & V_flType & "-" & movetoqc & "-" & subcat
					IF (Left(CName, 3) <> "ATT") THEN						
						Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
						
						IF Documentid = "" Then
								RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=2", "")
						ELSE
								Alias = Documentid & "." &V_flType
								'msgbox(OutDir)
								'msgbox(Alias)
								CAttachments(i).SaveAsFile (OutDir & Alias)							
								Documentid = ""					
						END IF													
				   	END IF '/*If (Left(CName, 3) = "ATT") Then
					i = i + 1
				LOOP
				END IF  ''CAttachments.Count > 0  						
			ELSEIF extract="3" THEN    '''====  EXTRACT BODY AND ATTACHMENT
				IF (len(applid)=0) THEN	movetoqc="yes" ELSE movetoqc="no" END IF
				If(category="PublicAccess") Then
					V_flType="pdf"
					Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
					IF Documentid = "" Then
						RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=1", "")
					ELSE
					Call saveMailAsPdf(citem, Documentid)
					End If					
				Else
				V_flType="txt"
				'''===Extract body only
				Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
				IF Documentid = "" Then
						RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=3", "")
				ELSE
					Alias = Documentid & ".txt"
					'''''==============CItem.SaveAs OutDir & Alias, olTXT						
					set outfile = objFslog.OpenTextFile(OutDir & Alias, 8, True)
					outfile.WriteLine("From: " & CItem.SenderName)
					outfile.WriteLine("Sent: " & CItem.ReceivedTime)
					outfile.WriteLine("To: " & CItem.To)                                                                        
					outfile.WriteLine("Subject: " & CItem.Subject)					                                                                      
					If(CAttachments.Count>0) Then
						attchlist=""                                                                         
						For Each attch In CAttachments
							attchlist = attchlist & ", " & attch.FileName 
						Next
					End IF
					outfile.WriteLine("Attachments: " & attchlist )                                                                    
					outfile.WriteLine(CItem.Body)
					outfile.close				
					Documentid = ""					
				END IF																	

				'''''====Extract attachment
				IF CAttachments.Count > 0 THEN
				i = 1
				cKeytxt=""
				Do While i <= CAttachments.Count
					CName = removejunk(CAttachments(i).FileName)
					V_flType = getFileType(CName)
					movetoqc = IsQCRequired(V_flType)
				       	'/* DO NOT EXTRACT ATTCHMENTS NAME STARTS WITH ATT
					IF (Left(CName, 3) <> "ATT") THEN						
						Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)		
						IF Documentid = "" Then
								RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found, PROD Exchange_Latest vbs /extract=3", "")
						ELSE
							Alias = Documentid & "." &V_flType
							CAttachments(i).SaveAsFile (OutDir & Alias)			
							Documentid = ""
						END IF																							
				    END IF '/*If (Left(CName, 3) = "ATT") Then
					i = i + 1
				LOOP
				END IF  ''CAttachments.Count > 0 
				End If
			END IF		''''else if extract
			
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
				Errmsg1="Error Occured! PROD Exchange_Latest vbs"
				Errmsg2=errorMssg
				Call RaiseErrortoAdmin(CItem,Errmsg1,Errmsg2)
				if((Err.Number=-2147221223) Or (Err.Source= "Microsoft Outlook")) Then
						strServiceName = "ClickToRunSvc"	
						Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
						
						''Close outlook
							Set colProcessList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'Outlook.exe'")  
							IF(colProcessList.count>0) Then
							  For Each objProcess in colProcessList 
								objProcess.Terminate() 
							  Next  
							End If
						''Stop ClickToRunSvc services
							Set colListOfServices = objWMIService.ExecQuery("Select * from Win32_Service Where Name ='" & strServiceName & "'")
							For Each objService in colListOfServices
							objService.StopService()
							Next
						''Start ClickToRunSvc services	
							For Each objService in colListOfServices
							objService.StartService()
							Next
						''Retry moving e-mail item
							Call MovetoOldFolder(CFolder,CItem)	
						''Start outlook
							Set oShell = WScript.CreateObject("WScript.Shell")
							oShell.Run "outlook" 
							
				
				Set colProcessList=Nothing
				Set colListOfServices=Nothing
				set oShell=Nothing 
				Set objWMIService=Nothing
				
				End If
				Err.Clear
				Exit DO
			End If
						
			
			ItemsProcessed=ItemsProcessed + 1
			
			If ItemsProcessed>=30 Then
				Errmsg1="Warning! PROD Exchange_Latest vbs has processed 30 mail items in one instance!"			
				Errmsg2="Hello Admin, 30 items have been processed in one instance and the application is now exiting. Please check whether there is duplicate items processing."
				Call emailme(Errmsg1,Errmsg2)
				Exit DO	
			End If		
	
	ItemtoProcess = CFolder.Items.Count
	LOOP		
	
	oConn.close
	set oConn=Nothing	
'wscript.echo ItemProcessed & " of " & totcnt & " items has been processed"
End Sub

'==========================================
'==========================================
Function RaiseErrortoAdmin(CItem,eRRMsg1,eRRMsg2)
	Set OutMail = CItem.Forward
	With OutMail
		.Recipients.Add("ayehualem.anteneh@nih.gov")
		.Recipients.Add("leul.ayana@nih.gov")
		'.Recipients.Add("omairi@mail.nih.gov")
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
		.To="leul.ayana@nih.gov;guillermo.choy-leon@nih.gov"			
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
'==========================================
Function IsQCRequired(Fl_type)
	IF LEN(Fl_type) > 0  THEN
		IF Fl_type="pdf" OR  Fl_type="txt" OR  Fl_type="doc" OR  Fl_type="xls" OR  Fl_type="docx" OR  Fl_type="xlsx" OR  Fl_type="ppt"  THEN
			IsQCRequired = "no"
		ELSE
			IsQCRequired ="yes"
		END IF
	ELSE
		IsQCRequired ="yes"
	END IF
End Function
'==========================================
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

Function removejunk(flnm)
            flnm = Replace(flnm, ":", " ")
            flnm = Replace(flnm, "/", " ")
            flnm = Replace(flnm, "\", " ")
            flnm = Replace(flnm, "&", "and")
            flnm = Replace(flnm, ";", " ")
            removejunk = LTrim(RTrim(flnm))
End Function
'==========================================
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

Function MovetoOldFolder(CFolder,CItem)
	set oldfolder = CFolder.Folders("old")
	CItem.Move(oldfolder)
	MovetoOldFolder="Done"
End Function

'==========================================
'==========================================
Function Extractvalue(p,name)
	Dim catname 'As String
	Dim namevalsepa 'As String
       
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = name Then
			Extractvalue = x(1)
			''''MsgBox "Extractval="&Extractvalue
		Else
			Extractvalue = " "
		End If
    	Else
            		Extractvalue = " "
    	End If
End Function
'==========================================
'==========================================
Function getApplid(str,oConn)
		
	Dim oRS1	
	strSQLTextU = "select dbo.Imm_fn_applid_match( ' "& str &" ') as applid"
	
	set oRS1 =  oConn.execute(strSQLTextU)				
		
	If IsNull(oRS1.Fields("applid").value) then
		getApplid=""
	else
		getApplid=oRS1.Fields("applid").value
	end if
			
	set oRS1=Nothing

End Function
'==========================================
'==========================================
Function getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
	
	Dim oRS2
	strSQL = "exec SP_CREATE_EGRANTS_DOCUMENT_NEW '"&documentid&"','"&category&"','"&applid&"','"&profileid&"','"&docdt&"','"&v_SenderID&"','"&V_flType&"','"&movetoqc&"','"&subcat&"'"
	set oRS2 =  oConn.execute(strSQL , , adCmdTable)
		If (oRS2.BOF = True and oRS2.EOF = True) Then
			getDocumentId=""
		ElseIf (oRS2.Fields("name").value="Success") or (oRS2.Fields("name").value="Advisory") Then					
			getDocumentId=oRS2.Fields("value").value		
		End If
		set oRS2=Nothing
End Function
'==========================================
'==========================================
Function getCurrentFolder(dirpath)
	Dim sepchar
	Dim curFolder
	sepchar = "\"	
	'On Error Resume Next 
	If dirpath <> "" Then
		xArray = Split(dirpath, sepchar)
		i = 1
		Set curFolder = objNS.Folders(xArray(0))

		Do While i < UBound(xArray)
			Set curFolder = curFolder.Folders(xArray(i))
			i = i + 1
		Loop
		set getCurrentFolder=curFolder
	End If  'If dirpath <> "" Then		
	
	End Function
'==========================================
'==========================================
Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="eXchange_Latest_Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
		set logOutput=objFslog.OpenTextFile(logDir & flname,  code, True)
	
	If(errorInfo="") Then 
		logRecord= timeStamp & "  -" & vbTab & message 
	Else		
		logRecord= timeStamp & "  -" & vbTab & message & vbNewLine & vbTab & vbTab & "      -> " & errorInfo	
	End If
	
	logOutput.WriteLine(logRecord)
	logOutput.Close
			
End Function
'==========================================

Function saveMailAsPdf(citem, Documentid)
	set objFs=CreateObject("Scripting.FileSystemObject")
	pdfdir = "C:\eGrants\publicaccess\"
	pdfbackup = "C:\eGrants\publicaccess\backup\"
	bodypdf = "C:\eGrants\publicaccess\body.pdf"
	subjtxt = "C:\eGrants\publicaccess\subj.txt"
	subjpdf = "C:\eGrants\publicaccess\subj.pdf"
	pdfdoc = "C:\eGrants\publicaccess\" & Documentid & ".pdf"	
	Set pdfFld=objFs.getFolder(pdfdir)	
	If(pdfFld.Files.Count>0) Then
	set old_files=pdfFld.Files
	For each old_file in old_files
	 objFs.DeleteFile pdfdir & old_file.name 
	 Next
	End If
	Set app = CreateObject("AcroExch.App") 
	'Set objAVDoc = CreateObject("AcroExch.AVDoc")
	Set objInspector = citem.GetInspector
    Set objDoc = objInspector.WordEditor
	objDoc.ExportAsFixedFormat bodypdf ,17
	
	set outfile = objFs.OpenTextFile(subjtxt, 8, True)
		outfile.WriteLine("From: " & CItem.SenderName)
		outfile.WriteLine("Sent: " & CItem.ReceivedTime)
		outfile.WriteLine("To: " & CItem.To)                                                                        
		outfile.WriteLine("Subject: " & CItem.Subject)	
		set CAttachments=CItem.Attachments					
		If(CAttachments.Count>0) Then
			attchlist=""  
			'i=1
			For Each attch In CAttachments
				
				CName = removejunk(attch.FileName)		
							       	
				IF (Left(CName, 3) <> "ATT") THEN				
					If(attchlist="") Then
					  attchlist=attch.FileName
					Else
					  attchlist = attchlist & ", " & attch.FileName
					End If
					attch.SaveAsFile (pdfdir & attch.FileName)	
				End If
			Next
		End IF
		outfile.WriteLine("Attachments: " & attchlist )                                                                    
		'outfile.WriteLine(CItem.Body)
		outfile.close
		 Set subjobj = CreateObject("AcroExch.AVDoc")
			 subjobj.open subjtxt, ""
			 Set subjpdfobj = subjobj.GetPDDoc
			 subjpdfobj.Save 1, subjpdf
			 subjpdfobj.Close
			 subjobj.Close -1
			 objFs.DeleteFile subjtxt
			 'objFs.MoveFile subjtxt, backdir & "subj.txt"
			 
			 Set basepdf = CreateObject("AcroExch.PDDoc") 
			 Set insrtpdf = CreateObject("AcroExch.PDDoc") 
			 
			 basepdf.Open  subjpdf
			 insrtpdf.Open  bodypdf
			 pages= insrtpdf.GetNumPages()	
			 basepdf.InsertPages 0, insrtpdf, 0, pages, 1        
			 basepdf.Save 1, pdfdoc
			 basepdf.Close
			 insrtpdf.Close
			 objFs.DeleteFile subjpdf
			 'objFs.MoveFile subjpdf, backdir & "subj.pdf"
			 objFs.DeleteFile bodypdf
			 'objFs.MoveFile bodypdf, backdir & "body.doc"
	set objFs=Nothing
	app.CloseAllDocs
    app.Exit 	
	call converttopdf(pdfdir, pdfdoc)
		
End Function
'==========================================
'==========================================

Function converttopdf(pdfdir, pdfdoc)
	set objFs=CreateObject("Scripting.FileSystemObject")
	Set app = CreateObject("AcroExch.App") 
	Set srcFld=objFs.getFolder(pdfdir)	
	If(srcFld.Files.Count>0) Then
		Set fileOBJ = CreateObject("AcroExch.AVDoc")
		For Each objfile In srcFld.Files
			IF (LCase(objFS.GetExtensionName(objfile))<>"pdf")  Then
				fileOBJ.Open objfile, ""
				set filepdf=fileOBJ.GetPDDoc
				filepdf.Save 1,pdfdir & objfile.name & ".pdf"
				filepdf.Close
				fileOBJ.Close -1
				objFs.DeleteFile objfile
				'objFs.MoveFile pdfdir & objfile.name, pdfdir & "backup\" & objfile.name	
			End If
		Next	
	
		If(objFs.FileExists(pdfdoc)) Then	
			'wscript.echo srcFld.Files.count
			j=1
			For Each file In srcFld.Files
			
				If(pdfdir & file.name<>pdfdoc) Then			
					Set basepdf = CreateObject("AcroExch.PDDoc") 
					Set insrtpdf = CreateObject("AcroExch.PDDoc") 
					'wscript.echo file.name		
					'wscript.echo file.name			 
					 basepdf.Open pdfdoc
					 insrtpdf.Open file
					 
					 lastpg= basepdf.GetNumPages()-1	
					 'wscript.echo lastpg
					 
					 pages= insrtpdf.GetNumPages()	
					 basepdf.InsertPages lastpg, insrtpdf, 0, pages, 1	
					 'oldmailpdf=mailpdf
					 'mailpdf=basedir & "PDF\mail" & j & ".pdf"
					 basepdf.Save 1, pdfdoc
					 basepdf.Close
					 'wscript.echo file.name
					insrtpdf.Close
					 'objFs.DeleteFile oldmailpdf			 
					 objFs.DeleteFile file
					 lastpg=0		 
					 
					 j=j+1
					 'objFs.MoveFile basedir & objfile.FileName, basedir & "backup\" & objfile.FileName	 
				End If
			Next
		End If 
		objFs.MoveFile pdfdoc,OutDir
	End If
	set objFs=Nothing
	app.CloseAllDocs
    app.Exit
	
End Function
'==========================================
'==========================================



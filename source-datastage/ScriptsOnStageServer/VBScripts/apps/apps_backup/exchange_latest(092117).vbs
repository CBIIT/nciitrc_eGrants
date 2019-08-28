
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

	conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=egrants"
	'''''''conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=ncidb-d131-v\egrants_dev,52300;Application Name=egrants"
	
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")
	Set oConn = CreateObject("ADODB.Connection")
	
	'dirpath="Public Folders - NCIOGAeGrantsDev@mail.nih.gov\All Public Folders\NCI\GAB\eGrantsDev\test\"
	dirpath="Public Folders - NCIOGAeGrantsProd@mail.nih.gov\All Public Folders\NCI\GAB\efile\"
		

	Call Process(dirpath,oConn)	
	
	set oConn=Nothing
	Set objNS = Nothing
	Set OtlkApps = Nothing		
	
	taskEndMssg="Task Completed!"
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
	OutDir = "C:\eGrants\watch\out\"


	'Set OtlkApps = CreateObject("Outlook.application")
	On Error Resume Next 
	set CFolder=getCurrentFolder(dirpath)	
	ItemProcessed = 0
	totcnt = CFolder.Items.Count
	'wscript.echo "Total number of mail items to be processed are " &  totcnt
	ItemtoProcess=totcnt
	oConn.Open conStr
	
	'------MAIN LOOP TO PROCESS ALL MAIL	
	
	WHILE ItemtoProcess > 0

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
				ELSEIF InStr(LCase(V_saStr),"sub") > 0 Then
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
				V_flType="txt"				
				Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
				IF Documentid = "" Then
					RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found vbs/extract=1")
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
					IF (Left(CName, 3) <> "ATT") THEN						
						Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
						IF Documentid = "" Then
								RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found vbs/extract=1")
						ELSE
								Alias = Documentid & "." &V_flType
								CAttachments(i).SaveAsFile (OutDir & Alias)							
								Documentid = ""					
						END IF													
				   	END IF '/*If (Left(CName, 3) = "ATT") Then
					i = i + 1
				LOOP
				END IF  ''CAttachments.Count > 0  						
			ELSEIF extract="3" THEN    '''====  EXTRACT BODY AND ATTACHMENT
				IF (len(applid)=0) THEN	movetoqc="yes" ELSE movetoqc="no" END IF
				
				V_flType="txt"
				'''===Extract body only
				Documentid=getDocumentId(documentid,category,applid,profileid,docdt,v_SenderID,V_flType,movetoqc,subcat)
				IF Documentid = "" Then
						RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found vbs/extract=1")
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
								RetVal = RaiseErrortoAdmin(CItem,"DB Error: Document_id NOT Found vbs/extract=1")
						ELSE
							Alias = Documentid & "." &V_flType
							CAttachments(i).SaveAsFile (OutDir & Alias)			
							Documentid = ""
						END IF																							
				    END IF '/*If (Left(CName, 3) = "ATT") Then
					i = i + 1
				LOOP
				END IF  ''CAttachments.Count > 0  						
			END IF		''''else if extract
	ItemProcessed=ItemProcessed + 1		
	
	processTimeStamp=Now
	If Err.Number <> 0 Then
		logMssg= "Error Occured! => EmailSender:" & v_SenderID & "; " & "Subjectline :" & v_SubLine & "; "& " Recieved Date: " & CItem.ReceivedTime
		errorMssg= "Error Number: " & Err.Number & "," & "Error Description: " & Err.Description & ", " & "Error Source: " & Err.Source
	Else		
		logMssg= "Processed! => EmailSender:" & v_SenderID & "; " & "Subjectline :" & v_SubLine & "; " & " Recieved Date: " & CItem.ReceivedTime
		errorMssg=""
		
	End If
	Err.Clear
	
	Call writeLog(forAppending, logMssg, errorMssg, processTimeStamp)						
	Call MovetoOldFolder(CFolder,CItem)	
	ItemtoProcess = CFolder.Items.Count
	WEND		
	
	oConn.close
	set oConn=Nothing	
'wscript.echo ItemProcessed & " of " & totcnt & " items has been processed"
End Sub

'==========================================
'==========================================
Function RaiseErrortoAdmin(CItem,eRRMsg)
	Set OutMail = CItem.Forward
	With OutMail
		.Recipients.Add("woldezf@mail.nih.gov")
		.Subject = eRRMsg & "=> " & CItem.Subject 
		.Send
	End With					
	Set OutMail=nothing	
	RaiseErrortoAdmin ="Done"
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


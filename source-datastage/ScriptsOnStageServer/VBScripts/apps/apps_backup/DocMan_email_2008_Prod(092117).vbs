'**********************************************************************
'DocMan_email_2008_Prod

'************************************************************************
'wscript.echo "DocMan script is going to run!! "

	startTimeStamp=Now	
	logDir="C:\egrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)	
	Dim ItemsProcessed
	Dim dirpath
	Dim oConn, oRS, cmd
	
	conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=DOCMAN;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=docman"
	'conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=DOCMAN;Data Source=ncidb-d131-v\egrants_dev,52300;Application Name=docman"
	
	
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")
	Set oConn = CreateObject("ADODB.Connection")
	oConn.Open conStr	
	
	dirpath="Public Folders - NCIOGAeGrantsProd@mail.nih.gov\All Public Folders\NCI\GAB\eContracts\"	


	Call Process()
	
	oConn.close
	set oRS = Nothing	
	set oConn=Nothing
	
	taskEndMssg=  "******* Task Completed! *******" & ItemsProcessed & " Mail Items Have Been Processed"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	
	
	Set objFS=Nothing

'==========================================
Sub Process()

	Dim Contact 
	Dim Sender
	Dim cmd
	Dim objNS 'As Outlook.NameSpace
	Dim OutMail 'As Object
	Dim CFolder
	Const adCmdStoredProc = 4	
	Dim Documentid
	Dim totcnt
	
	
	On Error Resume Next
	
	Set cmd = CreateObject("ADODB.Command")     
	set cmd.ActiveConnection = oConn   

	Const ForReading = 1, ForWriting = 2, ForAppending = 8
	Set filesys = CreateObject("Scripting.FileSystemObject")

	'ItemProcesslimit = 12 'Do not create more than 20 elements in a single xmlfile

	
	OutDir = "C:\egrants\watch\out\docman\prod\"

	Set CFolder=getCurrFolder()	
	'wscript.echo "Current folder is :" & CFolder
	Set OldFldr = CFolder.Folders("old")	
	ItemsProcessed = 0
	totcnt = CFolder.Items.Count	
	'wscript.echo "Total Count :" & totcnt
	ItemtoProcess=totcnt

	WHILE ItemtoProcess > 0		

			cpiid = " "
			category = " "
			 id= " "
			ddt= " "
			cKeytxt = " "
			v_SenderID = ""
			
			Set CItem = CFolder.Items(ItemtoProcess)
			v_SubLine = CItem.Subject			
			v_SenderID=getSenderID(CItem)		
			
    		cpiid = ExtractcPIID(ExtractElement(v_SubLine, 1))
				
			If (cpiid =" ") then
					docid = Extractdocid(ExtractElement(v_SubLine, 1))
					If ( docid <> " ") then
						ddt = Extractddt(ExtractElement(v_SubLine, 2))
						reason = Extractreason(ExtractElement(v_SubLine, 3))
						cmd.CommandText = "SP_CREATE_DOCMAN_DOCUMENT_NEW"
						cmd.CommandType = 4
						'cmd.CommandType = adCmdText	
						
						cmd.Parameters.Refresh
						cmd.Parameters("@CP").Value=NULL
						cmd.Parameters("@CAT").Value=NULL
						cmd.Parameters("@SEQ").Value=NULL
						cmd.Parameters("@DD").Value=ddt
						cmd.Parameters("@UID").Value=v_SenderID
						cmd.Parameters("@FT").Value="pdf"
						cmd.Parameters("@ACTIONID").Value="2"
						cmd.Parameters("@DOCID").Value=docid
						cmd.Parameters("@REASON").Value=reason
					End If
			Else
     				      catid = ExtractCatid(ExtractElement(v_SubLine, 2))
        				      num = Extractnum(ExtractElement(v_SubLine, 3))
				      ddt = Extractddt(ExtractElement(v_SubLine, 4))

						cmd.CommandText = "SP_CREATE_DOCMAN_DOCUMENT_NEW"
						cmd.CommandType = 4

						'cmd.CommandType = adCmdText	
						cmd.Parameters.Refresh

						cmd.Parameters("@CP").Value=cpiid
						cmd.Parameters("@CAT").Value=catid
						cmd.Parameters("@SEQ").Value=num
						cmd.Parameters("@DD").Value=ddt
						cmd.Parameters("@UID").Value=v_SenderID
						cmd.Parameters("@FT").Value="pdf"
						cmd.Parameters("@ACTIONID").Value="1"
						cmd.Parameters("@DOCID").Value=NULL
						cmd.Parameters("@REASON").Value=NULL

			End If
			
			If CItem.Attachments.Count > 0 then
				Set oRS = cmd.Execute

				If (oRS.BOF = True and oRS.EOF = True) Then
					set oRS = Nothing
					'MsgBox "No data found"
				Else
					'MsgBox oRS(0) & " and " & oRS(1)
					If oRS(0)="Success" OR oRS(0)="Duplicate"  Then
						Documentid=oRS(1)
						Set CAttachments = CItem.Attachments

							i = 1
							cKeytxt=""
							Do While i <= CAttachments.Count
								CName = removejunk(CAttachments(i).FileName)
					       			'/* DO NOT EXTRACT ATTCHMENTS NAME STARTS WITH ATT
								IF (Left(CName, 3) <> "ATT") THEN
									cKeytxt = cKeytxt & " " & CAttachments(i).FileName
            									V_flType = getFileType(CName)
            									Alias ="c" & Documentid & ".pdf"									
									CAttachments(i).SaveAsFile (OutDir &Alias )									
					    		END IF '/*If (Left(CName, 3) = "ATT") Then
								i = i + 1
    						LOOP
						
						set oRS = Nothing
					Else If oRS(0)="Error" Then
						'Set OutMail = CItem.ReplyAll	'OtlkApps.CreateItem(0)
						Set OutMail = CItem.Reply
						 replyText=oRS(0) & "->" & oRS(1) &" : " & v_SubLine					
   						 With OutMail
       							 .Subject = replyText
       							 .Body = replyText & vbNewLine  & vbNewLine & CItem.body
       							 .Send 
							
   						 End With
						 Set OutMail = Nothing         
								     
							
						End If
					End If
				End If			
				
			End If
			
			
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
			ItemsProcessed=ItemsProcessed + 1
	
			ItemtoProcess = CFolder.Items.Count
	WEND
				
	Set objNS = Nothing
	Set OtlkApps = Nothing
	Set OutMail = Nothing	
End Sub

'==========================================
Function Extractddt(p)
	Dim ddt 'As String
	Dim namevalsepa 'As String
       
	ddt = "date"
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = ddt Then
			Extractddt = x(1)
		Else
			Extractddt = " "
		End If
    	Else
            		Extractddt = " "
    	End If
End Function


'==========================================
Function Extractnum(p)
	Dim catid 'As String
	Dim namevalsepa 'As String
       
	catid = "num"
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = catid Then
			Extractnum = x(1)
		Else
			Extractnum = " "
		End If
    	Else
            		Extractnum = " "
    	End If
End Function
'==========================================

Function ExtractElement(str, n)

	'   Returns the nth element from a string,
	'   using a specified separator character
	Dim x 'As Variant
	x = Split(str, ",")
	If n > 0 And n - 1 <= UBound(x) Then
		ExtractElement = x(n - 1)
	Else
		ExtractElement = ""
	End If
'MsgBox "ExtractElement out "+ExtractElement
End Function
'==========================================
Function Extractreason(p)
    	Dim svar 'As String
    	Dim namevalsepa 'As String
    	svar = "reason"
    	namevalsepa = "="
    	x = Split(p, namevalsepa)
    	If (UBound(x) = 1) Then
    		If Trim(LCase(x(0))) = svar Then
            			Extractreason = x(1)
        		Else
            		    Extractreason = " "
        		End If
    	Else
            		Extractreason = " "
    	End If
End Function
'==========================================
Function Extractdocid(p)
    	Dim svar 'As String
    	Dim namevalsepa 'As String
    	svar = "docid"
    	namevalsepa = "="
    	x = Split(p, namevalsepa)
    	If (UBound(x) = 1) Then
    		If Trim(LCase(x(0))) = svar Then
            			Extractdocid = x(1)
        		Else
            		    Extractdocid = " "
        		End If
    	Else
            		Extractdocid = " "
    	End If
End Function
'==========================================
Function ExtractcPIID(p)
	'   best try to determine whether fgn is an NIH Grant Number
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
Function ExtractCatid(p)
	Dim catname 'As String
	Dim namevalsepa 'As String
       
	catname = "catid"
	namevalsepa = "="
	x = Split(p, namevalsepa)
	If (UBound(x) = 1) Then
		If Trim(LCase(x(0))) = catname Then
			ExtractCatid = x(1)
		Else
			ExtractCatid = " "
		End If
    	Else
            		ExtractCatid = " "
    	End If
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

Function MovetoOldFolder(CFolder,CItem)
	set oldfolder = CFolder.Folders("old")
	CItem.Move(oldfolder)
	MovetoOldFolder="Done"
End Function

'==========================================
Function getCurrFolder()
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
	
	flname="Extract-DocMan-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
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


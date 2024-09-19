'**********************************************************************
' Created by: Imran Omair
' Created Date: 2/5/2013
' Description: NIHeMail
'Released date: 2/5/2013
'Modification description:
'Modification date:
'************************************************************************

startTimeStamp=Now	
	logDir="C:\egrants\apps\log\"	
	forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)
	
	Dim objNS 
	Dim OtlkApps	
	dirpath="NCIOGAeGrantsProd@mail.nih.gov\Inbox\"	
	'dirpath="nciogastage@mail.nih.gov\Inbox\"	
	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")
		
		Call Process(dirpath)

taskEndMssg="Task Completed!"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)		
	set objFS=Nothing	
'=========================================='
Sub Process(dirpath)

	

	On Error Resume Next 
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

	'Set OldFldr = CFolder.Folders("Old emails")
	Set OldFldr = CFolder.Folders("AddSupp_Vote")
	itemsProcessed=0	
	CurrItem=CFolder.Items.Count
	Do While CurrItem > 0

		'MsgBox "Mail count=" & CFolder.Items.Count
  		'Set CItem = CFolder.Items(CFolder.Items.Count)
		Set CItem = CFolder.Items(CurrItem)			
			v_SubLine = CItem.Subject
			'v_Sender = CItem.Sender
			'MsgBox "Sender= "  & v_Sender
			''MsgBox "SUBJECT LINE: "  & v_SubLine

			IF ( InStr(v_SubLine,"Accepted:") or InStr(v_SubLine,"Rejected:")) > 0 Then
				''MsgBox "FOUND->"&v_SubLine
				Set OutMail = CItem.Forward
				FWDSubject="DO NOT REPLY :   Forwarding Responce [" & v_SubLine & "]"
		   		With OutMail
					'.Recipients.Add("NCIOGASupplements@mail.nih.gov")						
					.Recipients.Add("emily.driskell@nih.gov")		
					.Recipients.Add("jonesni@mail.nih.gov")
					.Subject =FWDSubject
					.Send
		   		End With
				Set OutMail=nothing
				itemsProcessed=itemsProcessed+1
				
					processTimeStamp=Now
					If Err.Number <> 0 Then
						logMssg= "Error Occured! => EmailSender:" & CItem.SenderName & "; " & "Subjectline :" & v_SubLine & "; "& " Recieved Date: " & CItem.ReceivedTime
						errorMssg= "Error Number: " & Err.Number & "," & "Error Description: " & Err.Description & ", " & "Error Source: " & Err.Source
					Else		
						logMssg= "Processed! => EmailSender:" & CItem.SenderName & "; " & "Subjectline :" & v_SubLine & "; " & " Recieved Date: " & CItem.ReceivedTime
						errorMssg=""
						
					End If
					Err.Clear
				Call writeLog(forAppending, logMssg, errorMssg, processTimeStamp)	
				CItem.Move(OldFldr)
			END IF
	CurrItem=CurrItem - 1
	'CItem.Move(OldFldr)
 	Loop	
	'Wend	
	Set objNS = Nothing
	Set OtlkApps = Nothing
End Sub

'==========================================
Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="Supp-VoteColl-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
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

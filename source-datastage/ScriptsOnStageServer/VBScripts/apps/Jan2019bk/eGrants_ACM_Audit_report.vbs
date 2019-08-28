	startTimeStamp=Now	
	logDir="C:\egrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFslog
	set objFslog=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)	

	''conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-D201-V.NCI.NIH.GOV\MSSQLEGRANTSD,52000;Application Name=egrants"
	''conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=egrants"
	''conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P232-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"
	  conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P232-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"
	
	SRCDIRPATH= "C:\eGrants\Reports\ACMMonthlyReport\"
	BCKDIRPATH= "C:\eGrants\Reports\ACMMonthlyReport\BACKUP\"
	
	'===============Do not implement following two lines==========================''''
	'donot use'''IMGSVRPTH= "\\nciws-p288-v\egrants\funded\egrantsadmin\auditreport\"
	'IMGSVRPTH= "\\nciws-p288-v\egrants\Imran\AuditReport\"
	IMGSVRPTH= "\\nciws-p288-v\egrants\funded\egrantsadmin\auditreport"
	
	'IMGSVRPTH2= "\\nciis-p401.nci.nih.gov\group02\GAB_SP\GAB Application Repository\AuditReport
	IMGSVRPTH2= "\\nciis-p401.nci.nih.gov\group02\eGrants_Audit_reports\"
	'IMGSVRPTH2= "\\nciis-p401.nci.nih.gov\group04\"
	
	'SRCDIRPATH= "C:\eGrants\dev\reports\ACM_Audit_Report\"
	'BCKDIRPATH= "C:\eGrants\dev\reports\BACKUP\"
	'IMGSVRPTH= "\\nciws-d284-v\egrants\funded\egrantsadmin\auditreport\"
	'IMGSVRPTH2= "\\nciis-p401.nci.nih.gov\group02\eGrants_Audit_reports\"
		
	Call Process()		

	taskEndMssg="Task Completed!"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	

	set oConn=Nothing
	set oRS=Nothing	
	set objFslog=Nothing	
	
'==========================================
Sub Process()

Dim report_NM
Dim file_NM
Dim run_DT
Dim file_URL
Dim unix_PTH

'On Error Resume Next

report_NM="Egrants ACM Monthly Audit Report"
Set oConn = CreateObject("ADODB.Connection")
Set oRS = CreateObject("ADODB.Recordset")
set objFS=CreateObject("Scripting.FileSystemObject") 
oConn.Open conStr	
srcFolderExists=objFS.FolderExists(SRCDIRPATH)
errmsg=""
counts=0
If(srcFolderExists) Then
	Set srcFld=objFS.getFolder(SRCDIRPATH)	
	If(srcFld.Files.Count>0) Then
		For Each objfile In srcFld.Files
		IF (objFS.GetExtensionName(objfile)="xlsx" or objFS.GetExtensionName(objfile)="xls")  Then
			counts=counts + 1
			file_Size=objfile.size
			If file_Size=0 Then
				errmsg=errmsg & vbCrLf & file_NM & ": " & "has zero(0) byte size"
			Else
				file_NM=objfile.name
				run_DT=objfile.DateLastModified
				file_URL="/data/funded/egrantsadmin/auditreport/" & file_NM
				file_src=SRCDIRPATH & file_NM
				file_backdest=BCKDIRPATH & file_NM
				file_finaldest=IMGSVRPTH & file_NM
				file_finaldest2=IMGSVRPTH2 & file_NM
				
				sqlstring= "INSERT INTO dbo.egrants_audit_report (Report_name,File_name,Run_date, url) VALUES('" & report_NM & "','" & file_NM & "', '" & run_DT &"', '"&file_URL&"' )"
				oRS.Open sqlstring, oConn
				
				'''Commenting the following line because Due to FIPS setting network access is not possible from shell and I do not have time to fix this. I am using VC to do this job
				'If (objFS.FileExists(file_finaldest) ) Then
				'	objFS.DeleteFile file_finaldest	
				'End If				
				'objFS.CopyFile file_src, file_finaldest	
				
				'If (objFS.FileExists(file_finaldest2) ) Then
				'	objFS.DeleteFile file_finaldest2	
				'End If				
				'objFS.CopyFile file_src, file_finaldest2
				
				'If (objFS.FileExists(file_backdest) ) Then
				'	objFS.DeleteFile file_backdest	
				'End If				
				'objFS.MoveFile file_src, file_backdest			
				
		
				processTimeStamp=Now
				dim errorMssg
				dim logMssg
				If Err.Number <> 0 Then
					logMssg= "Error Occured! => File Name:" & file_NM & ";" & " generate Date: " & run_DT
					errorMssg= "Error Number: " & Err.Number & "," & "Error Description: " & Err.Description & ", " & "Error Source: " & Err.Source 
				Else		
					logMssg= "Processed! => File Name:" & file_NM & ";" & " Generate Date: " & run_DT
					errorMssg=""
					
				End If
				Err.Clear	
				Call writeLog(forAppending, logMssg, errorMssg, processTimeStamp)
				logMssg=""
				errorMssg=""
				
			End If ''test 0 byte
		End IF ''test file type
		Next
	Else
		errmsg=errmsg & vbCrLf & "No file has been generated on: " & Date
	End If ''test count
 End If
 mesg="(" & counts &")" & "ACM Audit reports has(ve) been processed on " & Date & vbCrLf & "Please find the reports here:  " & IMGSVRPTH2
''''Call NotifyAdmin(errmsg, mesg)
oConn.close
Set oConn = Nothing
Set oRS = Nothing
Set objFS=Nothing
End Sub
'==========================================

Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="ACMAudReport-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
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

'==========================================
Function NotifyAdmin(errmsg, mesg)
	Set OtlkApps = GetObject("","Outlook.application")
	Set OutMail = OtlkApps.CreateItem(olMailItem)
	With OutMail
		.Recipients.Add("omairi@mail.nih.gov")
		'.Recipients.Add("robert.jones2@nih.gov")
		.Subject = "Monthly ACM Audit Report Notification"
		.body=errmsg & vbCrLf & vbCrLf & mesg
		.Send
	End With					
	Set OutMail=nothing	
	'logOutput.WriteLine('Email sent!')
	RaiseErrortoAdmin ="Done"
End Function
'==========================================
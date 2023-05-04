'**********************************************************************
'  Visual Basic ActiveX Script
'	AdMinSupp_Emailer
'	olFormatHTML 2 HTML format
'	olFormatPlain 1 Plain format
'	olFormatRichText 3 Rich text format
'	olFormatUnspecified 0 Unspecified format
'	Usage=  olMail.BodyFormat = 2
'************************************************************************
	
	startTimeStamp=Now	
	logDir="C:\eGrants\apps\log\"	
    forAppending=8	
	taskStartMssg="...........Task Started!..........."
	Dim objFS
	Set objFS=CreateObject("Scripting.FileSystemObject")
	
	Call writeLog(forAppending, taskStartMssg, "", startTimeStamp)
	
	
	Dim OtlkApps
	Dim objNS
	Dim oConn
	Dim Mitem 
	Dim suppMailesSent

	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")
	Set oConn=CreateObject("ADODB.Connection")

	'conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=eim;Data Source=ncidb-d131-v\egrants_dev,52300;Application Name=eGrants"	
	'conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=eim;Data Source=ncidb-d131-v\egrants_dev,52300\egrants_piv;Application Name=TestApp"
	''conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=NCIDB-P232-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=eGrants"
	conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"
	Call Process()	
	
	set oConn=Nothing
	
	taskEndMssg=  "******* Task Completed! *******" & suppMailesSent & " Mail Items Have Been Delivered"
	endTimeStamp=Now	
	call writeLog(forAppending, taskEndMssg, "", endTimeStamp)	
	
	Set objFS=Nothing
''************************************************''
	
	Sub Process()
	
	On Error Resume Next
	
	oConn.Open conStr
	suppMailesSent=0

	'sql1="select distinct Notification_id from dbo.adsup_Notification_email_status where email_date is null and Notification_id in (150,151)  order by 1desc"
	sql1="select distinct Notification_id from dbo.adsup_Notification_email_status where email_date is null  order by Notification_id desc"
	set rs1=oConn.Execute(sql1)
	
	'''''   For each Notification that has not been sent yet
	while (not rs1.EOF)
	sql="select distinct Notification_id,email,dbo.fn_adsupp_getemail_string(Notification_id,email) as emailstr, email_template_id from dbo.adsup_Notification_email_status where email_date is null and Notification_id=" & rs1("Notification_id") 
	set rs=oConn.Execute(sql)
	NotId=rs1("Notification_id")

		SQLSUB="SELECT DBO.fn_adsupp_getemail_subject("& rs1("Notification_id")  &" ) as sub "
		''MsgBox SQLSUB
		SET RSSUB=oConn.Execute(SQLSUB)
		SubLine=RSSUB.Fields("sub").value

		SQLSUB="SELECT DBO.fn_adsupp_getemail_body("& rs1("Notification_id")  &" ) as bod "
		SET RSSUB=oConn.Execute(SQLSUB)
		BodyLine=RSSUB.Fields("bod").value

		'MsgBox (SubLine)
		'MsgBox (BodyLine)

		while (not rs.EOF)
			If rs.Fields("email").value="to"  Then
				toemailstr=rs.Fields("emailstr").value
				'''toemailstr="leul.ayana@nih.gov"

			ElseIf rs.Fields("email").value="cc"  Then
				ccemailstr=rs.Fields("emailstr").value
				'''ccemailstr="leul.ayana@nih.gov"
			End If
		rs.MoveNext()
		wend
		rs.close()

		If  len(Trim(toemailstr))<>0 Then
		Set Mitem = OtlkApps.CreateItem(olMailItem )
			'On Error Resume Next
			'On Error GoTo err_UpdatDB
			With Mitem
				'.Sender.Add='nciogastage'
				.To=toemailstr
				.CC=ccemailstr &"; emily.driskell@nih.gov;jonesni@mail.nih.gov;NCIOGASupplements@mail.nih.gov"       
				.Subject=SubLine
				.VotingOptions = "Accepted;Rejected"
				.Importance = 2   '''''''olImportanceHigh
				.BodyFormat = 2
				.HTMLBody = BodyLine&"Notification Id="&NotId
				.Send

				'.Body=BodyLine
				'.Display
				'.Recipients.Add(toemailstr)	
				'.Recipients.Add(ccemailstr)	

				SQLSUB="update dbo.adsup_Notification_email_status set email_date=GETDATE() , email_send_status='Send' where Notification_id= "&  rs1("Notification_id") 
				SET RSSUB=oConn.Execute(SQLSUB)
			
			End With
		
		Set Mitem=nothing
''		

		Else	 		
		
		Set Mitem = OtlkApps.CreateItem(olMailItem )
		
		toemailstr="omairi@mail.nih.gov;nciogaissues@mail.nih.gov"
		ccemailstr="emily.driskell@nih.gov;jonesni@mail.nih.gov"
		
			With Mitem
				'.Sender.Add='nciogastage'
				.To=toemailstr
				.CC=ccemailstr
				.Subject="ERROR Refering : "&SubLine
				.Importance = 2   '''''''olImportanceHigh
				.BodyFormat = 2
				.HTMLBody = "ERROR : Some how Admin Suplement Automated WorkFlow emailer system could not find PD email address in GPMATS as main recipient of Grant Number mention in subject. Email could not be sent for Notification_id = "&NotId
				.Send
				
				SQLSUB="update dbo.adsup_Notification_email_status set email_date=GETDATE() , email_send_status='NtSend' where Notification_id= "&  rs1("Notification_id") 
				SET RSSUB=oConn.Execute(SQLSUB)
			
			End With		
		Set Mitem=nothing	
		End If
	'Wscript.echo "Notif_ID : " & NotId & "Subject:" & SubLine & "; To(real):" & toemailstr & "; CC(real):" & ccemailstr
		processTimeStamp=Now
			If Err.Number <> 0 Then
				logMssg= "Error Occured! => with Notification_ID: " & NotId 
				errorMssg= "Error Number: " & Err.Number & "," & "Error Description: " & Err.Description & ", " & "Error Source: " & Err.Source
			Else		
				logMssg= "Processed! => Notification_ID: " & NotId  & "; Sent to: " & toemailstr & "; Subject: " & SubLine
				errorMssg=""
				
			End If
			Err.Clear
			
			Call writeLog(forAppending, logMssg, errorMssg, processTimeStamp)
			
	suppMailesSent=suppMailesSent+1	
	
	rs1.MoveNext()
	wend

rs1.close()
oConn.close()

End Sub
'==========================================
Function writeLog(code, message, errorInfo, timeStamp)	
	
	flname="Suppl-Emailer-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	
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



'**********************************************************************
' Created by: Imran Omair:\
' Created Date: 10/31/2015
' Description: This dts checks efile publick folder and extract all emails, finds out egrants node, and attach email and it's attachment to egnts.
'Released date: 
'Modification description:
'Modification date:
'************************************************************************

	Dim oConn, oRs, cmd
	Dim XDoc 'As Object
	Dim objFSO ,  objFolder ,  objFile ,  i, objLogF
	Const ForReading = 1, ForWriting = 2, ForAppending = 8, TristateFalse = 0
	
	'''conStr = "Provider=SQLOLEDB.1;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=eim;Data Source=ncidb-d131-v\egrants_dev,52300;Application Name=eGrants"
	''''conStr = "Provider=SQLOLEDB.1;Password=DayofSpr!ng;Persist Security Info=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=docman"
	conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=True;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"
	
	DOCSRCFLPATH="C:\egrants\SUPP_PFR\"	
	BAKDSTFLPATH="C:\egrants\SUPP_PFR\BAK\"
	FINALDSTFLPATH="C:\egrants\WATCH\out\"
	LOGFLPATH="C:\egrants\apps\log\"
	
	logFlNm=LOGFLPATH & "SUPP-PFR-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"	
	Set oConn = CreateObject("ADODB.Connection")
	Set oRS = CreateObject("ADODB.Recordset")
	oConn.Open conStr
	Set cmd = CreateObject("ADODB.Command")     
	Set cmd.ActiveConnection = oConn   

	Set objFSO = CreateObject("Scripting.FileSystemObject")	
	Set objFolder = objFSO.GetFolder(DOCSRCFLPATH)	
	Set objLogF=objFSO.OpenTextFile(logFlNm , ForAppending , True, TristateFalse)
	objLogF.WriteLine vbNewLine
	objLogF.WriteLine Now & "  -" & vbTab & ".........Task Started!........" 
	objLogF.WriteLine "......................................................................................."
	
	FilesProcessed=0
	'loops through each file in the directory and prints their names and path
	For Each objFile In objFolder.Files
		objLogF.WriteLine Now  & "  -" & vbTab & "File =" & objFile.Path
		Set XDoc = CreateObject("MSXML2.DOMDocument")
		XDoc.async = False 
		XDoc.validateOnParse = False
		If XDoc.Load (objFile.Path) Then
	
			'Get Document Elements
		 	Set lists = XDoc.DocumentElement
	     
			'Traverse all elements 2 branches deep
		    	For Each listNode In lists.ChildNodes
				objLogF.WriteLine vbTab & vbTab & "    => List Node =" & ListNode.BaseName
				objLogF.WriteLine vbTab & vbTab & " 		......................."
				applid=""
				catname=""    
				filename=""
				filenumbername=""
				docdt= ""
				filetype=""
				'MsgBox ListNode.BaseName

		        		For Each fieldNode In listNode.ChildNodes
					objLogF.WriteLine vbTab & vbTab & "      	fieldNode =" & fieldNode.BaseName  & " Value = " &  fieldNode.Text
					'MsgBox  "[" & fieldNode.BaseName & "] = [" & fieldNode.Text & "]"
					If fieldNode.BaseName="applid" Then
						applid=fieldNode.Text
					ElseIf fieldNode.BaseName="folderid" and fieldNode.Text="19" Then
						catname="PFR"
					ElseIf fieldNode.BaseName="filename" Then
						filename=fieldNode.Text
					ElseIf fieldNode.BaseName="date" Then
						docdt=fieldNode.Text
					ElseIf fieldNode.BaseName="file_type" Then
						file_type=fieldNode.Text
					End If
				Next 'fieldNode
				''MsgBox "Applid="&applid&" doctd="&docdt&" catname="&catname&" file_type"
				objLogF.WriteLine vbTab & vbTab & " 		......................."
					objLogF.WriteLine vbTab & vbTab & "     	Calling DB Proc = getPlaceHolder_new"
					cmd.CommandText = "getPlaceHolder_new"
					cmd.CommandType = 4
					cmd.Parameters.Refresh
					cmd.Parameters(1).Value=applid
					cmd.Parameters(2).Value=" "
					cmd.Parameters(3).Value=DateValue(docdt)
					cmd.Parameters(4).Value=catname
					cmd.Parameters(5).Value=file_type
					cmd.Parameters(6).Value=" "
					cmd.Parameters(7).Value=" "
					cmd.Parameters(8).Value=" "

					Set oRS = cmd.Execute
					objLogF.WriteLine vbTab & vbTab & "     	Finished DB Proc = getPlaceHolder_new"
					If (oRS.BOF = True and oRS.EOF = True) Then
						set oRS = Nothing
						'MsgBox "No data found"
						objLogF.WriteLine vbTab & vbTab & "     		>>Error<< DB Proc = getPlaceHolder_new >>> No Data Found"
						'If getPlaceHolder_new did not returned any thing this means there is an error to be investigated for ward this to admin. 
						replysubj="ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new"
						bodystr="Could not create entry in WIP. Check DB proc : getPlaceHolder_new"						
						Call emailme(replysubj,bodystr)
					Else	
						filenumbername=oRs.Fields("ABC").value
						objLogF.WriteLine vbTab & vbTab & "        ReturnValue From Proc getPlaceHolder_new=" & filenumbername
						''bak XML FILE
						XmlSrc = objFile.Path 
						XmlDest = BAKDSTFLPATH & objFile.Name
						'''''Backup xml file
						'Name objFile.Path As destflnm
						'MsgBox "Move XML FROM: " & XmlSrc & "  TO:   " & XmlDest						
						objFSO.MoveFile XmlSrc , XmlDest
						objLogF.WriteLine "			Moved XML FROM: " & XmlSrc & "  TO:   " & XmlDest						
						
						''''BAK COPY  PDf file
						pdf_src=DOCSRCFLPATH & filename		
						pdf_dest=BAKDSTFLPATH & filename						
						'MsgBox "BakUp PDF file from : "&pdf_src&" TO: "& pdf_dest
						objFSO.CopyFile pdf_src , pdf_dest
						objLogF.WriteLine "			BakUp PDF file from : "&pdf_src&" TO: "& pdf_dest
			
						''''MOVE PDf file to Watch OUT						
						pdf_dest=FINALDSTFLPATH & filenumbername & "." & file_type
						'MsgBox "Move PDF to watch Out From : "&pdf_src&" TO: "& pdf_dest
						objFSO.MoveFile pdf_src , pdf_dest
						objLogF.WriteLine "			Move PDF to watch Out From : "&pdf_src&" TO: "& pdf_dest						
						
						set oRS = Nothing
						
					end if
						        		
		    	Next 'listNode
	     
		    Set XDoc = Nothing
		Else
			'MsgBox "Document could NOT be loaded"
			objLogF.WriteLine " 	>>>Error<<  Document could NOT be loaded "
			objLogF.WriteLine " 	Going to Next ObjFile"
		End If
		objLogF.WriteLine "......................................................................................."
		FilesProcessed=FilesProcessed+1
	Next 'objFile

	set oRS = Nothing
	oConn.close
	set oConn=Nothing
	objLogF.WriteLine "......................................................................................."
	objLogF.WriteLine  Now  & "  -" & vbTab & "=====STOPPING  Load_Supp_PFR :: " & FilesProcessed & " files have been processed!"
	objLogF.WriteLine""
	objLogF.Close
	SET ObjLogFS=nothing
	SET objLogF = nothing	

'==========================================

'==========================================
Function emailme(substr,bodystr)
	Dim objNS
	Dim Mitem 

	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")		

	Set Mitem = OtlkApps.CreateItem(olMailItem )
	With Mitem
		'.To="omairi@mail.nih.gov;qians@mail.nih.gov;hareeshj@mail.nih.gov"	
		.To="ayehualem.anteneh@nih.gov"	
		.CC="omairi@mail.nih.gov"			
		.Subject = "PROD: " & substr
		.BodyFormat = 2
		.HTMLBody = " " & bodystr
		.Send

	End With
	Set Mitem=nothing
	Set objNS=Nothing
	Set OtlkApps=Nothing		

End Function
'==========================================

'**********************************************************************
' Created by: Imran Omair:\
' Created Date: 8/26/2016
' Description: This dts load PFR from a local folder, metadata is in xml file
'Released date: 8/31/2016
'Modification description:
'Modification date: 
'************************************************************************

 
	Dim oConn, oRS, cmd
	Dim XDoc 
	Dim objFSO ,  objFolder ,  objFile, ObjLogFS, objLogF
	Const ForReading = 1, ForWriting = 2, ForAppending = 3, TristateFalse = 0
	
	''conStr = "Provider=SQLNCLI11;Password=DayofSpr!ng;Persist Security Info1=True;User ID=AllWebUSER;Initial Catalog=EIM;Data Source=ncidb-p133-v\egrants_prod,52300;Application Name=egrants"
	conStr = "Provider=SQLNCLI11;Password=Jo0ne62017!;Persist Security Info=False;User ID=egrantsuser;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\MSSQLEGRANTSP,59000;Application Name=egrants"

	Set oConn = CreateObject("ADODB.Connection")
	Set oRS = CreateObject("ADODB.Recordset")
	oConn.Open conStr

	Set cmd = CreateObject("ADODB.Command")     
	Set cmd.ActiveConnection = oConn   

	DOCSRCFLPATH="C:\egrants\PFR\"
	PDFSRCFLPATH=""
	BAKDSTFLPATH="C:\egrants\PFR\BAK\"
	FINALDSTFLPATH="C:\egrants\watch\out\"
	LOGFLPATH="C:\egrants\apps\Log\"
	

	'Create an instance of the FileSystemObject
	Set objFSO = CreateObject("Scripting.FileSystemObject")
	'Get the folder object
	Set objFolder = objFSO.GetFolder(DOCSRCFLPATH)

	Set ObjLogFS=CreateObject("Scripting.FileSystemObject")
	logFlNm=LOGFLPATH & "PFR-Log-" & Year(Date) & "-"& Month(Date) & "-" & Day(Date) & ".txt"		
	Set objLogF=ObjLogFS.OpenTextFile(logFlNm , 8 , True, 0)
	objLogF.WriteLine vbNewLine 
	objLogF.WriteLine Now & "  -" & vbTab & ".........Task Started!........" 
	objLogF.WriteLine "......................................................................................."
	NumberOfFilesProcessesd=0
	'loops through each file in the directory and prints their names and path
	applList = " "
	For Each objFile In objFolder.Files		
		objLogF.WriteLine Now  & "  -" & vbTab & " File to be processed => " & objFile.Path
		Set XDoc = CreateObject("MSXML2.DOMDocument")
		XDoc.async = False 
		XDoc.validateOnParse = False
		If XDoc.Load (objFile.Path) Then
	
			'Get Document Elements
		 	Set lists = XDoc.DocumentElement
	     
			'Traverse all elements 2 branches deep
		    	For Each listNode In lists.ChildNodes
				objLogF.WriteLine vbTab & vbTab & "      -> List Node = " & ListNode.BaseName
				objLogF.WriteLine vbTab & vbTab & vbTab & "-----------------------------"
				applid=""
				catname=""    
				filename=""
				filenumbername=""
				docdt= ""
				filetype=""
				'MsgBox ListNode.BaseName

		       		 For Each fieldNode In listNode.ChildNodes
					objLogF.WriteLine vbTab & vbTab & "      fieldNode = " & fieldNode.BaseName  & ", Value = " &  fieldNode.Text 
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
					ElseIf fieldNode.BaseName="uid" Then
						createdby=fieldNode.Text						
					End If
				Next 'fieldNode
				objLogF.WriteLine vbTab & vbTab & vbTab & "-----------------------------"
				pdf_src=DOCSRCFLPATH & filename
				If (objFSO.FileExists(pdf_src) ) Then

					applList = applList + applid + "   "

					objLogF.WriteLine Now  & "  ->" & " Calling DB Procedure 'Create_PFR' "
					cmd.CommandText = "Create_PFR"
					cmd.CommandType = 4
					cmd.Parameters.Refresh
					cmd.Parameters("@APPLID").Value=applid
					cmd.Parameters("@Rcvd_dt").Value=DateValue(docdt)
					cmd.Parameters("@Catname").Value=catname
					cmd.Parameters("@filetype").Value=file_type
					cmd.Parameters("@CreatedBy").Value=createdby

					Set oRS = cmd.Execute
					objLogF.WriteLine Now  & "  ->" & " Finished executing DB Procedure 'Create_PFR'"
					If (oRS.BOF = True and oRS.EOF = True) Then
						set oRS = Nothing
						'MsgBox "No data found"
						objLogF.WriteLine vbTab & vbTab & "      -> >>Error<< DB Proc = Create_PFR >>> No Data Found"
						'If Create_PFR did not returned any thing this means there is an error to be investigated for ward this to admin. 
						substr="PROD=>> ERROR: Could not create PFR in DB using  Create_PFR"
						bodystr="Could not create PFR in Prod DB using  Create_PFR"
						abc = emailme(substr,bodystr)

					Else	
						filenumbername=oRS.Fields("ABC").value
						objLogF.WriteLine Now  & "  -> Return Value From Proc Create_PFR => " & filenumbername
						''bak XML FILE
						XmlSrc = objFile.Path 
						XmlDest = BAKDSTFLPATH & objFile.Name
						'''''Backup xml file

						If (objFSO.FileExists(XmlDest) ) Then
							objFSO.DeleteFile XmlDest
							objFSO.MoveFile  XmlSrc , XmlDest	
						Else
							objFSO.MoveFile  XmlSrc , XmlDest
						End If

						'Name objFile.Path As destflnm
						'MsgBox "Move XML FROM: " & XmlSrc & "  TO:   " & XmlDest						
						'objFSO.MoveFile XmlSrc , XmlDest
						objLogF.WriteLine Now  & "  -> XML Moved  FROM: " & XmlSrc & " TO: " & XmlDest						
						
						''''BAK COPY  PDf file
						pdf_src=DOCSRCFLPATH & filename		
						pdf_dest=BAKDSTFLPATH & filename						
						'MsgBox "BakUp PDF file from : "&pdf_src&" TO: "& pdf_dest
						objFSO.CopyFile pdf_src , pdf_dest
						objLogF.WriteLine Now  & "  -> BackUp PDF file FROM: "&pdf_src&"  TO: "& pdf_dest
			
						''''MOVE PDf file to Watch OUT						
						'''''''pdf_dest="E:\egrants\watch\out\stage\" & filenumbername & "." & file_type
						pdf_dest=FINALDSTFLPATH & filenumbername & "." & file_type
						
						If (objFSO.FileExists(pdf_dest) ) Then
							objFSO.DeleteFile pdf_dest
							objLogF.WriteLine "		Removed Duplicate " & pdf_dest
							objFSO.MoveFile pdf_src , pdf_dest	
							objLogF.WriteLine Now  & "  -> PDF moved to 'watch Out' FROM : "&pdf_src&" TO: "& pdf_dest
						Else
							objFSO.MoveFile pdf_src , pdf_dest
							objLogF.WriteLine Now  & "  -> PDF moved to 'watch Out' FROM : "&pdf_src&" TO: "& pdf_dest
						End If
					
						set oRS = Nothing
					end if
				else
					objLogF.WriteLine vbTab & vbTab & "       ERROR=> No pdf available" & pdf_src
					ERRORBODY="PDF SOURCE="&pdf_src
					abc = emailme("ERROR=> PDF NOT FOUND",ERRORBODY)
				end if  '''''If (objFSO.FileExists(pdf_src) ) Then	      				        		
		    	Next 'listNode
	     
		    Set XDoc = Nothing
		Else
			'MsgBox "Document could NOT be loaded"
			objLogF.WriteLine " Could Not Load File : " &objFile.Path
			objLogF.WriteLine " Going to Next ObjFile"
		End If		
		NumberOfFilesProcessesd=NumberOfFilesProcessesd+1
		objLogF.WriteLine "......................................................................................."
	Next 'objFile

	set oRS = Nothing
	oConn.close
	set oConn=Nothing
	objLogF.WriteLine "......................................................................................."
	objLogF.WriteLine vbTab & vbTab & "      ===== " & NumberOfFilesProcessesd & " files have been processed!" &  "  ===== "
	objLogF.WriteLine Now  & "  =====Task Completed  Load_Supp_PFR :: " 
	'objLogF.WriteLine""
	objLogF.Close
	SET ObjLogFS=nothing
	SET objLogF = nothing
	SET objFSO = nothing

	IF Len(Trim(applList)) > 0 Then
		abc = emailme("STAGE=>>PFR Processed",applList)
	End IF


'==========================================
'==========================================
Function emailme(substr,bodystr)
	Dim objNS
	Dim Mitem 

	Set OtlkApps = GetObject("","Outlook.application")
	Set objNS = OtlkApps.GetNamespace("MAPI")		

	Set Mitem = OtlkApps.CreateItem(olMailItem )
	With Mitem
		'.To="leul.ayana@nih.gov;hareeshj@mail.nih.gov"	
		.To="guillermo.choy-leon@nih.gov;leul.ayana@nih.gov"	
		.CC="leul.ayana@nih.gov"			
		.Subject = "PROD: " & substr
		.BodyFormat = 2
		.HTMLBody = " " & bodystr
		.Send

	End With
	Set Mitem=nothing	
	emailme = true

End Function
'==========================================

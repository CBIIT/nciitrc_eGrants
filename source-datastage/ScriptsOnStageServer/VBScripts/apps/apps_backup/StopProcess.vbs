 
Dim strComputer = "."
Dim strProcessKill = "'Outlook.exe'" 

Set objWMIService = GetObject("winmgmts:" _
& "{impersonationLevel=impersonate}!\\" _ 
& strComputer & "\root\cimv2") 

Set colProcess = objWMIService.ExecQuery _
("Select * from Win32_Process Where Name = " & strProcessKill )
IF colProcess.count>0 Then
For Each objProcess in colProcess
objProcess.Terminate()
Next 
End IF

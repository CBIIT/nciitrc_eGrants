  Dim objWMIService, colProcessList
  Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
  Set colProcessList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'Outlook.exe'")   
 
  Set oShell = WScript.CreateObject("WScript.Shell")
      oShell.Run "outlook" 
  set oShell=Nothing  
 
 set objWMIService=Nothing
 set colProcessList=Nothing
 
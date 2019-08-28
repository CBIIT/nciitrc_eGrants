'**********************************************************************
'DocMan_email_2008_Prod

'************************************************************************
'wscript.echo "DocMan script is going to run!! "
'Stop Service

Set Outlook = GetObject(, "Outlook.Application")
Outlook.Quit()

strServiceName = "ClickToRunSvc"
Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
Set colListOfServices = objWMIService.ExecQuery("Select * from Win32_Service Where Name ='" & strServiceName & "'")
For Each objService in colListOfServices
objService.StopService()
Next


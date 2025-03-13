# MLH : Note : Make sure this isn't already running (Hit windows key, type "Scheduled", and make sure there isn't already one named PowerShell HTTP Conversion Server or anything on 8081')
# Also remember that "Ready" is not the same thing as "Running" !! :o  

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-File D:\Content\egrants-web-dev.nci.nih.gov\DocPDFConversion\serverDocToPdf.ps1"
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount
Register-ScheduledTask -TaskName "PowerShell HTTP Conversion Server" -Action $action -Trigger $trigger -Principal $principal -Description "This task starts a powershell listener to facilitate converting .doc files to .pdf via Libre Office, which must be installed locally"
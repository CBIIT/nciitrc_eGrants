@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\EGrantsAcmAuditReport"
"C:\eGrants\apps\EGrantsAcmAuditReport\EGrantsAcmAuditReport.exe"

endlocal
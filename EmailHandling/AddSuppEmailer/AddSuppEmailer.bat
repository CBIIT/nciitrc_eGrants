@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\AddSuppEmailer"
"C:\eGrants\apps\AddSuppEmailer\AddSuppEmailer.exe"

endlocal
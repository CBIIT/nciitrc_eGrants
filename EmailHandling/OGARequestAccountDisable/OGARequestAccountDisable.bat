@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\OGARequestAccountDisable"
"C:\eGrants\apps\OGARequestAccountDisable\OGARequestAccountDisable.exe"

endlocal
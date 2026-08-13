@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\LoadPfr"
"C:\eGrants\apps\LoadPfr\LoadPfr.exe"

endlocal
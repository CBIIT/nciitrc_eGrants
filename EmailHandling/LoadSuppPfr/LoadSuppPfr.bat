@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\LoadSuppPfr"
"C:\eGrants\apps\LoadSuppPfr\LoadSuppPfr.exe"

endlocal
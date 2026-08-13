@echo off
setlocal

set DOTNET_ENVIRONMENT=%~1

cd /d "C:\eGrants\apps\Router"
"C:\eGrants\apps\Router\Router.exe"

endlocal
@echo off
setlocal

set "PS_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe"
set "LOG_DIR=C:\temp"
set "LOG_FILE=%LOG_DIR%\Run_Router_Exchange_AddSupp.log"
set "SCRIPT_PARAM=%~1"

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

if "%SCRIPT_PARAM%"=="" (
  echo ERROR: No parameter supplied.
  echo Usage: %~nx0 ^<value^>
  exit /b 1
)

echo =============================================== >> "%LOG_FILE%"
echo Run started: %date% %time% >> "%LOG_FILE%"
echo Parameter: %SCRIPT_PARAM% >> "%LOG_FILE%"
echo =============================================== >> "%LOG_FILE%"

echo Running Router.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\Router.ps1" -MyParam "%SCRIPT_PARAM%" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo Router.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo Router.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo Router.ps1 succeeded.
  echo Router.ps1 succeeded. >> "%LOG_FILE%"
)

echo Running ExchangeFixed.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\ExchangeFixed.ps1" -MyParam "%SCRIPT_PARAM%" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo ExchangeFixed.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo ExchangeFixed.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo ExchangeFixed.ps1 succeeded.
  echo ExchangeFixed.ps1 succeeded. >> "%LOG_FILE%"
)

echo Running AddSuppProd.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\AddSuppProd.ps1" -MyParam "%SCRIPT_PARAM%" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo AddSuppProd.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo AddSuppProd.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo AddSuppProd.ps1 succeeded.
  echo AddSuppProd.ps1 succeeded. >> "%LOG_FILE%"
)

echo Running AddSuppEmailer.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\AddSuppEmailer.ps1" -MyParam "%SCRIPT_PARAM%" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo AddSuppEmailer.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo AddSuppEmailer.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo AddSuppEmailer.ps1 succeeded.
  echo AddSuppEmailer.ps1 succeeded. >> "%LOG_FILE%"
)

echo All scripts completed successfully.
echo All scripts completed successfully. >> "%LOG_FILE%"
exit /b 0
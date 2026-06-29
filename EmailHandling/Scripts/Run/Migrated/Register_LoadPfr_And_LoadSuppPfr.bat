@echo off
setlocal

set PS_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe
set LOG_DIR=C:\temp
set LOG_FILE=%LOG_DIR%\Run_LoadPfr_And_LoadSuppPfr.log

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

echo =============================================== >> "%LOG_FILE%"
echo Run started: %date% %time% >> "%LOG_FILE%"
echo =============================================== >> "%LOG_FILE%"

echo Running LoadPfr.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\LoadPfr.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo LoadPfr.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo LoadPfr.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo LoadPfr.ps1 succeeded.
  echo LoadPfr.ps1 succeeded. >> "%LOG_FILE%"
)

echo Running LoadSuppPfr.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\LoadSuppPfr.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
  echo LoadSuppPfr.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
  echo LoadSuppPfr.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
  exit /b %errorlevel%
) else (
  echo LoadSuppPfr.ps1 succeeded.
  echo LoadSuppPfr.ps1 succeeded. >> "%LOG_FILE%"
)

echo All scripts completed successfully.
echo All scripts completed successfully. >> "%LOG_FILE%"
exit /b 0
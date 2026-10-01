@echo off
setlocal

set PS_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe
set LOG_DIR=C:\temp
set LOG_FILE=%LOG_DIR%\Register-LoadPfr-Tasks.log

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

echo =============================================== >> "%LOG_FILE%"
echo Run started: %date% %time% >> "%LOG_FILE%"
echo =============================================== >> "%LOG_FILE%"

echo Running LoadPfr_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\LoadPfr_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo LoadPfr_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo LoadPfr_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo LoadPfr_Legacy.ps1 succeeded.
    echo LoadPfr_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo Running LoadSuppPfr_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\LoadSuppPfr_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo LoadSuppPfr_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo LoadSuppPfr_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo LoadSuppPfr_Legacy.ps1 succeeded.
    echo LoadSuppPfr_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo All scripts completed successfully.
echo All scripts completed successfully. >> "%LOG_FILE%"
pause
exit /b 0
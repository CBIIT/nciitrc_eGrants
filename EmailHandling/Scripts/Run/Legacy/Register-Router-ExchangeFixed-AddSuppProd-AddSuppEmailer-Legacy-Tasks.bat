@echo off
setlocal

set PS_EXE=%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe
set LOG_DIR=C:\temp
set LOG_FILE=%LOG_DIR%\Register-Additional-Legacy-Tasks.log

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%"

echo =============================================== >> "%LOG_FILE%"
echo Run started: %date% %time% >> "%LOG_FILE%"
echo =============================================== >> "%LOG_FILE%"

echo Running Router_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\Router_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo Router_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo Router_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo Router_Legacy.ps1 succeeded.
    echo Router_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo Running ExchangeFixed_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\ExchangeFixed_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo ExchangeFixed_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo ExchangeFixed_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo ExchangeFixed_Legacy.ps1 succeeded.
    echo ExchangeFixed_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo Running AddSuppEmailer_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\AddSuppEmailer_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo AddSuppEmailer_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo AddSuppEmailer_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo AddSuppEmailer_Legacy.ps1 succeeded.
    echo AddSuppEmailer_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo Running AddSuppProd_Legacy.ps1...
"%PS_EXE%" -NoProfile -ExecutionPolicy Bypass -File "C:\temp\AddSuppProd_Legacy.ps1" >> "%LOG_FILE%" 2>&1
if errorlevel 1 (
    echo AddSuppProd_Legacy.ps1 FAILED with exit code %errorlevel%. See log: %LOG_FILE%
    echo AddSuppProd_Legacy.ps1 FAILED with exit code %errorlevel%. >> "%LOG_FILE%"
    pause
    exit /b %errorlevel%
) else (
    echo AddSuppProd_Legacy.ps1 succeeded.
    echo AddSuppProd_Legacy.ps1 succeeded. >> "%LOG_FILE%"
)

echo.
echo All scripts completed successfully.
echo All scripts completed successfully. >> "%LOG_FILE%"
pause
exit /b 0
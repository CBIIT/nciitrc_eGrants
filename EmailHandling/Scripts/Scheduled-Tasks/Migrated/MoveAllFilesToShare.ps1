# MoveAllFilesToShare.ps1
# Creates C:\egrants\watch\Move-PdfToShare.ps1 and registers scheduled task MovePdfToShare
# Schedule: every day at :05, :15, :25, :35, :45, :55

$ErrorActionPreference = 'Stop'

# ----------------------------
# Config
# ----------------------------
$TaskName         = 'MoveAllFilesToShare'
$MoveScriptPath   = 'C:\egrants\watch\Move-AllFilesToShare.ps1'
$WorkingDirectory = 'C:\egrants\watch'

# ----------------------------
# 1) Write the worker script (copy + verify + delete)
# ----------------------------
$moveScript = @'
# Move-PdfToShare.ps1
$ErrorActionPreference = 'Stop'

$Source      = 'C:\egrants\watch\out'
$Destination = '\\nciws-q2594-v\egrants\funded2\nci\main'
$LogFile     = 'C:\egrants\watch\logs\Move-AllFilesToShare.log'

function Write-Log {
    param([string]$Message,[string]$Level='INFO')
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $line = "$timestamp [$Level] $Message"
    Write-Output $line

    $logDir = Split-Path -Path $LogFile -Parent
    if (-not (Test-Path -LiteralPath $logDir)) {
        New-Item -Path $logDir -ItemType Directory -Force | Out-Null
    }
    Add-Content -Path $LogFile -Value $line
}

try {
    Write-Log "Task started. Source='$Source' Destination='$Destination'"

    if (-not (Test-Path -LiteralPath $Source)) {
        throw "Source folder not found: $Source"
    }
    if (-not (Test-Path -LiteralPath $Destination)) {
        throw "Destination folder not reachable/found: $Destination"
    }

    $pdfFiles = Get-ChildItem -LiteralPath $Source -Filter '*.pdf' -File
    if (-not $pdfFiles -or $pdfFiles.Count -eq 0) {
        Write-Log "No PDF files found. Nothing to process."
        exit 0
    }

    foreach ($file in $pdfFiles) {
        $targetPath = Join-Path -Path $Destination -ChildPath $file.Name

        try {
            if (Test-Path -LiteralPath $targetPath) {
                Remove-Item -LiteralPath $targetPath -Force
                Write-Log "Removed existing destination file: '$targetPath'"
            }

            Copy-Item -LiteralPath $file.FullName -Destination $targetPath -Force

            if (-not (Test-Path -LiteralPath $targetPath)) {
                throw "Copy verification failed for '$targetPath'"
            }

            $srcSize = (Get-Item -LiteralPath $file.FullName).Length
            $dstSize = (Get-Item -LiteralPath $targetPath).Length
            if ($srcSize -ne $dstSize) {
                throw "Size mismatch for '$($file.Name)' (src=$srcSize, dst=$dstSize)"
            }

            Remove-Item -LiteralPath $file.FullName -Force
            Write-Log "Copied+Verified+Deleted: '$($file.FullName)' -> '$targetPath'"
        }
        catch {
            Write-Log "Failed processing '$($file.FullName)': $($_.Exception.Message)" 'ERROR'
            throw
        }
    }

    Write-Log "Task completed successfully. Files processed: $($pdfFiles.Count)"
    exit 0
}
catch {
    Write-Log "Task failed: $($_.Exception.Message)" 'ERROR'
    exit 1
}
'@

$scriptDir = Split-Path -Path $MoveScriptPath -Parent
if (-not (Test-Path -LiteralPath $scriptDir)) {
    New-Item -Path $scriptDir -ItemType Directory -Force | Out-Null
}
Set-Content -Path $MoveScriptPath -Value $moveScript -Encoding UTF8 -Force

# ----------------------------
# 2) Build scheduled-task XML (daily, every 10 min starting at 00:05)
# ----------------------------
$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

$escapedScript  = [System.Security.SecurityElement]::Escape($MoveScriptPath)
$escapedWorkDir = [System.Security.SecurityElement]::Escape($WorkingDirectory)
$escapedUser    = [System.Security.SecurityElement]::Escape($currentUser)

$taskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>Runs Move-PdfToShare.ps1 every day at minutes 05,15,25,35,45,55.</Description>
  </RegistrationInfo>
  <Triggers>
    <CalendarTrigger>
      <StartBoundary>2026-01-01T00:05:00</StartBoundary>
      <Enabled>true</Enabled>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
      <Repetition>
        <Interval>PT10M</Interval>
        <Duration>P1D</Duration>
        <StopAtDurationEnd>false</StopAtDurationEnd>
      </Repetition>
    </CalendarTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <UserId>$escapedUser</UserId>
      <LogonType>S4U</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>false</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT1H</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>powershell.exe</Command>
      <Arguments>-NoProfile -ExecutionPolicy Bypass -File "$escapedScript"</Arguments>
      <WorkingDirectory>$escapedWorkDir</WorkingDirectory>
    </Exec>
  </Actions>
</Task>
"@

# ----------------------------
# 3) Register task
# ----------------------------
if (Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
}

Register-ScheduledTask -TaskName $TaskName -Xml $taskXml | Out-Null

Write-Host "Created setup script file name: MoveAllFilesToShare.ps1"
Write-Host "Created worker script: $MoveScriptPath"
Write-Host "Created scheduled task: $TaskName"
Write-Host "Schedule: Every day at :05, :15, :25, :35, :45, :55"
Write-Host "Run-as: $currentUser (S4U)"
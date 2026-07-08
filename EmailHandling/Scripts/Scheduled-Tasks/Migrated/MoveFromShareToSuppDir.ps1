# MoveFromShareToSuppDir-Setup.ps1
# Creates C:\egrants\watch\MoveFromShareToSuppDir.ps1 and registers scheduled task MoveFromShareToSuppDir
# Schedule: M-F, hours 7-19, minutes 03, 18, 58, seconds 00

$ErrorActionPreference = 'Stop'

# ----------------------------
# Config
# ----------------------------
$TaskName         = 'MoveFromShareToSuppDir'
$MoveScriptPath   = 'C:\egrants\watch\MoveFromShareToSuppDir.ps1'
$WorkingDirectory = 'C:\egrants\watch'

# ----------------------------
# 1) Write the worker script
# ----------------------------
$moveScript = @'
# MoveFromShareToSuppDir.ps1
# Moves all files from network share to local SUPP directory.
# Exit code 0 = success, non-zero = failure.

$ErrorActionPreference = 'Stop'

$Source      = '\\nciws-p2590-v.nci.nih.gov\egrants\scripts\PFRScript\OUT_Files\SUPP\prod'
$Destination = 'C:\eGrants\SUPP_PFR'
$LogFile     = 'C:\egrants\watch\logs\MoveFromShareToSuppDir.log'

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
        New-Item -Path $Destination -ItemType Directory -Force | Out-Null
        Write-Log "Created destination folder: $Destination"
    }

    $files = Get-ChildItem -LiteralPath $Source -File

    if (-not $files -or $files.Count -eq 0) {
        Write-Log "No files found. Nothing to move."
        exit 0
    }

    foreach ($file in $files) {
        $targetPath = Join-Path -Path $Destination -ChildPath $file.Name
        Move-Item -LiteralPath $file.FullName -Destination $targetPath -Force
        Write-Log "Moved: '$($file.FullName)' -> '$targetPath'"
    }

    Write-Log "Task completed successfully. Files moved: $($files.Count)"
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
# 2) Build scheduled-task XML
# ----------------------------
$currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name

$escapedScript  = [System.Security.SecurityElement]::Escape($MoveScriptPath)
$escapedWorkDir = [System.Security.SecurityElement]::Escape($WorkingDirectory)
$escapedUser    = [System.Security.SecurityElement]::Escape($currentUser)

$triggerXml = New-Object System.Text.StringBuilder

foreach ($hour in 7..19) {
    foreach ($minute in 3,18,58) {
        $timeText = ('2026-01-05T{0:D2}:{1:D2}:00' -f $hour, $minute)
        [void]$triggerXml.AppendLine(@"
    <CalendarTrigger>
      <StartBoundary>$timeText</StartBoundary>
      <Enabled>true</Enabled>
      <ScheduleByWeek>
        <WeeksInterval>1</WeeksInterval>
        <DaysOfWeek>
          <Monday />
          <Tuesday />
          <Wednesday />
          <Thursday />
          <Friday />
        </DaysOfWeek>
      </ScheduleByWeek>
    </CalendarTrigger>
"@)
    }
}

$taskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>Runs MoveFromShareToSuppDir.ps1 Monday-Friday at hours 07-19, minutes 03, 18, and 58, seconds 00.</Description>
  </RegistrationInfo>
  <Triggers>
$($triggerXml.ToString())
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

Write-Host "Created setup script file name: MoveFromShareToSuppDir-Setup.ps1"
Write-Host "Created worker script: $MoveScriptPath"
Write-Host "Created scheduled task: $TaskName"
Write-Host "Schedule: M-F, hours 7-19, minutes 03, 18, 58, seconds 00"
Write-Host "Run-as: $currentUser (S4U)"
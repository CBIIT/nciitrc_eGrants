param (
    [ValidateSet("Development", "Production")]
    [string]$Environment = "Development"
)

$sourceZip = "C:\temp\ExchangeFixed.zip"
$destinationRoot = "C:\eGrants\apps"
$destinationZip = Join-Path $destinationRoot "ExchangeFixed.zip"
$extractTemp = Join-Path $destinationRoot "ExchangeFixed"
$exchangeFixedExe = Join-Path $extractTemp "ExchangeFixed.exe"
$tempScriptDir = "C:\temp"
$tempXml = Join-Path $tempScriptDir "ExchangeFixed-task.xml"

$taskName = "ExchangeFixed"
$taskDescription = "Runs ExchangeFixed.exe every day at minutes 5,15,25,35,45,55 of every hour."

$execute = "cmd.exe"
$arguments = "/c set DOTNET_ENVIRONMENT=$Environment&& `"C:\eGrants\apps\ExchangeFixed\ExchangeFixed.exe`""
$workingDirectory = "C:\eGrants\apps\ExchangeFixed"

$success = $false

try {
    if (-not (Test-Path $sourceZip)) {
        throw "Source ZIP file not found: $sourceZip"
    }

    if (-not (Test-Path $destinationRoot)) {
        New-Item -Path $destinationRoot -ItemType Directory -Force | Out-Null
    }

    if (-not (Test-Path $tempScriptDir)) {
        New-Item -Path $tempScriptDir -ItemType Directory -Force | Out-Null
    }

    Move-Item -Path $sourceZip -Destination $destinationZip -Force

    if (Test-Path $extractTemp) {
        Remove-Item -Path $extractTemp -Recurse -Force -ErrorAction SilentlyContinue
    }

    New-Item -Path $extractTemp -ItemType Directory -Force | Out-Null
    Expand-Archive -Path $destinationZip -DestinationPath $extractTemp -Force

    $exeExists = Test-Path $exchangeFixedExe
    $hasFiles = (Get-ChildItem -Path $extractTemp -Recurse -File -ErrorAction SilentlyContinue | Measure-Object).Count -gt 0

    if (-not ($exeExists -or $hasFiles)) {
        throw "Extraction failed. '$extractTemp' does not contain extracted files."
    }

    $minutes = @(5, 15, 25, 35, 45, 55)
    $triggers = @()

    foreach ($minute in $minutes) {
        $startBoundary = (Get-Date).Date.AddMinutes($minute).ToString("s")

        $triggerXml = @"
<CalendarTrigger>
  <StartBoundary>$startBoundary</StartBoundary>
  <Enabled>true</Enabled>
  <ScheduleByWeek>
    <DaysOfWeek>
      <Monday />
      <Tuesday />
      <Wednesday />
      <Thursday />
      <Friday />
      <Saturday />
      <Sunday />
    </DaysOfWeek>
    <WeeksInterval>1</WeeksInterval>
  </ScheduleByWeek>
  <Repetition>
    <Interval>PT1H</Interval>
    <Duration>P1D</Duration>
    <StopAtDurationEnd>false</StopAtDurationEnd>
  </Repetition>
</CalendarTrigger>
"@

        $triggers += $triggerXml
    }

    $triggerBlock = $triggers -join "`r`n"

    $taskXml = @"
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.4" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <RegistrationInfo>
    <Description>$taskDescription</Description>
  </RegistrationInfo>
  <Triggers>
$triggerBlock
  </Triggers>
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
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>$execute</Command>
      <Arguments>$arguments</Arguments>
      <WorkingDirectory>$workingDirectory</WorkingDirectory>
    </Exec>
  </Actions>
</Task>
"@

    $taskXml | Out-File -FilePath $tempXml -Encoding Unicode

    schtasks.exe /Delete /TN $taskName /F 2>$null | Out-Null
    schtasks.exe /Create /TN $taskName /XML $tempXml /F

    if ($LASTEXITCODE -ne 0) {
        throw "Scheduled task creation failed."
    }

    $success = $true

    if (Test-Path $destinationZip) {
        Remove-Item -Path $destinationZip -Force
    }

    if (Test-Path $tempXml) {
        Remove-Item -Path $tempXml -Force
    }
}
catch {
    Write-Error $_.Exception.Message
    if (-not $success) {
        Write-Warning "An error occurred. '$destinationZip' has been preserved for retry."
    }
    throw
}
$tempScriptDir = "C:\temp"
$tempXml = Join-Path $tempScriptDir "ExchangeFixed-Legacy-task.xml"

$taskName = "ExchangeFixed-Legacy"
$taskDescription = "Runs exchange_Fixed.bat every day at minutes 5,15,25,35,45,55 of every hour."

$execute = "cmd.exe"
$arguments = '/c "C:\eGrants\apps\exchange_Fixed.bat"'
$workingDirectory = "C:\eGrants\apps"

try {
    if (-not (Test-Path $tempScriptDir)) {
        New-Item -Path $tempScriptDir -ItemType Directory -Force | Out-Null
    }

    $minutes = @(5, 15, 25, 35, 45, 55)
    $triggers = @()

    foreach ($minute in $minutes) {
        $startBoundary = (Get-Date).Date.AddMinutes($minute).ToString("s")

        $triggerXml = @"
<CalendarTrigger>
  <StartBoundary>$startBoundary</StartBoundary>
  <Enabled>true</Enabled>
  <ScheduleByDay>
    <DaysInterval>1</DaysInterval>
  </ScheduleByDay>
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
    <Enabled>false</Enabled>
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

    if (Test-Path $tempXml) {
        Remove-Item -Path $tempXml -Force
    }

    Write-Host "Scheduled task '$taskName' registered successfully." -ForegroundColor Green
    Write-Host "  Triggers : Daily at :05, :15, :25, :35, :45, and :55 every hour"
    Write-Host "  Command  : $execute $arguments"
}
catch {
    Write-Error $_.Exception.Message
    throw
}
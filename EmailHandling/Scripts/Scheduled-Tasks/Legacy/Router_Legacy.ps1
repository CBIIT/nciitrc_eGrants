$tempScriptDir = "C:\temp"
$tempXml = Join-Path $tempScriptDir "Router-Legacy-task.xml"

$taskName = "Router-Legacy"
$taskDescription = "Runs Router.bat every day except Sunday at minutes 1,10,20,30,40,50,59 of every hour."

$execute = "cmd.exe"
$arguments = '/c "C:\eGrants\apps\Router.bat"'
$workingDirectory = "C:\eGrants\apps"

try {
    if (-not (Test-Path $tempScriptDir)) {
        New-Item -Path $tempScriptDir -ItemType Directory -Force | Out-Null
    }

    $minutes = @(1, 10, 20, 30, 40, 50, 59)
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
    Write-Host "  Triggers : Mon-Sat at :01, :10, :20, :30, :40, :50, :59"
    Write-Host "  Action   : $execute $arguments"
    Write-Host "  WorkingDir: $workingDirectory"
}
catch {
    Write-Error $_
    exit 1
}
$tempScriptDir = "C:\temp"
$tempXml = Join-Path $tempScriptDir "AddSuppEmailer-Legacy-task.xml"

$taskName = "AddSuppEmailer-Legacy"
$taskDescription = "Runs Add_Supp_Emailer.bat Monday through Friday at minutes 3,18,58 during hours 7 through 19."

$execute = "cmd.exe"
$arguments = '/c "C:\eGrants\apps\Add_Supp_Emailer.bat"'
$workingDirectory = "C:\eGrants\apps"

try {
    if (-not (Test-Path $tempScriptDir)) {
        New-Item -Path $tempScriptDir -ItemType Directory -Force | Out-Null
    }

    $hours = 7..19
    $minutes = @(3, 18, 58)
    $triggers = @()

    foreach ($hour in $hours) {
        foreach ($minute in $minutes) {
            $startBoundary = (Get-Date).Date.AddHours($hour).AddMinutes($minute).ToString("s")

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
    </DaysOfWeek>
    <WeeksInterval>1</WeeksInterval>
  </ScheduleByWeek>
</CalendarTrigger>
"@

            $triggers += $triggerXml
        }
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
    Write-Host "  Triggers : M-F, hours 7-19, at :03, :18, and :58"
    Write-Host "  Command  : $execute $arguments"
}
catch {
    Write-Error $_.Exception.Message
    throw
}
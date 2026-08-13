$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$sourceZip = "C:\temp\LoadPfr.zip"
$destinationRoot = "C:\eGrants\apps"
$destinationZip = Join-Path $destinationRoot "LoadPfr.zip"
$extractTemp = Join-Path $destinationRoot "LoadPfr"
$tempScriptDir = "C:\temp"
$tempXml = Join-Path $tempScriptDir "LoadPfr-task.xml"

$taskName = "LoadPfr"
$taskDescription = "Runs LoadPfr.bat Monday through Friday at minutes 7,21,42 of every hour."

$execute = "C:\eGrants\apps\LoadPfr\LoadPfr.bat"
$arguments = "Development"
$workingDirectory = "C:\eGrants\apps\LoadPfr"

$success = $false
$credential = $null
$plainPassword = $null
$bstr = [IntPtr]::Zero

function Write-Diag {
    param(
        [string]$Message
    )
    Write-Host "[INFO ] $Message"
}

function Write-DiagWarn {
    param(
        [string]$Message
    )
    Write-Warning $Message
}

function Write-DiagError {
    param(
        [string]$Message
    )
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Test-Admin {
    try {
        $currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = New-Object Security.Principal.WindowsPrincipal($currentIdentity)
        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch {
        return $false
    }
}

function Show-PathState {
    param(
        [string]$Label,
        [string]$Path
    )

    $exists = Test-Path $Path
    Write-Diag "$Label : $Path"
    Write-Diag "$Label exists: $exists"

    if ($exists) {
        try {
            $item = Get-Item $Path -ErrorAction Stop

            if ($item.PSIsContainer) {
                $itemType = 'Directory'
            }
            else {
                $itemType = 'File'
            }

            Write-Diag "$Label type: $itemType"

            if (-not $item.PSIsContainer) {
                Write-Diag "$Label size: $($item.Length) bytes"
                Write-Diag "$Label last write: $($item.LastWriteTime)"
            }
        }
        catch {
            Write-DiagWarn "Unable to inspect ${Label}: $($_.Exception.Message)"
        }
    }
}

function Invoke-Schtasks {
    param(
        [string[]]$Arguments,
        [string]$OperationName
    )

    Write-Diag "Running schtasks.exe for: $OperationName"
    Write-Diag "Command: schtasks.exe $($Arguments -join ' ')"

    $tempOut = [System.IO.Path]::GetTempFileName()
    $tempErr = [System.IO.Path]::GetTempFileName()

    try {
        $process = Start-Process -FilePath "schtasks.exe" `
            -ArgumentList $Arguments `
            -Wait `
            -NoNewWindow `
            -PassThru `
            -RedirectStandardOutput $tempOut `
            -RedirectStandardError $tempErr

        $stdout = @()
        $stderr = @()

        if (Test-Path $tempOut) {
            $stdout = @(Get-Content $tempOut -ErrorAction SilentlyContinue)
        }

        if (Test-Path $tempErr) {
            $stderr = @(Get-Content $tempErr -ErrorAction SilentlyContinue)
        }

        Write-Diag "schtasks.exe exit code for ${OperationName}: $($process.ExitCode)"

        if (@($stdout).Count -gt 0) {
            Write-Diag "schtasks stdout for ${OperationName}:"
            $stdout | ForEach-Object { Write-Host "         $_" }
        }

        if (@($stderr).Count -gt 0) {
            Write-Diag "schtasks stderr for ${OperationName}:"
            $stderr | ForEach-Object { Write-Host "         $_" }
        }

        if ((@($stdout).Count -eq 0) -and (@($stderr).Count -eq 0)) {
            Write-Diag "schtasks.exe produced no output for ${OperationName}."
        }

        return [PSCustomObject]@{
            ExitCode = $process.ExitCode
            StdOut   = @($stdout)
            StdErr   = @($stderr)
        }
    }
    finally {
        Remove-Item $tempOut, $tempErr -Force -ErrorAction SilentlyContinue
    }
}

try {
    Write-Diag "===== START LoadPfr scheduled task setup ====="
    Write-Diag "PowerShell version: $($PSVersionTable.PSVersion)"
    Write-Diag "Computer name: $env:COMPUTERNAME"
    Write-Diag "Current user: $env:USERDOMAIN\$env:USERNAME"
    Write-Diag "Running as admin: $(Test-Admin)"
    Write-Diag "Current location: $(Get-Location)"

    Show-PathState -Label "Source ZIP" -Path $sourceZip
    Show-PathState -Label "Destination root" -Path $destinationRoot
    Show-PathState -Label "Temp script dir" -Path $tempScriptDir

    if (-not (Test-Path $sourceZip)) {
        throw "Source ZIP file not found: $sourceZip"
    }

    if (-not (Test-Path $destinationRoot)) {
        Write-Diag "Creating destination root: $destinationRoot"
        New-Item -Path $destinationRoot -ItemType Directory -Force | Out-Null
    }

    if (-not (Test-Path $tempScriptDir)) {
        Write-Diag "Creating temp script dir: $tempScriptDir"
        New-Item -Path $tempScriptDir -ItemType Directory -Force | Out-Null
    }

    Write-Diag "Moving ZIP from '$sourceZip' to '$destinationZip'"
    Move-Item -Path $sourceZip -Destination $destinationZip -Force

    Show-PathState -Label "Moved ZIP" -Path $destinationZip

    if (Test-Path $extractTemp) {
        Write-Diag "Removing existing extract folder: $extractTemp"
        Remove-Item -Path $extractTemp -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Diag "Creating extract folder: $extractTemp"
    New-Item -Path $extractTemp -ItemType Directory -Force | Out-Null

    Write-Diag "Expanding archive '$destinationZip' to '$extractTemp'"
    Expand-Archive -Path $destinationZip -DestinationPath $extractTemp -Force

    Show-PathState -Label "Extract folder" -Path $extractTemp

    $extractedFiles = @(Get-ChildItem -Path $extractTemp -Recurse -File -ErrorAction SilentlyContinue)
    $hasFiles = (@($extractedFiles).Count -gt 0)

    Write-Diag "Extracted file count: $(@($extractedFiles).Count)"
    if (@($extractedFiles).Count -gt 0) {
        Write-Diag "First extracted files:"
        $extractedFiles | Select-Object -First 20 | ForEach-Object {
            Write-Host "         $($_.FullName)"
        }
    }

    if (-not $hasFiles) {
        throw "Extraction failed. '$extractTemp' does not contain extracted files."
    }

    Show-PathState -Label "Execute target" -Path $execute
    Show-PathState -Label "Working directory" -Path $workingDirectory

    if (-not (Test-Path $workingDirectory)) {
        throw "Working directory not found: $workingDirectory"
    }

    if (-not (Test-Path $execute)) {
        Write-DiagWarn "Expected batch file was not found at: $execute"
        Write-Diag "Searching extracted folder for LoadPfr.bat..."

        $foundBatFiles = @(Get-ChildItem -Path $extractTemp -Recurse -File -Filter "LoadPfr.bat" -ErrorAction SilentlyContinue)

        if (@($foundBatFiles).Count -gt 0) {
            Write-DiagWarn "Found LoadPfr.bat in unexpected location(s):"
            $foundBatFiles | ForEach-Object {
                Write-Host "         $($_.FullName)"
            }
        }
        else {
            Write-DiagWarn "No LoadPfr.bat file was found anywhere under: $extractTemp"
        }

        throw "Batch file not found at expected path: $execute"
    }

    $credential = Get-Credential -Message "Enter the account that should run scheduled task '$taskName' (Run whether user is logged on or not)."
    if ($null -eq $credential) {
        throw "No credentials were provided."
    }

    if ([string]::IsNullOrWhiteSpace($credential.UserName)) {
        throw "Credential user name was empty."
    }

    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($credential.Password)
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)

    if ([string]::IsNullOrEmpty($plainPassword)) {
        Write-DiagWarn "The supplied password appears to be empty."
    }

    Write-Diag "Task will run as user: $($credential.UserName)"
    Write-Diag "Security option: Run whether user is logged on or not"
    Write-Diag "Run level: HighestAvailable"

    $minutes = @(7, 21, 42)
    $triggers = @()

    foreach ($minute in $minutes) {
        $startBoundary = (Get-Date).Date.AddMinutes($minute).ToString("s")

        Write-Diag "Creating trigger for minute $minute with StartBoundary $startBoundary"

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
  <Principals>
    <Principal id="Author">
      <LogonType>Password</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
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

    Write-Diag "Writing task XML to: $tempXml"
    $taskXml | Out-File -FilePath $tempXml -Encoding Unicode

    Show-PathState -Label "Task XML" -Path $tempXml

    Write-Diag "Generated task XML contents:"
    Get-Content -Path $tempXml | ForEach-Object {
        Write-Host $_
    }

    $deleteResult = Invoke-Schtasks -OperationName "Delete existing task" -Arguments @(
        '/Delete'
        '/TN', $taskName
        '/F'
    )

    $createResult = Invoke-Schtasks -OperationName "Create task" -Arguments @(
        '/Create'
        '/TN', $taskName
        '/XML', $tempXml
        '/RU', $credential.UserName
        '/RP', $plainPassword
        '/F'
    )

    if ($createResult.ExitCode -ne 0) {
        Write-DiagError "Scheduled task creation failed."
        Write-DiagError "Possible causes include invalid credentials, missing 'Log on as a batch job', or a bad referenced path."
        throw "Scheduled task creation failed with exit code $($createResult.ExitCode). Review schtasks.exe output above."
    }

    Write-Diag "Scheduled task created successfully."

    $queryResult = Invoke-Schtasks -OperationName "Query created task" -Arguments @(
        '/Query'
        '/TN', $taskName
        '/V'
        '/FO', 'LIST'
    )

    if ($queryResult.ExitCode -ne 0) {
        Write-DiagWarn "Task was created, but query returned a non-zero exit code."
    }

    $success = $true

    Write-Diag "Preserving ZIP file for inspection: $destinationZip"
    Write-Diag "Preserving task XML for inspection: $tempXml"
    Write-Diag "===== SUCCESS ====="
}
catch {
    Write-DiagError "===== FAILURE ====="
    Write-DiagError "Exception type: $($_.Exception.GetType().FullName)"
    Write-DiagError "Exception message: $($_.Exception.Message)"

    if ($_.InvocationInfo) {
        Write-DiagError "Script line number: $($_.InvocationInfo.ScriptLineNumber)"
        Write-DiagError "Failing line: $($_.InvocationInfo.Line.Trim())"
    }

    Write-Diag "Diagnostic state at failure:"
    Show-PathState -Label "Source ZIP" -Path $sourceZip
    Show-PathState -Label "Destination ZIP" -Path $destinationZip
    Show-PathState -Label "Extract folder" -Path $extractTemp
    Show-PathState -Label "Execute target" -Path $execute
    Show-PathState -Label "Working directory" -Path $workingDirectory
    Show-PathState -Label "Task XML" -Path $tempXml

    if (Test-Path $tempXml) {
        Write-Diag "Task XML preserved at failure: $tempXml"
        Write-Diag "Task XML contents at failure:"
        Get-Content -Path $tempXml | ForEach-Object {
            Write-Host $_
        }
    }

    if (-not $success) {
        Write-DiagWarn "An error occurred. '$destinationZip' has been preserved for retry."
        Write-DiagWarn "The task XML has also been preserved at '$tempXml' for inspection."
    }

    throw
}
finally {
    if ($bstr -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }

    $plainPassword = $null
}
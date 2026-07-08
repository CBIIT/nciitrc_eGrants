# MoveFromShareToSuppDir.ps1
# Moves all files from network share to local SUPP directory.
# Exit code 0 = success, non-zero = failure.

$ErrorActionPreference = 'Stop'

# $Source      = '\\nciws-p2590-v.nci.nih.gov\egrants\scripts\PFRScript\OUT_Files\SUPP\prod'
$Source = '\\nciws-q2594-v\egrants\scripts\PFRScript\OUT_Files\SUPP\dev'
$Destination = 'C:\eGrants\SUPP_PFR'
$LogFile     = 'C:\egrants\watch\logs\Move-FromShareToSuppDir.log'

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = 'INFO'
    )
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $line = "$timestamp [$Level] $Message"
    Write-Output $line

    $logDir = Split-Path -Path $LogFile -Parent
    if (-not (Test-Path $logDir)) {
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
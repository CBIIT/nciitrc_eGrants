# StartOutlook

Utility application to start and initialize Microsoft Outlook for automated email processing tasks.

## Overview

The StartOutlook application:
- Starts Microsoft Outlook if not already running
- Connects to MAPI namespace and establishes session
- Verifies Outlook is properly configured
- Logs in to the default Outlook profile
- Ensures public folders are accessible
- Validates email processing prerequisites
- Provides health check for email automation

## Purpose

This utility is used to:
- **Pre-start Outlook** before scheduled email processing tasks
- **Verify Configuration** that Outlook is properly set up
- **Troubleshoot Issues** with COM automation
- **Health Checks** for automated email systems
- **Service Account Setup** for task scheduler jobs

## Features

### Outlook Initialization

- **COM Activation**: Creates Outlook.Application COM object
- **MAPI Login**: Connects to MAPI namespace
- **Profile Verification**: Validates default Outlook profile
- **Folder Access**: Checks public folder accessibility
- **Session Establishment**: Maintains persistent Outlook session

### Validation Checks

- Outlook installed and registered
- Default profile configured
- MAPI provider available
- Public folders accessible
- Service account permissions
- COM registration status

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "y",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "ProfileName": "",
    "CheckPublicFolders": "true",
    "TestFolderPath": "Public Folders - email@mail.nih.gov\\Test"
  }
}
```

### Configuration Parameters

- **Verbose**: Always "y" for startup diagnostics
- **LogDir**: Directory for log files
- **ProfileName**: Outlook profile name (empty for default)
- **CheckPublicFolders**: Verify public folder access
- **TestFolderPath**: Folder to test access (optional)

## Running

### Command Line

```bash
cd StartOutlook
dotnet run
```

Or:

```bash
StartOutlook.exe
```

### Task Scheduler

Run before email processing tasks:

1. **Trigger**: Before other email tasks
2. **Program**: `C:\eGrants\apps\StartOutlook\StartOutlook.exe`
3. **Start in**: `C:\eGrants\apps\StartOutlook\`

### As Startup Script

Add to Windows Startup folder or Group Policy for service accounts.

## Execution Workflow

1. **Check if Outlook Running**
   - Attempts to connect to existing instance
   - Falls back to creating new instance

2. **Create Outlook Application**
   - Uses COM automation
   - Late-bound activation
   - No PIA required

3. **Connect to MAPI Namespace**
   - Gets default MAPI namespace
   - Logs in with service account credentials

4. **Verify Configuration**
   - Checks default profile
   - Tests folder access
   - Validates permissions

5. **Log Results**
   - Success or failure
   - Diagnostic information
   - Performance metrics

6. **Keep Session Alive** (optional)
   - Maintains Outlook instance
   - Or exits cleanly

## Logging

Logs are written to the configured `LogDir`:
- File: `StartOutlook-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Outlook startup attempts
- COM object creation
- MAPI namespace connection
- Profile information
- Folder access tests
- Success/failure status
- Performance timings
- Error details

## Exit Codes

- **0**: Success - Outlook started and configured
- **1**: Failure - Outlook not installed
- **2**: Failure - COM registration error
- **3**: Failure - MAPI connection failed
- **4**: Failure - Profile not found
- **5**: Failure - Public folders inaccessible
- **99**: Unexpected error

## Troubleshooting

### Outlook Not Starting

1. Verify Outlook is installed
2. Check COM registration: `regsvr32 /s "C:\Program Files\Microsoft Office\root\Office16\ADDINS\OLMAPI32.DLL"`
3. Verify service account has Outlook license
4. Check Windows Event Viewer for errors

### MAPI Connection Failed

1. Verify default Outlook profile exists
2. Check profile is configured correctly
3. Test logging in manually as service account
4. Review Outlook profile settings

### Public Folders Not Accessible

1. Verify service account has public folder permissions
2. Check Exchange server connectivity
3. Test accessing folders manually
4. Review Exchange administrator settings

### COM Registration Errors

1. Repair Office installation
2. Re-register Outlook COM components
3. Check Windows registry for Office keys
4. Verify .NET Framework installed

## Service Account Setup

### Prerequisites

For running under Task Scheduler:

1. **Outlook Installed**: On the server
2. **Profile Configured**: For the service account
3. **License Assigned**: Outlook license for service account
4. **Permissions**: Public folder read/write access
5. **Interactive Login**: Service account must log in once to configure profile

### Configuration Steps

1. **Log in as service account** on the server
2. **Open Outlook** and configure profile
3. **Test public folder access** manually
4. **Close Outlook**
5. **Run StartOutlook.exe** to verify
6. **Configure Task Scheduler** with service account

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed)
- Microsoft Office (registered)
- MAPI provider
- Service account with Outlook access

## COM Automation

Uses COM late-binding:
```csharp
Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
dynamic outlook = Activator.CreateInstance(outlookType);
dynamic namespace = outlook.GetNamespace("MAPI");
namespace.Logon("", "", false, true);
```

## Use Cases

### Pre-Start for Email Processing

```batch
REM Start Outlook first
START /WAIT StartOutlook.exe
IF %ERRORLEVEL% NEQ 0 EXIT /B 1

REM Then run email processor
Router.exe
```

### Health Check Script

```powershell
# Check if Outlook automation is working
$result = & "C:\eGrants\apps\StartOutlook\StartOutlook.exe"
if ($LASTEXITCODE -eq 0) {
    Write-Host "Outlook is healthy"
} else {
    Write-Host "Outlook has issues"
    Send-MailMessage -To "admin@nih.gov" -Subject "Outlook Health Check Failed"
}
```

### Task Scheduler Dependency

Create two tasks:
1. **Task 1**: StartOutlook (runs first)
2. **Task 2**: Router (runs after Task 1 succeeds)

## Testing

### Manual Test

1. Close all Outlook instances
2. Run `StartOutlook.exe`
3. Check logs for success
4. Verify Outlook is running
5. Check Task Manager for OUTLOOK.EXE

### Automated Test

```bash
# Test startup
dotnet test --filter Category=OutlookStartup
```

## Performance

- **Startup Time**: 5-15 seconds
- **Memory Usage**: ~50 MB (Outlook process)
- **CPU Usage**: Minimal after startup
- **Network**: Required for Exchange connection

## Security Notes

- Runs with service account credentials
- No passwords stored in code
- Uses Windows integrated authentication
- COM objects properly released
- No sensitive data in logs

## Notes

- Must run with interactive session for first-time profile setup
- Outlook remains running after execution
- Can be run multiple times (idempotent)
- Recommended to run before scheduled email tasks
- Useful for troubleshooting COM automation issues
- Exit code indicates success/failure
- Verbose logging always enabled for diagnostics

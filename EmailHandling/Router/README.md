# Router

Email routing processor for the eGrants system that monitors an Outlook public folder and forwards emails based on subject line patterns.

## Overview

The Router application:
- Monitors a configured Outlook public folder for incoming emails
- Parses email subject lines to identify email types (FCOI, Public Access, JIT, RPPR, etc.)
- Routes emails to appropriate recipients based on business rules
- Forwards processed emails with modified subjects to designated email addresses
- Moves processed emails to an "Old emails" archive folder
- Logs all processing activity

## Migrated From

Original VBScript: `eMailRouter.vbs`

## Key Features

### Email Types Processed

1. **eSNAP/RPPR Notifications**
   - Non-compliance notifications
   - Forwarded to Bryan, Nicole, and Edward

2. **IC ACTION REQUIRED - Relinquishing Statement**
   - Forwarded to Emily, Dvellaj, and Edward

3. **Supplement Requests**
   - Forwarded to NCIOGASupplements

4. **FCOI (Financial Conflict of Interest)**
   - Extracts application ID from subject
   - Looks up SPEC officer emails from database
   - Forwards to NCIOGABOBTEAM1 and appropriate officers

5. **Public Access Compliance**
   - Parses grant numbers from subject
   - Categorizes as Compliant or Non-compliant
   - Forwards to eFile system for processing

6. **JIT (Just-In-Time) Requests**
   - JIT Request for Grant
   - JIT Documents Submitted
   - Forwarded to eFile system

7. **Progress Report Reminders**
   - Late Progress Report notifications
   - RPPR/IRPPR reminders
   - Forwarded to eFile system

8. **Closeout Reports**
   - Expiring Funds notifications
   - Past Due Documents reminders
   - F-RPPR Acceptance Past Due
   - Forwarded to eFile system

9. **SBIR/STTR Foreign Risk Management**
   - DCI-InTh Cleared/Not Cleared notifications
   - Forwarded to eFile system

10. **Other Notifications**
    - Prior Approval requests ? NCIGrantsPostAward
    - FFR NOTIFICATION : REJECTED ? eFile system
    - FRAM/PRAM requests ? eFile system
    - Change of Institution ? Dvellaj, Emily, Edward

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "DirPath": "Public Folders - egrantstest@mail.nih.gov\\All Public Folders\\NCIeGrants Test",
    "RoutingBreakDuration": "1000",
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **DirPath**: Outlook folder path to monitor (format: `Public Folders - email\path\to\folder`)
- **RoutingBreakDuration**: Milliseconds to wait between processing emails (default: 1000)
- **Verbose**: Set to "y" for detailed console output
- **Debug**: Set to "y" to route emails to debug recipients instead of production
- **LogDir**: Directory for log files

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Stored Procedures
- `sp_getOfficersEmailForGrantNum` - Gets SPEC officer emails for FCOI routing

### Functions
- `dbo.Imm_fn_applid_match()` - Matches grant numbers to application IDs

## Running

### Development

```bash
cd Router
dotnet run
```

### Production

```bash
Router.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Email Router"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every 15 minutes)
   - Or: At startup (with delay)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\Router\Router.exe`
   - Start in: `C:\eGrants\apps\Router\`

4. **Settings:**
   - Allow task to be run on demand
   - Stop task if it runs longer than: 1 hour
   - If task is already running: Do not start a new instance

## Email Recipient Configuration

### Debug Recipients (when debug=y)
- `leul.ayana@nih.gov`
- `eGrantsDev@mail.nih.gov`

### Production Recipients (when debug=n)
- `efile@mail.nih.gov`
- `eGrantsDev@mail.nih.gov`
- `eGrantsTest1@mail.nih.gov`
- `eGrantsStage@mail.nih.gov`

Plus specific recipients based on email type.

## Logging

Logs are written to the configured `LogDir` (from database connection config):
- File: `eMailRouter-Log-YYYY-MM-DD.txt`
- Format: `[Timestamp] - [Message] - [Error Details]`

## Error Handling

- **Failed Processing**: Errors are logged and admin is notified via email
- **Failed to Move**: If email can't be moved to "Old emails", admin is notified
- **Safety Limit**: Stops after processing 50 emails in one run (prevents duplicate processing)
- **COM Exceptions**: Attempts to restart ClickToRunSvc service

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database)
- Access to Outlook public folders

## Security Notes

- Application ID lookups use parameterized SQL queries
- Sender IDs are validated before processing
- Debug mode prevents accidental production routing during testing
- Grant numbers are sanitized before database lookups

## Troubleshooting

### Emails Not Being Processed

1. Verify Outlook is running and logged in
2. Check folder path in `config.csv` is correct
3. Verify service account has access to the public folder
4. Check logs for specific errors

### Emails Not Being Routed

1. Verify `debug` setting in config.csv
2. Check subject line patterns match expected formats
3. Review logs for pattern matching failures
4. Verify recipient email addresses are correct

### Database Connection Failures

1. Verify `DB_USER` and `DB_PASSWORD` environment variables are set
2. Verify connection string in appsettings.json uses `%DB_USER%` and `%DB_PASSWORD%` placeholders
3. Check service account has SELECT permissions
4. Verify stored procedures and functions exist
5. Test connection string separately

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS EMAILS** to real recipients. Testing must be done with extreme caution:
>
> - ? **ALWAYS** set `debug=y` in configuration during testing
> - ? **ALWAYS** verify recipient addresses before running
> - ? **NEVER** test against production Outlook folders with real customer emails
> - ? **USE** a dedicated test folder with test emails only
> - ? **VERIFY** debug recipients are set to test accounts only
> - ? **DO NOT** run in production environment without thorough testing
> - ? **DO NOT** process folders containing real customer correspondence
>
> **Consequences of improper testing:**
> - Customers may receive duplicate, incorrect, or test emails
> - Grant officers may be notified unnecessarily
> - Confidential information may be sent to wrong recipients
> - Loss of customer trust and potential compliance violations
>
> **Before testing:**
> 1. Set `debug=y` in config.csv
> 2. Verify `_dBugEmail` and `_eGrantsDevEmail` are test accounts
> 3. Use a test Outlook folder, never production folders
> 4. Create test emails that do NOT reference real grants or customers
> 5. Have a supervisor review your test plan

Use the `EmailTests` project to run unit and integration tests:

```bash
cd EmailTests
dotnet test
```

## Notes

- The application caches application IDs during processing to reduce database calls
- COM objects are properly released to prevent memory leaks
- Outlook instances are reused when possible
- The "Old emails" folder must exist in the monitored folder

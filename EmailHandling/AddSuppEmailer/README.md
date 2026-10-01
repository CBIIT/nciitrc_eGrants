# AddSuppEmailer

Administrative Supplement notification emailer that queries the database for pending supplement notifications and sends voting emails via Outlook.

## Overview

The AddSuppEmailer application:
- Queries the `adsup_Notification_email_status` table for pending notifications
- Retrieves email subject, body, and recipients from database functions
- Creates Outlook emails with voting buttons ("Accepted" / "Rejected")
- Sends high-importance HTML-formatted emails
- Updates database to mark notifications as sent
- Supports debug mode to prevent actual email sending during testing

## Migrated From

Original VBScript: `add_supp_prod.vbs`

## Key Features

### Voting Emails

Creates Outlook emails with:
- **Voting Options**: "Accepted;Rejected"
- **Importance**: High
- **Format**: HTML
- **Recipients**: Pulled from database functions

### Database-Driven Content

All email content is retrieved from the database:
- **Subject**: `dbo.fn_adsupp_getemail_subject(notification_id)`
- **Body**: `dbo.fn_adsupp_getemail_body(notification_id)`
- **TO Recipients**: `dbo.fn_adsupp_getemail_string(notification_id, 'TO')`
- **CC Recipients**: `dbo.fn_adsupp_getemail_string(notification_id, 'CC')`

### Environment-Aware Routing

In Development environment or when `debug=y`:
- Emails sent to: `leul.ayana@nih.gov` and `eGrantsDev@mail.nih.gov`

In Production:
- Emails sent to recipients from database

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
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

- **Verbose**: Set to "y" for detailed console output
- **Debug**: Set to "y" to prevent actual email sending (logs only)
- **LogDir**: Directory for log files

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

### Environment Detection

Set environment variable to control recipient routing:
```
DOTNET_ENVIRONMENT=Development
```
Or:
```
ASPNETCORE_ENVIRONMENT=Development
```

## Database Dependencies

### Tables
- `dbo.adsup_Notification_email_status` - Notification queue and status tracking
  - Columns: `Notification_id`, `email_date`, `email_send_status`

### Functions
- `dbo.fn_adsupp_getemail_subject(notification_id)` - Returns email subject
- `dbo.fn_adsupp_getemail_body(notification_id)` - Returns HTML email body
- `dbo.fn_adsupp_getemail_string(notification_id, 'TO')` - Returns TO recipients
- `dbo.fn_adsupp_getemail_string(notification_id, 'CC')` - Returns CC recipients

## Running

### Development

```bash
cd AddSuppEmailer
dotnet run
```

### Production

```bash
AddSuppEmailer.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Admin Supplement Emailer"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., daily at specific time)
   - Or: On demand (manual trigger)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\AddSuppEmailer\AddSuppEmailer.exe`
   - Start in: `C:\eGrants\apps\AddSuppEmailer\`

4. **Settings:**
   - Allow task to be run on demand
   - Stop task if it runs longer than: 30 minutes

## Email Format

### Example Email

**To**: Recipients from database
**CC**: Recipients from database
**Subject**: "[Subject from database]"
**Importance**: High
**Voting Options**: Accepted;Rejected
**Body**: 
```
[HTML body from database]

Notification Id=12345
```

## Logging

Logs are written to the configured `LogDir`:
- File: `AddSuppEmailer-YYYY-MM-DD.log`
- Uses Serilog for structured logging
- Legacy log file: `AddSuppEmailer-Log-YYYY-MM-DD.txt`

### Log Levels
- **Information**: Processing start/complete, emails sent
- **Debug**: Outlook initialization, notification processing, debug mode skips
- **Error**: Processing failures with full exception details

## Error Handling

- **Processing Errors**: Logged with notification ID and error details
- **Outlook Failures**: Logged if Outlook.Application COM class not found
- **Database Errors**: Logged if functions return null or fail
- **COM Exceptions**: Properly handled and logged

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database with supplement notification tables and functions)

## COM Automation

Uses late-bound COM automation for Outlook:
- No Primary Interop Assembly (PIA) required at compile time
- Dynamic object creation via `Activator.CreateInstance`
- Attempts to connect to existing Outlook instance before creating new one
- Proper COM object cleanup via `Marshal.ReleaseComObject`

## Outlook Constants Used

```csharp
// CreateItem types
olMailItem = 0

// Importance levels
olImportanceHigh = 2

// Body formats
olFormatHTML = 2
```

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS VOTING EMAILS** with high importance to real recipients. Improper testing can result in:
>
> - **Mass emailing** grant officers and administrators with test/duplicate voting requests
> - **Confusion** about which voting emails are real vs. test
> - **Compliance issues** if test votes are recorded as real votes
> - **Loss of trust** from recipients who receive invalid voting requests
>
> **REQUIRED before ANY testing:**
>
> 1. ? **Set `debug=y`** in appsettings.json - This prevents actual email sending
> 2. ? **Set `DOTNET_ENVIRONMENT=Development`** - This redirects emails to test accounts
> 3. ? **Verify test notification records** in database have test data only
> 4. ? **Clear test notifications** from database after testing
> 5. ? **Never test with production notification IDs**
> 6. ? **Coordinate with team** before any production testing
>
> **Debug mode (SAFEST - no emails sent):**
> - Set `debug=y` to log only, no emails sent, no database updates
> - Use this for development and initial testing
>
> **Development mode (emails sent to test accounts only):**
> - Set `DOTNET_ENVIRONMENT=Development` to redirect to test recipients
> - Use this only after debug testing is complete
> - Still send real emails, but only to designated test accounts
>
> **Production mode (DANGEROUS):**
> - Only run in production after extensive testing
> - Requires approval from supervisor
> - Emails sent to real grant officers and administrators

### Debug Mode Testing

Set `debug=y` in appsettings.json:
```json
{
  "AppSettings": {
    "Debug": "y"
  }
}
```

This will:
- Log what emails would be sent
- Show recipient information
- NOT actually send emails
- NOT update database status

### Development Environment Testing

Set environment variable:
```
DOTNET_ENVIRONMENT=Development
```

This will:
- Send emails to `leul.ayana@nih.gov` and `eGrantsDev@mail.nih.gov`
- Prevent sending to production recipients
- Actually send emails (unless debug=y)
- Update database status normally

## Troubleshooting

### Emails Not Being Sent

1. Verify Outlook is installed and configured
2. Check service account has Outlook access
3. Verify `debug` setting is "n" for production
4. Check environment variables (DOTNET_ENVIRONMENT)
5. Review logs for specific errors

### No Notifications Found

1. Check `adsup_Notification_email_status` table for pending records
2. Verify `email_date IS NULL` for unsent notifications
3. Check database functions return valid data

### Voting Options Not Working

1. Verify Outlook version supports voting buttons
2. Check recipients have compatible email clients
3. Test voting manually in Outlook

### Database Update Failures

1. Verify service account has UPDATE permissions
2. Check notification_id exists in table
3. Review stored procedure/function permissions

## Security Notes

- SQL queries use parameterized inputs (for update operations)
- SQL string interpolation used for function calls (consider parameterization)
- Environment-aware recipient routing prevents accidental production emails
- Debug mode provides safe testing

## Performance Notes

- Processes notifications sequentially (not parallel)
- Reuses Outlook instance across all notifications
- DataReader closed before processing to avoid connection blocking
- No transaction management (each notification independent)

## Notes

- Notification ID is appended to email body for tracking
- Voting responses are collected by Outlook (not this application)
- Email subject and body formatting controlled entirely by database
- No limit on number of notifications processed per run
- COM objects properly released to prevent memory leaks

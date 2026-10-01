# AddSuppProd

Administrative Supplement production processor that monitors a specific Outlook public folder for supplement request emails and processes them by extracting application information and saving attachments.

## Overview

The AddSuppProd application:
- Monitors a configured Outlook public folder for supplement request emails
- Extracts application ID from email body text
- Validates supplement requests against database criteria
- Saves email attachments to designated directories
- Inserts supplement request records into database
- Moves processed emails to "old" archive folder
- Sends notification emails for successful processing

## Migrated From

Original VBScript: `add_supp_prod.vbs`

## Key Features

### Email Processing Workflow

1. **Connect to Outlook** and open configured public folder
2. **Scan emails** in the folder
3. **Extract Application ID** from email body (looks for 7-8 digit numbers)
4. **Validate request** via database query
5. **Save attachments** to application-specific directory
6. **Insert database record** for the supplement request
7. **Send notification** to administrators
8. **Move email** to "old" folder

### Application ID Extraction

Searches email body for patterns like:
- `Application ID: 12345678`
- `APPLID: 12345678`
- `App ID 12345678`
- Any 7-8 digit number in the email

### Attachment Handling

- Saves attachments to: `OutDir\{ApplicationID}\{Filename}`
- Creates directory if it doesn't exist
- Preserves original filenames
- Skips files starting with "ATT" prefix

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "OutDir": "C:\\eGrants\\data\\supplements\\",
    "AdminRecipients": "egrantsdevs@mail.nih.gov",
    "DirPath": "Public Folders - email@mail.nih.gov\\Supplements"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **OutDir**: Base directory for saving supplement attachments
- **AdminRecipients**: Email recipients for notifications
- **DirPath**: Outlook folder path to monitor
- **Verbose**: Set to "y" for detailed console output
- **Debug**: Set to "y" to skip database inserts and file operations

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Queries
- Validates application IDs exist in the system
- Checks for existing supplement requests
- Verifies application is eligible for supplements

### Insert Operations
- Creates supplement request records
- Links attachments to applications
- Tracks processing timestamps

## Running

### Development

```bash
cd AddSuppProd
dotnet run
```

### Production

```bash
AddSuppProd.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Admin Supplement Production Processor"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every 30 minutes)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\AddSuppProd\AddSuppProd.exe`
   - Start in: `C:\eGrants\apps\AddSuppProd\`

4. **Settings:**
   - Stop task if it runs longer than: 1 hour
   - If task is already running: Do not start a new instance

## Notification Emails

### Success Notification

Sent when supplement request is successfully processed:
- **To**: Admin recipients
- **Subject**: "Admin Supplement Request Processed"
- **Body**: Application ID, request date, attachment count

### Error Notification

Sent when processing fails:
- **To**: Admin recipients
- **Subject**: "Admin Supplement Processing Error"
- **Body**: Error details, application ID (if found), email subject

## Logging

Logs are written to the configured `LogDir`:
- File: `AddSuppProd-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Outlook connection established
- Emails found for processing
- Application ID extracted
- Attachments saved
- Database records inserted
- Emails moved to archive
- Errors and exceptions

## Error Handling

- **No Application ID Found**: Email skipped, logged as warning
- **Invalid Application ID**: Email skipped, admin notified
- **Database Errors**: Logged, admin notified, email not moved
- **Attachment Save Failures**: Logged, processing continues
- **Move Failures**: Logged, admin notified

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database)
- File system write access to OutDir

## COM Automation

Uses late-bound COM automation for Outlook:
- Dynamic object creation
- No Primary Interop Assembly (PIA) required
- Proper COM object cleanup

## Troubleshooting

### Emails Not Being Processed

1. Verify Outlook is running and logged in
2. Check folder path in appsettings.json
3. Verify service account has folder access
4. Review logs for specific errors

### Application ID Not Found

1. Check email body format
2. Verify application ID is present in email
3. Review extraction logic in logs
4. Test with known good email

### Attachments Not Saving

1. Verify OutDir exists and is writable
2. Check service account permissions
3. Verify disk space available
4. Review logs for I/O errors

### Database Insert Failures

1. Check database connection string
2. Verify service account has INSERT permissions
3. Review table schema requirements
4. Check for duplicate key violations

## Security Notes

- Application IDs validated against database
- File paths sanitized before saving
- SQL queries use parameterized inputs
- Admin notifications prevent sensitive data exposure

## Performance Notes

- Processes emails sequentially
- Creates directories on demand
- No batch processing (each email independent)
- COM objects released after each email

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS NOTIFICATION EMAILS** to administrators about supplement processing.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending notifications during testing
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test Outlook folders with synthetic test emails
> - ? Never test with folders containing real supplement requests
> - ? DO NOT process real supplement request emails during testing
> - ? DO NOT use production administrator addresses
>
> **Consequences of improper testing:**
> - Administrators may receive false notifications about supplement requests
> - Real supplement requests may be processed incorrectly
> - Test attachments may be saved to production directories
> - Database may contain invalid test supplement records

### Debug Mode Testing

Set `debug=y` in appsettings.json to:
- Skip database inserts
- Skip file saves
- Log all operations
- Process emails normally otherwise

### Test Email Format

```
Subject: Admin Supplement Request for Grant 5R01CA258784

Body:
Application ID: 12345678
Requesting administrative supplement for...
```

Attach files to test attachment processing.

## Notes

- The "old" subfolder must exist in the monitored folder
- Only processes emails with valid application IDs
- Notification emails sent via Outlook (not SMTP)
- Multiple attachments per email are supported
- Directory structure: `OutDir\ApplicationID\Filename`

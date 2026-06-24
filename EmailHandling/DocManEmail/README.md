# DocManEmail

> **?? DEPRECATED:** This job is no longer running in production and is **excluded from the Task Scheduler migration**. This code is retained for reference only.

Document Management email processor that monitors Outlook folders for document submission emails and processes them into the eGrants document management system.

## Overview

The DocManEmail application:
- Monitors configured Outlook public folders for document emails
- Extracts metadata from email subject lines and body
- Saves email content and attachments to file system
- Inserts document records into eGrants database
- Supports multiple document categories and types
- Moves processed emails to archive folders
- Sends error notifications to administrators

## Migrated From

Original VBScript: `DocMan_email_2008_Prod.vbs`

## Key Features

### Document Categories Supported

- **Correspondence**: General correspondence documents
- **Budget**: Budget-related documents
- **Progress Reports**: RPPRs and progress reports
- **Closeout**: Closeout documentation
- **Compliance**: Compliance-related documents
- **Notifications**: System notifications
- **And more...**

### Metadata Extraction

Extracts from email subject:
- Document category
- Sub-category
- Application ID
- Grant number
- Document date
- Special flags

### Content Processing

1. **Email Body**: Saved as .txt file with headers
2. **Attachments**: Saved individually with metadata
3. **Database Records**: Created for each document
4. **File Naming**: Uses database-generated document IDs

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "OutDir": "C:\\eGrants\\data\\documents\\",
    "AdminRecipients": "egrantsdevs@mail.nih.gov",
    "DirPath": "Public Folders - email@mail.nih.gov\\Documents"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **OutDir**: Directory where documents are saved
- **AdminRecipients**: Email recipients for error notifications
- **DirPath**: Outlook folder path to monitor
- **Verbose**: Set to "y" for detailed logging
- **Debug**: Set to "y" to skip database operations

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Tables
- Document repository tables
- Document metadata tables
- Application linkage tables

### Stored Procedures
- `SP_CREATE_EGRANTS_DOCUMENT` - Creates document records
- `SP_LINK_DOCUMENT_TO_APPLICATION` - Links documents to applications

### Functions
- `dbo.Imm_fn_applid_match()` - Matches grant numbers to application IDs

## Running

### Development

```bash
cd DocManEmail
dotnet run
```

### Production

```bash
DocManEmail.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Document Management Email Processor"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every 15 minutes)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\DocManEmail\DocManEmail.exe`
   - Start in: `C:\eGrants\apps\DocManEmail\`

4. **Settings:**
   - Stop task if it runs longer than: 2 hours
   - If task is already running: Do not start a new instance

## Email Format Requirements

### Subject Line Format

```
category=Correspondence, sub=General, applid=12345678, Original Subject
```

### Supported Subject Parameters

- `category`: Document category
- `sub`: Sub-category
- `applid`: Application ID (direct)
- `grantnumber`: Grant number (for lookup)
- `documentdate`: Document date
- `documentid`: Existing document ID (for updates)

## Document Processing Workflow

1. **Email Retrieved** from Outlook folder
2. **Subject Parsed** for metadata
3. **Application ID Resolved** (direct or via grant number lookup)
4. **Document Record Created** in database
5. **Email Body Saved** as text file (if applicable)
6. **Attachments Saved** individually (if applicable)
7. **Email Moved** to "old" archive folder
8. **Notifications Sent** (if errors occur)

## Logging

Logs are written to the configured `LogDir`:
- File: `DocManEmail-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Folder connection status
- Emails processed count
- Document IDs generated
- File save operations
- Database insert operations
- Errors and exceptions

## Error Handling

- **Missing Metadata**: Email skipped, logged as warning
- **Invalid Application ID**: Email skipped, admin notified
- **Database Errors**: Logged, admin notified via email
- **File Save Failures**: Logged, processing continues
- **Move Failures**: Logged, admin notified

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database)
- File system write access to OutDir

## COM Automation

Uses Outlook COM automation for:
- Folder access and navigation
- Email reading and parsing
- Attachment extraction
- Email archiving (move to "old")

## Troubleshooting

### Emails Not Being Processed

1. Verify Outlook is installed and logged in
2. Check folder path configuration
3. Verify service account has folder permissions
4. Review logs for connection errors

### Documents Not Being Saved

1. Check OutDir exists and is writable
2. Verify service account permissions
3. Check disk space
4. Review logs for I/O errors

### Database Insert Failures

1. Verify connection string
2. Check stored procedures exist
3. Verify service account has EXECUTE permissions
4. Review parameter types and values

### Application ID Not Found

1. Verify grant number format
2. Check `Imm_fn_applid_match` function
3. Review email subject parsing
4. Test with known valid grant numbers

## Security Notes

- SQL queries use parameterized inputs
- File names sanitized before saving
- Email content sanitized before database insert
- Grant numbers validated against database

## Performance Notes

- Processes emails sequentially
- COM objects properly released
- Database connections managed per email
- File I/O operations single-threaded

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS ERROR NOTIFICATION EMAILS** to administrators.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent error notifications during testing
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test Outlook folders, never production document submission folders
> - ? Test emails should contain only synthetic test documents
> - ? DO NOT test with folders containing real customer documents
> - ? DO NOT process real document submission emails during testing
>
> **Before testing:**
> 1. Configure test-only administrator email addresses
> 2. Use dedicated test folder in Outlook
> 3. Create test emails with fake grant numbers and test documents
> 4. Verify no real customer data will be processed

### Debug Mode

Set `debug=y` to:
- Skip database inserts
- Skip file writes
- Log all operations
- Process parsing logic normally

### Test Email

```
To: docman@email.nih.gov
Subject: category=Correspondence, sub=Test, applid=12345678, Test Document
Body: This is a test document.
Attachments: test.pdf
```

## Notes

- The "old" subfolder must exist in monitored folder
- Zero-byte files are rejected
- Document dates default to email received time
- Multiple attachments per email supported
- File naming uses database-generated document IDs

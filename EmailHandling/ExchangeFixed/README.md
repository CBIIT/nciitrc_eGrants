# ExchangeFixed

Email processor for eGrants document management system that processes emails with structured metadata in subject lines and files them into the document management system.

## Overview

The ExchangeFixed application:
- Monitors a configured Outlook public folder for emails with structured subject lines
- Parses comma-delimited metadata from email subjects (category, applid, extract mode, etc.)
- Files email content and/or attachments into the eGrants document management system
- Generates PDFs from emails using Word or Acrobat SDK for specific categories
- Moves processed emails to an "old" archive subfolder
- Supports special processing for Public Access, JIT, CT.gov, and other categories

## Migrated From

Original VBScript: `exchange_Fixed.vbs` (also known as `exchange_latest.vbs`)

## Key Features

### Subject Line Parsing

Emails must have comma-delimited key=value pairs in the subject:

```
category=PublicAccess, sub=Compliant, applid=12345678, extract=1, documentdate=1/15/2024, Original Subject
```

**Supported Parameters:**
- `grantnumber`: Grant number (used to look up applid via `Imm_fn_applid_match`)
- `applid`: Application ID (takes precedence over grant number)
- `category`: Document category (e.g., "Correspondence", "Budget", "PublicAccess")
- `sub`: Sub-category for finer classification
- `extract`: Content extraction mode (1=body, 2=attachments, 3=both)
- `documentdate`: Document date (defaults to email received time)
- `documentid`: Existing document ID for update scenarios

### Extract Modes

1. **Extract=1 (Body Only)**
   - Standard: Writes email headers + body to .txt file
   - PublicAccess: Generates merged PDF (subject + body + attachments)
   - JIT Info, CT.gov: Generates PDF with embedded images
   - Closeout, eRA Notification/JIT Submitted: PDF with embedded images
   - Funding/dci-inth: PDF with embedded images

2. **Extract=2 (Attachments Only)**
   - Saves all attachments individually
   - Skips files with names starting with "ATT"
   - Each attachment gets its own document ID

3. **Extract=3 (Body and Attachments)**
   - Combines extract=1 and extract=2
   - Body saved first, then all attachments

### Special Category Processing

**PublicAccess:**
- Generates comprehensive PDF using Acrobat SDK
- Merges subject header + email body + all attachments
- Outputs single consolidated PDF

**JIT Info / CT.gov / Closeout:**
- Generates PDF via Word automation
- Preserves embedded images
- Sets narrow margins (0.25 inches)

**eRA Notification (JIT Submitted):**
- Clears old JIT submissions via `SP_CLEAR_OLD_JIT_SUBMISSIONS`
- Generates PDF with embedded images

**Funding (DCI-InTh):**
- PDF generation with embedded images
- Special handling for SBIR/STTR risk assessments

**NCIOGAPROGESS Sender:**
- Auto-categorizes as "Notification" / "Late Progress Report"
- Sends notification email

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "OutDir": "C:\\eGrants\\data\\",
    "PublicAccessBackup": "C:\\eGrants\\temp\\publicaccess\\",
    "AdminRecipients": "egrantsdevs@mail.nih.gov",
    "DirPath": "Public Folders - email@mail.nih.gov\\path\\to\\folder"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

### Configuration Parameters

- **OutDir**: Directory where processed files are saved
- **PublicAccessBackup**: Working directory for PDF generation (for PublicAccess category)
- **AdminRecipients**: Email recipients for error notifications
- **DirPath**: Outlook folder path to monitor

## Database Dependencies

### Stored Procedures
- `SP_CREATE_EGRANTS_DOCUMENT_NEW` - Registers document in eGrants
- `SP_CLEAR_OLD_JIT_SUBMISSIONS` - Clears old JIT submission records

### Functions
- `dbo.Imm_fn_applid_match()` - Matches grant numbers to application IDs

## Running

### Development

```bash
cd ExchangeFixed
dotnet run
```

### Production

```bash
ExchangeFixed.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Exchange Fixed Processor"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every 30 minutes)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\ExchangeFixed\ExchangeFixed.exe`
   - Start in: `C:\eGrants\apps\ExchangeFixed\`

4. **Settings:**
   - Stop task if it runs longer than: 2 hours
   - If task is already running: Do not start a new instance

## Sender Identification

### Exchange (EX) Senders
- Resolves Exchange alias via `GetExchangeUser()`
- Falls back to extracting alias from EX address

### SMTP Senders
- Uses raw SMTP email address

### Special Sender Handling
- `FD6862D09E7043D49596358F980D064F-NCI OGA PRO` ? Auto-categorized as "NCIOGAPROGESS"

## QC (Quality Control) Flagging

Files are flagged for QC review when:
- Non-standard file extensions (not pdf, txt, doc, xls, docx, xlsx, ppt)
- No application ID found
- Set `movetoqc="yes"` in database

## Logging

Logs are written to the configured `LogDir`:
- File: `ExchangeFixed-YYYY-MM-DD.log`
- Uses Serilog for structured logging
- Legacy log format for backward compatibility

## Error Handling

- **Processing Errors**: Logged and admin is notified via email
- **Database Errors**: Document ID not found warnings
- **PDF Generation Failures**: Logged with specific error details
- **Safety Limit**: Stops after processing 30 emails in one run

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- Microsoft Word (for PDF generation with embedded images)
- Adobe Acrobat SDK (for PublicAccess PDF merging) - **Optional**
- SQL Server (EIM database)

## COM Automation

Uses late-bound COM automation for:
- **Outlook**: Email access and folder navigation
- **Word**: PDF generation with embedded images
- **Acrobat SDK**: PDF merging (PublicAccess category only)

**Note:** Acrobat SDK requirement can cause failures if not properly registered. Consider replacing with modern PDF libraries (PdfSharp, iTextSharp) for better reliability.

## Troubleshooting

### PDF Generation Failures

**Acrobat SDK Error (0x80004002):**
- Adobe Acrobat Pro not installed or not properly registered
- Solution: Install Adobe Acrobat Pro and register COM components
- Alternative: Disable PublicAccess processing or implement modern PDF library

**Word Automation Failures:**
- Verify Word is installed
- Check service account has Word automation permissions
- Ensure no Word processes are hanging

### Files Not Being Saved

1. Verify `OutDir` exists and is writable
2. Check service account permissions
3. Review logs for specific I/O errors
4. Verify disk space available

### Database Insert Failures

1. Check `SP_CREATE_EGRANTS_DOCUMENT_NEW` stored procedure exists
2. Verify service account has EXECUTE permissions
3. Review stored procedure parameters match expected format

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS NOTIFICATION EMAILS** to administrators when errors occur.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending error notifications during testing
> - ? Use test Outlook folders, never production folders with real emails
> - ? Verify `AdminRecipients` points to test accounts only
> - ? Create test emails that do NOT reference real grants or contain real customer data
> - ? DO NOT test with folders containing real customer correspondence
> - ? DO NOT use production administrator email addresses during testing
>
> **Before testing:**
> 1. Set `AdminRecipients` to test email addresses only
> 2. Set `debug=y` if you want to skip email notifications
> 3. Use a dedicated test folder structure
> 4. Ensure test emails contain only synthetic test data

Use the `EmailTests` project to run unit and integration tests:

```bash
cd EmailTests
dotnet test
```

## Security Notes

- Application ID lookups use parameterized SQL queries
- File names are sanitized before saving
- Attachment names starting with "ATT" are skipped (security measure)
- Grant numbers are sanitized before database lookups

## Performance Optimization

- COM objects are properly released after use
- 1-second delay between items to ensure file handles are released
- Fresh item references obtained before moving to old folder
- Application ID lookups could be cached for better performance

## Notes

- The "old" subfolder must exist in the monitored folder
- Zero-byte files are rejected
- Document dates default to email received time if not specified
- URL format: `/data/funded/egrantsadmin/auditreport/{filename}`
- Network UNC paths may fail due to FIPS or network restrictions

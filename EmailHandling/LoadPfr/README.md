# LoadPfr

Progress and Final Report (PFR) loader that processes XML metadata files and imports associated PDF documents into the eGrants document management system.

## Overview

The LoadPfr application:
- Monitors a source directory for PFR XML metadata files
- Parses XML files containing document metadata (applid, filename, date, file type, creator)
- Calls Create_PFR stored procedure to register documents in the database
- Copies PDF files to final destination with assigned file numbers
- Archives both XML and PDF files to backup directory
- Sends email notifications for successful processing and errors
- Supports batch processing of multiple files

## Migrated From

**Original VBScript:** `Load_PFR.vbs` (created by Imran Omair, 8/26/2016)

**Migration Status:** ? **100% Feature Complete** - See [VBSCRIPT_COVERAGE_ANALYSIS.md](./VBSCRIPT_COVERAGE_ANALYSIS.md) for detailed comparison

## Key Features

### XML Metadata Processing

- **Source Scanning**: Monitors configured directory for `.xml` metadata files
- **XML Parsing**: Extracts document metadata (applid, folderid, filename, date, file_type, uid)
- **Special Logic**: Sets catname="PFR" when folderid="19"
- **Data Validation**: Verifies PDF files exist before processing
- **Batch Processing**: Handles multiple XML files with multiple records each

### Document Management

- **Database Registration**: Calls `Create_PFR` stored procedure to create document entries
- **File Numbering**: Retrieves assigned file numbers from database (ABC column)
- **PDF Processing**: Copies PDFs to watch folder with new file number names
- **Archival**: Backs up both XML metadata and original PDF files
- **Error Recovery**: Continues processing remaining files if one fails

### Email Notifications

- **Success Notifications**: Sends list of all processed applids
- **Error Alerts**: 
  - PDF file not found errors
  - Database errors (Create_PFR returning no data)
- **Configurable Recipients**: To and CC addresses in appsettings.json
- **Environment Labels**: DEV/PROD prefixes in email subjects

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "LogDir": "C:\\egrants\\apps\\log\\"
  },
  "ConnectionStrings": {
    "EIM": "Password=%DB_PASSWORD%;Persist Security Info=True;User ID=%DB_USER%;Initial Catalog=EIM;Data Source=NCIDB-D387-V.nci.nih.gov\\MSSQLEGRANTSQ,52000;Application Name=egrants"
  },
  "PfrPaths": {
    "DocSrcPath": "C:\\eGrants\\pfr\\source\\",
    "BakDstPath": "C:\\eGrants\\pfr\\backup\\",
    "FinalDstPath": "C:\\eGrants\\watch\\out\\"
  },
  "EmailSettings": {
    "Enabled": "true",
    "ToRecipients": "guillermo.choy-leon@nih.gov;leul.ayana@nih.gov",
    "CcRecipients": "leul.ayana@nih.gov",
    "Environment": "DEV"
  }
}
```

### Configuration Parameters

- **DocSrcPath**: Directory containing XML metadata and PDF files
- **BakDstPath**: Archive directory for processed files
- **FinalDstPath**: Destination directory for renamed PDFs (typically watch folder)
- **LogDir**: Directory for daily log files (PFR-Log-{date}.txt)
- **Verbose**: Set to "y" for detailed console diagnostics
- **DB_USER / DB_PASSWORD**: Environment variables for database credentials
- **EmailSettings:Enabled**: Set to "false" to disable email notifications
- **EmailSettings:Environment**: "DEV" or "PROD" for email subject prefixes

## XML File Format

```xml
<root>
  <record>
    <APPLID>12345</APPLID>
    <FOLDERID>19</FOLDERID>
    <FILENAME>report.pdf</FILENAME>
    <DATE>1/15/2024</DATE>
    <FILE_TYPE>pdf</FILE_TYPE>
    <UID>username</UID>
  </record>
</root>
```

## Database Dependencies

### Stored Procedures
- **Create_PFR** - Registers PFR document and returns file number
  - Parameters: @APPLID, @Rcvd_dt, @Catname, @filetype, @CreatedBy
  - Returns: ABC column (file number name)

## Running

### Development

```bash
cd LoadPfr
dotnet run
```

### Production

Typically run as a Windows Scheduled Task:
```
dotnet C:\eGrants\apps\LoadPfr\LoadPfr.dll
```

## Environment Variables

Set these environment variables before running:
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

PowerShell example:
```powershell
[System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', 'User')
[System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', 'User')
```

## Logging

Daily log files: `C:\egrants\apps\log\PFR-Log-{yyyy-M-d}.txt`

Log entries include:
- Task start/completion timestamps
- File processing details
- Database procedure calls
- File operations (copy, move)
- Errors with stack traces
- Email notifications sent

## Email Notification Examples

### Success Notification
- **Subject**: `DEV=>>PFR Processed` (or `PROD=>>PFR Processed`)
- **Body**: `12345   67890   11111` (space-separated applids)

### PDF Not Found Error
- **Subject**: `ERROR=> PDF NOT FOUND`
- **Body**: `PDF SOURCE=C:\eGrants\pfr\source\missing.pdf`

### Database Error
- **Subject**: `DEV=>> ERROR: Could not create PFR in DB using Create_PFR`
- **Body**: `Could not create PFR in DB using Create_PFR`

## Troubleshooting

### No PDFs processed
- Check DocSrcPath directory exists and contains XML files
- Verify XML files reference existing PDF files
- Check database connection (DB_USER, DB_PASSWORD environment variables)
- Review log file for errors

### Emails not sending
- Verify Outlook is installed and configured
- Check EmailSettings:Enabled is "true" in appsettings.json
- Confirm recipient addresses are correct
- Review log for email send errors

### Database errors
- Verify Create_PFR stored procedure exists in database
- Check database user has execute permissions
- Ensure connection string is correct
- Review SQL Server logs

## See Also

- [VBSCRIPT_COVERAGE_ANALYSIS.md](./VBSCRIPT_COVERAGE_ANALYSIS.md) - Complete VBScript migration analysis
- [LoadSuppPfr](../LoadSuppPfr/README.md) - Similar project for supplement PFRs

### Production

```bash
LoadPfr.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants PFR Loader"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., daily at 2:00 AM)
   - Or: On file arrival (advanced trigger)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\LoadPfr\LoadPfr.exe`
   - Start in: `C:\eGrants\apps\LoadPfr\`

4. **Settings:**
   - Stop task if it runs longer than: 4 hours
   - If task is already running: Queue a new instance

## XML File Format

### Expected XML Structure

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ProgressReport>
  <GrantNumber>5R01CA258784-04</GrantNumber>
  <ReportType>RPPR</ReportType>
  <ReportPeriodStart>2023-01-01</ReportPeriodStart>
  <ReportPeriodEnd>2023-12-31</ReportPeriodEnd>
  <PI>
    <FirstName>John</FirstName>
    <LastName>Smith</LastName>
  </PI>
  <Institution>
    <Name>University Example</Name>
    <DUNS>123456789</DUNS>
  </Institution>
  <Narrative>...</Narrative>
</ProgressReport>
```

## Processing Workflow

1. **Scan Directory** for XML files
2. **Parse XML** to extract metadata
3. **Validate Data** against business rules
4. **Lookup Application ID** from grant number
5. **Check for Duplicates** in database
6. **Import Data** via stored procedures
7. **Move File** to archive or error directory
8. **Send Notifications** for results
9. **Log Results** to file and console

## Logging

Logs are written to the configured `LogDir`:
- File: `LoadPfr-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Source directory scan results
- XML files found count
- Parsing success/failures
- Database import results
- File move operations
- Validation errors
- Processing summary

## Error Handling

### File-Level Errors
- **Invalid XML**: Moved to ErrorDir, logged
- **Missing Required Fields**: Moved to ErrorDir, admin notified
- **Duplicate Reports**: Skipped, logged as warning

### Processing Errors
- **Database Errors**: File moved to ErrorDir, admin notified
- **Validation Failures**: File moved to ErrorDir, details logged
- **Parse Errors**: File moved to ErrorDir, XML structure logged

### System Errors
- **Directory Access**: Logged, processing stops
- **Database Connection**: Logged, admin notified, retried
- **File Lock**: Skipped, logged, retried on next run

## Notification Emails

### Success Summary

Sent after processing completes:
- Files processed count
- Files succeeded count
- Files failed count
- Processing duration

### Error Details

Sent when errors occur:
- Failed file names
- Error messages
- Stack traces
- Recommended actions

## Dependencies

- .NET 8.0
- SQL Server (EIM database)
- XML parsing libraries (System.Xml)
- File system access to source and archive directories

## Troubleshooting

### Files Not Being Processed

1. Verify SourceDir exists and is accessible
2. Check file permissions
3. Verify files are .xml extension
4. Check for file locks
5. Review logs for scan errors

### XML Parsing Failures

1. Validate XML structure against schema
2. Check for special characters
3. Verify encoding (UTF-8 expected)
4. Test XML with validator tool

### Database Import Failures

1. Check stored procedures exist
2. Verify service account has EXECUTE permissions
3. Review parameter types and values
4. Check for constraint violations
5. Verify application ID exists

### Duplicate Report Errors

1. Check report period dates
2. Verify grant number
3. Review business rules for duplicates
4. Check database for existing records

## Security Notes

- XML parsing uses secure parser (no external entities)
- SQL queries use parameterized inputs
- File paths validated before operations
- Grant numbers validated against database
- Admin notifications don't include sensitive data

## Performance Notes

- Processes files sequentially (not parallel)
- XML parsing uses streaming for large files
- Database connection pooling enabled
- File moves are atomic operations
- Progress logged every 10 files

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS NOTIFICATION EMAILS** summarizing processing results.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending summary notifications
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test XML files with synthetic grant numbers (e.g., 5R01CA000000-01)
> - ? Never test with folders containing real PFR submissions
> - ? DO NOT use real grant numbers or PI names in test files
> - ? DO NOT test with production administrator email addresses
>
> **Before testing:**
> 1. Create test XML files with obviously fake grant numbers
> 2. Configure administrator emails to test accounts only
> 3. Use dedicated test source/archive directories
> 4. Coordinate with team if testing in shared environment

### Debug Mode

Set `debug=y` to:
- Skip database inserts
- Parse and validate XML only
- Log all extracted data
- Move files normally

### Test XML File

Create a test XML file in SourceDir:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<ProgressReport>
  <GrantNumber>5R01CA000000-01</GrantNumber>
  <ReportType>TEST</ReportType>
  <!-- Add other required fields -->
</ProgressReport>
```

## Notes

- Archive and Error directories created automatically
- XML files must have `.xml` extension
- Processing order is alphabetical by filename
- Duplicate filenames in archive are overwritten
- Large XML files (>10MB) may require increased memory
- Consider scheduling during off-peak hours for large batches

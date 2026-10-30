# LoadSuppPfr

Supplement Progress and Final Report (Supp PFR) loader that processes XML metadata files and imports associated PDF documents for administrative supplement applications into the eGrants document management system.

## Overview

The LoadSuppPfr application:
- Monitors a source directory for supplement PFR XML metadata files
- Parses XML files containing document metadata (applid, folderid, filename, date, file_type)
- Calls getPlaceHolder_new stored procedure to register supplement documents in the database
- Copies PDF files to final destination with assigned file numbers
- Archives both XML and PDF files to backup directory
- Sends email notifications for database errors
- Supports batch processing of multiple files

## Migrated From

**Original VBScript:** `Load_Supp_PFR.vbs` (created by Imran Omair, 10/31/2015)

**Migration Status:** ? **100% Feature Complete** - See [VBSCRIPT_COVERAGE_ANALYSIS.md](./VBSCRIPT_COVERAGE_ANALYSIS.md) for detailed comparison

**Critical Bug Fixes Applied:**
- ? Stored procedure parameters 6, 7, 8 corrected (now pass single spaces instead of empty strings)
- ? Email notifications added for database errors

## Key Features

### XML Metadata Processing

- **Source Scanning**: Monitors configured directory for `.xml` metadata files
- **XML Parsing**: Extracts document metadata (applid, folderid, filename, date, file_type)
- **Special Logic**: Sets catname="PFR" when folderid="19"
- **Data Validation**: Verifies PDF files exist before processing
- **Batch Processing**: Handles multiple XML files with multiple records each

### Document Management

- **Database Registration**: Calls `getPlaceHolder_new` stored procedure to create WIP entries
- **File Numbering**: Retrieves assigned file numbers from database (first column of result)
- **PDF Processing**: Moves PDFs to watch folder with new file number names
- **Archival**: Backs up both XML metadata and copies PDF files before moving
- **Error Recovery**: Continues processing remaining files if one fails

### Email Notifications

- **Error Alerts Only**: 
  - Database errors (getPlaceHolder_new returning no data)
- **Configurable Recipients**: To and CC addresses in appsettings.json
- **Environment Labels**: DEV/PROD prefixes in email subjects

**Note:** Unlike LoadPfr, LoadSuppPfr does NOT send success notifications with applid lists.

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
  "SuppPfrPaths": {
    "DocSrcPath": "C:\\eGrants\\supp_pfr\\source\\",
    "BakDstPath": "C:\\eGrants\\supp_pfr\\backup\\",
    "FinalDstPath": "C:\\eGrants\\watch\\out\\"
  },
  "EmailSettings": {
    "Enabled": "true",
    "ToRecipients": "ayehualem.anteneh@nih.gov",
    "CcRecipients": "omairi@mail.nih.gov",
    "Environment": "DEV"
  }
}
```

### Configuration Parameters

- **DocSrcPath**: Directory containing XML metadata and PDF files
- **BakDstPath**: Archive directory for processed files
- **FinalDstPath**: Destination directory for renamed PDFs (typically watch folder)
- **LogDir**: Directory for daily log files (SUPP-PFR-Log-{date}.txt)
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
  </record>
</root>
```

## Database Dependencies

### Stored Procedures
- **getPlaceHolder_new** - Creates WIP entry for supplement document and returns file number
  - Parameter 1: @PARENTAPPLID (applid)
  - Parameter 2: @pa (single space " ")
  - Parameter 3: @Rcvd_dt (document date)
  - Parameter 4: @Catname (e.g., "PFR")
  - Parameter 5: @filetype (e.g., "pdf")
  - Parameter 6: @Sub (single space " ") ?? **Must be space, not empty string**
  - Parameter 7: @body (single space " ") ?? **Must be space, not empty string**
  - Parameter 8: @SubCatname (single space " ") ?? **Must be space, not empty string**
  - Returns: File number in first column

## Running

### Development

```bash
cd LoadSuppPfr
dotnet run
```

### Production

Typically run as a Windows Scheduled Task:
```
dotnet C:\eGrants\apps\LoadSuppPfr\LoadSuppPfr.dll
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

Daily log files: `C:\egrants\apps\log\SUPP-PFR-Log-{yyyy-M-d}.txt`

Log entries include:
- Task start/completion timestamps
- File processing details
- Database procedure calls
- File operations (copy, move)
- Errors with stack traces
- Email notifications sent

## Email Notification Examples

### Database Error (Only notification type)
- **Subject**: `DEV: ERROR: Could not create entry in WIP. Check DB proc : getPlaceHolder_new`
- **Body**: `Could not create entry in WIP. Check DB proc : getPlaceHolder_new`
- **To**: ayehualem.anteneh@nih.gov
- **CC**: omairi@mail.nih.gov

## Key Differences from LoadPfr

| Aspect | LoadPfr | LoadSuppPfr |
|--------|---------|-------------|
| **Stored Procedure** | Create_PFR | getPlaceHolder_new |
| **Parameters** | 5 parameters | 8 parameters (6-8 must be spaces) |
| **Success Email** | Yes (lists applids) | No |
| **Error Email** | Yes | Yes |
| **Log File** | PFR-Log-{date}.txt | SUPP-PFR-Log-{date}.txt |
| **Purpose** | Primary grants | Supplement grants |

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
- Verify getPlaceHolder_new stored procedure exists in database
- **Ensure parameters 6, 7, 8 are being passed as single spaces (not empty strings)**
- Check database user has execute permissions
- Ensure connection string is correct
- Review SQL Server logs

### "Could not create entry in WIP" error
- This indicates getPlaceHolder_new returned no data
- Check if supplement applid exists in database
- Verify parent grant relationship is valid
- Review stored procedure logic
- Check database constraints and triggers

## See Also

- [VBSCRIPT_COVERAGE_ANALYSIS.md](./VBSCRIPT_COVERAGE_ANALYSIS.md) - Complete VBScript migration analysis including critical bug fixes
- [LoadPfr](../LoadPfr/README.md) - Similar project for primary application PFRs
cd LoadSuppPfr
dotnet run
```

### Production

```bash
LoadSuppPfr.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Supplement PFR Loader"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., daily at 3:00 AM)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\LoadSuppPfr\LoadSuppPfr.exe`
   - Start in: `C:\eGrants\apps\LoadSuppPfr\`

4. **Settings:**
   - Stop task if it runs longer than: 4 hours
   - If task is already running: Queue a new instance

## XML File Format

### Expected XML Structure

```xml
<?xml version="1.0" encoding="UTF-8"?>
<SupplementProgressReport>
  <ParentGrantNumber>5R01CA258784-04</ParentGrantNumber>
  <SupplementGrantNumber>5R01CA258784-04S1</SupplementGrantNumber>
  <SupplementType>Administrative</SupplementType>
  <ReportType>RPPR</ReportType>
  <ReportPeriodStart>2023-01-01</ReportPeriodStart>
  <ReportPeriodEnd>2023-12-31</ReportPeriodEnd>
  <BudgetPeriod>1</BudgetPeriod>
  <PI>
    <FirstName>John</FirstName>
    <LastName>Smith</LastName>
  </PI>
  <Institution>
    <Name>University Example</Name>
  </Institution>
  <ProgressNarrative>...</ProgressNarrative>
  <Accomplishments>...</Accomplishments>
  <BudgetInfo>...</BudgetInfo>
</SupplementProgressReport>
```

## Processing Workflow

1. **Scan Directory** for supplement PFR XML files
2. **Parse XML** to extract supplement metadata
3. **Validate Supplement** is linked to valid parent grant
4. **Validate Data** against business rules
5. **Lookup Application IDs** for parent and supplement
6. **Check for Duplicates** in database
7. **Import Data** via stored procedures
8. **Update Status** for supplement and parent
9. **Move File** to archive or error directory
10. **Send Notifications** for results

## Supplement Validation Rules

- **Parent Grant Exists**: Parent application ID must be valid
- **Supplement Authorized**: Supplement must be authorized in system
- **Report Period Valid**: Must fall within supplement award period
- **Budget Period Valid**: Must match supplement budget period
- **No Duplicates**: Same report period not already submitted
- **Data Complete**: All required fields populated

## Logging

Logs are written to the configured `LogDir`:
- File: `LoadSuppPfr-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Source directory scan
- XML files found
- Parent grant lookups
- Supplement validations
- Database import results
- File operations
- Errors and warnings
- Processing summary

## Error Handling

### Validation Errors
- **Parent Grant Not Found**: File moved to ErrorDir, admin notified
- **Supplement Not Authorized**: File moved to ErrorDir, logged
- **Invalid Report Period**: File moved to ErrorDir, details logged
- **Missing Required Fields**: File moved to ErrorDir, field list logged

### Processing Errors
- **Database Errors**: File moved to ErrorDir, admin notified
- **XML Parse Errors**: File moved to ErrorDir, structure logged
- **Duplicate Reports**: Skipped with warning

### System Errors
- **Directory Access Issues**: Logged, processing stops
- **Database Connection Failures**: Logged, retried
- **File Lock Issues**: Skipped, retried on next run

## Notification Emails

### Success Summary

```
Subject: Supplement PFR Load Complete

Body:
Total files processed: 15
Successful: 14
Failed: 1
Duration: 5 minutes

Failed files:
- supplement_5R01CA258784-04S1.xml (Parent grant not found)
```

### Error Notification

```
Subject: Supplement PFR Load Error

Body:
Error processing: supplement_5R01CA258784-04S1.xml
Error: Parent grant 5R01CA258784-04 not found in database
Stack trace: ...
```

## Dependencies

- .NET 8.0
- SQL Server (EIM database with supplement tables)
- XML parsing libraries (System.Xml)
- File system access

## Troubleshooting

### Parent Grant Not Found

1. Verify parent grant number in XML
2. Check parent grant exists in database
3. Verify `Imm_fn_applid_match` function works
4. Check for typos in grant number

### Supplement Not Authorized

1. Verify supplement was properly authorized
2. Check supplement grant number format
3. Review supplement authorization records
4. Confirm supplement type is valid

### Duplicate Report Errors

1. Check existing supplement reports in database
2. Verify report period dates
3. Review business rules for duplicates
4. Check if file was already processed

### XML Parsing Failures

1. Validate XML structure
2. Check for special characters
3. Verify encoding (UTF-8)
4. Compare against expected schema

## Security Notes

- XML parsing uses secure parser
- SQL queries use parameterized inputs
- File paths validated
- Grant numbers validated against database
- Supplement authorization verified

## Performance Notes

- Sequential file processing
- XML streaming for large files
- Database connection pooling
- Atomic file moves
- Progress logged regularly

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS NOTIFICATION EMAILS** about supplement report processing.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending notifications
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test XML files with synthetic supplement grant numbers
> - ? Never test with folders containing real supplement PFR submissions
> - ? DO NOT use real grant or supplement numbers in test files
> - ? DO NOT test with production administrator addresses
>
> **Before testing:**
> 1. Create test XML with fake parent and supplement grant numbers
> 2. Configure test-only administrator email addresses
> 3. Use dedicated test directories
> 4. Ensure no real supplement data is used

### Debug Mode

Set `debug=y` to:
- Skip database inserts
- Parse and validate only
- Log all extracted data
- Move files normally

### Test XML File

```xml
<?xml version="1.0" encoding="UTF-8"?>
<SupplementProgressReport>
  <ParentGrantNumber>5R01CA000000-01</ParentGrantNumber>
  <SupplementGrantNumber>5R01CA000000-01S1</SupplementGrantNumber>
  <SupplementType>Administrative</SupplementType>
  <ReportType>TEST</ReportType>
  <!-- Add other required fields -->
</SupplementProgressReport>
```

## Notes

- Directories created automatically if missing
- Files must have `.xml` extension
- Processing order is alphabetical
- Parent grant must exist before processing supplement report
- Supplement authorization must be recorded in database
- Large XML files may require increased memory allocation

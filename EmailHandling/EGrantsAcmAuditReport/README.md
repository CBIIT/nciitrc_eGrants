# EGrantsAcmAuditReport

eGrants ACM (Administrative Cost Management) Audit Report processor - an alternative implementation of the ACM audit report processing system.

## Overview

The EGrantsAcmAuditReport application:
- Processes ACM audit reports similar to ACMReportProcessor
- Provides alternative implementation with enhanced features
- Scans directories for audit report files
- Inserts report metadata into database
- Optionally copies files to network locations
- Sends notification emails
- Supports multiple report formats

## Relationship to ACMReportProcessor

This project provides an alternative or enhanced implementation of ACM audit report processing. Both projects serve similar purposes but may have different:
- Processing logic
- Configuration options
- Database schemas
- Notification mechanisms

## Key Features

### Report Processing

- **File Scanning**: Monitors source directory for report files
- **Format Support**: Handles multiple report formats (Excel, PDF, etc.)
- **Metadata Extraction**: Parses report information
- **Database Integration**: Stores report records
- **File Management**: Archives processed files
- **Notification System**: Sends processing summaries

### Enhanced Capabilities

Compared to ACMReportProcessor, may include:
- Additional validation rules
- Extended metadata support
- Different database schema
- Alternative notification templates
- Enhanced error handling
- Custom reporting features

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "SourceDirectory": "C:\\eGrants\\Reports\\ACMAudit\\",
    "ArchiveDirectory": "C:\\eGrants\\Reports\\ACMAudit\\Archive\\",
    "NetworkPath": "\\\\server\\share\\ACMAudit\\",
    "AdminRecipients": "egrantsdevs@mail.nih.gov",
    "EnableFileCopy": "false"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **SourceDirectory**: Directory to scan for audit reports
- **ArchiveDirectory**: Directory for processed files
- **NetworkPath**: Network location for file distribution
- **AdminRecipients**: Email recipients for notifications
- **EnableFileCopy**: Enable copying to network locations
- **Verbose**: Detailed logging
- **Debug**: Skip database operations

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Tables
- Audit report metadata tables
- Processing status tables
- Report history tables

### Stored Procedures
- `SP_INSERT_ACM_AUDIT_REPORT` - Inserts report records
- `SP_VALIDATE_AUDIT_REPORT` - Validates report data
- `SP_UPDATE_REPORT_STATUS` - Updates processing status

## Running

### Development

```bash
cd EGrantsAcmAuditReport
dotnet run
```

### Production

```bash
EGrantsAcmAuditReport.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants ACM Audit Report Processor (Alternative)"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., monthly after report generation)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\EGrantsAcmAuditReport\EGrantsAcmAuditReport.exe`
   - Start in: `C:\eGrants\apps\EGrantsAcmAuditReport\`

4. **Settings:**
   - Stop task if it runs longer than: 2 hours
   - If task is already running: Do not start a new instance

## Report Formats Supported

### Excel Files
- `.xlsx` - Excel 2007+ format
- `.xls` - Excel 97-2003 format

### PDF Files
- `.pdf` - Scanned or generated PDFs

### Text Files
- `.txt` - Plain text reports
- `.csv` - Comma-separated values

## Processing Workflow

1. **Scan Source Directory** for report files
2. **Validate File Format** and size
3. **Extract Metadata** from filename or content
4. **Check for Duplicates** in database
5. **Insert Report Record** into database
6. **Copy to Network** (if enabled)
7. **Move to Archive** directory
8. **Send Notifications** for results
9. **Log Processing** summary

## Logging

Logs are written to the configured `LogDir`:
- File: `EGrantsAcmAuditReport-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Directory scan results
- Files found
- Validation results
- Database operations
- File copy operations
- Archive operations
- Errors and warnings
- Processing summary

## Error Handling

- **File Access Errors**: Logged, file skipped
- **Validation Errors**: File moved to error folder
- **Database Errors**: Admin notified, file not moved
- **Network Copy Failures**: Logged as warning, processing continues
- **Duplicate Files**: Logged, file skipped

## Notification Emails

### Success Summary

```
Subject: ACM Audit Report Processing Complete

Body:
Reports processed: 5
Successful: 4
Failed: 1
Duration: 2 minutes

Failed files:
- Report_2024-01.xlsx (Invalid format)
```

### Error Notification

```
Subject: ACM Audit Report Processing Error

Body:
Error processing: Report_2024-01.xlsx
Error: Database connection timeout
Stack trace: ...
```

## Dependencies

- .NET 8.0
- SQL Server (EIM database)
- File system access
- Optional: Network file share access
- Optional: Microsoft Outlook (for notifications)

## Comparison with ACMReportProcessor

| Feature | ACMReportProcessor | EGrantsAcmAuditReport |
|---------|-------------------|----------------------|
| Excel Support | ? | ? |
| PDF Support | ? | ? |
| Network Copy | Optional | Optional |
| Database Schema | Standard | Extended |
| Notifications | Outlook | Outlook/SMTP |
| Validation | Basic | Enhanced |

## Migration from ACMReportProcessor

If migrating from ACMReportProcessor:

1. Review configuration differences
2. Compare database schemas
3. Test with sample files
4. Run both processors in parallel (different folders)
5. Compare results
6. Switch over when confident

## Troubleshooting

### Files Not Being Processed

1. Verify source directory exists
2. Check file permissions
3. Verify file formats
4. Review logs for errors

### Database Insert Failures

1. Check connection string
2. Verify stored procedures exist
3. Check service account permissions
4. Review schema compatibility

### Network Copy Failures

1. Verify network path accessible
2. Check service account network permissions
3. Test UNC path manually
4. Review FIPS compliance settings

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS NOTIFICATION EMAILS** about audit report processing.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending notifications during testing
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test report files with synthetic data
> - ? Never test with real audit reports
> - ? DO NOT test with production audit reports
> - ? DO NOT use production administrator addresses
>
> **Before testing:**
> 1. Create test report files with obviously fake data
> 2. Configure administrator emails to test accounts only
> 3. Use dedicated test source directory
> 4. Verify no real audit data will be processed

### Debug Mode

Set `debug=y` to:
- Parse and validate files
- Skip database inserts
- Log all operations
- Process files normally

### Test Files

Place test files in source directory:
```
Test_Report_2024-01.xlsx
Test_Report_2024-02.pdf
```

## Performance Notes

- Sequential file processing
- Efficient file I/O
- Database connection pooling
- Network operations optional
- Minimal memory footprint

## Security Notes

- File paths validated
- SQL queries parameterized
- Network credentials secured
- Audit trail maintained
- Admin notifications sanitized

## Notes

- Archive directory created automatically
- Supports multiple file formats
- Network copy is optional
- Can run alongside ACMReportProcessor
- Consider consolidating with ACMReportProcessor in future
- Evaluate which implementation better fits requirements

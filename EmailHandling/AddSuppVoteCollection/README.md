# AddSuppVoteCollection

Administrative Supplement vote collection processor that monitors Outlook voting responses and records voting results in the database.

## Overview

The AddSuppVoteCollection application:
- Monitors Outlook inbox for voting response emails
- Extracts voting results (Accepted/Rejected) from email properties
- Parses notification IDs from email subjects or bodies
- Records voting responses in supplement notification database
- Tracks voter information and timestamps
- Sends summary reports of voting activity
- Supports both automatic and manual vote collection

## Migrated From

Original VBScript: Related to `add_supp_prod.vbs` voting functionality

## Key Features

### Voting Response Collection

- **Email Monitoring**: Scans inbox for voting response emails
- **Vote Extraction**: Reads Outlook voting button responses
- **Notification Linking**: Associates votes with notification records
- **Voter Tracking**: Records who voted and when
- **Response Types**: Handles "Accepted" and "Rejected" votes
- **Duplicate Prevention**: Prevents recording duplicate votes

### Vote Processing Workflow

1. **Connect to Outlook** inbox
2. **Filter voting responses** from regular emails
3. **Extract vote choice** from email properties
4. **Parse notification ID** from email content
5. **Validate notification** exists in database
6. **Record vote** with timestamp and voter info
7. **Archive email** to processed folder
8. **Send summary** of collected votes

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "InboxPath": "Inbox\\Supplement Votes",
    "ProcessedPath": "Inbox\\Supplement Votes\\Processed",
    "AdminRecipients": "egrantsdevs@mail.nih.gov"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **InboxPath**: Outlook folder to monitor for voting responses
- **ProcessedPath**: Folder for processed voting emails
- **AdminRecipients**: Email recipients for vote summaries
- **Verbose**: Set to "y" for detailed logging
- **Debug**: Set to "y" to skip database updates

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Tables
- `dbo.adsup_Notification_vote_responses` - Stores voting responses
  - Columns: `Notification_id`, `voter_email`, `vote_response`, `vote_date`, `response_id`

### Queries
- Validates notification IDs exist
- Checks for duplicate votes from same voter
- Retrieves notification details for reporting

## Running

### Development

```bash
cd AddSuppVoteCollection
dotnet run
```

### Production

```bash
AddSuppVoteCollection.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants Supplement Vote Collection"
   - Run whether user is logged on or not
   - Run with highest privileges

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every 30 minutes)
   - Or: On startup (with 5-minute delay)

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\AddSuppVoteCollection\AddSuppVoteCollection.exe`
   - Start in: `C:\eGrants\apps\AddSuppVoteCollection\`

4. **Settings:**
   - Stop task if it runs longer than: 30 minutes
   - If task is already running: Do not start a new instance

## Email Format

### Expected Voting Response Email

```
From: john.smith@nih.gov
Subject: Accepted: Admin Supplement Request for 5R01CA258784
Body: 
[Original voting email]
Notification Id=12345
```

### Voting Properties

Outlook voting responses have special properties:
- `VotingResponse`: "Accepted" or "Rejected"
- `VotingOptions`: Original voting options
- `ParentID`: Links to original voting email

## Vote Collection Workflow

1. **Scan Inbox** for emails with voting properties
2. **Extract Vote** from VotingResponse property
3. **Find Notification ID** in email body or subject
4. **Validate Notification** exists in database
5. **Check for Duplicate** vote from same voter
6. **Record Vote** in database with timestamp
7. **Move Email** to Processed folder
8. **Log Result** for tracking
9. **Send Summary** (periodic or on request)

## Logging

Logs are written to the configured `LogDir`:
- File: `AddSuppVoteCollection-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Inbox connection status
- Voting emails found
- Vote extraction results
- Database insert operations
- Duplicate vote detections
- Email move operations
- Processing summary

## Error Handling

### Vote Processing Errors
- **Notification ID Not Found**: Email skipped, logged as warning
- **Invalid Vote Response**: Email skipped, logged
- **Duplicate Vote**: Email skipped, logged with existing vote info
- **Database Insert Failure**: Email not moved, admin notified

### System Errors
- **Outlook Connection Failure**: Logged, retried
- **Folder Access Issues**: Logged, processing stops
- **Database Connection Failure**: Logged, retried

## Vote Tracking

### Recorded Information

For each vote:
- Notification ID
- Voter email address
- Vote response (Accepted/Rejected)
- Vote timestamp
- Unique response ID

### Duplicate Detection

Prevents duplicate votes by:
- Checking voter email + notification ID combination
- Logging attempts to vote multiple times
- Keeping original vote (first vote wins)

## Notification Emails

### Vote Summary Report

Sent periodically or on request:
```
Subject: Supplement Vote Collection Summary

Body:
Period: 2024-01-15 to 2024-01-15
Total votes collected: 25
Accepted: 18
Rejected: 7

By Notification:
- Notification 12345: 5 Accepted, 2 Rejected
- Notification 12346: 3 Accepted, 1 Rejected
...
```

### Error Notification

```
Subject: Vote Collection Error

Body:
Error processing voting email from john.smith@nih.gov
Notification ID: 12345
Error: Database connection timeout
Stack trace: ...
```

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database with voting tables)
- Outlook voting functionality enabled

## COM Automation

Uses Outlook COM automation for:
- Inbox folder access
- Email property reading (VotingResponse)
- Email content parsing
- Email archiving (move to Processed)

## Troubleshooting

### Votes Not Being Collected

1. Verify Outlook is running
2. Check InboxPath folder exists
3. Verify voting emails are present
4. Check VotingResponse property exists
5. Review logs for errors

### Notification ID Not Found

1. Check email body format
2. Verify "Notification Id=" pattern
3. Review notification ID extraction logic
4. Confirm notification exists in database

### Duplicate Vote Warnings

1. Review voter email address
2. Check notification ID
3. Verify first vote was recorded correctly
4. This is normal behavior (prevents multiple votes)

### Database Insert Failures

1. Check connection string
2. Verify table exists
3. Verify service account has INSERT permissions
4. Review column data types
5. Check for constraint violations

## Security Notes

- Voter email addresses extracted from Outlook
- SQL queries use parameterized inputs
- Notification IDs validated against database
- Original voting emails preserved in Processed folder

## Performance Notes

- Sequential email processing
- COM objects properly released
- Database connection pooling
- Voting properties read efficiently
- No batch operations (each vote independent)

## Testing

> **?? CRITICAL WARNING - EMAIL TESTING**
>
> This application **SENDS VOTE SUMMARY EMAILS** to administrators.
>
> **Testing precautions:**
>
> - ? Set `debug=y` to prevent sending summary reports
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test voting emails, never process real voting responses during testing
> - ? Clear test vote records from database after testing
> - ? DO NOT test with folders containing real voting responses
> - ? DO NOT record test votes as production votes
>
> **Before testing:**
> 1. Create test voting emails manually in test folder
> 2. Use test notification IDs that don't correspond to real supplements
> 3. Configure test-only administrator emails
> 4. Plan to clean up test vote records after testing

### Debug Mode

Set `debug=y` to:
- Extract and log voting information
- Skip database inserts
- Process emails normally
- Move emails to Processed folder

### Test Voting Email

Create a voting email in Outlook:
1. Create new email with voting buttons
2. Send to yourself
3. Click voting button to respond
4. Response email will appear in inbox
5. Application will process it

### Manual Testing

```csharp
// Test notification ID extraction
string subject = "Accepted: Admin Supplement Request for 5R01CA258784";
string body = "Notification Id=12345";
// Should extract "12345"
```

## Integration with AddSuppEmailer

This application works in conjunction with `AddSuppEmailer`:

1. **AddSuppEmailer**: Sends voting emails to recipients
2. **Recipients**: Click voting buttons (Accepted/Rejected)
3. **AddSuppVoteCollection**: Collects voting responses
4. **Database**: Stores vote results for reporting

## Notes

- Processed folder must exist before running
- Only emails with VotingResponse property are processed
- Duplicate votes from same voter are prevented
- Vote timestamps use email received time
- Voting responses are case-insensitive
- Original voting emails are preserved in Processed folder
- Vote summaries can be generated on demand or scheduled

# OGARequestAccountDisable

OGA (Office of Grants Administration) account disable request processor that monitors emails requesting account deactivation and processes them through the eGrants system.

## Overview

The OGARequestAccountDisable application:
- Monitors specific Outlook folder for account disable requests
- Extracts user information from email subject and body
- Validates requests against active user database
- Processes account deactivation through eGrants security system
- Sends confirmation emails to requesters
- Logs all account disable activities
- Supports both automatic and manual request processing

## Migrated From

Original VBScript or manual process for account disable requests

## Key Features

### Account Disable Processing

- **Request Validation**: Verifies user exists and is active
- **Email Parsing**: Extracts user ID, email, and justification
- **Security Integration**: Interfaces with eGrants security system
- **Confirmation Emails**: Notifies requesters of completion
- **Audit Trail**: Logs all disable requests and results
- **Rollback Support**: Can restore accounts if needed

### Request Types Supported

- **Individual User**: Single user account disable
- **Batch Requests**: Multiple users in one email
- **Emergency Disable**: High-priority security requests
- **Scheduled Disable**: Future-dated deactivation

## Configuration

Edit `appsettings.json`:

```json
{
  "AppSettings": {
    "Verbose": "n",
    "Debug": "n",
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "RequestFolderPath": "Public Folders - oga@mail.nih.gov\\Account Requests",
    "ProcessedFolderPath": "Public Folders - oga@mail.nih.gov\\Account Requests\\Processed",
    "AdminRecipients": "egrantsdevs@mail.nih.gov;oga-security@nih.gov"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

### Configuration Parameters

- **RequestFolderPath**: Outlook folder for account disable requests
- **ProcessedFolderPath**: Folder for processed requests
- **AdminRecipients**: Email recipients for notifications
- **Verbose**: Set to "y" for detailed logging
- **Debug**: Set to "y" to skip actual account disabling

### Environment Variables

- **DB_USER**: Database username (required)
- **DB_PASSWORD**: Database password (required)

## Database Dependencies

### Tables
- User account tables
- Account status tracking
- Audit log tables
- Security event logs

### Stored Procedures
- `SP_DISABLE_USER_ACCOUNT` - Disables user account
- `SP_VALIDATE_DISABLE_REQUEST` - Validates request
- `SP_LOG_ACCOUNT_DISABLE` - Logs disable activity
- `SP_REVOKE_USER_PERMISSIONS` - Revokes all user permissions

### Functions
- `fn_get_user_by_email()` - Looks up user by email
- `fn_check_user_active_grants()` - Checks for active user grants

## Running

### Development

```bash
cd OGARequestAccountDisable
dotnet run
```

### Production

```bash
OGARequestAccountDisable.exe
```

## Task Scheduler Setup

1. **General Tab:**
   - Name: "eGrants OGA Account Disable Processor"
   - Run whether user is logged on or not
   - Run with highest privileges
   - Use service account with security admin rights

2. **Triggers Tab:**
   - New Trigger: On a schedule (e.g., every hour during business hours)
   - Or: Multiple times daily

3. **Actions Tab:**
   - Program: `C:\eGrants\apps\OGARequestAccountDisable\OGARequestAccountDisable.exe`
   - Start in: `C:\eGrants\apps\OGARequestAccountDisable\`

4. **Settings:**
   - Stop task if it runs longer than: 1 hour
   - If task is already running: Do not start a new instance

## Email Format

### Expected Request Email

```
From: oga-requester@nih.gov
To: oga-accounts@mail.nih.gov
Subject: Account Disable Request - john.smith@nih.gov

Body:
Please disable the following account:

User ID: johnsmith
Email: john.smith@nih.gov
Reason: User left organization
Requested by: Jane Manager
Date needed: 2024-01-15
```

### Required Information

- User ID or email address
- Reason for disable
- Requester information
- Effective date (optional, defaults to immediately)

## Processing Workflow

1. **Monitor Folder** for new disable requests
2. **Parse Email** to extract user information
3. **Validate User** exists in system
4. **Check Permissions** of requester
5. **Validate Request** meets security requirements
6. **Backup User Data** before disabling
7. **Disable Account** via stored procedure
8. **Revoke Permissions** for all grants/applications
9. **Send Confirmation** to requester
10. **Move Email** to Processed folder
11. **Log Activity** for audit trail

## Account Disable Operations

### What Gets Disabled

- User login access
- Email notifications
- Grant access permissions
- Application reviewer permissions
- Admin console access
- API tokens (if any)

### What Gets Preserved

- Historical grant submissions
- Review history
- Audit logs
- Document ownership records

## Logging

Logs are written to the configured `LogDir`:
- File: `OGARequestAccountDisable-YYYY-MM-DD.log`
- Uses Serilog for structured logging

### Log Events
- Request emails found
- User lookups
- Validation results
- Account disable operations
- Permission revocations
- Confirmation emails sent
- Error conditions
- Security events

## Error Handling

### Request Validation Errors
- **User Not Found**: Email requester, log warning
- **User Already Disabled**: Skip, notify requester
- **Insufficient Permissions**: Reject, notify requester
- **Invalid Request Format**: Move to error folder

### Processing Errors
- **Database Errors**: Rollback, notify admin
- **Permission Errors**: Log, notify security team
- **Email Send Failures**: Log, retry later

### Security Errors
- **Unauthorized Requester**: Log security event, notify admin
- **Suspicious Pattern**: Hold for manual review
- **System Account Disable Attempt**: Block, alert security

## Confirmation Emails

### Success Confirmation

```
To: requester@nih.gov
Subject: Account Disable Completed - john.smith@nih.gov

Body:
The following account has been successfully disabled:

User ID: johnsmith
Email: john.smith@nih.gov
Disabled Date: 2024-01-15 10:30:00
Processed by: OGA Account Disable System
Reference: REQ-2024-00123

All associated permissions have been revoked.
Historical records have been preserved.
```

### Error Notification

```
To: requester@nih.gov, admin@nih.gov
Subject: Account Disable Failed - john.smith@nih.gov

Body:
Failed to disable account:

User ID: johnsmith
Email: john.smith@nih.gov
Error: User not found in system
Reference: REQ-2024-00124

Please review the request and resubmit if necessary.
```

## Dependencies

- .NET 8.0
- Microsoft Outlook (installed and configured)
- SQL Server (EIM database with security tables)
- Service account with security admin permissions

## Security Notes

- All disable requests are audited
- Requester permissions validated
- Cannot disable system accounts
- Cannot disable own account
- Rollback capability available
- Security events logged
- Admin notified of all disables

## Troubleshooting

### User Not Found

1. Verify user ID or email in request
2. Check user exists in database
3. Check spelling and format
4. Verify user hasn't already been deleted

### Permission Denied

1. Verify requester has security admin role
2. Check service account permissions
3. Review security policies
4. Contact database administrator

### Database Errors

1. Check connection string
2. Verify stored procedures exist
3. Check service account permissions
4. Review database logs

### Email Not Sent

1. Verify Outlook is running
2. Check email addresses
3. Review SMTP settings
4. Check network connectivity

## Rollback Procedures

### To Restore Disabled Account

1. Contact database administrator
2. Provide reference number from confirmation email
3. Request account restoration via `SP_RESTORE_USER_ACCOUNT`
4. Manually restore permissions as needed

### Backup Data

Before disabling, system backs up:
- User profile
- Permission sets
- Grant assignments
- Reviewer assignments

## Testing

> **?? CRITICAL SECURITY WARNING - EMAIL TESTING**
>
> This application **DISABLES USER ACCOUNTS** and **SENDS CONFIRMATION EMAILS**.
>
> **EXTREME CAUTION REQUIRED:**
>
> - ? **MANDATORY:** Set `debug=y` for ALL testing - this prevents actual account disabling
> - ? Set `AdminRecipients` to test accounts only
> - ? Use test user accounts that don't represent real employees
> - ? Never test with folders containing real account disable requests
> - ? Coordinate with security team before ANY testing
> - ? **NEVER** test with real user accounts or employee emails
> - ? **NEVER** process real disable requests during testing
> - ? **NEVER** test in production environment
>
> **Consequences of improper testing:**
> - **CRITICAL:** Real user accounts may be disabled incorrectly
> - **CRITICAL:** Employees may lose access to systems
> - Security events may be logged incorrectly
> - HR and legal implications if real accounts are affected
> - Loss of productivity if real users are locked out
>
> **Required approvals before testing:**
> 1. Written approval from IT Security team
> 2. Supervisor sign-off on test plan
> 3. Review with database administrator
> 4. Coordination with HR if testing affects any real accounts

### Debug Mode

Set `debug=y` to:
- Parse and validate requests
- Skip actual account disabling
- Log all operations
- Send test confirmation emails

### Test Request Email

```
From: test-requester@nih.gov
Subject: TEST - Account Disable Request - test.user@nih.gov

Body:
TEST REQUEST - DO NOT PROCESS IN PRODUCTION

User ID: testuser
Email: test.user@nih.gov
Reason: Testing account disable system
```

## Compliance Notes

- Meets NIH security requirements
- Follows least-privilege principle
- Maintains audit trail for compliance
- Preserves records per retention policy
- Supports SOX/FISMA compliance audits

## Performance Notes

- Sequential request processing
- Database transactions for consistency
- COM objects properly released
- Email operations optimized
- No batch mode (each request separate)

## Notes

- Processed folder must exist
- Requires security admin permissions
- Cannot undo disable without manual intervention
- Historical data always preserved
- Consider legal hold requirements before disabling
- Coordinate with HR for employee terminations
- Schedule during business hours for emergency support

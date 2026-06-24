# Manual Testing Guide for Scheduled Task Executables

## Pre-Deployment Testing Checklist

### Phase 1: Environment Setup (Test Environment)

#### 1.1 Environment Configuration
- [ ] Create test environment that mirrors production (separate server or VM)
- [ ] Install .NET 8 Runtime
- [ ] Install and configure Microsoft Outlook with test profile
- [ ] Set up test database (copy of production schema, sanitized test data)
- [ ] Create test public folders in Outlook
- [ ] Set up test email accounts

#### 1.2 Environment Variables
Set required environment variables on test machine:

```powershell
# Database credentials
[System.Environment]::SetEnvironmentVariable('DB_USER', 'test_db_user', [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'test_db_password', [System.EnvironmentVariableTarget]::Machine)

# Environment indicator
[System.Environment]::SetEnvironmentVariable('DOTNET_ENVIRONMENT', 'Development', [System.EnvironmentVariableTarget]::Machine)
```

#### 1.3 Directory Structure
Create required directories on test machine:

```powershell
New-Item -ItemType Directory -Path "C:\eGrants\apps\log" -Force
New-Item -ItemType Directory -Path "C:\eGrants\data" -Force
New-Item -ItemType Directory -Path "C:\eGrants\publicaccess" -Force
New-Item -ItemType Directory -Path "C:\eGrants\temp" -Force
```

---

## Phase 2: Individual Executable Testing

Test each executable independently before scheduling.

### 2.1 Router

**Purpose:** Routes incoming emails to appropriate recipients based on subject patterns

**Manual Test Steps:**
1. Place 2-3 test emails in the configured public folder
2. Run `Router.exe` manually from command line
3. Verify:
   - [ ] Console shows "Loading configuration"
   - [ ] Emails are processed (moved to "Old emails" folder)
   - [ ] Appropriate routing emails are sent
   - [ ] Log file created in `C:\eGrants\apps\log\eMailRouter-Log-YYYY-MM-DD.txt`
   - [ ] No unhandled exceptions
   - [ ] Exit code = 0

**Test Data:**
- FCOI email: Subject contains "Receipt of a New FCOI report [AppID] for grant number: [GrantNum]"
- Public Access email: Subject contains "category=PublicAccess"
- JIT email: Subject contains "JIT Request for Grant"

**Expected Results:**
- Debug mode: Emails sent to `eGrantsDev@mail.nih.gov`
- Production mode: Emails sent to appropriate officers

**Log Validation:**
```powershell
Get-Content "C:\eGrants\apps\log\eMailRouter-Log-*.txt" -Tail 20
```

---

### 2.2 ExchangeFixed

**Purpose:** Processes structured emails and files them into document management system

**Manual Test Steps:**
1. Create test email with metadata in subject: `category=Correspondence, applid=12345678, extract=1, test subject`
2. Place in configured public folder
3. Run `ExchangeFixed.exe` manually
4. Verify:
   - [ ] Email content saved to `C:\eGrants\data\`
   - [ ] Database record created in document table
   - [ ] Email moved to "old" subfolder
   - [ ] Log file created
   - [ ] Exit code = 0

**Test Data Examples:**
- Body only: `category=Correspondence, applid=12345678, extract=1`
- Attachments: `category=Budget, applid=12345678, extract=2` (with PDF attachment)
- Public Access: `category=PublicAccess, sub=Compliant, applid=12345678, extract=1`

**Validation Queries:**
```sql
-- Verify document was inserted
SELECT TOP 10 * FROM dbo.egrants_documents 
WHERE applid = '12345678' 
ORDER BY date_added DESC;
```

---

### 2.3 LoadPfr

**Purpose:** Loads PFR metadata from XML files and processes PDF documents

**Manual Test Steps:**
1. Create test XML file in format expected (see `LoadPfr/README.md`)
2. Place XML and corresponding PDF in watch directory
3. Run `LoadPfr.exe` manually
4. Verify:
   - [ ] XML parsed successfully
   - [ ] `Create_PFR` stored procedure called
   - [ ] PDF copied to output directory
   - [ ] Email notification sent (success or error)
   - [ ] Log file created
   - [ ] Exit code = 0

**Test Data:**
```xml
<PfrMetadata>
  <GrantNumber>5R01CA123456-01</GrantNumber>
  <PfrType>Annual</PfrType>
  <ReportingPeriodStart>2024-01-01</ReportingPeriodStart>
  <ReportingPeriodEnd>2024-12-31</ReportingPeriodEnd>
  <PdfFileName>PFR_5R01CA123456_2024.pdf</PdfFileName>
</PfrMetadata>
```

**Validation:**
```sql
-- Verify PFR was inserted
SELECT * FROM dbo.pfr_records WHERE grant_number = '5R01CA123456';
```

---

### 2.4 LoadSuppPfr

**Purpose:** Loads supplement PFR metadata from XML files

**Manual Test Steps:**
1. Create test XML file with supplement data
2. Place XML and PDF in watch directory
3. Run `LoadSuppPfr.exe` manually
4. Verify:
   - [ ] XML parsed successfully
   - [ ] `getPlaceHolder_new` stored procedure called with correct parameters
   - [ ] Parameters 6-8 are single spaces (not empty strings)
   - [ ] PDF moved to archive directory
   - [ ] Error email sent only if database failure
   - [ ] Log file created
   - [ ] Exit code = 0

**Important:** Verify parameters 6-8 (`@Sub`, `@body`, `@SubCatname`) are `" "` (single space), not `""`.

---

### 2.5 AddSuppEmailer

**Purpose:** Sends administrative supplement notification emails with voting buttons

**Manual Test Steps:**
1. Insert test notification records in database:
```sql
INSERT INTO dbo.adsup_Notification_email_status (Notification_id, email_date, email_send_status)
VALUES (99999, GETDATE(), 'Pending');
```
2. Run `AddSuppEmailer.exe` manually
3. Verify:
   - [ ] Notification retrieved from database
   - [ ] Email created with voting buttons ("Accepted"/"Rejected")
   - [ ] Email sent to appropriate recipients (or debug recipients)
   - [ ] Database updated with send status
   - [ ] Log file created
   - [ ] Exit code = 0

**Validation:**
```sql
SELECT * FROM dbo.adsup_Notification_email_status WHERE Notification_id = 99999;
-- email_send_status should be updated
```

---

### 2.6 AddSuppProd

**Purpose:** Processes supplement request emails from public folder

**Manual Test Steps:**
1. Create test email with application ID in body
2. Attach test PDF document
3. Place in configured public folder
4. Run `AddSuppProd.exe` manually
5. Verify:
   - [ ] Email processed
   - [ ] Application ID extracted from body
   - [ ] Attachments saved to `C:\eGrants\data\supplements\{ApplicationID}\`
   - [ ] Database record inserted
   - [ ] Email moved to "old" folder
   - [ ] Notification email sent
   - [ ] Exit code = 0

---

### 2.7 AddSuppVoteCollection

**Purpose:** Collects voting responses from Outlook

**Manual Test Steps:**
1. Reply to a voting email with "Accepted" or "Rejected"
2. Run `AddSuppVoteCollection.exe` manually
3. Verify:
   - [ ] Voting response detected
   - [ ] Notification ID parsed from email
   - [ ] Vote recorded in database
   - [ ] Response email moved to processed folder
   - [ ] Exit code = 0

**Validation:**
```sql
SELECT * FROM dbo.adsup_Notification_vote_responses 
WHERE Notification_id = [YourTestID];
```

---

### 2.8 ~~DocManEmail~~ ? DEPRECATED

**Status:** This job is no longer running in production and is **excluded from migration and testing**.

---

### 2.9 OGARequestAccountDisable

**Purpose:** Processes account disable requests

**Manual Test Steps:**
1. Create test email requesting account disable
2. Run `OGARequestAccountDisable.exe` manually
3. Verify:
   - [ ] Request processed
   - [ ] Account disable procedure called (or simulated)
   - [ ] Confirmation email sent
   - [ ] Audit log created

---

### 2.10 EGrantsAcmAuditReport

**Purpose:** Processes ACM audit report files

**Manual Test Steps:**
1. Place test audit report file in source directory
2. Run `EGrantsAcmAuditReport.exe` manually
3. Verify file processing

---

### 2.11 StartOutlook

**Purpose:** Starts and validates Outlook configuration

**Manual Test Steps:**
1. Run `StartOutlook.exe` manually
2. Verify:
   - [ ] Outlook starts (if not already running)
   - [ ] MAPI session established
   - [ ] Public folders accessible
   - [ ] Exit code = 0

---

## Phase 3: Smoke Testing in Test Environment

Run all executables in sequence:

```powershell
# Script to run all executables once
$executables = @(
    "C:\eGrants\apps\StartOutlook\StartOutlook.exe",
    "C:\eGrants\apps\Router\Router.exe",
    "C:\eGrants\apps\ExchangeFixed\ExchangeFixed.exe",
    "C:\eGrants\apps\LoadPfr\LoadPfr.exe",
    "C:\eGrants\apps\LoadSuppPfr\LoadSuppPfr.exe",
    "C:\eGrants\apps\AddSuppEmailer\AddSuppEmailer.exe",
    "C:\eGrants\apps\AddSuppProd\AddSuppProd.exe",
    "C:\eGrants\apps\AddSuppVoteCollection\AddSuppVoteCollection.exe",
    # "C:\eGrants\apps\DocManEmail\DocManEmail.exe",  # DEPRECATED - Not in production
    "C:\eGrants\apps\OGARequestAccountDisable\OGARequestAccountDisable.exe",
    "C:\eGrants\apps\EGrantsAcmAuditReport\EGrantsAcmAuditReport.exe"
)

foreach ($exe in $executables) {
    if (Test-Path $exe) {
        Write-Host "`nRunning: $exe" -ForegroundColor Cyan
        $result = & $exe
        Write-Host "Exit Code: $LASTEXITCODE" -ForegroundColor $(if ($LASTEXITCODE -eq 0) { "Green" } else { "Red" })
    } else {
        Write-Host "Not found: $exe" -ForegroundColor Yellow
    }
}
```

---

## Phase 4: Task Scheduler Configuration (Test Environment)

Configure Windows Task Scheduler with test schedules.

### 4.1 Create Scheduled Tasks

Use provided Task Scheduler templates for each executable:

**Example: Router Task**
```powershell
$action = New-ScheduledTaskAction -Execute "C:\eGrants\apps\Router\Router.exe" -WorkingDirectory "C:\eGrants\apps\Router"
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 15)
$principal = New-ScheduledTaskPrincipal -UserId "DOMAIN\ServiceAccount" -LogonType Password -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
$task = New-ScheduledTask -Action $action -Principal $principal -Trigger $trigger -Settings $settings

Register-ScheduledTask -TaskName "eGrants_Router_Test" -InputObject $task -User "DOMAIN\ServiceAccount" -Password "SecurePassword"
```

### 4.2 Test Task Execution

For each scheduled task:
1. Run manually from Task Scheduler
2. Verify "Last Run Result" = 0x0 (success)
3. Check "Last Run Time"
4. Verify log files were created
5. Verify expected behavior occurred

### 4.3 Monitor Scheduled Execution

Let tasks run on schedule for 24-48 hours:
- [ ] Monitor Task Scheduler history
- [ ] Review all log files
- [ ] Verify database operations
- [ ] Check for failed tasks
- [ ] Validate email routing

---

## Phase 5: Integration Testing

Test interactions between executables:

### 5.1 End-to-End Workflows

**Workflow 1: Email Receipt to Document Storage**
1. Send email to monitored folder
2. Router processes and forwards
3. ExchangeFixed receives and stores document
4. Verify end-to-end flow in logs

**Workflow 2: PFR Loading Pipeline**
1. Place XML+PDF files
2. LoadPfr processes files
3. Verify database records
4. Verify email notifications

**Workflow 3: Supplement Notification Workflow**
1. AddSuppEmailer sends notification
2. Recipient votes
3. AddSuppVoteCollection records vote
4. Verify complete cycle

---

## Phase 6: Performance & Load Testing

Test under realistic load:

### 6.1 Volume Testing
- Place 50-100 test emails in Router folder
- Verify processing time
- Check for memory leaks
- Validate all emails processed

### 6.2 Concurrent Execution
- Ensure tasks don't overlap
- Set Task Scheduler to "Do not start a new instance" if already running
- Test behavior when task runs longer than interval

### 6.3 Error Recovery
- Simulate database unavailable
- Simulate Outlook not running
- Verify graceful degradation
- Verify error logging and notifications

---

## Phase 7: Monitoring & Logging Setup

### 7.1 Log Aggregation
Set up centralized log monitoring:
```powershell
# Example: Copy logs to network share daily
$logSource = "C:\eGrants\apps\log\*"
$logDest = "\\server\share\eGrants\logs\$(Get-Date -Format 'yyyy-MM-dd')"
Copy-Item $logSource $logDest -Force
```

### 7.2 Alerting
Create alerts for:
- Task failures (exit code != 0)
- Long-running tasks (> timeout threshold)
- No log file created
- Database connection failures
- Outlook connection failures

### 7.3 Health Check Dashboard
Create monitoring dashboard showing:
- Last successful run time for each task
- Count of processed items
- Error counts
- Database connectivity status
- Outlook connectivity status

---

## Phase 8: Documentation for Operations Team

Prepare operations documentation:
1. [ ] Deployment guide (server setup, installation steps)
2. [ ] Task Scheduler configuration templates
3. [ ] Environment variable requirements
4. [ ] Troubleshooting guide (common issues and resolutions)
5. [ ] Runbook for each executable
6. [ ] Escalation procedures
7. [ ] Rollback procedures

---

## Phase 9: Production Pilot

### 9.1 Parallel Run (Recommended)
- Run VBScript and .NET executables in parallel for 1-2 weeks
- Compare outputs
- Verify data consistency
- Monitor for any discrepancies

### 9.2 Gradual Rollout
1. Deploy one executable at a time (start with lowest risk)
2. Monitor for 1 week
3. Deploy next executable
4. Continue until all are migrated

### 9.3 Rollback Plan
Maintain VBScript versions as backup:
- Keep VBScript files in archive location
- Document rollback procedure
- Test rollback in test environment

---

## Phase 10: Production Deployment

### 10.1 Pre-Deployment Checklist
- [ ] All tests passing (620 unit/integration/smoke tests)
- [ ] Manual testing completed successfully
- [ ] Task Scheduler tested in test environment
- [ ] Performance acceptable under load
- [ ] Operations team trained
- [ ] Documentation complete
- [ ] Rollback plan tested
- [ ] Change control approval obtained

### 10.2 Deployment Steps
1. Schedule maintenance window
2. Disable VBScript scheduled tasks
3. Deploy .NET executables to production servers
4. Set environment variables on production
5. Create scheduled tasks
6. Validate Task Scheduler configuration
7. Enable scheduled tasks (one at a time if gradual rollout)
8. Monitor first executions
9. Verify log files and database operations

### 10.3 Post-Deployment Monitoring
First 24 hours:
- [ ] Monitor every scheduled execution
- [ ] Review all logs
- [ ] Verify email routing
- [ ] Check database operations
- [ ] Validate no regressions

First week:
- [ ] Daily log review
- [ ] Weekly stakeholder report
- [ ] Performance metrics collection
- [ ] Error rate monitoring

---

## Success Criteria

### Functional
- ? All 620 automated tests passing
- ? All executables run successfully manually
- ? All scheduled tasks execute without errors
- ? Email routing matches VBScript behavior
- ? Database operations produce identical results
- ? File processing completes successfully
- ? Outlook integration works reliably

### Performance
- ? Processing time <= VBScript performance
- ? No memory leaks over 24-hour period
- ? Handles expected load (e.g., 100+ emails)
- ? Task execution completes within timeout

### Reliability
- ? 99%+ success rate over 1-week test period
- ? Graceful error handling
- ? Comprehensive logging
- ? Alerts working correctly

### Operations
- ? Operations team trained
- ? Documentation complete
- ? Monitoring in place
- ? Rollback tested

---

## Common Issues & Troubleshooting

### Issue: Task shows "Last Run Result: 0x1"
**Cause:** Executable exited with error code 1  
**Fix:** Check log files for error details

### Issue: Task doesn't run at all
**Cause:** User account permissions, password expired  
**Fix:** Verify service account credentials

### Issue: Outlook errors
**Cause:** Outlook not running, profile not configured  
**Fix:** Run StartOutlook.exe first, verify profile

### Issue: Database connection fails
**Cause:** Environment variables not set, connection string incorrect  
**Fix:** Verify DB_USER and DB_PASSWORD are set at Machine level

### Issue: Files not found
**Cause:** Working directory incorrect  
**Fix:** Set "Start in" directory in Task Scheduler

---

## Appendix A: Smoke Test Results Template

```
Executable: Router.exe
Test Date: 2024-01-18
Tester: [Name]

Configuration:
- Environment: Test
- Database: EIM_Test
- Outlook Profile: TestProfile

Test Results:
? Executable launches
? Configuration loads
? Processes 10 test emails
? Routes to correct recipients
? Logs created
? Exit code = 0
? No exceptions

Performance:
- Processing time: 15 seconds for 10 emails
- Memory usage: 150 MB peak

Notes:
- All expected behaviors verified
- Ready for scheduling

Approval: _________________ Date: _______
```

---

## Appendix B: Task Scheduler Templates

See individual project README files for detailed Task Scheduler configuration for each executable.

---

**Next Document:** [DEPLOYMENT_GUIDE.md] - Step-by-step production deployment instructions

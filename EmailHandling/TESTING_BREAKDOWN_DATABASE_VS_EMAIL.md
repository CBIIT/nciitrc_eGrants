# Testing Breakdown: Database Setup vs. Email Folder Processing

## Executive Summary

**The good news:** Most of your **manual testing** for the 5-week timeline will involve **placing emails in folders**, not complex database setup!

### Quick Answer

| Test Type | Database Setup Required | Email Folder Testing | Complexity |
|-----------|------------------------|---------------------|------------|
| **Router** | Minimal (read-only queries) | ? **PRIMARY** - Place emails in public folder | ?? Low |
| **ExchangeFixed** | Moderate (document inserts) | ? **PRIMARY** - Emails with metadata | ?? Medium |
| **LoadPfr** | Moderate (stored proc calls) | ?? XML files + PDFs (not emails) | ?? Medium |
| **LoadSuppPfr** | Moderate (stored proc calls) | ?? XML files + PDFs (not emails) | ?? Medium |
| **AddSuppEmailer** | ? **PRIMARY** - Insert notification records | Minimal (receives votes) | ?? Medium |
| **AddSuppProd** | Moderate (document inserts) | ? **PRIMARY** - Place emails in folder | ?? Low |
| **AddSuppVoteCollection** | Read-only | ? **PRIMARY** - Reply to voting emails | ?? Low |
| ~~**DocManEmail**~~ | ? **DEPRECATED** | ? Not in production | ? Excluded |
| **OGARequestAccountDisable** | Moderate (user account queries) | ? **PRIMARY** - Place emails in folder | ?? Low |
| **EGrantsAcmAuditReport** | Moderate (report inserts) | Files in watch folder (not emails) | ?? Medium |
| **StartOutlook** | None | None (just validates Outlook) | ?? Very Low |

---

## Detailed Breakdown

### ?? Email-Focused Testing (7 of 11 executables)

These are **easy to test** - just create test emails and place them in the right folder!

#### 1. Router (HIGH PRIORITY)
**Setup Required:**
- ? Database: NONE for basic testing (uses read-only queries)
- ? Outlook: Test public folder created
- ? Test data: Just create test emails

**Testing Process:**
```
1. Create test emails with specific subject patterns:
   - FCOI: "Receipt of a New FCOI report 12345 for grant number: 5R01CA123456"
   - Public Access: "category=PublicAccess, sub=Compliant, applid=12345678"
   - JIT: "JIT Request for Grant 5R01CA123456"

2. Place in public folder: "NCI CA eRA Notifications\Inbox"

3. Run Router.exe

4. Verify:
   - Emails moved to "Old emails" subfolder
   - Routing emails sent to correct recipients
   - Log file shows processing
```

**Database Setup:** ? MINIMAL - The database already has grant/application data; Router just reads it

**Time Estimate:** 30 minutes total

---

#### 2. ExchangeFixed (HIGH PRIORITY)
**Setup Required:**
- ?? Database: Application IDs must exist (but likely already do in test DB)
- ? Outlook: Test public folder
- ? Test data: Create emails with metadata

**Testing Process:**
```
1. Create test email with subject:
   "category=Correspondence, applid=12345678, extract=1, Test Document"

2. Optional: Attach PDF file

3. Place in configured public folder

4. Run ExchangeFixed.exe

5. Verify:
   - Email saved to C:\eGrants\data\
   - Database record created (check egrants_documents table)
   - Email moved to "old" subfolder
```

**Database Setup:** ?? LIGHT - Just need valid applid (application ID)
- If applid doesn't exist: `SELECT TOP 1 applid FROM applications` and use that

**Time Estimate:** 30 minutes

---

#### 3. AddSuppProd
**Setup Required:**
- ?? Database: Application ID must exist
- ? Outlook: Test public folder
- ? Test data: Email with application ID in body

**Testing Process:**
```
1. Create email with body containing:
   "Please process supplement request for Application ID: 12345678"

2. Attach test PDF

3. Place in "Supplements" public folder

4. Run AddSuppProd.exe

5. Verify:
   - Attachment saved to C:\eGrants\data\supplements\12345678\
   - Database record inserted
   - Email moved to "old" subfolder
```

**Database Setup:** ?? LIGHT - Same as ExchangeFixed (just need valid applid)

**Time Estimate:** 20 minutes

---

#### 4. AddSuppVoteCollection
**Setup Required:**
- ? Database: NONE (just reads notification records that already exist)
- ? Outlook: Reply to a voting email

**Testing Process:**
```
1. Prerequisites: AddSuppEmailer has sent a voting email

2. Reply to that email with "Accepted" or "Rejected"

3. Run AddSuppVoteCollection.exe

4. Verify:
   - Vote recorded in database (adsup_Notification_vote_responses table)
   - Email moved to processed folder
```

**Database Setup:** ? NONE - Just responds to existing votes

**Time Estimate:** 15 minutes

---

#### 5. ~~DocManEmail~~ ? DEPRECATED

**Status:** This job is no longer running in production and is **excluded from migration**.

---

#### 6. OGARequestAccountDisable
**Setup Required:**
- ?? Database: User account must exist in eGrants
- ? Outlook: Test public folder
- ? Test data: Email requesting account disable

**Testing Process:**
```
1. Create email with body:
   "Please disable account for user: testuser@nih.gov"

2. Place in "Account Requests" folder

3. Run OGARequestAccountDisable.exe

4. Verify:
   - Request processed
   - Confirmation email sent (or logged in debug mode)
```

**Database Setup:** ?? LIGHT - Need a test user account ID

**Time Estimate:** 15 minutes

---

#### 7. StartOutlook
**Setup Required:**
- ? None - just validates Outlook

**Testing Process:**
```
1. Run StartOutlook.exe
2. Verify Outlook starts
3. Check log file
```

**Database Setup:** ? NONE

**Time Estimate:** 5 minutes

---

### ?? Database-Focused Testing (3 of 11 executables)

These require more database setup but are **still manageable**:

#### 8. AddSuppEmailer (MEDIUM PRIORITY)
**Setup Required:**
- ? **PRIMARY:** Database records for notifications
- ?? Outlook: For sending emails (but in debug mode, just logs)

**Database Setup:**
```sql
-- Insert test notification
INSERT INTO dbo.adsup_Notification_email_status 
(Notification_id, email_date, email_send_status)
VALUES 
(99999, GETDATE(), 'Pending');

-- Verify functions exist (they should):
-- fn_adsupp_getemail_subject(99999)
-- fn_adsupp_getemail_body(99999)
-- fn_adsupp_getemail_string(99999, 'TO')
-- fn_adsupp_getemail_string(99999, 'CC')
```

**Testing Process:**
```
1. Insert test notification record (see above)
2. Run AddSuppEmailer.exe
3. Verify:
   - Email created (or logged in debug mode)
   - Database updated (email_send_status)
   - Log file shows processing
```

**Time Estimate:** 30 minutes (including SQL setup)

---

#### 9. LoadPfr (MEDIUM PRIORITY)
**Setup Required:**
- ?? Database: Stored procedure `Create_PFR` must exist
- ? File System: XML + PDF files in watch folder

**Testing Process:**
```
1. Create test XML file:
   <?xml version="1.0"?>
   <PfrMetadata>
     <GrantNumber>5R01CA123456-01</GrantNumber>
     <PfrType>Annual</PfrType>
     <ReportingPeriodStart>2024-01-01</ReportingPeriodStart>
     <ReportingPeriodEnd>2024-12-31</ReportingPeriodEnd>
     <PdfFileName>PFR_5R01CA123456_2024.pdf</PdfFileName>
   </PfrMetadata>

2. Create matching PDF file

3. Place both in watch directory

4. Run LoadPfr.exe

5. Verify:
   - Stored procedure called
   - PDF moved
   - Email notification sent
```

**Database Setup:** ?? MODERATE
- Stored procedure likely already exists
- May need valid grant number in database

**Time Estimate:** 30 minutes

---

#### 10. LoadSuppPfr
**Setup Required:**
- ?? Database: Stored procedure `getPlaceHolder_new` must exist
- ? File System: XML + PDF files

**Testing Process:**
```
Similar to LoadPfr, but for supplement PFRs
```

**Database Setup:** ?? MODERATE (same as LoadPfr)

**Time Estimate:** 30 minutes

---

#### 11. EGrantsAcmAuditReport
**Setup Required:**
- ?? Database: Report processing stored procedures
- ? File System: Audit report files in source directory

**Testing Process:**
```
1. Place test audit report file in source directory
2. Run EGrantsAcmAuditReport.exe
3. Verify file processing and database insert
```

**Database Setup:** ?? MODERATE

**Time Estimate:** 20 minutes

---

## Fast-Track Testing Strategy for 5-Week Timeline

### Week 1: Email-Focused Testing (2-3 hours total)

**Day 1-2 (June 16-17): Critical Email Processors**
1. Router (30 min) ? Email-focused
2. ExchangeFixed (30 min) ? Email-focused
3. AddSuppProd (20 min) ? Email-focused

**Day 3 (June 18): Remaining Email Processors**
4. ~~DocManEmail~~ ? DEPRECATED (excluded from migration)
5. OGARequestAccountDisable (15 min) ? Email-focused
6. AddSuppVoteCollection (15 min) ? Email-focused
7. StartOutlook (5 min) ? No setup

**Day 5 (June 20): Database & File Processors**
8. AddSuppEmailer (30 min) ?? Database setup
9. LoadPfr (30 min) ?? File + Database
10. LoadSuppPfr (30 min) ?? File + Database
11. EGrantsAcmAuditReport (20 min) ?? File + Database

**TOTAL TESTING TIME: ~4 hours**

---

## Minimal Database Setup Script

For fast-track testing, you only need to set up **one valid application ID** and **one notification record**:

```sql
-- Get a valid application ID from test database
DECLARE @TestApplId VARCHAR(20)
SELECT TOP 1 @TestApplId = applid FROM applications WHERE applid IS NOT NULL
PRINT 'Use this Application ID for testing: ' + @TestApplId

-- Insert test notification for AddSuppEmailer
INSERT INTO dbo.adsup_Notification_email_status 
(Notification_id, email_date, email_send_status)
VALUES 
(99999, GETDATE(), 'Pending');

PRINT 'Test notification inserted: 99999'

-- Verify stored procedures exist
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Create_PFR' AND type = 'P')
    PRINT '? Create_PFR exists'
ELSE
    PRINT '? Create_PFR missing - LoadPfr will fail'

IF EXISTS (SELECT * FROM sys.objects WHERE name = 'getPlaceHolder_new' AND type = 'P')
    PRINT '? getPlaceHolder_new exists'
ELSE
    PRINT '? getPlaceHolder_new missing - LoadSuppPfr will fail'

-- Done!
PRINT ''
PRINT 'Database setup complete!'
PRINT 'Use Application ID: ' + @TestApplId + ' in all test emails'
```

**Run this once, get an Application ID, and you're ready to test!**

---

## The Reality

### For Manual Testing (Week 1):
- **80% of testing = Place emails in folders** ??
- **15% of testing = Place files in watch directories** ??
- **5% of testing = One-time database setup** ???

### For Automated Tests (Already Done!):
- **598 tests already passing** ?
- Tests handle all the complex database mocking
- You don't need to recreate this for manual testing

---

## Quick Test Data Package

Create these 3 test emails once, reuse for all testing:

### Test Email 1: FCOI (for Router)
```
To: NCIOGAeGrantsTest@mail.nih.gov
Subject: Receipt of a New FCOI report 27381 for grant number: 5U01CA265713-03
Body: Test FCOI notification
```

### Test Email 2: Document Submission (for ExchangeFixed)
```
To: NCIOGAeGrantsTest@mail.nih.gov
Subject: category=Correspondence, applid=12345678, extract=1, Test Document Submission
Body: This is a test document submission
Attachment: TestDocument.pdf
```

### Test Email 3: Supplement Request (for AddSuppProd)
```
To: NCIOGAeGrantsTest@mail.nih.gov  
Subject: Supplement Request
Body: Please process supplement for Application ID: 12345678
Attachment: SupplementRequest.pdf
```

**That's it!** These 3 emails cover most of your testing needs.

---

## Bottom Line

### Good News for Your Timeline:
1. ? **Most testing is email-based** (just drag & drop into folders)
2. ? **Minimal database setup required** (run one SQL script, get one ID)
3. ? **Can test 7 of 11 executables in 2 hours** (email-focused ones)
4. ? **Automated tests already validate complex logic** (598 tests passing)
5. ? **Manual testing is about integration validation**, not exhaustive testing

### Your Week 1 Plan:
- **Day 1:** Set up test environment + get one valid Application ID
- **Day 2-3:** Test email processors (7 executables, ~2 hours total)
- **Day 4-5:** Test database/file processors (4 executables, ~2 hours total)

**You can complete all manual testing in Week 1 with minimal database effort!**

---

## What You DON'T Need to Do

? Create complex test data scenarios  
? Set up multiple application IDs  
? Recreate all 598 automated test scenarios manually  
? Validate business logic (tests already do this)  
? Test edge cases (tests already do this)  

## What You DO Need to Do

? Verify executables run without crashing  
? Verify emails/files are processed  
? Verify database connections work  
? Verify logs are created  
? Verify Task Scheduler integration works  

**Focus on integration smoke testing, not comprehensive testing. You've already got comprehensive test coverage!** ??

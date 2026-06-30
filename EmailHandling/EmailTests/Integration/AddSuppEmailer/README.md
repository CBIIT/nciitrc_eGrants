# AddSuppEmailer Test Suite

This directory contains comprehensive tests for the **AddSuppEmailer** (Administrative Supplement Emailer) project.

## Overview

The `AddSuppEmailer` project queries the database for pending supplement notifications and sends Outlook emails with voting buttons ("Accepted"/"Rejected"). The emails are sent to PIs and administrators to notify them about administrative supplement opportunities.

## Test Structure

### 1. **AddSuppEmailerUnitTests.cs** (Unit Tests)
Tests individual methods and logic in isolation without external dependencies.

**Test Categories:**
- **Notification Processing Tests** (3 tests)
  - Single notification processing
  - Multiple notifications tracking
  - Distinct notification ID preservation

- **Email Content Tests** (4 tests)
  - Subject setting
  - Body setting
  - Recipients setting
  - Notification ID in body

- **Voting and Formatting Tests** (3 tests)
  - Voting options ("Accepted;Rejected")
  - High importance setting
  - HTML format setting

- **Reset Tests** (4 tests)
  - Email records clearing
  - Notification counter reset
  - Error state clearing
  - Simulated data clearing

- **Timestamp Tests** (2 tests)
  - Timestamp setting
  - Distinct timestamps for multiple emails

- **Error Handling Tests** (1 test)
  - Normal processing without errors

- **Verbose Mode Tests** (2 tests)
  - Verbose mode processing
  - Non-verbose mode processing

- **Edge Cases** (5 tests)
  - Zero notification ID
  - Large notification ID
  - Empty email body
  - HTML special characters
  - International characters

**Total:** 24 unit tests

### 2. **AddSuppEmailerTests.cs** (Scenario Tests)
Tests end-to-end email creation scenarios without requiring Outlook or database.

**Test Categories:**
- **Email Creation Tests** (3 tests)
  - Recipient setting
  - Subject with notification ID
  - Custom subject usage

- **Voting Options Tests** (3 tests)
  - Voting options setting
  - "Accepted" option presence
  - "Rejected" option presence

- **Email Importance Tests** (1 test)
  - High importance flag

- **Email Format Tests** (3 tests)
  - HTML body format
  - Notification ID in HTML body
  - Custom HTML body

- **Processing Counter Tests** (2 tests)
  - Counter increment
  - Reset functionality

- **Multiple Recipients Tests** (1 test)
  - Multiple recipient handling

- **Scenario-Based Tests** (11 tests)
  - Single PI notification
  - Multiple stakeholders
  - Batch processing (5 notifications)
  - HTML-formatted body
  - Diversity supplement
  - Urgent notification
  - After system restart
  - International characters
  - Long recipient list (20 recipients)
  - Embedded notification ID

**Total:** 24 scenario tests

### 3. **AddSuppEmailerIntegrationTests.cs** (Integration Tests)
Tests database connectivity and functions for email generation.

**Prerequisites:**
- Environment variables: `EGRANTS_DB_USER`, `EGRANTS_DB_PASSWORD`
- **EIM database** with required functions:
  - `fn_adsupp_getemail_subject(notification_id)` - Returns email subject
  - `fn_adsupp_getemail_body(notification_id)` - Returns email body HTML
  - `fn_adsupp_getemail_string(notification_id, email_type)` - Returns recipients
  - `adsup_Notification_email_status` table must exist

**Database Connection:**
- Server: `NCIDB-D387-V.nci.nih.gov\MSSQLEGRANTSQ` (port 52000)
- Database: `EIM`

**Test Categories:**
- **Database Connection Tests** (1 test)
  - Connection validation

- **Database Function Tests** (4 tests)
  - `fn_adsupp_getemail_subject` existence
  - `fn_adsupp_getemail_body` existence
  - `fn_adsupp_getemail_string` existence
  - `adsup_Notification_email_status` table existence
  - Notification query structure

- **Database Helper Method Tests** (4 tests)
  - `GetEmailSubject` method
  - `GetEmailBody` method
  - `GetEmailRecipients` for TO
  - `GetEmailRecipients` for CC

- **Configuration Tests** (2 tests)
  - Environment variables reading
  - Processor instantiation

- **File I/O Tests** (2 tests)
  - Log directory creation
  - Test log file writing

**Note:** Integration tests are skipped if database credentials are not configured.

**Total:** 13 integration tests

## Running Tests

### Run All AddSuppEmailer Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppEmailer"
```

### Run Only Unit Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppEmailerUnitTests"
```

### Run Only Scenario Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppEmailerTests"
```

### Run Only Integration Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppEmailerIntegrationTests"
```

### Run with Detailed Output
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppEmailer" --logger "console;verbosity=detailed"
```

## Configuration for Integration Tests

Integration tests require database credentials to be set as **environment variables**:

### Set Environment Variables (PowerShell)

```powershell
# Set for current user (recommended for development)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)
```

### Verify Environment Variables

```powershell
[System.Environment]::GetEnvironmentVariable('EGRANTS_DB_USER', [System.EnvironmentVariableTarget]::User)
```

**Important:** After setting environment variables, restart Visual Studio for changes to take effect.

See `../ENVIRONMENT_VARIABLES.md` for comprehensive configuration guide.

## Test Coverage Summary

| Test Suite | Tests | Focus Area |
|------------|-------|------------|
| Unit Tests | 24 | Helper methods, logic isolation |
| Scenario Tests | 24 | End-to-end email workflows (mocked) |
| Integration Tests | 13 | Database functions and connectivity |
| **Total** | **61** | **Comprehensive coverage** |

## Helper Classes

### TestAddSuppProcessor
Extends `Processor` to enable testing without Outlook/database dependencies.

**Key Features:**
- Overrides `ProcessNotification` to capture email details instead of sending
- Tracks all emails that would have been sent
- Provides simulated subject, body, and recipients for testing
- Exposes counters and error state

**Properties:**
- `EmailsSentThisSession` - List of captured email records
- `NotificationsProcessed` - Count of notifications processed
- `ErrorOccurred` - Error flag
- `LastErrorMessage` - Last error message
- `SimulatedSubject` - Test subject override
- `SimulatedBody` - Test body override
- `SimulatedRecipients` - Test recipients override

**Methods:**
- `TestProcessSingleNotification(notifId, verbose)` - Process single notification for testing
- `Reset()` - Clear all state and counters

### TestEmailRecord
Record class for storing captured email details during testing.

**Properties:**
- `To`, `Subject`, `Body`
- `VotingOptions`, `Importance`, `BodyFormat`
- `TimeCaptured`

## Email Features Tested

? **Voting Buttons:** "Accepted" / "Rejected"  
? **High Importance:** All emails marked as high importance  
? **HTML Body Format:** Rich HTML content support  
? **Multiple Recipients:** TO and CC support  
? **Notification ID Tracking:** Embedded in email body  
? **Batch Processing:** Multiple notifications in one run  
? **International Characters:** UTF-8 support  
? **Development Mode:** First email sent, others logged

## Manual Testing

For full end-to-end testing with actual Outlook and database:

1. **Set Environment:**
   ```powershell
   $env:DOTNET_ENVIRONMENT="Development"
   ```

2. **Configure Debug Email:**
   Update `appsettings.Development.json`:
   ```json
   {
     "AppSettings": {
       "DebugEmail": "your.test.email@nih.gov",
       "Verbose": "y"
     }
   }
   ```

3. **Run Application:**
   ```powershell
   cd AddSuppEmailer
   dotnet run
   ```

4. **Verify:**
   - First email is sent to debug email address
   - Subsequent emails are logged only
   - Subject prefixed with `[TEST]`
   - Database status updated for sent email

## Maintenance

When adding new functionality to `AddSuppEmailer.Processor`:
1. Add unit tests for new helper methods
2. Add scenario tests for new email types or features
3. Update integration tests if new database functions are added
4. Ensure all tests pass before committing

## Test Results

**Latest Run:**
- ? **61 tests available**
- ? **Build successful**
- ? **Integration tests run when credentials are set**
- ? **Integration tests skip gracefully when credentials are not set**

---

**Related Documentation:**
- `../ENVIRONMENT_VARIABLES.md` - Environment variable configuration
- `../README.md` - Email handling test suite overview

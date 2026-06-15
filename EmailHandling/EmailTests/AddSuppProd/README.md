# AddSuppProd Test Suite

This directory contains comprehensive tests for the **AddSuppProd** (Administrative Supplement Production) email processing project.

## Overview

The `AddSuppProd` project processes emails from the `NCIOGASupplements` public folder and routes them based on sender:
- **System notifications** from `nciogaegrantsprod`
- **eRA notifications** from `caeranotifications`
- **Staff manual uploads** from authorized staff (driskelleb, jonesni, omairi, woldezf)
- **PD/PI replies** to supplement notifications

## Test Structure

### 1. **AddSuppProdUnitTests.cs** (Unit Tests)
Tests individual helper methods in isolation without external dependencies.

**Test Categories:**
- **ExtractNotificationID Tests** (6 tests)
  - Valid notification ID extraction
  - Case-insensitive matching
  - Missing pattern handling
  - Empty/null input handling

- **RemoveSpecialCharacters Tests** (9 tests)
  - Special character replacement (`:`, `/`, `\`, `&`, `;`, `<`, `>`, `^`, `%`, `@`, `'`)
  - Space removal
  - Null/empty string handling

- **ParseSubjectParameters Tests** (7 tests)
  - Staff upload parameter parsing
  - Case-insensitive key matching
  - Whitespace trimming
  - Invalid parameter handling

- **GetFileExtension Tests** (8 tests)
  - Extension extraction from filenames
  - Multiple dots handling
  - Case conversion
  - Default fallback to `txt`

- **Edge Cases and Integration** (3 tests)
  - Real-world grant number formatting
  - Realistic email body parsing
  - Complete staff upload scenarios

**Total:** 33 unit tests

### 2. **AddSuppProdTests.cs** (Scenario Tests)
Tests end-to-end processing scenarios without requiring Outlook or database.

**Test Categories:**
- **Processing Tests** (4 tests)
  - Single item processing
  - Subject/body/sender capture

- **Multiple Items Tests** (3 tests)
  - Batch processing
  - Counter management

- **Item Movement Tests** (2 tests)
  - Archive folder movement

- **Timestamp Tests** (2 tests)
  - Received/processed time tracking

- **Reset Tests** (2 tests)
  - State cleanup

- **Error Handling Tests** (1 test)
  - Normal processing verification

- **Simulated Mail Item Tests** (4 tests)
  - Mock email creation

- **Scenario-Based Tests** (11 tests)
  - System notifications
  - eRA notifications
  - Staff correspondence and application files
  - PD/PI replies
  - Unknown senders
  - Batch processing of multiple types
  - Authorized staff handling
  - Diversity supplements
  - Status changes
  - Response required notifications

**Total:** 29 scenario tests

### 3. **AddSuppProdIntegrationTests.cs** (Integration Tests)
Tests database connectivity and full workflow with real database functions.

**Prerequisites:**
- `secrets.local.csv` configured with database credentials
- **EIM database** with required stored procedures and functions:
  - `getPlaceHolder_new` (stored procedure)
  - `fn_PA_match` (function)
  - `Imm_fn_applid_match` (function)
  - `adsup_notification` (table)
  - `adsup_Notification_email_status` (table)

**Database Connection:**
- Connects to the same **EIM database** that production uses
- Server: `NCIDB-D387-V.nci.nih.gov\MSSQLEGRANTSQ` (port 52000)
- Database: `EIM` (NOT eGrants or eGrants_test)
- Uses credentials from `EGRANTS_DB_USER` and `EGRANTS_DB_PASSWORD` environment variables

**Test Categories:**
- **Database Helper Method Tests** (2 tests)
  - `GetApplIdFromText` with real DB function
  - `GetPAFromText` with real DB function

- **Database Connection Tests** (5 tests)
  - Connection validation
  - Table existence verification
  - Stored procedure existence
  - Function existence

- **File I/O Tests** (2 tests)
  - Output directory creation
  - File write/read operations

- **Configuration Tests** (2 tests)
  - Secret loading from CSV
  - Processor instantiation

**Note:** Integration tests are skipped if database credentials are not configured.

**Total:** 11 integration tests (7 may be skipped without DB)

## Running Tests

### Run All AddSuppProd Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppProd"
```

### Run Only Unit Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppProdUnitTests"
```

### Run Only Scenario Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppProdTests"
```

### Run Only Integration Tests
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppProdIntegrationTests"
```

### Run with Detailed Output
```powershell
dotnet test EmailTests\EmailHandlingTests.csproj --filter "FullyQualifiedName~AddSuppProd" --logger "console;verbosity=detailed"
```

## Configuration for Integration Tests

Integration tests require database credentials to be set as **environment variables** (NOT in a file):

### Set Environment Variables (PowerShell)

```powershell
# Set for current user (recommended for development)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)

# Or set for entire machine (requires admin, recommended for servers)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_USER', 'your_username', [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('EGRANTS_DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::Machine)
```

### Verify Environment Variables

```powershell
# Check if variables are set
[System.Environment]::GetEnvironmentVariable('EGRANTS_DB_USER', [System.EnvironmentVariableTarget]::User)
[System.Environment]::GetEnvironmentVariable('EGRANTS_DB_PASSWORD', [System.EnvironmentVariableTarget]::User)
```

**Important:** After setting environment variables, restart Visual Studio or your terminal for changes to take effect.

### Why Environment Variables?

- ? **Security:** Credentials never stored in files or committed to source control
- ? **Consistency:** Same approach used by production applications
- ? **Standards:** Follows .NET and cloud deployment best practices
- ? **Simplicity:** No intermediate CSV files to manage

## Test Coverage Summary

| Test Suite | Tests | Focus Area |
|------------|-------|------------|
| Unit Tests | 33 | Helper methods, text processing |
| Scenario Tests | 29 | End-to-end workflows (mocked) |
| Integration Tests | 11 | Database and file I/O |
| **Total** | **73** | **Comprehensive coverage** |

## Helper Classes

### TestAddSuppProdProcessor
Extends `Processor` to enable testing without Outlook/database dependencies.

**Key Features:**
- Tracks processed items in-memory
- Simulates email processing
- Exposes internal methods for unit testing
- Provides reset functionality for test isolation

**Exposed Test Methods:**
- `TestExtractNotificationID(string body)`
- `TestRemoveSpecialCharacters(string text)`
- `TestParseSubjectParameters(string subject)`
- `TestGetFileExtension(string fileName)`

### TestProcessedItem
Record class for storing processed email details during testing.

**Properties:**
- `Subject`, `Body`, `SenderEmail`
- `ReceivedTime`, `ProcessedTime`
- `WasMovedToOld`

### SimulatedMailItem
Mock email item for testing without Outlook.

**Properties:**
- `Subject`, `Body`, `SenderEmail`, `ReceivedTime`

## Manual Testing

For full end-to-end testing with actual Outlook and database, see the parent README for instructions on:
1. Setting up test Outlook folders
2. Creating test emails
3. Running the application in development mode
4. Verifying database entries and file creation

## Maintenance

When adding new functionality to `AddSuppProd.Processor`:
1. Add unit tests for any new helper methods
2. Add scenario tests for new email types or workflows
3. Update integration tests if new database interactions are added
4. Ensure all tests pass before committing changes

## Test Results

Last test run:
- ? **65 tests passed**
- ?? **7 tests skipped** (integration tests without DB)
- ? **0 tests failed**
- ?? **16.97 seconds** total time

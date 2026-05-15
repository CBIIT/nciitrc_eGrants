# NCI ITRC eGrants

Source repository for eGrants application and supporting resources.

## Repository Structure

```
eGrants/
??? EmailHandling/        # Email processing automation suite
?   ??? AddSuppEmailer/      # Sends administrative supplement notification emails
?   ??? AddSuppProd/  # Processes supplement production emails
?   ??? AddSuppVoteCollection/   # Collects voting responses for supplement requests
?   ??? CommonUtilties/      # Shared utility library with Serilog logging
?   ??? DocManEmail/         # Document management email processor
?   ??? EGrantsAcmAuditReport/   # ACM audit report file processor
?   ??? EmailTests/# Integration and unit tests
?   ??? ExchangeFixed/       # eFile/Exchange email processor
?   ??? LoadPfr/    # Progress/Final Report loader
?   ??? LoadSuppPfr/         # Supplement Progress/Final Report loader
?   ??? OGARequestAccountDisable/ # Account deactivation notifications
?   ??? Router/      # Main email routing engine
?   ??? StartOutlook/        # Outlook startup utility
??? [Other eGrants components]
```

---

## Web Configuration

When the repo is cloned you will notice a number of web.config files.

**Purpose:**
1. **Web.Base.Config** - This file contains the web.config file that is required to run on the server. 
   - Whenever the code is compiled with the "Debug" configuration the debug flag IS NOT removed.
   ```xml
   <system.web>
       <compilation debug="true" targetFramework="4.7.2"/>
       ...
   </system.web>
   ```
   - Whenever the code is compiled with the "Release" configuration the debug flag IS removed and the code is also "Optimized"

2. **Web.Config** - This file contains the web.config file after the transformations have been applied and is then used in the execution of the application.

---

# EmailHandling Solution

## Overview

The EmailHandling solution contains a collection of .NET 8 console applications that automate email processing, document management, and account administration for the NIH NCI eGrants system. These applications integrate with Microsoft Outlook via COM interop and SQL Server databases to route, process, and archive grant-related correspondence.

---

## Logging

All EmailHandling applications use **Serilog** for structured logging with the following features:

### Log Configuration
- **Daily Rolling Files**: Log files roll over daily with pattern `{ApplicationName}-{yyyy-MM-dd}.log`
- **Retention**: 31 days of log files retained
- **File Size Limit**: 10MB per file with automatic rollover
- **Console Output**: Color-coded log levels with timestamps

### Log Levels
| Level | Usage |
|-------|-------|
| **Verbose** | Detailed tracing (subfolder navigation) |
| **Debug** | Diagnostic info (config values, item details) |
| **Information** | Normal operations (start/stop, items processed) |
| **Warning** | Non-critical issues (legacy log failures) |
| **Error** | Exceptions and processing failures |

### Structured Parameters
Log entries include named parameters for easy filtering and querying:
- `{ApplicationName}` - Name of the running application
- `{ItemCount}` - Number of items processed
- `{NotificationId}` - Notification being processed
- `{VoteType}` - "Accepted" or "Rejected" vote
- `{Subject}` - Email subject line
- `{Sender}` - Email sender name

### Usage Example
```csharp
// Initialize logging at application start
CommonUtilities.InitializeLogging("MyApplication", logDir);

// Log with structured parameters
CommonUtilities.Logger.Information("Processing {ItemCount} items", count);
CommonUtilities.Logger.Error(ex, "Failed to process {NotificationId}", id);

// Close logging at application end
CommonUtilities.CloseLogging();
```

---

## Projects

### AddSuppEmailer
**Purpose:** Sends administrative supplement notification emails with voting options.

**Key Features:**
- Queries database for pending supplement notifications
- Creates Outlook emails with "Accepted/Rejected" voting buttons
- Marks emails as high importance
- Uses HTML body format
- Tracks email send status in database
- Structured logging with Serilog

**Log File:** `AddSuppEmailer-{date}.log`

**Database Tables:**
- `dbo.adsup_Notification_email_status`

**Database Functions:**
- `dbo.fn_adsupp_getemail_subject()`
- `dbo.fn_adsupp_getemail_body()`
- `dbo.fn_adsupp_getemail_string()`

---

### AddSuppProd
**Purpose:** Processes administrative supplement production emails by moving them to archive folders.

**Key Features:**
- Monitors specified Outlook folder for supplement emails
- Moves processed items to "old" archive folder
- Structured logging with Serilog

**Log File:** `AddSuppProd-{date}.log`

---

### AddSuppVoteCollection
**Purpose:** Collects and forwards voting responses for administrative supplement requests.

**Key Features:**
- Monitors Outlook folder for vote responses
- Detects "Accepted:" or "Rejected:" in email subjects
- Forwards responses to designated staff (emily.driskell@nih.gov, jonesni@mail.nih.gov)
- Archives processed emails to "AddSupp_Vote" folder
- Adds "DO NOT REPLY" prefix to forwarded subjects
- Structured logging with vote type tracking

**Log File:** `AddSuppVoteCollection-{date}.log`

---

### CommonUtilties
**Purpose:** Shared utility library used by all projects.

**Logging (Serilog):**
- `InitializeLogging()` - Initialize Serilog with file and console sinks
- `CloseLogging()` - Flush and close all log sinks
- `Logger` - Static ILogger instance for structured logging

**Configuration:**
- `GetConfigVal()` - Configuration file reader (config.csv)
- Uses `config.csv` with `,,,,,` delimiter format
- Supports: `logDir`, `conStr`, `Verbose`, `dBug`, directory paths

**String Utilities:**
- `ShowDiagnosticIfVerbose()` - Conditional diagnostic output
- `RemoveSpaceCharacters()` - Text sanitization for grant numbers
- `ExtractElement()` - Extract nth comma-separated element
- `ExtractValue()` - Extract value from "name=value" pair
- `GetLastWord()` - Get last word from space-separated string
- `GetNthWord()` - Get nth word from space-separated string

**File Utilities:**
- `WriteLog()` - Legacy logging (also logs to Serilog)
- `GetFileType()` - Extract file extension
- `RemoveJunk()` - Sanitize filename characters

---

### DocManEmail
**Purpose:** Processes document management emails and saves attachments to the file system.

**Key Features:**
- Extracts document metadata from email subjects (cpiid, docid, catid, date)
- Saves PDF attachments with database-generated filenames
- Calls stored procedure `SP_CREATE_DOCMAN_DOCUMENT_NEW`
- Processes up to 50 items per run

**Subject Line Format:**
```
cpiid=12345, catid=ABC, num=1, date=2024-01-15
```

---

### EGrantsAcmAuditReport
**Purpose:** Processes ACM (Access Control Management) audit report Excel files.

**Key Features:**
- Monitors source directory for `.xls*` files
- Inserts report metadata into `dbo.egrants_audit_report` table
- Copies files to backup and image server locations
- Removes processed source files

**Database Table:**
- `dbo.egrants_audit_report` (Report_name, File_name, Run_date, url)

---

### ExchangeFixed
**Purpose:** Processes eFile/Exchange emails and extracts content to the document management system.

**Key Features:**
- Parses structured email subjects for grant metadata
- Extracts: grantnumber, category, applid, sub-category, extract mode
- Saves email body as `.txt` and/or attachments based on extract flag
- Calls `getPlaceHolder_new` stored procedure
- Uses `dbo.Imm_fn_applid_match()` to resolve grant numbers

**Extract Modes:**
- `1` = Save email body only
- `2` = Save attachment only  
- `3` = Save both body and attachment

---

### LoadPfr
**Purpose:** Loads Progress/Final Reports (PFR) from XML metadata files.

**Key Features:**
- Processes XML files containing document metadata
- Extracts: applid, folderid, filename, date, file_type, uid
- Calls `Create_PFR` stored procedure
- Copies PDF files to final destination with renamed filenames
- Archives processed XML and PDF files

---

### LoadSuppPfr
**Purpose:** Loads Supplement Progress/Final Reports from XML metadata files.

**Key Features:**
- Similar to LoadPfr but for supplement-specific reports
- Processes XML files with supplement metadata
- Calls `getPlaceHolder_new` stored procedure
- Copies and renames PDF files based on database output

---

### OGARequestAccountDisable
**Purpose:** Manages account deactivation notifications for inactive eGrants users.

**Key Components:**

#### Processor (Deactivation Notifications)
- Queries users from `dbo.people_for_oga_to_disable`
- Filters users with valid name information
- Sends HTML email to OGA team listing deactivated accounts
- Updates `sent_to_oga_date` after notification

#### ProcessorWarning (Warning Notifications)
- Identifies users inactive for 46+ days
- Sends warning emails before 60-day deactivation deadline
- Tracks warning status in `dbo.people_sent_warning` table
- Re-sends warnings if user approaches deactivation date

**Email Recipients:**
- Production: `NCIOGABOBTeam2@mail.nih.gov`
- Development: `eGrantsDev@mail.nih.gov`

---

### Router
**Purpose:** Main email routing engine that processes incoming eRA/NIH notifications and forwards them to appropriate recipients.

**Key Features:**
- Routes emails based on subject line patterns
- Handles 20+ different email types including:
  - eSNAP/RPPR notifications
  - JIT requests
  - FCOI notifications
  - Public Access compliance
  - No Cost Extensions
  - Prior Approvals
  - FFR notifications
  - Closeout reminders
  - SBIR/STTR risk management
- Extracts grant numbers and resolves to application IDs
- Forwards to eFile system for document management
- Archives processed emails to "Old emails" folder
- Sends error notifications to administrators

**Email Type Examples:**
| Subject Pattern | Action |
|-----------------|--------|
| "eSNAP Received at NIH" | Forward to Bryan/Nicole |
| "JIT Request for Grant" | Forward to eFile with category tag |
| "FCOI" | Lookup specialist email and forward |
| "No Cost Extension Submitted" | Forward to eFile |
| "Public Access" | Extract applid, forward with category |

**Safety Features:**
- Limits to 50 items per run
- Configurable processing delay between items
- Error notification emails to administrators
- Debug mode to prevent actual email sending

---

### StartOutlook
**Purpose:** Simple utility to start Microsoft Outlook.

**Usage:** Ensures Outlook is running before other email processing applications execute.

```csharp
System.Diagnostics.Process.Start("outlook.exe");
```

---

### EmailTests
**Purpose:** Integration and unit tests for all projects.

**Test Coverage:**
- Router email routing logic
- Vote detection and forwarding
- User filtering for OGA notifications
- Common utilities (text processing, logging)
- Document processing workflows

**Test Folders:**
- `AddSuppEmailer/` - Supplement emailer tests
- `AddSuppProd/` - Supplement production tests
- `AddSuppVoteCollection/` - Vote collection tests
- `CommonUtilities/` - Utility function tests
- `DocManEmail/` - Document email tests
- `EGrantsAcmAuditReport/` - Audit report tests
- `ExchangeFixed/` - Exchange processor tests
- `LoadPfr/` - PFR loader tests
- `LoadSuppPfr/` - Supplement PFR tests
- `OGADisableEmail/` - Account disable tests
- `Router/` - Email routing tests
- `StartOutlook/` - Startup utility tests

---

## Configuration

All EmailHandling applications read configuration from `config.csv` using a `,,,,,` delimiter (5 commas):

```csv
logDir,,,,,C:\Logs\EmailHandling
conStr,,,,,Server=myserver;Database=eGrants;Trusted_Connection=True
Verbose,,,,,y
dBug,,,,,n
dirpathRouter,,,,,Public Folders\All Public Folders\eGrants\Inbox
dirpathSupplement,,,,,Public Folders\All Public Folders\NCI\GAB\NCIOGASupplements
dirpathVoteCollection,,,,,Public Folders\All Public Folders\NCI\GAB\VoteCollection
routingBreakDuration,,,,,1000
OutDir,,,,,C:\eGrants\watch\out
```

## Dependencies

### EmailHandling Projects
- **.NET 8.0** (Windows)
- **Microsoft.Office.Interop.Outlook** (15.0.4797.1004)
- **System.Data.SqlClient** (4.8.6)
- **Serilog** (latest) - Structured logging
- **Serilog.Sinks.File** - File logging with rolling
- **Serilog.Sinks.Console** - Console output
- **Microsoft Office** (Outlook must be installed and configured)

## Building

```bash
# Build entire solution
dotnet build eGrants.sln

# Build EmailHandling only
dotnet build EmailHandling/EmailHandling.sln
```

## Running Tests

```bash
dotnet test EmailHandling/EmailTests/EmailTests.csproj
```

## Deployment Notes

### EmailHandling Applications
1. Ensure Microsoft Outlook is installed and configured with appropriate mailbox access
2. Configure `config.csv` with correct database connection strings and folder paths
3. Grant appropriate SQL Server permissions for stored procedures and tables
4. Schedule applications using Windows Task Scheduler as needed
5. Monitor log files in configured `logDir` for errors
6. Serilog creates daily rolling logs: `{AppName}-{date}.log`

## Error Handling

All EmailHandling applications include:
- Try-catch blocks around main processing loops
- Structured logging with Serilog (Information, Warning, Error levels)
- Legacy text file logging for backward compatibility
- Email notifications to administrators on critical failures
- Debug mode to prevent email sending during testing
- Proper log flushing on application shutdown

## Authors

NCI ITRC eGrants Development Team

## Repository

https://github.com/CBIIT/nciitrc_eGrants

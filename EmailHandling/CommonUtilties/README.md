# CommonUtilities

Shared utility library for eGrants Email Handling projects providing common functionality for configuration, logging, string utilities, and secrets management.

## Overview

The CommonUtilities library provides:
- **Configuration Management**: Read configuration from JSON files with environment variable expansion
- **Structured Logging**: Serilog-based logging with file and console sinks
- **String Utilities**: Common string manipulation and sanitization functions
- **Subject Line Parsing**: Extract key=value pairs from email subjects
- **File Utilities**: File type detection and filename sanitization

## Key Features

### Configuration Reading

**JSON Configuration (`appsettings.json`):**
```csharp
var config = AppConfig.Load();
string value = config["AppSettings:LogDir"];
string connStr = AppConfig.GetConnectionString(config, "EIM");
```

### Environment Variable Expansion

Connection strings and other sensitive configuration values reference environment variables using the `%VARIABLE_NAME%` syntax:

```json
{
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;"
  }
}
```

The `%VARIABLE_NAME%` syntax is automatically expanded using `Environment.ExpandEnvironmentVariables()`.

**Required Environment Variables:**
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

**Legacy Support:**
For backward compatibility during migration, `AppConfig` still accepts:
- `EGRANTS_DB_USER` (mapped to `DB_USER`)
- `EGRANTS_DB_PASSWORD` (mapped to `DB_PASSWORD`)

**Setting Environment Variables (Windows):**
```powershell
[System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)
```

### Structured Logging

**Initialize logging:**
```csharp
CommonUtilities.InitializeLogging("ApplicationName", logDirectory, LogEventLevel.Information);
```

**Log messages:**
```csharp
CommonUtilities.Logger.Information("Processing started");
CommonUtilities.Logger.Error(ex, "Error processing file: {FileName}", fileName);
CommonUtilities.Logger.Debug("Cache hit for key: {Key}", key);
```

**Verbose diagnostics:**
```csharp
CommonUtilities.ShowDiagnosticIfVerbose("Processing email...", verbose);
```

**Legacy logging:**
```csharp
CommonUtilities.WriteLog(8, "Task completed", null, DateTime.Now);
```

**Close logging:**
```csharp
CommonUtilities.CloseLogging();
```

### String Utilities

**Remove special characters:**
```csharp
string cleaned = CommonUtilities.RemoveSpaceCharacters(input);
// Removes: : / \ & ; < > ^ % @ ' and spaces
// Replaces: & with "and"
```

**Remove junk from filenames:**
```csharp
string cleanName = CommonUtilities.RemoveJunk(fileName);
// Removes: : / \ & ;
// Replaces: & with "and"
```

### Subject Line Parsing

**Extract elements by position:**
```csharp
string element = CommonUtilities.ExtractElement("a,b,c,d", 2); // Returns "b"
```

**Extract key=value pairs:**
```csharp
string value = CommonUtilities.ExtractValue("category=PublicAccess", "category");
// Returns "PublicAccess"
```

### File Utilities

**Get file extension:**
```csharp
string ext = CommonUtilities.GetFileType("document.pdf"); // Returns "pdf"
string ext = CommonUtilities.GetFileType("noext"); // Returns "txt" (default)
```

## Configuration File Formats

### appsettings.json Format

```json
{
  "AppSettings": {
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "Verbose": "n",
    "DirPath": "Public Folders - email@mail.nih.gov\\path\\to\\folder"
  },
  "ConnectionStrings": {
    "EIM": "Server=myserver;Database=EIM;User Id=%DB_USER%;Password=%DB_PASSWORD%;TrustServerCertificate=True"
  }
}
```

## Logging Configuration

### Log File Naming
- Pattern: `{ApplicationName}-YYYY-MM-DD.log`
- Example: `Router-2024-01-15.log`

### Log Rotation
- **Rolling Interval**: Daily
- **Retention**: 31 days
- **Size Limit**: 10 MB per file
- **Roll on Size**: Yes (creates numbered files)

### Log Output Template

**Console:**
```
[HH:mm:ss LEV] Message
Exception
```

**File:**
```
yyyy-MM-dd HH:mm:ss.fff zzz [LEV] Message
Exception
```

### Log Levels
- **Debug**: Verbose diagnostic information
- **Information**: Normal operational messages
- **Warning**: Potential issues that don't stop processing
- **Error**: Errors that prevent specific operations
- **Fatal**: Critical errors that stop the application

## Best Practices

### Secrets Management

? **DO:**
- Store secrets in `secrets.local.csv` (gitignored)
- Use environment variables in production
- Reference secrets using `%VARIABLE_NAME%` syntax
- Provide `secrets.template.csv` for documentation

? **DON'T:**
- Hardcode secrets in source code
- Commit secrets to source control
- Share secrets file via email or chat
- Use the same secrets for dev/test/prod

### Configuration Management

? **DO:**
- Use `config.csv` for application-specific settings
- Use `appsettings.json` for structured configuration
- Use environment variable expansion for sensitive values
- Document all configuration keys

? **DON'T:**
- Store connection strings directly in config files
- Hardcode paths or URLs
- Mix secrets with configuration

### Logging

? **DO:**
- Initialize logging at application startup
- Use structured logging with named parameters
- Close logging in finally blocks
- Use appropriate log levels

? **DON'T:**
- Log sensitive information (passwords, PII)
- Log in tight loops (use sparingly)
- Forget to close/flush logs on exit

## Usage Examples

### Basic Application Setup

```csharp
using CommonUtilties;
using Serilog.Events;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Load secrets
            CommonUtilities.LoadLocalSecrets("secrets.local.csv");

            // Load configuration
            var config = AppConfig.Load();
            string logDir = config["AppSettings:LogDir"];

            // Initialize logging
            CommonUtilities.InitializeLogging("MyApp", logDir, LogEventLevel.Information);
            CommonUtilities.Logger.Information("Application started");

            // Your application logic here

            CommonUtilities.Logger.Information("Application completed");
        }
        catch (Exception ex)
        {
            CommonUtilities.Logger?.Fatal(ex, "Fatal error");
            Environment.Exit(1);
        }
        finally
        {
            CommonUtilities.CloseLogging();
        }
    }
}
```

### Reading Configuration

```csharp
// CSV configuration
string logDir = CommonUtilities.GetConfigVal("LogDir");
string verbose = CommonUtilities.GetConfigVal("Verbose");

// JSON configuration
var config = AppConfig.Load();
string logDir = config["AppSettings:LogDir"];
string connStr = AppConfig.GetConnectionString(config, "EIM");
```

### String Cleaning

```csharp
// Remove special characters for database lookups
string grantNumber = "5R01CA258784-04";
string cleaned = CommonUtilities.RemoveSpaceCharacters(grantNumber);
// Result: "5R01CA25878404"

// Clean filename
string filename = "Report:2024/01/15.xlsx";
string cleanName = CommonUtilities.RemoveJunk(filename);
// Result: "Report 2024 01 15.xlsx"
```

### Subject Line Parsing

```csharp
string subject = "category=PublicAccess, sub=Compliant, applid=12345, Original Subject";

// Extract by position
string category = CommonUtilities.ExtractElement(subject, 1); // "category=PublicAccess"

// Extract value
string categoryValue = CommonUtilities.ExtractValue(
    CommonUtilities.ExtractElement(subject, 1), 
    "category"
); // "PublicAccess"
```

## Dependencies

- **Serilog** (3.1.1): Structured logging framework
- **Serilog.Sinks.Console** (5.0.1): Console output
- **Serilog.Sinks.File** (5.0.0): File output with rolling
- **Microsoft.Extensions.Configuration** (8.0.0): Configuration abstraction
- **Microsoft.Extensions.Configuration.Json** (8.0.0): JSON configuration

## Target Framework

- .NET 8.0

## Notes

- All utility methods are static for ease of use
- Configuration files use a unique 5-comma delimiter for CSV parsing
- Environment variable expansion happens automatically
- Log directory is created automatically if it doesn't exist
- Serilog logger instance is accessible via `CommonUtilities.Logger`

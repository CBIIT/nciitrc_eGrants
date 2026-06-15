# AddSuppProd - Environment Variables Setup

## Database Credentials

The AddSuppProd application uses environment variables for database credentials. The connection string in `appsettings.json` uses placeholders:

```json
"ConnectionStrings": {
  "EIM": "Password=%DB_PASSWORD%;User ID=%DB_USER%;..."
}
```

**At runtime**, the application automatically replaces:
- `%DB_USER%` ? Value from environment variable `DB_USER` (or falls back to `EGRANTS_DB_USER`)
- `%DB_PASSWORD%` ? Value from environment variable `DB_PASSWORD` (or falls back to `EGRANTS_DB_PASSWORD`)

This is handled by `AppConfig.GetConnectionString()` which calls `ResolveEnvironmentVariables()`.

## Setup Instructions

### For Local Development (User-Level)

Set user-level environment variables in PowerShell:

```powershell
[System.Environment]::SetEnvironmentVariable("DB_USER", "your_dev_username", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("DB_PASSWORD", "your_dev_password", [System.EnvironmentVariableTarget]::User)
```

**Important:** Restart Visual Studio after setting environment variables.

### For Servers (Machine-Level)

Set machine-level environment variables in PowerShell **as Administrator**:

```powershell
[System.Environment]::SetEnvironmentVariable("DB_USER", "prod_username", [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable("DB_PASSWORD", "prod_password", [System.EnvironmentVariableTarget]::Machine)
```

**Or using Command Prompt as Administrator:**

```cmd
setx DB_USER "prod_username" /M
setx DB_PASSWORD "prod_password" /M
```

**Important:** Restart Windows Scheduled Tasks service after setting environment variables, or restart the machine.

## Verification

To verify environment variables are set:

**User-level (development):**
```powershell
[System.Environment]::GetEnvironmentVariable("DB_USER", [System.EnvironmentVariableTarget]::User)
```

**Machine-level (servers):**
```powershell
[System.Environment]::GetEnvironmentVariable("DB_USER", [System.EnvironmentVariableTarget]::Machine)
```

## Environment-Specific Settings

- **Development**: Uses `appsettings.Development.json` (set `DOTNET_ENVIRONMENT=Development`)
  - Uses development folder path (e.g., `NCIOGASupplements\RobinTest`)
  - Verbose logging enabled
  - Safe for testing

- **Production**: Uses `appsettings.json` (default when `DOTNET_ENVIRONMENT` is not set)
  - Uses production folder path
  - Standard logging
  - Normal production behavior

Each environment can point to different database servers via the connection string in its respective appsettings file.

## Task Scheduler Configuration

When running from Windows Task Scheduler, set the environment variable in the command line:

**Program/script:**
```
cmd.exe
```

**Add arguments:**
```
/c "set DOTNET_ENVIRONMENT=Development && C:\path\to\AddSuppProd.exe"
```

Note: Make sure there's no trailing space after "Development".

## How It Works

1. **Application starts** and loads `appsettings.json`
2. **Checks `DOTNET_ENVIRONMENT`** to determine if environment-specific config should be loaded
3. **Reads connection string** with placeholders: `"Password=%DB_PASSWORD%;User ID=%DB_USER%;..."`
4. **Calls `AppConfig.GetConnectionString()`** which invokes `ResolveEnvironmentVariables()`
5. **Replaces placeholders**:
   - Finds `%DB_USER%` ? Calls `Environment.GetEnvironmentVariable("DB_USER")` or `Environment.GetEnvironmentVariable("EGRANTS_DB_USER")`
   - Finds `%DB_PASSWORD%` ? Calls `Environment.GetEnvironmentVariable("DB_PASSWORD")` or `Environment.GetEnvironmentVariable("EGRANTS_DB_PASSWORD")`
6. **Returns resolved connection string** with actual credentials
7. **Connects to database** using the fully resolved connection string

## Troubleshooting

### Error: "Required environment variable 'DB_USER' is not set"

**Cause:** The environment variable is not configured.

**Solution:**
```powershell
# For local development
[System.Environment]::SetEnvironmentVariable("DB_USER", "your_username", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("DB_PASSWORD", "your_password", [System.EnvironmentVariableTarget]::User)

# Restart Visual Studio
```

### Application uses Production config instead of Development

**Cause:** The `DOTNET_ENVIRONMENT` variable is not set or has trailing spaces.

**Check:**
```powershell
$env:DOTNET_ENVIRONMENT
```

**Fix:**
```powershell
[System.Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development", [System.EnvironmentVariableTarget]::User)
```

### Changes to environment variables not taking effect

**Solution:**
- Restart the application (close all instances)
- Restart Visual Studio if running from IDE
- Restart Task Scheduler service if running from scheduled tasks
- When in doubt, restart Windows

## Security Notes

- **Never commit credentials** to source control
- Environment variables are stored in the Windows registry
- User-level variables: `HKEY_CURRENT_USER\Environment`
- Machine-level variables: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment`
- Set appropriate permissions on production servers to protect these registry keys

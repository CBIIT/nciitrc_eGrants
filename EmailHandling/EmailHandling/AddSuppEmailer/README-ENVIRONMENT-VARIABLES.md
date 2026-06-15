# Environment Variables Setup

## Database Credentials

The AddSuppEmailer application uses environment variables for database credentials. The connection string in `appsettings.json` uses placeholders:

```json
"ConnectionStrings": {
  "EIM": "Password=%EGRANTS_DB_PASSWORD%;User ID=%EGRANTS_DB_USER%;..."
}
```

**At runtime**, the application automatically replaces:
- `%EGRANTS_DB_USER%` ? Value from environment variable `EGRANTS_DB_USER`
- `%EGRANTS_DB_PASSWORD%` ? Value from environment variable `EGRANTS_DB_PASSWORD`

This is handled by `AppConfig.GetConnectionString()` which calls `ResolveEnvironmentVariables()`.

## Setup Instructions

### For Local Development (User-Level)

Set user-level environment variables in PowerShell:

```powershell
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_USER", "your_dev_username", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_PASSWORD", "your_dev_password", [System.EnvironmentVariableTarget]::User)
```

**Important:** Restart Visual Studio after setting environment variables.

### For Servers (Machine-Level)

Set machine-level environment variables in PowerShell **as Administrator**:

```powershell
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_USER", "prod_username", [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_PASSWORD", "prod_password", [System.EnvironmentVariableTarget]::Machine)
```

**Or using Command Prompt as Administrator:**

```cmd
setx EGRANTS_DB_USER "prod_username" /M
setx EGRANTS_DB_PASSWORD "prod_password" /M
```

**Important:** Restart Windows Scheduled Tasks after setting environment variables.

## Verification

To verify environment variables are set:

**User-level (development):**
```powershell
[System.Environment]::GetEnvironmentVariable("EGRANTS_DB_USER", [System.EnvironmentVariableTarget]::User)
```

**Machine-level (servers):**
```powershell
[System.Environment]::GetEnvironmentVariable("EGRANTS_DB_USER", [System.EnvironmentVariableTarget]::Machine)
```

## Environment-Specific Settings

- **Development**: Uses `appsettings.Development.json` (set `DOTNET_ENVIRONMENT=Development`)
  - **Emails are NOT sent** - only logged for review
  - Email recipients redirected to `DebugEmail` address
  - Notification status NOT updated in database
  - Safe for testing without sending real emails

- **Production**: Uses `appsettings.Production.json` (default when `DOTNET_ENVIRONMENT` is not set)
  - **Emails ARE sent** to actual recipients
  - Notification status updated in database
  - Normal production behavior

Each environment can point to different database servers via the connection string in its respective appsettings file.

## How It Works

1. **Application starts** and loads `appsettings.json`
2. **Reads connection string** with placeholders: `"Password=%EGRANTS_DB_PASSWORD%;User ID=%EGRANTS_DB_USER%;..."`
3. **Calls `AppConfig.GetConnectionString()`** which invokes `ResolveEnvironmentVariables()`
4. **Replaces placeholders**:
   - Finds `%EGRANTS_DB_USER%` ? Calls `Environment.GetEnvironmentVariable("EGRANTS_DB_USER")`
   - Finds `%EGRANTS_DB_PASSWORD%` ? Calls `Environment.GetEnvironmentVariable("EGRANTS_DB_PASSWORD")`
5. **Returns resolved connection string** with actual credentials
6. **Connects to database** using the fully resolved connection string

## Troubleshooting

### Error: "Required environment variable 'EGRANTS_DB_USER' is not set"

**Cause:** The environment variable is not configured.

**Solution:**
```powershell
# For local development
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_USER", "your_username", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("EGRANTS_DB_PASSWORD", "your_password", [System.EnvironmentVariableTarget]::User)

# Restart Visual Studio
```

### Error: "Cannot connect to database"

**Check credentials are set:**
```powershell
$user = [System.Environment]::GetEnvironmentVariable("EGRANTS_DB_USER", [System.EnvironmentVariableTarget]::User)
$pass = [System.Environment]::GetEnvironmentVariable("EGRANTS_DB_PASSWORD", [System.EnvironmentVariableTarget]::User)

if ($user -and $pass) {
    Write-Host "Credentials are set: User=$user, Password=$('*' * $pass.Length)"
} else {
    Write-Host "Credentials are NOT set!"
}
```

### Application shows: "WARNING: Database credentials not found"

**Solution:** Set the environment variables as described above and restart the application or Visual Studio.

# Process-Level Smoke Tests

## Overview

These smoke tests verify that all scheduler-run executables can be built, launched, and run successfully. They catch issues that unit tests and integration tests might miss, such as:

- ? Broken configuration files
- ? Missing runtime dependencies (DLLs)
- ? DI/startup failures
- ? Bad working-directory assumptions
- ? Unhandled exceptions in `Main()`
- ? Missing environment variables
- ? COM registration issues (Outlook)
- ? Platform target mismatches
- ? .NET runtime version issues

## Test Categories

### 1. SchedulerExecutableSmokeTests.cs
**Purpose:** End-to-end process execution tests

**What it tests:**
- Can launch each executable as a process
- Executable exits with expected exit code (or graceful failure)
- Console output contains expected messages
- Configuration files (appsettings.json) exist and are valid
- Connection strings use environment variable placeholders (no hardcoded credentials)

**Projects tested:**
- Router
- ExchangeFixed
- DocManEmail
- AddSuppEmailer
- AddSuppProd
- AddSuppVoteCollection
- LoadPfr
- LoadSuppPfr
- EGrantsAcmAuditReport
- OGARequestAccountDisable
- StartOutlook

### 2. DependencySmokeTests.cs
**Purpose:** Verify all required dependencies are present

**What it tests:**
- All assemblies can be loaded
- CommonUtilities.dll is present
- Serilog dependencies are present
- SQL Client dependencies are present (for database projects)
- Runtime config files exist (*.runtimeconfig.json)
- Dependencies manifest exists (*.deps.json)
- Platform targets are correct (AnyCPU, x64, x86)
- Working directory assumptions work correctly

### 3. LogOutputSmokeTests.cs
**Purpose:** Verify logging and error handling

**What it tests:**
- Console output contains expected application name
- Environment detection works (DOTNET_ENVIRONMENT)
- Missing environment variables are handled gracefully
- Missing Outlook is handled gracefully (for COM projects)
- Log files are created (when Serilog is configured)
- Startup time is reasonable (under 30 seconds)
- Error messages are clear and actionable

## Running the Tests

### Prerequisites

1. **Build the solution first:**
   ```powershell
   dotnet build
   ```

2. **Set environment variables** (optional, tests will run without credentials):
   ```powershell
   [System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
   [System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)
   ```

   **Note:** Tests are designed to pass gracefully when environment variables are missing. The smoke tests verify that applications handle missing credentials properly.

### Run All Smoke Tests

From Visual Studio Test Explorer:
- Filter by Category: `SmokeTest`
- Click "Run All"

From command line:
```powershell
dotnet test --filter "TestCategory=SmokeTest"
```

### Run Specific Test Categories

**Process execution tests only:**
```powershell
dotnet test --filter "TestCategory=Process"
```

**Dependency tests only:**
```powershell
dotnet test --filter "TestCategory=Dependencies"
```

**Logging tests only:**
```powershell
dotnet test --filter "TestCategory=Logging"
```

**Configuration tests only:**
```powershell
dotnet test --filter "TestCategory=Configuration"
```

**Error handling tests only:**
```powershell
dotnet test --filter "TestCategory=ErrorHandling"
```

### Run Tests for Specific Project

**Router only:**
```powershell
dotnet test --filter "FullyQualifiedName~Router_Executable"
```

**ExchangeFixed only:**
```powershell
dotnet test --filter "FullyQualifiedName~ExchangeFixed_Executable"
```

## Expected Results

### In Development Environment (without Outlook/Database)

Most tests should **pass** with warnings:
- ? Executables launch successfully
- ? Configuration files load correctly
- ? Dependencies are present
- ?? Exit with error code 1 (Outlook not available) - **This is expected**
- ?? Database connection fails when `DB_USER`/`DB_PASSWORD` are not set - **This is expected**

The tests are designed to verify that failures are **graceful** (proper error messages, no crashes).

**Note:** Smoke tests include Outlook availability checks and will skip Outlook-dependent process tests when COM interop is unavailable. See `SMOKE_TEST_OUTLOOK_SETUP.md` for details.

### In Production Environment (with Outlook/Database)

All tests should **pass**:
- ? Executables launch and connect to Outlook
- ? Database connections succeed
- ? Exit with code 0 (success) or after processing emails

## CI/CD Integration

Add to your build pipeline:

```yaml
# Azure DevOps example
- task: DotNetCoreCLI@2
  displayName: 'Run Smoke Tests'
  inputs:
    command: 'test'
    projects: '**/EmailHandlingTests.csproj'
    arguments: '--filter "TestCategory=SmokeTest" --logger trx --results-directory $(Build.ArtifactStagingDirectory)/TestResults'
  continueOnError: true  # Outlook may not be available in build agents

- task: PublishTestResults@2
  displayName: 'Publish Smoke Test Results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
```

## Troubleshooting

### "Executable not found"
**Cause:** Project not built  
**Fix:** Run `dotnet build` first

### "Assembly could not be loaded"
**Cause:** Missing dependencies  
**Fix:** Check NuGet packages are restored: `dotnet restore`

### "appsettings.json not found"
**Cause:** Configuration file not copied to output  
**Fix:** Verify `appsettings.json` has `Copy to Output Directory = Copy if newer` in project file

### "Timeout waiting for process to exit"
**Cause:** Process hung or waiting for input  
**Fix:** Check application doesn't wait for user input; verify Outlook/database connections fail gracefully

### All tests fail with "Could not find solution directory"
**Cause:** Tests are running from unexpected directory  
**Fix:** Run tests from solution root or set working directory explicitly

## What These Tests DON'T Cover

These are **smoke tests**, not comprehensive tests. They don't cover:
- Business logic correctness (use unit tests)
- Email routing rules (use integration tests)
- Database schema validation (use database tests)
- Performance under load (use performance tests)
- Email parsing edge cases (use unit tests)

## Adding Tests for New Projects

When adding a new scheduler-run executable:

1. **Add to SchedulerExecutableSmokeTests.cs:**
   ```csharp
   [TestMethod]
   [TestCategory("SmokeTest")]
   [TestCategory("Process")]
   public void YourProject_Executable_CanLaunchAndExitGracefully()
   {
       var exePath = GetExecutablePath("YourProject");
       var result = RunExecutableTest(exePath, expectedExitCode: 0, timeoutSeconds: 30);
       Assert.IsTrue(result.Output.Contains("YourProject") || result.Output.Contains("error"));
   }
   ```

2. **Add to project lists** in DependencySmokeTests.cs and LogOutputSmokeTests.cs

3. **Run the new tests** to verify they pass

## Best Practices

1. **Run smoke tests before every deployment**
2. **Run smoke tests after infrastructure changes** (Outlook updates, .NET runtime updates)
3. **Run smoke tests on build servers** to catch environment-specific issues
4. **Include smoke test results in deployment reports**
5. **Investigate failures immediately** - they indicate deployment-blocking issues

## Benefits

? **Catches deployment issues early** - before scheduled tasks run in production  
? **Verifies configuration** - ensures appsettings.json is valid  
? **Validates dependencies** - confirms all DLLs are present  
? **Tests error handling** - verifies graceful failures when Outlook/DB unavailable  
? **Quick feedback** - runs in under 5 minutes  
? **No external dependencies needed** - works on build servers without Outlook

## Related Documentation

- [Unit Tests README](../README_UnitTests.md) - For business logic tests
- [Integration Tests README](../README_IntegrationTests.md) - For database and email tests
- [Deployment Guide](../../docs/Deployment.md) - For production deployment procedures

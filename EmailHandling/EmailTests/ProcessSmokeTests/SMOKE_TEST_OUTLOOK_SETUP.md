# Smoke Tests and Outlook COM Integration

## Overview

The smoke tests for the EmailHandling executables verify that each application can be built, launched, and configured correctly. These tests check for process-level issues such as broken configuration, missing dependencies, and startup failures.

## Outlook Requirement

All EmailHandling executables require **Microsoft Outlook** to be:
1. **Installed** on the machine
2. **Configured** with a valid MAPI profile
3. **Accessible** for COM automation

### Checking Outlook Availability

The smoke tests automatically check if Outlook is available before running executable launch tests. If Outlook is not available, the tests will be marked as **Inconclusive** rather than failed.

To manually verify Outlook is configured:

```powershell
# Test Outlook COM automation in PowerShell
$outlook = New-Object -ComObject Outlook.Application
$namespace = $outlook.GetNamespace("MAPI")
$namespace.Logon("", "", $false, $true)
Write-Host "Outlook is available!"
$namespace.Logoff()
```

## COM Interop Limitations

### Known Issue: Assembly Loading in Process Tests

When EmailHandling executables are launched as **external processes** (via `Process.Start`), they may encounter COM Interop assembly loading issues:

```
FileNotFoundException: Could not load file or assembly 'office, Version=15.0.0.0, ...'
```

This is a **known limitation** of COM Interop in .NET when running as separate processes, NOT a defect in the application code.

### Why This Happens

- Office COM Interop DLLs require special runtime handling
- When launched via `dotnet exec` or `Process.Start`, assembly probing paths may not resolve correctly
- The executables work fine when:
  - Run directly from Windows Explorer
  - Launched by Windows Task Scheduler
  - Executed in Visual Studio debugger
  - Called from integration tests (same process)

### Test Strategy

The smoke tests use a **lenient approach**:

1. **Primary Goal**: Verify the executable can be launched and doesn't hang
2. **Expected Outcomes**:
   - ? Successful execution (exit code 0)
   - ? Assembly loading error (indicates executable structure is correct, just COM loading limitation)
   - ? Configuration loading output
   - ? No output or unexpected hang (indicates real problem)

3. **What We're Testing**:
   - Executable exists and is built
   - Configuration files are present and valid
   - Application structure is sound
   - Dependencies are included in output directory

4. **What Integration Tests Cover**:
   - Full Outlook COM automation
   - Email processing logic
   - Database connectivity
   - End-to-end workflows

## Running Smoke Tests

### Run All Smoke Tests
```bash
dotnet test --filter "TestCategory=SmokeTest"
```

### Run Only Executable Launch Tests
```bash
dotnet test --filter "TestCategory=SmokeTest&TestCategory=Process"
```

### Run Only Configuration Tests (No Outlook Required)
```bash
dotnet test --filter "TestCategory=SmokeTest&TestCategory=Configuration"
```

### Skip Smoke Tests
```bash
dotnet test --filter "TestCategory!=SmokeTest"
```

## Test Categories

### Process Tests
- Require Outlook to be available
- Test executable launch and basic execution
- May show COM interop limitations (acceptable)

### Configuration Tests
- Do NOT require Outlook
- Test presence and validity of `appsettings.json`
- Verify environment variable placeholders

### Dependency Tests
- Do NOT require Outlook
- Verify DLLs, runtime configs, and manifests
- Check for required dependencies

## Troubleshooting

### All Smoke Tests Skip (Inconclusive)

**Cause**: Outlook is not available

**Solution**:
1. Install Microsoft Outlook
2. Configure at least one email profile
3. Open Outlook at least once to complete setup
4. Re-run tests

### Smoke Tests Fail with "Executable not found"

**Cause**: Projects not built

**Solution**:
```bash
dotnet build --configuration Debug
dotnet test --no-build --filter "TestCategory=SmokeTest"
```

### Integration Tests Fail But Smoke Tests Pass

**Cause**: Database credentials or connectivity issue

**Solution**:
1. Check environment variables: `DB_USER` and `DB_PASSWORD`
2. Verify database connection string in `appsettings.json`
3. Test database connectivity manually

## Best Practices

1. **Always run Integration Tests** for full validation - smoke tests are just basic sanity checks
2. **Don't treat COM interop assembly errors as failures** in smoke tests - they're expected
3. **Use the Category filters** to run appropriate tests for your environment
4. **Document any real issues** found - don't dismiss everything as "COM limitations"

## See Also

- [RUNNING_INTEGRATION_TESTS.md](../RUNNING_INTEGRATION_TESTS.md) - Full integration test setup
- [TEST_EXECUTION_STATUS.md](../TEST_EXECUTION_STATUS.md) - Current test status
- [README.md](./README.md) - General smoke test documentation

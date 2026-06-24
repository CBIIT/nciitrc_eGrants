# IMMEDIATE ACTION CHECKLIST - START TODAY (June 16, 2026)

## ?? YOU HAVE 35 DAYS UNTIL VISUALCRON LICENSE EXPIRES

### ? TODAY (Monday, June 16) - DO THESE NOW

#### Morning (Next 2 Hours)
- [ ] **HOUR 1:** Get test server access
  - Request test server credentials from IT
  - Install .NET 8 Runtime: `winget install Microsoft.DotNet.Runtime.8`
  - Verify Outlook is installed on test server

- [ ] **HOUR 2:** Build and package all executables
  ```powershell
  cd C:\Development\eGrants-EmailHandlingUpgrade\EmailHandling
  dotnet build --configuration Release

  # Package all executables
  $projects = @("Router","ExchangeFixed","LoadPfr","LoadSuppPfr","AddSuppEmailer",
                "AddSuppProd","AddSuppVoteCollection",
                "OGARequestAccountDisable","EGrantsAcmAuditReport","StartOutlook")

  foreach ($proj in $projects) {
      $source = ".\$proj\bin\Release\net8.0-windows\*"
      $dest = "C:\Temp\eGrants-Deploy\$proj"
      New-Item -ItemType Directory -Path $dest -Force
      Copy-Item $source $dest -Recurse -Force
  }

  # Create deployment package
  Compress-Archive -Path "C:\Temp\eGrants-Deploy\*" -DestinationPath "C:\Temp\eGrants-Deploy.zip"

  Write-Host "`n? Deployment package ready: C:\Temp\eGrants-Deploy.zip"
  ```

#### Afternoon (Next 3 Hours)
- [ ] **HOUR 3:** Deploy to test server
  - Copy `eGrants-Deploy.zip` to test server
  - Extract to `C:\eGrants\apps\`
  - Set environment variables (see script below)

- [ ] **HOUR 4-5:** Quick smoke test all executables
  ```powershell
  # Test each executable manually
  cd C:\eGrants\apps

  $exes = Get-ChildItem -Recurse -Filter "*.exe" | Where-Object { $_.Directory.Parent.Name -eq "apps" }

  foreach ($exe in $exes) {
      Write-Host "`nTesting: $($exe.Name)" -ForegroundColor Cyan
      & $exe.FullName
      Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor $(if ($LASTEXITCODE -eq 0) { "Green" } else { "Red" })
      Start-Sleep -Seconds 5
  }
  ```

**END OF DAY GOAL:** All executables deployed to test server and running manually

---

### ? TUESDAY (June 17) - Task Scheduler Setup

#### Morning
- [ ] Get service account credentials from IT
- [ ] Run Task Scheduler bulk creation script (see EMERGENCY_5WEEK_DEPLOYMENT.md)
- [ ] Test each task manually from Task Scheduler

#### Afternoon  
- [ ] Enable all tasks
- [ ] Monitor for 4 hours
- [ ] Check logs for errors
- [ ] Prepare for Juneteenth holiday (Thursday) - ensure automated monitoring

**END OF DAY GOAL:** All tasks scheduled and running automatically

---

### ? WEDNESDAY (June 18) - Critical Testing

#### Focus: Test highest-priority executables
- [ ] Router - Email routing (30 min)
- [ ] ExchangeFixed - Document processing (30 min)
- [ ] LoadPfr - PFR loading (20 min)
- [ ] LoadSuppPfr - Supplement PFR (20 min)

**END OF DAY GOAL:** Critical executables validated

---

### ? THURSDAY (June 19) - JUNETEENTH HOLIDAY ??

#### Automated Systems Only
- Systems continue to run on Task Scheduler
- Optional: Remote log monitoring
- No manual work expected

**REST AND RECHARGE** - You've earned it!

---

### ? FRIDAY (June 20) - Remaining Testing + Weekend Prep

#### Morning: Test remaining executables
- [ ] AddSuppEmailer, AddSuppProd, AddSuppVoteCollection (10 min each)
- [ ] OGARequestAccountDisable, EGrantsAcmAuditReport, StartOutlook (5 min each)
> **Note:** DocManEmail is deprecated and excluded from testing.

#### Afternoon: Weekend monitoring prep
- [ ] Review all test results from the week
- [ ] Fix any critical issues found
- [ ] Set up weekend monitoring dashboard
- [ ] Prepare weekend monitoring plan

---

### ? WEEK 2 (June 23-27) - Stability & Testing

- [ ] Monday-Wednesday: Fix any remaining issues
- [ ] Thursday-Friday: Basic load testing
- [ ] Weekend: Extended monitoring (48 hours)

---

### ? WEEK 3 (June 30 - July 4) - Production Prep

- [ ] Mon-Tue: Production server setup
- [ ] Wed-Thu: Production Task Scheduler configuration  
- [ ] Holiday Weekend: Documentation & final prep

---

### ? WEEK 4 (July 7-13) - Parallel Run (WITH TIME OFF)

**Monday-Tuesday (July 7-8):**
- [ ] **CRITICAL:** Run VisualCron + Task Scheduler together
- [ ] Daily comparison of outputs
- [ ] Document any discrepancies
- [ ] Ensure stable before time off

**Wednesday-Thursday (July 9-10): TIME OFF** ???
- Systems run autonomously (automated monitoring only)
- Colleague checks for critical failures
- No manual work expected

**Friday (July 11):**
- [ ] Review outputs from July 9-10
- [ ] Complete parallel run validation
- [ ] Fix any issues found

**Weekend (July 12-13):**
- [ ] Final validation
- [ ] Stakeholder approval for cutover

---

### ? WEEK 5 (July 14-20) - CUTOVER

- [ ] Monday-Tuesday: Final prep
- [ ] **Wednesday July 16: CUTOVER DAY** ??
- [ ] Thursday-Friday: Post-cutover monitoring
- [ ] **Monday July 21: VisualCron license expires** ?

---

## ?? CRITICAL SUCCESS FACTORS

1. **START TODAY** - No delays allowed
2. **Test environment FIRST** - Get comfortable before production
3. **Parallel run MANDATORY** - Catch issues before cutover
4. **Daily monitoring** - Check logs every single day
5. **Communication** - Update stakeholders daily

---

## ?? GET HELP IMMEDIATELY IF:

- Can't access test server by end of today
- Can't install .NET 8 Runtime
- Outlook not working on test server
- Database connection fails
- Any executable fails smoke test

**DON'T WAIT - ESCALATE IMMEDIATELY!**

---

## ? FAST-TRACK SCRIPTS

### Environment Setup (Run on test/prod servers)

```powershell
# Complete environment setup - 10 minutes
Write-Host "Setting up eGrants environment..." -ForegroundColor Cyan

# 1. Environment variables (CHANGE THESE VALUES!)
[System.Environment]::SetEnvironmentVariable('DB_USER', 'your_db_user', [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_db_password', [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable('DOTNET_ENVIRONMENT', 'Development', [System.EnvironmentVariableTarget]::Machine)

# 2. Create directories
$dirs = @(
    "C:\eGrants\apps\log",
    "C:\eGrants\data",
    "C:\eGrants\publicaccess",
    "C:\eGrants\temp"
)
foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    Write-Host "? Created: $dir" -ForegroundColor Green
}

# 3. Verify .NET 8
$dotnetVersion = dotnet --version
Write-Host "? .NET Version: $dotnetVersion" -ForegroundColor Green

# 4. Verify Outlook
try {
    $outlook = New-Object -ComObject Outlook.Application
    Write-Host "? Outlook COM available" -ForegroundColor Green
    [System.Runtime.Interopservices.Marshal]::ReleaseComObject($outlook) | Out-Null
} catch {
    Write-Host "? Outlook NOT available - FIX THIS NOW!" -ForegroundColor Red
}

Write-Host "`n? Environment setup complete!" -ForegroundColor Magenta
```

### Quick Health Check (Run daily)

```powershell
# Daily health check - 2 minutes
Write-Host "`n=== eGrants Health Check ===" -ForegroundColor Magenta
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -ForegroundColor Cyan

# Check tasks
$tasks = Get-ScheduledTask | Where-Object {$_.TaskName -like "eGrants_*"}
$failedTasks = $tasks | Where-Object {(Get-ScheduledTaskInfo $_).LastTaskResult -ne 0}

Write-Host "`nTasks: $($tasks.Count) total" -ForegroundColor White
if ($failedTasks) {
    Write-Host "??  FAILED TASKS: $($failedTasks.Count)" -ForegroundColor Red
    $failedTasks | ForEach-Object { Write-Host "  - $($_.TaskName)" -ForegroundColor Red }
} else {
    Write-Host "? All tasks successful" -ForegroundColor Green
}

# Check logs
$logDir = "C:\eGrants\apps\log"
$todayLogs = Get-ChildItem $logDir | Where-Object {$_.LastWriteTime -gt (Get-Date).Date}
Write-Host "`nLogs created today: $($todayLogs.Count)" -ForegroundColor White

# Check for errors in logs
$errorCount = $todayLogs | ForEach-Object {
    (Get-Content $_.FullName | Select-String -Pattern "error|exception|fail" -CaseSensitive:$false).Count
} | Measure-Object -Sum | Select-Object -ExpandProperty Sum

if ($errorCount -gt 0) {
    Write-Host "??  ERRORS FOUND: $errorCount" -ForegroundColor Red
} else {
    Write-Host "? No errors in logs" -ForegroundColor Green
}

Write-Host "`n$(if ($failedTasks -or $errorCount -gt 0) { '?? ACTION REQUIRED' } else { '? ALL SYSTEMS GO' })" -ForegroundColor $(if ($failedTasks -or $errorCount -gt 0) { 'Red' } else { 'Green' })
```

---

## ?? MILESTONES TRACKER

| Date | Milestone | Status |
|------|-----------|--------|
| June 16 | Test environment ready | ? IN PROGRESS |
| June 17 | Task Scheduler configured | ? PENDING |
| June 20 | 24-hour monitoring complete | ? PENDING |
| June 27 | All bugs fixed | ? PENDING |
| July 1 | Production setup complete | ? PENDING |
| July 11 | Parallel run validated | ? PENDING |
| July 16 | **CUTOVER DAY** | ? PENDING |
| July 21 | **LICENSE DEADLINE** | ? PENDING |

---

## ?? YOUR #1 PRIORITY THIS WEEK

**GET TEST ENVIRONMENT WORKING WITH ALL 11 EXECUTABLES RUNNING ON TASK SCHEDULER**

Everything else is secondary. Focus 100% on this goal for Week 1.

---

## ?? YOU CAN DO THIS!

**Why you'll succeed:**
- ? You have 598 passing tests (code is solid!)
- ? All executables already built and working
- ? You have 5 full weeks (most teams get 2-3 weeks for similar migrations)
- ? Clear plan and timeline
- ? Parallel run gives you a safety net

**START NOW - EVERY HOUR COUNTS!** ??

---

**NEXT STEP:** Run the environment setup script above on your test server RIGHT NOW.

# Coverage Plan for July 9-10 (Time Off)

## Overview
**Dates:** Wednesday-Thursday, July 9-10, 2026  
**Status:** Week 4 of 5-week emergency deployment - Parallel Run phase  
**Systems:** Both VisualCron AND Task Scheduler running simultaneously  

## What's Running

### VisualCron (Existing System)
- All existing eGrants jobs continue on normal schedule
- **DO NOT DISABLE** - This is the production system backup

### Task Scheduler (New System - Being Validated)
All eGrants_* tasks should be running:
1. eGrants_StartOutlook
2. eGrants_Router
3. eGrants_ExchangeFixed
4. eGrants_LoadPfr
5. eGrants_LoadSuppPfr
6. eGrants_AddSuppEmailer
7. eGrants_AddSuppProd
8. eGrants_AddSuppVoteCollection
9. eGrants_OGARequestAccountDisable
10. eGrants_EGrantsAcmAuditReport

> **Note:** DocManEmail (eGrants_DocManEmail) is deprecated and not included in this migration.

## Your Role (Minimal Monitoring)

**YOU DO NOT NEED TO FIX ANYTHING** - Just observe and alert if critical

### Morning Check (9:00 AM Each Day)

```powershell
# Quick health check script
# Run on the test/production server

Write-Host "`n=== eGrants Health Check - $(Get-Date) ===" -ForegroundColor Cyan

# 1. Check Task Scheduler
Write-Host "`n1. TASK SCHEDULER STATUS:" -ForegroundColor Yellow
Get-ScheduledTask | Where-Object {$_.TaskName -like "eGrants_*"} | ForEach-Object {
    $info = Get-ScheduledTaskInfo $_
    $status = if ($info.LastTaskResult -eq 0) { "? OK" } else { "? FAIL" }
    $color = if ($info.LastTaskResult -eq 0) { "Green" } else { "Red" }
    Write-Host "  $status - $($_.TaskName) (Last: $($info.LastRunTime))" -ForegroundColor $color
}

# 2. Check log files
Write-Host "`n2. LOG FILES:" -ForegroundColor Yellow
$logDir = "C:\eGrants\apps\log"
$todayLogs = Get-ChildItem $logDir -Filter "*$(Get-Date -Format 'yyyy-MM-dd')*" -ErrorAction SilentlyContinue
Write-Host "  Logs created today: $($todayLogs.Count)" -ForegroundColor $(if ($todayLogs.Count -gt 0) { "Green" } else { "Red" })

# 3. Check for critical errors
$errorPattern = "error|exception|critical|fail"
$errorCount = 0
if ($todayLogs) {
    $errorCount = ($todayLogs | ForEach-Object {
        (Get-Content $_.FullName | Select-String -Pattern $errorPattern -CaseSensitive:$false).Count
    } | Measure-Object -Sum).Sum
}
Write-Host "  Errors in logs: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { "Green" } else { "Yellow" })

Write-Host "`n3. VISUALCRON STATUS:" -ForegroundColor Yellow
Write-Host "  (Check VisualCron UI manually)" -ForegroundColor Gray

Write-Host "`n=== STATUS: $(if ($todayLogs.Count -gt 0) { 'SYSTEMS RUNNING' } else { 'NEEDS ATTENTION' }) ===" -ForegroundColor $(if ($todayLogs.Count -gt 0) { "Green" } else { "Red" })
```

### Afternoon Check (3:00 PM Each Day)

Run the same script again. That's it!

## When to Call for Help

### ?? CALL IMMEDIATELY if:

1. **Multiple Task Scheduler tasks show Last Result != 0**
   - Indicates system-wide failure
   - Check: Task Scheduler ? eGrants_* tasks

2. **VisualCron shows critical job failures**
   - Open VisualCron
   - Look for red indicators on eGrants jobs

3. **No log files created today**
   - Check: `C:\eGrants\apps\log\`
   - Should see files with today's date

4. **Database connection alerts** (if monitoring system configured)

5. **Outlook completely down** (rare but critical)

### ?? DOCUMENT (but don't call) if:

1. **Single task failure** (shows Result != 0)
   - Note which task and time
   - Task will retry automatically

2. **Few errors in logs** (< 10 per day)
   - Expected during testing phase

3. **Tasks taking longer than usual**
   - Note in monitoring log

## What NOT to Do

? **DO NOT** disable VisualCron jobs  
? **DO NOT** disable Task Scheduler tasks  
? **DO NOT** restart servers  
? **DO NOT** change any configurations  
? **DO NOT** try to fix issues yourself  

**Just observe and report.**

## Contact Information

**For Critical Issues:**
- Primary: [Your Phone Number]
- Secondary: [Your Email - check periodically]
- Escalation: [Manager/Team Lead Contact]

**Non-Critical Issues:**
- Document in email
- Review on Friday, July 11

## Monitoring Log Template

Use this to document your checks:

```
MONITORING LOG - JULY 9-10, 2026

Wednesday, July 9
-----------------
Morning Check (9:00 AM):
[ ] Task Scheduler: ___/11 tasks successful
[ ] Logs created: Yes / No
[ ] VisualCron: All green / Issues noted
[ ] Overall status: OK / ISSUES

Afternoon Check (3:00 PM):
[ ] Task Scheduler: ___/11 tasks successful
[ ] Logs created: Yes / No
[ ] VisualCron: All green / Issues noted
[ ] Overall status: OK / ISSUES

Notes:
_________________________________

Thursday, July 10
-----------------
Morning Check (9:00 AM):
[ ] Task Scheduler: ___/11 tasks successful
[ ] Logs created: Yes / No
[ ] VisualCron: All green / Issues noted
[ ] Overall status: OK / ISSUES

Afternoon Check (3:00 PM):
[ ] Task Scheduler: ___/11 tasks successful
[ ] Logs created: Yes / No
[ ] VisualCron: All green / Issues noted
[ ] Overall status: OK / ISSUES

Notes:
_________________________________

ISSUES REQUIRING ATTENTION:
_________________________________
```

## Expected Behavior

**Normal operation:**
- Task Scheduler shows 11/11 tasks with Last Result = 0
- New log files created daily in `C:\eGrants\apps\log\`
- VisualCron jobs all showing green/successful
- Few or no errors in log files

**This is a validation period** - both systems should run smoothly without intervention. If they do, that's success!

## Quick Reference

### Server Access
- Server: [SERVER_NAME]
- Remote Desktop: [RDP_CONNECTION]
- Credentials: [SERVICE_ACCOUNT or your credentials]

### Key Directories
- Logs: `C:\eGrants\apps\log\`
- Executables: `C:\eGrants\apps\`
- Task Scheduler: Windows ? Task Scheduler ? Task Scheduler Library ? look for eGrants_*

### VisualCron
- Application: [PATH_TO_VISUALCRON] or shortcut on desktop
- Look for: eGrants job group

## What Happens on Friday (July 11)

I will:
1. Review the monitoring log you created
2. Compare VisualCron and Task Scheduler outputs for July 9-10
3. Address any issues found
4. Continue the parallel run validation

**Your monitoring helps validate that systems can run autonomously - critical for Task Scheduler success!**

---

## Questions Before July 9?

- [ ] Do you have server access?
- [ ] Can you run the health check script?
- [ ] Do you have contact information?
- [ ] Do you understand when to call vs. when to document?
- [ ] Any questions about the monitoring procedure?

**Thank you for the coverage!** ??

This is a critical phase, but the goal is autonomous operation - your job is just to watch and alert if major issues occur.

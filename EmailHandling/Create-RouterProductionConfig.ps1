# Script to create Router appsettings.Production.json
# Run this from PowerShell in the EmailHandling directory

$productionConfig = @'
{
  "AppSettings": {
    "LogDir": "C:\\eGrants\\apps\\log\\",
    "Verbose": "n",
    "dBug": "n",
    "RoutingBreakDuration": 1000
  },
  "FolderPaths": {
    "dirpathRouter": "NCI CA eRA Notifications (NIH/NCI)\\Inbox\\"
  },
  "ConnectionStrings": {
    "EIM": "Password=%DB_PASSWORD%;Persist Security Info=True;User ID=%DB_USER%;Initial Catalog=EIM;Data Source=NCIDB-P391-V.nci.nih.gov\\MSSQLEGRANTSP,59000;Application Name=egrants"
  },
  "EmailRecipients": {
    "DebugEmail": "leul.ayana@nih.gov",
    "EGrantsDevEmail": "eGrantsDev@mail.nih.gov",
    "EGrantsTestEmail": "eGrantsTest1@mail.nih.gov",
    "EGrantsStageEmail": "eGrantsStage@mail.nih.gov",
    "EFileEmail": "efile@mail.nih.gov",
    "NCIGrantsPostAwardEmail": "NCIGrantsPostAward@nih.gov",
    "ErrorNotificationRecipients": "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov",
    "PublicAccessComplianceRecipients": "jonesni@mail.nih.gov;bakerb@mail.nih.gov;edward.mikulich@nih.gov",
    "RelinquishingStatementRecipients": "emily.driskell@nih.gov;dvellaj@mail.nih.gov;edward.mikulich@nih.gov",
    "NCIOGASupplementsEmail": "NCIOGASupplements@mail.nih.gov",
    "NCIOGABOBTeamEmail": "nciogabobteam1@mail.nih.gov",
    "LegacyErrorRecipient": "leul.ayana@nih.gov"
  }
}
'@

$targetPath = "Router\appsettings.Production.json"

try {
    Set-Content -Path $targetPath -Value $productionConfig -Encoding UTF8
    Write-Host "? Successfully created $targetPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "File contents:" -ForegroundColor Cyan
    Get-Content $targetPath | Write-Host
    Write-Host ""
    Write-Host "Next step: Build the solution to verify everything works!" -ForegroundColor Yellow
} catch {
    Write-Host "? Error creating file: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Manual creation steps:" -ForegroundColor Yellow
    Write-Host "1. In Visual Studio, right-click Router project"
    Write-Host "2. Add ? New Item ? JSON File"
    Write-Host "3. Name it: appsettings.Production.json"
    Write-Host "4. Copy content from ROUTER_EMAIL_CONFIG_MIGRATION.md"
}

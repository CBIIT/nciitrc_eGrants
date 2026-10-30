using System;
using System.Collections.Generic;
using System.IO;
using EGrantsAcmAuditReport;
using CommonUtilties;

namespace EmailHandlingTests.Unit.EGrantsAcmAuditReport
{
    internal class TestAcmAuditReportProcessor : Processor
    {
   public const string TestReportName = "Egrants ACM Monthly Audit Report";
        
        public List<TestAuditReportRecord> ReportsProcessedThisSession { get; } = new List<TestAuditReportRecord>();
public List<TestFileCopyOperation> FileCopyOperations { get; } = new List<TestFileCopyOperation>();
        public List<string> FilesDeleted { get; } = new List<string>();
    public int ProcessedCount { get; private set; }
 public bool ErrorOccurred { get; private set; }
        public string LastErrorMessage { get; private set; }
        public List<SimulatedAuditFile> SimulatedFiles { get; set; } = new List<SimulatedAuditFile>();

        public TestAuditReportRecord TestProcessSingleFile(string fileName, long fileSize = 1024, 
  DateTime? lastWriteTime = null, string verbose = "n")
        {
  try
 {
                if (fileSize == 0) return null;
   if (!IsValidAuditFile(fileName)) return null;

     ProcessedCount++;
        DateTime runDate = lastWriteTime ?? DateTime.Now.AddHours(-1);
                string fileUrl = BuildFileUrl(fileName);

         var record = new TestAuditReportRecord
      {
         ReportName = TestReportName,
    FileName = fileName,
             FileSize = fileSize,
              RunDate = runDate,
         FileUrl = fileUrl,
               ProcessedTime = DateTime.Now,
   WasInsertedToDatabase = true
  };

 ReportsProcessedThisSession.Add(record);

       // Simulate file copy operations
     FileCopyOperations.Add(new TestFileCopyOperation
      {
         SourceFile = fileName,
         DestinationType = "Backup",
 Success = true
  });
     FileCopyOperations.Add(new TestFileCopyOperation
        {
           SourceFile = fileName,
DestinationType = "ImageServer1",
         Success = true
    });
      FileCopyOperations.Add(new TestFileCopyOperation
        {
    SourceFile = fileName,
                    DestinationType = "ImageServer2",
        Success = true
  });

       FilesDeleted.Add(fileName);

                return record;
          }
            catch (Exception ex)
      {
       ErrorOccurred = true;
  LastErrorMessage = ex.Message;
 return null;
          }
        }

 public int TestProcessSimulatedFiles(string verbose = "n")
   {
try
            {
        int processed = 0;
         foreach (var file in SimulatedFiles)
  {
            var result = TestProcessSingleFile(file.FileName, file.FileSize, file.LastWriteTime, verbose);
           if (result != null) processed++;
          }
            return processed;
            }
     catch (Exception ex)
          {
                ErrorOccurred = true;
           LastErrorMessage = ex.Message;
      return ProcessedCount;
          }
        }

        public bool IsValidAuditFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
    string ext = Path.GetExtension(fileName).ToLower();
            return ext == ".xls" || ext == ".xlsx" || ext == ".xlsm";
        }

     public string BuildFileUrl(string fileName)
  {
            return $"/data/funded/egrantsadmin/auditreport/{fileName}";
  }

        public string BuildInsertSql(string fileName, DateTime runDate, string fileUrl)
        {
        return $"INSERT INTO dbo.egrants_audit_report (Report_name, File_name, Run_date, url) VALUES('{TestReportName}', '{fileName}', '{runDate:yyyy-MM-dd HH:mm:ss}', '{fileUrl}')";
        }

        public void AddSimulatedFile(string fileName, long fileSize = 1024, DateTime? lastWriteTime = null)
  {
            SimulatedFiles.Add(new SimulatedAuditFile
            {
         FileName = fileName,
      FileSize = fileSize,
    LastWriteTime = lastWriteTime ?? DateTime.Now.AddHours(-1)
         });
        }

        public void Reset()
        {
        ReportsProcessedThisSession.Clear();
            FileCopyOperations.Clear();
            FilesDeleted.Clear();
          ProcessedCount = 0;
            ErrorOccurred = false;
            LastErrorMessage = null;
            SimulatedFiles.Clear();
        }
    }

    public class TestAuditReportRecord
    {
        public string ReportName { get; set; }
    public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime RunDate { get; set; }
        public string FileUrl { get; set; }
      public DateTime ProcessedTime { get; set; }
   public bool WasInsertedToDatabase { get; set; }
    }

    public class TestFileCopyOperation
    {
        public string SourceFile { get; set; }
        public string DestinationType { get; set; }
        public bool Success { get; set; }
    }

    public class SimulatedAuditFile
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime LastWriteTime { get; set; }
    }
}

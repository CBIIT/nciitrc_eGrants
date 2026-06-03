using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.EGrantsAcmAuditReport
{
    [TestClass]
    public class EGrantsAcmAuditReportTests
    {
        #region File Validation Tests

        [TestMethod]
        public void IsValidAuditFile_XlsExtension_ReturnsTrue()
        {
     var testProcessor = new TestAcmAuditReportProcessor();
    Assert.IsTrue(testProcessor.IsValidAuditFile("AuditReport_2024.xls"));
        }

     [TestMethod]
        public void IsValidAuditFile_XlsxExtension_ReturnsTrue()
        {
            var testProcessor = new TestAcmAuditReportProcessor();
            Assert.IsTrue(testProcessor.IsValidAuditFile("AuditReport_2024.xlsx"));
   }

        [TestMethod]
        public void IsValidAuditFile_XlsmExtension_ReturnsTrue()
      {
            var testProcessor = new TestAcmAuditReportProcessor();
      Assert.IsTrue(testProcessor.IsValidAuditFile("AuditReport_2024.xlsm"));
        }

 [TestMethod]
        public void IsValidAuditFile_PdfExtension_ReturnsFalse()
        {
          var testProcessor = new TestAcmAuditReportProcessor();
    Assert.IsFalse(testProcessor.IsValidAuditFile("AuditReport_2024.pdf"));
        }

      [TestMethod]
        public void IsValidAuditFile_DocxExtension_ReturnsFalse()
        {
    var testProcessor = new TestAcmAuditReportProcessor();
            Assert.IsFalse(testProcessor.IsValidAuditFile("AuditReport_2024.docx"));
        }

        [TestMethod]
        public void IsValidAuditFile_EmptyFileName_ReturnsFalse()
    {
            var testProcessor = new TestAcmAuditReportProcessor();
            Assert.IsFalse(testProcessor.IsValidAuditFile(""));
        }

        [TestMethod]
        public void IsValidAuditFile_NullFileName_ReturnsFalse()
        {
      var testProcessor = new TestAcmAuditReportProcessor();
     Assert.IsFalse(testProcessor.IsValidAuditFile(null));
        }

        [TestMethod]
        public void IsValidAuditFile_UppercaseExtension_ReturnsTrue()
     {
            var testProcessor = new TestAcmAuditReportProcessor();
    Assert.IsTrue(testProcessor.IsValidAuditFile("AuditReport_2024.XLSX"));
   }

        #endregion

        #region URL Building Tests

        [TestMethod]
     public void BuildFileUrl_ValidFileName_ReturnsCorrectUrl()
        {
    var testProcessor = new TestAcmAuditReportProcessor();
          string fileName = "ACM_Audit_Jan2024.xlsx";
   
      string result = testProcessor.BuildFileUrl(fileName);
    
       Assert.AreEqual("/data/funded/egrantsadmin/auditreport/ACM_Audit_Jan2024.xlsx", result);
     }

        [TestMethod]
        public void BuildFileUrl_ContainsCorrectPath()
        {
        var testProcessor = new TestAcmAuditReportProcessor();
      
            string result = testProcessor.BuildFileUrl("test.xls");
   
       Assert.IsTrue(result.StartsWith("/data/funded/egrantsadmin/auditreport/"));
   }

        [TestMethod]
        public void BuildFileUrl_PreservesFileName()
        {
            var testProcessor = new TestAcmAuditReportProcessor();
   string fileName = "MyReport_2024_01.xlsx";
 
            string result = testProcessor.BuildFileUrl(fileName);
     
        Assert.IsTrue(result.EndsWith(fileName));
        }

    #endregion

   #region SQL Building Tests

        [TestMethod]
   public void BuildInsertSql_ContainsReportName()
        {
            var testProcessor = new TestAcmAuditReportProcessor();
            
   string sql = testProcessor.BuildInsertSql("test.xlsx", DateTime.Now, "/path/test.xlsx");
            
    Assert.IsTrue(sql.Contains("Egrants ACM Monthly Audit Report"));
        }

        [TestMethod]
     public void BuildInsertSql_ContainsFileName()
        {
      var testProcessor = new TestAcmAuditReportProcessor();
        string fileName = "ACM_Report_2024.xlsx";
            
  string sql = testProcessor.BuildInsertSql(fileName, DateTime.Now, "/path/test.xlsx");
            
            Assert.IsTrue(sql.Contains(fileName));
        }

        [TestMethod]
        public void BuildInsertSql_ContainsTableName()
        {
        var testProcessor = new TestAcmAuditReportProcessor();
            
            string sql = testProcessor.BuildInsertSql("test.xlsx", DateTime.Now, "/path/test.xlsx");
      
         Assert.IsTrue(sql.Contains("dbo.egrants_audit_report"));
        }

        [TestMethod]
        public void BuildInsertSql_ContainsFileUrl()
   {
            var testProcessor = new TestAcmAuditReportProcessor();
            string fileUrl = "/data/funded/egrantsadmin/auditreport/test.xlsx";

  string sql = testProcessor.BuildInsertSql("test.xlsx", DateTime.Now, fileUrl);
 
    Assert.IsTrue(sql.Contains(fileUrl));
        }

    [TestMethod]
  public void BuildInsertSql_FormatsDateCorrectly()
        {
       var testProcessor = new TestAcmAuditReportProcessor();
            DateTime runDate = new DateTime(2024, 1, 15, 10, 30, 45);
    
     string sql = testProcessor.BuildInsertSql("test.xlsx", runDate, "/path/test.xlsx");
     
            Assert.IsTrue(sql.Contains("2024-01-15 10:30:45"));
        }

    #endregion

      #region Single File Processing Tests

        [TestMethod]
        public void ProcessSingleFile_ValidFile_CreatesRecord()
      {
      var testProcessor = new TestAcmAuditReportProcessor();
            
   var result = testProcessor.TestProcessSingleFile("ACM_Audit_2024.xlsx");
     
          Assert.IsNotNull(result);
     Assert.AreEqual("ACM_Audit_2024.xlsx", result.FileName);
        }

        [TestMethod]
   public void ProcessSingleFile_SetsReportName()
        {
          var testProcessor = new TestAcmAuditReportProcessor();
            
    var result = testProcessor.TestProcessSingleFile("test.xlsx");
            
    Assert.AreEqual("Egrants ACM Monthly Audit Report", result.ReportName);
     }

   [TestMethod]
        public void ProcessSingleFile_SetsFileUrl()
        {
        var testProcessor = new TestAcmAuditReportProcessor();
       
   var result = testProcessor.TestProcessSingleFile("report.xlsx");
   
          Assert.AreEqual("/data/funded/egrantsadmin/auditreport/report.xlsx", result.FileUrl);
        }

        [TestMethod]
        public void ProcessSingleFile_ZeroFileSize_ReturnsNull()
    {
            var testProcessor = new TestAcmAuditReportProcessor();
    
   var result = testProcessor.TestProcessSingleFile("empty.xlsx", fileSize: 0);
            
     Assert.IsNull(result);
        }

     [TestMethod]
        public void ProcessSingleFile_InvalidExtension_ReturnsNull()
        {
      var testProcessor = new TestAcmAuditReportProcessor();
       
      var result = testProcessor.TestProcessSingleFile("report.pdf");
       
            Assert.IsNull(result);
        }

        [TestMethod]
     public void ProcessSingleFile_IncrementsCounter()
        {
 var testProcessor = new TestAcmAuditReportProcessor();
  
 testProcessor.TestProcessSingleFile("report1.xlsx");
            testProcessor.TestProcessSingleFile("report2.xlsx");
            testProcessor.TestProcessSingleFile("report3.xlsx");
      
       Assert.AreEqual(3, testProcessor.ProcessedCount);
    }

        [TestMethod]
        public void ProcessSingleFile_CapturesFileSize()
      {
         var testProcessor = new TestAcmAuditReportProcessor();
         long expectedSize = 50000;
            
            var result = testProcessor.TestProcessSingleFile("report.xlsx", fileSize: expectedSize);
            
        Assert.AreEqual(expectedSize, result.FileSize);
        }

     [TestMethod]
        public void ProcessSingleFile_CapturesRunDate()
   {
        var testProcessor = new TestAcmAuditReportProcessor();
            DateTime expectedDate = new DateTime(2024, 1, 15, 14, 30, 0);
            
      var result = testProcessor.TestProcessSingleFile("report.xlsx", lastWriteTime: expectedDate);
            
Assert.AreEqual(expectedDate, result.RunDate);
     }

        [TestMethod]
        public void ProcessSingleFile_MarksAsInsertedToDatabase()
        {
     var testProcessor = new TestAcmAuditReportProcessor();
            
     var result = testProcessor.TestProcessSingleFile("report.xlsx");
     
     Assert.IsTrue(result.WasInsertedToDatabase);
        }

        #endregion

        #region File Copy Operations Tests

        [TestMethod]
   public void ProcessSingleFile_CreatesThreeCopyOperations()
        {
 var testProcessor = new TestAcmAuditReportProcessor();
            
     testProcessor.TestProcessSingleFile("report.xlsx");
   
            Assert.AreEqual(3, testProcessor.FileCopyOperations.Count);
        }

        [TestMethod]
        public void ProcessSingleFile_CopiesToBackup()
   {
var testProcessor = new TestAcmAuditReportProcessor();
       
            testProcessor.TestProcessSingleFile("report.xlsx");
         
     Assert.IsTrue(testProcessor.FileCopyOperations.Exists(
      op => op.DestinationType == "Backup"));
      }

        [TestMethod]
        public void ProcessSingleFile_CopiesToImageServer1()
      {
      var testProcessor = new TestAcmAuditReportProcessor();
        
            testProcessor.TestProcessSingleFile("report.xlsx");
            
 Assert.IsTrue(testProcessor.FileCopyOperations.Exists(
       op => op.DestinationType == "ImageServer1"));
        }

        [TestMethod]
 public void ProcessSingleFile_CopiesToImageServer2()
  {
       var testProcessor = new TestAcmAuditReportProcessor();
  
            testProcessor.TestProcessSingleFile("report.xlsx");
      
            Assert.IsTrue(testProcessor.FileCopyOperations.Exists(
      op => op.DestinationType == "ImageServer2"));
        }

        [TestMethod]
        public void ProcessSingleFile_DeletesSourceFile()
        {
   var testProcessor = new TestAcmAuditReportProcessor();
            string fileName = "report.xlsx";
    
            testProcessor.TestProcessSingleFile(fileName);
 
            Assert.IsTrue(testProcessor.FilesDeleted.Contains(fileName));
     }

      #endregion

        #region Multiple Files Processing Tests

        [TestMethod]
        public void ProcessSimulatedFiles_ProcessesAllValid()
        {
     var testProcessor = new TestAcmAuditReportProcessor();
    testProcessor.AddSimulatedFile("report1.xlsx");
   testProcessor.AddSimulatedFile("report2.xlsx");
            testProcessor.AddSimulatedFile("report3.xlsx");
        
int result = testProcessor.TestProcessSimulatedFiles();
       
 Assert.AreEqual(3, result);
  Assert.AreEqual(3, testProcessor.ReportsProcessedThisSession.Count);
        }

        [TestMethod]
        public void ProcessSimulatedFiles_SkipsInvalidFiles()
    {
            var testProcessor = new TestAcmAuditReportProcessor();
            testProcessor.AddSimulatedFile("report1.xlsx");
   testProcessor.AddSimulatedFile("document.pdf");
 testProcessor.AddSimulatedFile("report2.xlsx");
     
    int result = testProcessor.TestProcessSimulatedFiles();
            
      Assert.AreEqual(2, result);
   }

        [TestMethod]
        public void ProcessSimulatedFiles_SkipsEmptyFiles()
        {
   var testProcessor = new TestAcmAuditReportProcessor();
       testProcessor.AddSimulatedFile("report1.xlsx", fileSize: 1024);
            testProcessor.AddSimulatedFile("empty.xlsx", fileSize: 0);
            testProcessor.AddSimulatedFile("report2.xlsx", fileSize: 2048);
            
  int result = testProcessor.TestProcessSimulatedFiles();
  
      Assert.AreEqual(2, result);
      }

 [TestMethod]
public void ProcessSimulatedFiles_NoFiles_ReturnsZero()
        {
     var testProcessor = new TestAcmAuditReportProcessor();
     
  int result = testProcessor.TestProcessSimulatedFiles();
  
 Assert.AreEqual(0, result);
      }

        #endregion

        #region Reset Tests

        [TestMethod]
      public void Reset_ClearsAllData()
        {
            var testProcessor = new TestAcmAuditReportProcessor();
          testProcessor.TestProcessSingleFile("report1.xlsx");
      testProcessor.TestProcessSingleFile("report2.xlsx");
    
            testProcessor.Reset();
       
            Assert.AreEqual(0, testProcessor.ProcessedCount);
     Assert.AreEqual(0, testProcessor.ReportsProcessedThisSession.Count);
      Assert.AreEqual(0, testProcessor.FileCopyOperations.Count);
          Assert.AreEqual(0, testProcessor.FilesDeleted.Count);
        }

        [TestMethod]
        public void Reset_ClearsSimulatedFiles()
        {
         var testProcessor = new TestAcmAuditReportProcessor();
      testProcessor.AddSimulatedFile("report.xlsx");
    
          testProcessor.Reset();
      
     Assert.AreEqual(0, testProcessor.SimulatedFiles.Count);
        }

        [TestMethod]
  public void Reset_ClearsErrorState()
        {
    var testProcessor = new TestAcmAuditReportProcessor();
            testProcessor.TestProcessSingleFile("report.xlsx");
  
         testProcessor.Reset();
 
         Assert.IsFalse(testProcessor.ErrorOccurred);
         Assert.IsNull(testProcessor.LastErrorMessage);
 }

  #endregion

        #region Error Handling Tests

   [TestMethod]
        public void ProcessSingleFile_NoErrorDuringNormalProcessing()
     {
  var testProcessor = new TestAcmAuditReportProcessor();
     
    testProcessor.TestProcessSingleFile("report.xlsx");
   
 Assert.IsFalse(testProcessor.ErrorOccurred);
          Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

        #region Add Simulated File Tests

        [TestMethod]
public void AddSimulatedFile_AddsToList()
        {
       var testProcessor = new TestAcmAuditReportProcessor();
      
  testProcessor.AddSimulatedFile("report.xlsx");
      
    Assert.AreEqual(1, testProcessor.SimulatedFiles.Count);
    }

        [TestMethod]
        public void AddSimulatedFile_SetsProperties()
        {
    var testProcessor = new TestAcmAuditReportProcessor();
string fileName = "ACM_Report_Jan2024.xlsx";
       long fileSize = 50000;
         DateTime lastWrite = new DateTime(2024, 1, 15, 10, 0, 0);
            
       testProcessor.AddSimulatedFile(fileName, fileSize, lastWrite);
 
     var file = testProcessor.SimulatedFiles[0];
          Assert.AreEqual(fileName, file.FileName);
      Assert.AreEqual(fileSize, file.FileSize);
  Assert.AreEqual(lastWrite, file.LastWriteTime);
        }

        [TestMethod]
   public void AddSimulatedFile_DefaultFileSize()
        {
      var testProcessor = new TestAcmAuditReportProcessor();
      
      testProcessor.AddSimulatedFile("report.xlsx");
     
         Assert.AreEqual(1024, testProcessor.SimulatedFiles[0].FileSize);
        }

        #endregion

        #region Report Name Tests

        [TestMethod]
public void TestReportName_HasCorrectValue()
        {
     Assert.AreEqual("Egrants ACM Monthly Audit Report", TestAcmAuditReportProcessor.TestReportName);
        }

      #endregion
    }
}

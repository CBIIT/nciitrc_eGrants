using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailTests.LoadPfr
{
    [TestClass]
    public class LoadPfrTests
    {
        #region Folder Validation Tests

        [TestMethod]
        public void IsValidPfrFolder_FolderId19_ReturnsTrue()
        {
            var testProcessor = new TestLoadPfrProcessor();
   Assert.IsTrue(testProcessor.IsValidPfrFolder("19"));
        }

   [TestMethod]
  public void IsValidPfrFolder_OtherFolderId_ReturnsFalse()
        {
     var testProcessor = new TestLoadPfrProcessor();
            Assert.IsFalse(testProcessor.IsValidPfrFolder("20"));
     }

        [TestMethod]
     public void IsValidPfrFolder_EmptyFolderId_ReturnsFalse()
 {
            var testProcessor = new TestLoadPfrProcessor();
  Assert.IsFalse(testProcessor.IsValidPfrFolder(""));
        }

        [TestMethod]
     public void IsValidPfrFolder_NullFolderId_ReturnsFalse()
        {
    var testProcessor = new TestLoadPfrProcessor();
        Assert.IsFalse(testProcessor.IsValidPfrFolder(null));
        }

        #endregion

  #region File Extension Validation Tests

    [TestMethod]
        public void IsValidFileExtension_Pdf_ReturnsTrue()
        {
            var testProcessor = new TestLoadPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("pdf"));
        }

     [TestMethod]
        public void IsValidFileExtension_Doc_ReturnsTrue()
        {
            var testProcessor = new TestLoadPfrProcessor();
    Assert.IsTrue(testProcessor.IsValidFileExtension("doc"));
        }

        [TestMethod]
        public void IsValidFileExtension_Docx_ReturnsTrue()
        {
            var testProcessor = new TestLoadPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("docx"));
     }

     [TestMethod]
        public void IsValidFileExtension_Txt_ReturnsTrue()
        {
            var testProcessor = new TestLoadPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("txt"));
        }

    [TestMethod]
   public void IsValidFileExtension_Xlsx_ReturnsFalse()
        {
       var testProcessor = new TestLoadPfrProcessor();
         Assert.IsFalse(testProcessor.IsValidFileExtension("xlsx"));
        }

      [TestMethod]
   public void IsValidFileExtension_Empty_ReturnsFalse()
        {
    var testProcessor = new TestLoadPfrProcessor();
 Assert.IsFalse(testProcessor.IsValidFileExtension(""));
        }

        [TestMethod]
        public void IsValidFileExtension_UpperCase_ReturnsTrue()
        {
       var testProcessor = new TestLoadPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("PDF"));
  }

        #endregion

     #region Destination Alias Building Tests

        [TestMethod]
        public void BuildDestinationAlias_ValidInputs_ReturnsCorrectAlias()
        {
            var testProcessor = new TestLoadPfrProcessor();
     
            string result = testProcessor.BuildDestinationAlias("12345678", "pdf");
       
     Assert.AreEqual("12345678.pdf", result);
        }

        [TestMethod]
        public void BuildDestinationAlias_EmptyFileNumber_ReturnsNull()
        {
            var testProcessor = new TestLoadPfrProcessor();
          
            string result = testProcessor.BuildDestinationAlias("", "pdf");
        
            Assert.IsNull(result);
        }

     [TestMethod]
        public void BuildDestinationAlias_NullFileNumber_ReturnsNull()
        {
            var testProcessor = new TestLoadPfrProcessor();
            
            string result = testProcessor.BuildDestinationAlias(null, "pdf");
      
            Assert.IsNull(result);
        }

        [TestMethod]
    public void BuildDestinationAlias_PreservesFileType()
    {
         var testProcessor = new TestLoadPfrProcessor();
     
      string result = testProcessor.BuildDestinationAlias("report123", "docx");
            
   Assert.IsTrue(result.EndsWith(".docx"));
        }

        #endregion

        #region XML Parsing Tests

     [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsApplId()
        {
      var testProcessor = new TestLoadPfrProcessor();
       string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>report.pdf</filename></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

            Assert.AreEqual("12345678", result.ApplId);
     }

        [TestMethod]
  public void ParseXmlNode_ValidXml_ExtractsFolderId()
        {
        var testProcessor = new TestLoadPfrProcessor();
     string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>report.pdf</filename></record></root>";

     var result = testProcessor.ParseXmlNodePublic(xml);

            Assert.AreEqual("19", result.FolderId);
        }

      [TestMethod]
        public void ParseXmlNode_FolderId19_SetsCategoryNamePFR()
        {
     var testProcessor = new TestLoadPfrProcessor();
      string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>report.pdf</filename></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

            Assert.AreEqual("PFR", result.CategoryName);
        }

     [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsFileName()
        {
         var testProcessor = new TestLoadPfrProcessor();
      string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>progress_report.pdf</filename></record></root>";

         var result = testProcessor.ParseXmlNodePublic(xml);

            Assert.AreEqual("progress_report.pdf", result.FileName);
     }

   [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsDate()
    {
            var testProcessor = new TestLoadPfrProcessor();
     string xml = @"<root><record><applid>12345678</applid><date>2024-01-15</date></record></root>";

      var result = testProcessor.ParseXmlNodePublic(xml);

            Assert.AreEqual("2024-01-15", result.DocDate);
        }

     [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsFileType()
    {
            var testProcessor = new TestLoadPfrProcessor();
     string xml = @"<root><record><applid>12345678</applid><file_type>pdf</file_type></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

     Assert.AreEqual("pdf", result.FileType);
        }

        [TestMethod]
      public void ParseXmlNode_ValidXml_ExtractsCreatedBy()
        {
            var testProcessor = new TestLoadPfrProcessor();
            string xml = @"<root><record><applid>12345678</applid><uid>jsmith</uid></record></root>";

       var result = testProcessor.ParseXmlNodePublic(xml);

 Assert.AreEqual("jsmith", result.CreatedBy);
     }

     [TestMethod]
        public void ParseXmlNode_EmptyXml_ReturnsEmptyData()
  {
     var testProcessor = new TestLoadPfrProcessor();

  var result = testProcessor.ParseXmlNodePublic("");

          Assert.IsNull(result.ApplId);
    Assert.IsNull(result.FileName);
      }

        [TestMethod]
        public void ParseXmlNode_InvalidXml_ReturnsEmptyData()
        {
            var testProcessor = new TestLoadPfrProcessor();

     var result = testProcessor.ParseXmlNodePublic("not valid xml");

            Assert.IsNull(result.ApplId);
        }

        #endregion

     #region Single Record Processing Tests

        [TestMethod]
        public void ProcessSingleXmlRecord_ValidData_CreatesRecord()
    {
            var testProcessor = new TestLoadPfrProcessor();

         var result = testProcessor.TestProcessSingleXmlRecord(
  "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

   Assert.IsNotNull(result);
       Assert.AreEqual("12345678", result.ApplId);
        }

        [TestMethod]
 public void ProcessSingleXmlRecord_FolderId19_SetsCategoryNamePFR()
        {
            var testProcessor = new TestLoadPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
      "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

      Assert.AreEqual("PFR", result.CategoryName);
        }

   [TestMethod]
     public void ProcessSingleXmlRecord_OtherFolderId_EmptyCategoryName()
    {
        var testProcessor = new TestLoadPfrProcessor();

        var result = testProcessor.TestProcessSingleXmlRecord(
           "12345678", "20", "report.pdf", "2024-01-15", "pdf", "jsmith");

  Assert.AreEqual("", result.CategoryName);
        }

 [TestMethod]
        public void ProcessSingleXmlRecord_EmptyApplId_ReturnsNull()
   {
            var testProcessor = new TestLoadPfrProcessor();

    var result = testProcessor.TestProcessSingleXmlRecord(
                "", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

  Assert.IsNull(result);
     }

        [TestMethod]
  public void ProcessSingleXmlRecord_EmptyFileName_ReturnsNull()
        {
        var testProcessor = new TestLoadPfrProcessor();

    var result = testProcessor.TestProcessSingleXmlRecord(
              "12345678", "19", "", "2024-01-15", "pdf", "jsmith");

            Assert.IsNull(result);
  }

        [TestMethod]
    public void ProcessSingleXmlRecord_IncrementsCounter()
        {
            var testProcessor = new TestLoadPfrProcessor();

            testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf", "user1");
            testProcessor.TestProcessSingleXmlRecord("222", "19", "b.pdf", "2024-01-02", "pdf", "user2");
            testProcessor.TestProcessSingleXmlRecord("333", "19", "c.pdf", "2024-01-03", "pdf", "user3");

            Assert.AreEqual(3, testProcessor.ProcessedCount);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_CapturesAllFields()
        {
            var testProcessor = new TestLoadPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
              "12345678", "19", "progress_report.pdf", "2024-01-15", "pdf", "jsmith");

            Assert.AreEqual("12345678", result.ApplId);
     Assert.AreEqual("progress_report.pdf", result.FileName);
            Assert.AreEqual("2024-01-15", result.DocumentDate);
     Assert.AreEqual("pdf", result.FileType);
     Assert.AreEqual("jsmith", result.CreatedBy);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_MarksAsInserted()
      {
      var testProcessor = new TestLoadPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
     "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

     Assert.IsTrue(result.WasInsertedToDatabase);
        }

     #endregion

    #region File Copy Operations Tests

      [TestMethod]
   public void ProcessSingleXmlRecord_CreatesFileCopyOperation()
        {
   var testProcessor = new TestLoadPfrProcessor();

            testProcessor.TestProcessSingleXmlRecord(
       "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

      Assert.AreEqual(1, testProcessor.FileCopyOperations.Count);
     }

  [TestMethod]
        public void ProcessSingleXmlRecord_FileCopyToFinalDestination()
    {
          var testProcessor = new TestLoadPfrProcessor();

        testProcessor.TestProcessSingleXmlRecord(
    "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

            Assert.IsTrue(testProcessor.FileCopyOperations.Exists(
      op => op.DestinationType == "FinalDestination"));
        }

        #endregion

        #region File Backup Operations Tests

     [TestMethod]
        public void ProcessSingleXmlRecord_CreatesFileBackupOperation()
        {
  var testProcessor = new TestLoadPfrProcessor();

 testProcessor.TestProcessSingleXmlRecord(
   "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

         Assert.IsTrue(testProcessor.FileBackupOperations.Exists(
       op => op.BackupType == "PDF"));
        }

        [TestMethod]
        public void ProcessSimulatedXmlFiles_CreatesXmlBackup()
        {
        var testProcessor = new TestLoadPfrProcessor();
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
     "metadata.xml", "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

     testProcessor.TestProcessSimulatedXmlFiles();

       Assert.IsTrue(testProcessor.FileBackupOperations.Exists(
          op => op.BackupType == "XML"));
        }

        #endregion

      #region Multiple Files Processing Tests

    [TestMethod]
        public void ProcessSimulatedXmlFiles_ProcessesAllRecords()
   {
        var testProcessor = new TestLoadPfrProcessor();
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
      "file1.xml", "111", "19", "a.pdf", "2024-01-01", "pdf", "user1");
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
        "file2.xml", "222", "19", "b.pdf", "2024-01-02", "pdf", "user2");
 testProcessor.AddSimulatedXmlFileWithSingleRecord(
       "file3.xml", "333", "19", "c.pdf", "2024-01-03", "pdf", "user3");

     int result = testProcessor.TestProcessSimulatedXmlFiles();

       Assert.AreEqual(3, result);
   Assert.AreEqual(3, testProcessor.PfrRecordsProcessedThisSession.Count);
        }

        [TestMethod]
        public void ProcessSimulatedXmlFiles_MultipleRecordsPerFile()
        {
 var testProcessor = new TestLoadPfrProcessor();
          var records = new List<SimulatedPfrRecord>
 {
    new SimulatedPfrRecord { ApplId = "111", FolderId = "19", FileName = "a.pdf", DocDate = "2024-01-01", FileType = "pdf", CreatedBy = "user1" },
       new SimulatedPfrRecord { ApplId = "222", FolderId = "19", FileName = "b.pdf", DocDate = "2024-01-02", FileType = "pdf", CreatedBy = "user2" }
            };
            testProcessor.AddSimulatedXmlFile("multi.xml", records);

    int result = testProcessor.TestProcessSimulatedXmlFiles();

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ProcessSimulatedXmlFiles_NoFiles_ReturnsZero()
   {
            var testProcessor = new TestLoadPfrProcessor();

 int result = testProcessor.TestProcessSimulatedXmlFiles();

   Assert.AreEqual(0, result);
        }

        #endregion

        #region Reset Tests

        [TestMethod]
   public void Reset_ClearsAllData()
        {
     var testProcessor = new TestLoadPfrProcessor();
         testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf", "user1");
  testProcessor.TestProcessSingleXmlRecord("222", "19", "b.pdf", "2024-01-02", "pdf", "user2");

            testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.ProcessedCount);
      Assert.AreEqual(0, testProcessor.PfrRecordsProcessedThisSession.Count);
  Assert.AreEqual(0, testProcessor.FileCopyOperations.Count);
        Assert.AreEqual(0, testProcessor.FileBackupOperations.Count);
        }

        [TestMethod]
        public void Reset_ClearsSimulatedFiles()
        {
            var testProcessor = new TestLoadPfrProcessor();
testProcessor.AddSimulatedXmlFileWithSingleRecord(
    "test.xml", "111", "19", "a.pdf", "2024-01-01", "pdf", "user1");

            testProcessor.Reset();

    Assert.AreEqual(0, testProcessor.SimulatedXmlFiles.Count);
        }

        [TestMethod]
        public void Reset_ClearsErrorState()
        {
      var testProcessor = new TestLoadPfrProcessor();
            testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf", "user1");

 testProcessor.Reset();

            Assert.IsFalse(testProcessor.ErrorOccurred);
            Assert.IsNull(testProcessor.LastErrorMessage);
  }

      #endregion

        #region Error Handling Tests

[TestMethod]
      public void ProcessSingleXmlRecord_NoErrorDuringNormalProcessing()
        {
    var testProcessor = new TestLoadPfrProcessor();

testProcessor.TestProcessSingleXmlRecord(
     "12345678", "19", "report.pdf", "2024-01-15", "pdf", "jsmith");

            Assert.IsFalse(testProcessor.ErrorOccurred);
            Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

        #region Add Simulated File Tests

[TestMethod]
    public void AddSimulatedXmlFileWithSingleRecord_AddsToList()
    {
    var testProcessor = new TestLoadPfrProcessor();

   testProcessor.AddSimulatedXmlFileWithSingleRecord(
      "test.xml", "111", "19", "a.pdf", "2024-01-01", "pdf", "user1");

            Assert.AreEqual(1, testProcessor.SimulatedXmlFiles.Count);
        }

        [TestMethod]
        public void AddSimulatedXmlFileWithSingleRecord_SetsXmlFileName()
        {
        var testProcessor = new TestLoadPfrProcessor();
            string xmlFileName = "progress_report_metadata.xml";

  testProcessor.AddSimulatedXmlFileWithSingleRecord(
          xmlFileName, "111", "19", "a.pdf", "2024-01-01", "pdf", "user1");

    Assert.AreEqual(xmlFileName, testProcessor.SimulatedXmlFiles[0].XmlFileName);
        }

        [TestMethod]
        public void AddSimulatedXmlFile_MultipleRecords_AddsAll()
        {
 var testProcessor = new TestLoadPfrProcessor();
            var records = new List<SimulatedPfrRecord>
      {
       new SimulatedPfrRecord { ApplId = "111", FileName = "a.pdf" },
   new SimulatedPfrRecord { ApplId = "222", FileName = "b.pdf" },
      new SimulatedPfrRecord { ApplId = "333", FileName = "c.pdf" }
     };

            testProcessor.AddSimulatedXmlFile("multi.xml", records);

         Assert.AreEqual(3, testProcessor.SimulatedXmlFiles[0].Records.Count);
        }

        #endregion
    }
}

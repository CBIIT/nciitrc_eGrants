using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.LoadSuppPfr
{
    [TestClass]
    public class LoadSuppPfrTests
    {
 #region Folder Validation Tests

        [TestMethod]
        public void IsValidSuppPfrFolder_FolderId19_ReturnsTrue()
        {
    var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidSuppPfrFolder("19"));
 }

        [TestMethod]
        public void IsValidSuppPfrFolder_OtherFolderId_ReturnsFalse()
        {
 var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsFalse(testProcessor.IsValidSuppPfrFolder("20"));
   }

        [TestMethod]
        public void IsValidSuppPfrFolder_EmptyFolderId_ReturnsFalse()
{
       var testProcessor = new TestLoadSuppPfrProcessor();
 Assert.IsFalse(testProcessor.IsValidSuppPfrFolder(""));
        }

 [TestMethod]
        public void IsValidSuppPfrFolder_NullFolderId_ReturnsFalse()
        {
 var testProcessor = new TestLoadSuppPfrProcessor();
   Assert.IsFalse(testProcessor.IsValidSuppPfrFolder(null));
        }

   #endregion

  #region File Extension Validation Tests

        [TestMethod]
   public void IsValidFileExtension_Pdf_ReturnsTrue()
   {
      var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("pdf"));
        }

 [TestMethod]
        public void IsValidFileExtension_Doc_ReturnsTrue()
 {
            var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("doc"));
        }

        [TestMethod]
 public void IsValidFileExtension_Docx_ReturnsTrue()
      {
      var testProcessor = new TestLoadSuppPfrProcessor();
  Assert.IsTrue(testProcessor.IsValidFileExtension("docx"));
  }

[TestMethod]
        public void IsValidFileExtension_Txt_ReturnsTrue()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsTrue(testProcessor.IsValidFileExtension("txt"));
        }

        [TestMethod]
        public void IsValidFileExtension_Xlsx_ReturnsFalse()
     {
 var testProcessor = new TestLoadSuppPfrProcessor();
       Assert.IsFalse(testProcessor.IsValidFileExtension("xlsx"));
        }

        [TestMethod]
        public void IsValidFileExtension_Empty_ReturnsFalse()
        {
  var testProcessor = new TestLoadSuppPfrProcessor();
            Assert.IsFalse(testProcessor.IsValidFileExtension(""));
    }

        #endregion

        #region Subject Line Building Tests

        [TestMethod]
   public void BuildSubjectLine_ValidApplId_ReturnsCorrectFormat()
        {
  var testProcessor = new TestLoadSuppPfrProcessor();

         string result = testProcessor.BuildSubjectLine("12345678");

     Assert.AreEqual("Supplement PFR - 12345678", result);
 }

  [TestMethod]
    public void BuildSubjectLine_ContainsSupplementPfr()
        {
  var testProcessor = new TestLoadSuppPfrProcessor();

  string result = testProcessor.BuildSubjectLine("99999999");

            Assert.IsTrue(result.Contains("Supplement PFR"));
     }

        [TestMethod]
        public void BuildSubjectLine_ContainsApplId()
  {
            var testProcessor = new TestLoadSuppPfrProcessor();
 string applId = "87654321";

            string result = testProcessor.BuildSubjectLine(applId);

            Assert.IsTrue(result.Contains(applId));
        }

        [TestMethod]
        public void BuildSubjectLine_EmptyApplId_ReturnsEmpty()
        {
  var testProcessor = new TestLoadSuppPfrProcessor();

            string result = testProcessor.BuildSubjectLine("");

            Assert.AreEqual("", result);
        }

      [TestMethod]
        public void BuildSubjectLine_NullApplId_ReturnsEmpty()
      {
     var testProcessor = new TestLoadSuppPfrProcessor();

string result = testProcessor.BuildSubjectLine(null);

            Assert.AreEqual("", result);
      }

        #endregion

        #region Destination Alias Building Tests

     [TestMethod]
        public void BuildDestinationAlias_ValidInputs_ReturnsCorrectAlias()
        {
 var testProcessor = new TestLoadSuppPfrProcessor();

   string result = testProcessor.BuildDestinationAlias("12345678", "pdf");

  Assert.AreEqual("12345678.pdf", result);
      }

        [TestMethod]
     public void BuildDestinationAlias_EmptyFileNumber_ReturnsNull()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

            string result = testProcessor.BuildDestinationAlias("", "pdf");

   Assert.IsNull(result);
     }

        [TestMethod]
        public void BuildDestinationAlias_NullFileNumber_ReturnsNull()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

    string result = testProcessor.BuildDestinationAlias(null, "pdf");

            Assert.IsNull(result);
      }

        #endregion

        #region XML Parsing Tests

        [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsApplId()
        {
  var testProcessor = new TestLoadSuppPfrProcessor();
            string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>supp_report.pdf</filename></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

       Assert.AreEqual("12345678", result.ApplId);
        }

        [TestMethod]
   public void ParseXmlNode_ValidXml_ExtractsFolderId()
        {
     var testProcessor = new TestLoadSuppPfrProcessor();
         string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>supp_report.pdf</filename></record></root>";

var result = testProcessor.ParseXmlNodePublic(xml);

         Assert.AreEqual("19", result.FolderId);
    }

        [TestMethod]
   public void ParseXmlNode_FolderId19_SetsCategoryNamePFR()
    {
       var testProcessor = new TestLoadSuppPfrProcessor();
         string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>supp_report.pdf</filename></record></root>";

         var result = testProcessor.ParseXmlNodePublic(xml);

    Assert.AreEqual("PFR", result.CategoryName);
        }

        [TestMethod]
   public void ParseXmlNode_ValidXml_ExtractsFileName()
        {
      var testProcessor = new TestLoadSuppPfrProcessor();
            string xml = @"<root><record><applid>12345678</applid><folderid>19</folderid><filename>supplement_progress.pdf</filename></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

          Assert.AreEqual("supplement_progress.pdf", result.FileName);
        }

      [TestMethod]
    public void ParseXmlNode_ValidXml_ExtractsDate()
        {
    var testProcessor = new TestLoadSuppPfrProcessor();
            string xml = @"<root><record><applid>12345678</applid><date>2024-01-15</date></record></root>";

       var result = testProcessor.ParseXmlNodePublic(xml);

        Assert.AreEqual("2024-01-15", result.DocDate);
        }

        [TestMethod]
        public void ParseXmlNode_ValidXml_ExtractsFileType()
        {
    var testProcessor = new TestLoadSuppPfrProcessor();
         string xml = @"<root><record><applid>12345678</applid><file_type>pdf</file_type></record></root>";

            var result = testProcessor.ParseXmlNodePublic(xml);

      Assert.AreEqual("pdf", result.FileType);
      }

   [TestMethod]
public void ParseXmlNode_EmptyXml_ReturnsEmptyData()
        {
     var testProcessor = new TestLoadSuppPfrProcessor();

       var result = testProcessor.ParseXmlNodePublic("");

  Assert.IsNull(result.ApplId);
     Assert.IsNull(result.FileName);
        }

        [TestMethod]
        public void ParseXmlNode_InvalidXml_ReturnsEmptyData()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

      var result = testProcessor.ParseXmlNodePublic("not valid xml");

  Assert.IsNull(result.ApplId);
     }

    #endregion

        #region Single Record Processing Tests

 [TestMethod]
        public void ProcessSingleXmlRecord_ValidData_CreatesRecord()
        {
     var testProcessor = new TestLoadSuppPfrProcessor();

    var result = testProcessor.TestProcessSingleXmlRecord(
            "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

Assert.IsNotNull(result);
      Assert.AreEqual("12345678", result.ApplId);
        }

        [TestMethod]
      public void ProcessSingleXmlRecord_FolderId19_SetsCategoryNamePFR()
        {
var testProcessor = new TestLoadSuppPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
                "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

Assert.AreEqual("PFR", result.CategoryName);
      }

        [TestMethod]
     public void ProcessSingleXmlRecord_SetsSubCategoryPFR()
        {
       var testProcessor = new TestLoadSuppPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
           "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

            Assert.AreEqual("PFR", result.SubCategory);
   }

        [TestMethod]
        public void ProcessSingleXmlRecord_SetsCorrectSubjectLine()
    {
            var testProcessor = new TestLoadSuppPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
        "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

     Assert.AreEqual("Supplement PFR - 12345678", result.SubjectLine);
        }

   [TestMethod]
        public void ProcessSingleXmlRecord_OtherFolderId_EmptyCategoryName()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
                "12345678", "20", "supp_report.pdf", "2024-01-15", "pdf");

         Assert.AreEqual("", result.CategoryName);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_EmptyApplId_ReturnsNull()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

            var result = testProcessor.TestProcessSingleXmlRecord(
      "", "19", "supp_report.pdf", "2024-01-15", "pdf");

        Assert.IsNull(result);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_EmptyFileName_ReturnsNull()
        {
   var testProcessor = new TestLoadSuppPfrProcessor();

     var result = testProcessor.TestProcessSingleXmlRecord(
            "12345678", "19", "", "2024-01-15", "pdf");

   Assert.IsNull(result);
    }

      [TestMethod]
        public void ProcessSingleXmlRecord_IncrementsCounter()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

        testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf");
        testProcessor.TestProcessSingleXmlRecord("222", "19", "b.pdf", "2024-01-02", "pdf");
  testProcessor.TestProcessSingleXmlRecord("333", "19", "c.pdf", "2024-01-03", "pdf");

        Assert.AreEqual(3, testProcessor.ProcessedCount);
        }

 [TestMethod]
      public void ProcessSingleXmlRecord_CapturesAllFields()
      {
         var testProcessor = new TestLoadSuppPfrProcessor();

   var result = testProcessor.TestProcessSingleXmlRecord(
                "12345678", "19", "supplement_report.pdf", "2024-01-15", "pdf");

 Assert.AreEqual("12345678", result.ApplId);
            Assert.AreEqual("supplement_report.pdf", result.FileName);
         Assert.AreEqual("2024-01-15", result.DocumentDate);
            Assert.AreEqual("pdf", result.FileType);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_MarksAsInserted()
        {
   var testProcessor = new TestLoadSuppPfrProcessor();

       var result = testProcessor.TestProcessSingleXmlRecord(
       "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

       Assert.IsTrue(result.WasInsertedToDatabase);
        }

        #endregion

        #region File Copy Operations Tests

        [TestMethod]
        public void ProcessSingleXmlRecord_CreatesFileCopyOperation()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

testProcessor.TestProcessSingleXmlRecord(
         "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

            Assert.AreEqual(1, testProcessor.FileCopyOperations.Count);
        }

        [TestMethod]
        public void ProcessSingleXmlRecord_FileCopyToFinalDestination()
        {
     var testProcessor = new TestLoadSuppPfrProcessor();

          testProcessor.TestProcessSingleXmlRecord(
   "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

       Assert.IsTrue(testProcessor.FileCopyOperations.Exists(
     op => op.DestinationType == "FinalDestination"));
      }

   #endregion

     #region File Backup Operations Tests

     [TestMethod]
        public void ProcessSingleXmlRecord_CreatesFileBackupOperation()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

            testProcessor.TestProcessSingleXmlRecord(
   "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

    Assert.IsTrue(testProcessor.FileBackupOperations.Exists(
      op => op.BackupType == "PDF"));
      }

  [TestMethod]
        public void ProcessSimulatedXmlFiles_CreatesXmlBackup()
        {
var testProcessor = new TestLoadSuppPfrProcessor();
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
            "metadata.xml", "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

         testProcessor.TestProcessSimulatedXmlFiles();

       Assert.IsTrue(testProcessor.FileBackupOperations.Exists(
     op => op.BackupType == "XML"));
        }

        #endregion

        #region Multiple Files Processing Tests

        [TestMethod]
        public void ProcessSimulatedXmlFiles_ProcessesAllRecords()
     {
          var testProcessor = new TestLoadSuppPfrProcessor();
    testProcessor.AddSimulatedXmlFileWithSingleRecord(
         "file1.xml", "111", "19", "a.pdf", "2024-01-01", "pdf");
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
    "file2.xml", "222", "19", "b.pdf", "2024-01-02", "pdf");
        testProcessor.AddSimulatedXmlFileWithSingleRecord(
 "file3.xml", "333", "19", "c.pdf", "2024-01-03", "pdf");

          int result = testProcessor.TestProcessSimulatedXmlFiles();

            Assert.AreEqual(3, result);
        Assert.AreEqual(3, testProcessor.SuppPfrRecordsProcessedThisSession.Count);
        }

        [TestMethod]
        public void ProcessSimulatedXmlFiles_MultipleRecordsPerFile()
        {
var testProcessor = new TestLoadSuppPfrProcessor();
            var records = new List<SimulatedSuppPfrRecord>
  {
           new SimulatedSuppPfrRecord { ApplId = "111", FolderId = "19", FileName = "a.pdf", DocDate = "2024-01-01", FileType = "pdf" },
                new SimulatedSuppPfrRecord { ApplId = "222", FolderId = "19", FileName = "b.pdf", DocDate = "2024-01-02", FileType = "pdf" }
            };
            testProcessor.AddSimulatedXmlFile("multi.xml", records);

       int result = testProcessor.TestProcessSimulatedXmlFiles();

   Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ProcessSimulatedXmlFiles_NoFiles_ReturnsZero()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();

      int result = testProcessor.TestProcessSimulatedXmlFiles();

            Assert.AreEqual(0, result);
        }

        #endregion

        #region Reset Tests

 [TestMethod]
      public void Reset_ClearsAllData()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();
  testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf");
    testProcessor.TestProcessSingleXmlRecord("222", "19", "b.pdf", "2024-01-02", "pdf");

    testProcessor.Reset();

     Assert.AreEqual(0, testProcessor.ProcessedCount);
          Assert.AreEqual(0, testProcessor.SuppPfrRecordsProcessedThisSession.Count);
            Assert.AreEqual(0, testProcessor.FileCopyOperations.Count);
     Assert.AreEqual(0, testProcessor.FileBackupOperations.Count);
        }

  [TestMethod]
     public void Reset_ClearsSimulatedFiles()
  {
            var testProcessor = new TestLoadSuppPfrProcessor();
            testProcessor.AddSimulatedXmlFileWithSingleRecord(
      "test.xml", "111", "19", "a.pdf", "2024-01-01", "pdf");

 testProcessor.Reset();

         Assert.AreEqual(0, testProcessor.SimulatedXmlFiles.Count);
        }

        [TestMethod]
        public void Reset_ClearsErrorState()
        {
            var testProcessor = new TestLoadSuppPfrProcessor();
  testProcessor.TestProcessSingleXmlRecord("111", "19", "a.pdf", "2024-01-01", "pdf");

            testProcessor.Reset();

            Assert.IsFalse(testProcessor.ErrorOccurred);
      Assert.IsNull(testProcessor.LastErrorMessage);
        }

      #endregion

   #region Error Handling Tests

        [TestMethod]
        public void ProcessSingleXmlRecord_NoErrorDuringNormalProcessing()
        {
 var testProcessor = new TestLoadSuppPfrProcessor();

   testProcessor.TestProcessSingleXmlRecord(
          "12345678", "19", "supp_report.pdf", "2024-01-15", "pdf");

            Assert.IsFalse(testProcessor.ErrorOccurred);
        Assert.IsNull(testProcessor.LastErrorMessage);
        }

     #endregion

        #region Add Simulated File Tests

  [TestMethod]
        public void AddSimulatedXmlFileWithSingleRecord_AddsToList()
        {
         var testProcessor = new TestLoadSuppPfrProcessor();

            testProcessor.AddSimulatedXmlFileWithSingleRecord(
      "test.xml", "111", "19", "a.pdf", "2024-01-01", "pdf");

    Assert.AreEqual(1, testProcessor.SimulatedXmlFiles.Count);
        }

 [TestMethod]
        public void AddSimulatedXmlFileWithSingleRecord_SetsXmlFileName()
     {
       var testProcessor = new TestLoadSuppPfrProcessor();
 string xmlFileName = "supplement_pfr_metadata.xml";

       testProcessor.AddSimulatedXmlFileWithSingleRecord(
         xmlFileName, "111", "19", "a.pdf", "2024-01-01", "pdf");

       Assert.AreEqual(xmlFileName, testProcessor.SimulatedXmlFiles[0].XmlFileName);
        }

        [TestMethod]
      public void AddSimulatedXmlFile_MultipleRecords_AddsAll()
        {
         var testProcessor = new TestLoadSuppPfrProcessor();
var records = new List<SimulatedSuppPfrRecord>
     {
         new SimulatedSuppPfrRecord { ApplId = "111", FileName = "a.pdf" },
       new SimulatedSuppPfrRecord { ApplId = "222", FileName = "b.pdf" },
       new SimulatedSuppPfrRecord { ApplId = "333", FileName = "c.pdf" }
 };

          testProcessor.AddSimulatedXmlFile("multi.xml", records);

Assert.AreEqual(3, testProcessor.SimulatedXmlFiles[0].Records.Count);
        }

        #endregion
  }
}

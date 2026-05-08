using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailTests.ExchangeFixed
{
    [TestClass]
    public class ExchangeFixedTests
    {
        #region Subject Parsing Tests - GrantNumber

        [TestMethod]
        public void ParseSubjectLine_WithGrantNumber_ExtractsGrantNumber()
    {
   var testProcessor = new TestExchangeFixedProcessor();
    string subject = "grantnumber=5R01CA123456, category=Funding";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.AreEqual("5R01CA123456", result.GrantNumber);
    }

     [TestMethod]
        public void ParseSubjectLine_WithApplId_ExtractsApplId()
        {
            var testProcessor = new TestExchangeFixedProcessor();
      string subject = "applid=12345678, category=Correspondence";

  var result = testProcessor.ParseSubjectLinePublic(subject);

         Assert.AreEqual("12345678", result.ApplId);
      }

        [TestMethod]
        public void ParseSubjectLine_WithCategory_ExtractsCategory()
        {
  var testProcessor = new TestExchangeFixedProcessor();
  string subject = "grantnumber=5R01CA123456, category=Closeout";

          var result = testProcessor.ParseSubjectLinePublic(subject);

  Assert.AreEqual("Closeout", result.Category);
     }

        [TestMethod]
        public void ParseSubjectLine_WithSubCategory_ExtractsSubCategory()
    {
        var testProcessor = new TestExchangeFixedProcessor();
   string subject = "grantnumber=5R01CA123456, sub=Amendment";

      var result = testProcessor.ParseSubjectLinePublic(subject);

  Assert.AreEqual("Amendment", result.SubCategory);
  }

        [TestMethod]
        public void ParseSubjectLine_WithExtract_ExtractsExtract()
        {
  var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, extract=2";

          var result = testProcessor.ParseSubjectLinePublic(subject);

    Assert.AreEqual("2", result.Extract);
    }

        [TestMethod]
        public void ParseSubjectLine_AllParams_ExtractsAll()
        {
   var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding, sub=NoA, extract=3";

 var result = testProcessor.ParseSubjectLinePublic(subject);

     Assert.AreEqual("5R01CA123456", result.GrantNumber);
            Assert.AreEqual("Funding", result.Category);
     Assert.AreEqual("NoA", result.SubCategory);
 Assert.AreEqual("3", result.Extract);
        }

        [TestMethod]
        public void ParseSubjectLine_EmptySubject_ReturnsEmptyParams()
     {
 var testProcessor = new TestExchangeFixedProcessor();

     var result = testProcessor.ParseSubjectLinePublic("");

       Assert.IsNull(result.GrantNumber);
            Assert.IsNull(result.ApplId);
        }

        [TestMethod]
public void ParseSubjectLine_NoValidParams_ReturnsEmptyParams()
    {
 var testProcessor = new TestExchangeFixedProcessor();
 string subject = "RE: Please review this document";

     var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.IsNull(result.GrantNumber);
            Assert.IsNull(result.ApplId);
        }

        #endregion

        #region Value Extraction Tests

 [TestMethod]
        public void ExtractValue_ValidKeyValue_ReturnsValue()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.ExtractValuePublic("grantnumber=5R01CA123456", "grantnumber");

            Assert.AreEqual("5R01CA123456", result);
   }

    [TestMethod]
      public void ExtractValue_WrongKey_ReturnsNull()
        {
    var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.ExtractValuePublic("grantnumber=5R01CA123456", "applid");

       Assert.IsNull(result);
        }

        [TestMethod]
        public void ExtractValue_EmptyString_ReturnsNull()
        {
   var testProcessor = new TestExchangeFixedProcessor();

          string result = testProcessor.ExtractValuePublic("", "grantnumber");

  Assert.IsNull(result);
 }

        [TestMethod]
        public void ExtractValue_NoEqualsSign_ReturnsNull()
        {
            var testProcessor = new TestExchangeFixedProcessor();

        string result = testProcessor.ExtractValuePublic("grantnumber5R01CA123456", "grantnumber");

  Assert.IsNull(result);
        }

        [TestMethod]
        public void ExtractValue_TrimsSpaces_ReturnsCleanValue()
    {
          var testProcessor = new TestExchangeFixedProcessor();

     string result = testProcessor.ExtractValuePublic("grantnumber = 5R01CA123456 ", "grantnumber");

   Assert.AreEqual("5R01CA123456", result);
        }

        #endregion

        #region File Type Extraction Tests

     [TestMethod]
        public void GetFileType_PdfFile_ReturnsPdf()
  {
    var testProcessor = new TestExchangeFixedProcessor();

        string result = testProcessor.GetFileTypePublic("document.pdf");

       Assert.AreEqual("pdf", result);
        }

        [TestMethod]
  public void GetFileType_DocxFile_ReturnsDocx()
        {
var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.GetFileTypePublic("document.docx");

            Assert.AreEqual("docx", result);
        }

    [TestMethod]
        public void GetFileType_XlsxFile_ReturnsXlsx()
   {
       var testProcessor = new TestExchangeFixedProcessor();

 string result = testProcessor.GetFileTypePublic("spreadsheet.xlsx");

   Assert.AreEqual("xlsx", result);
        }

        [TestMethod]
     public void GetFileType_NoExtension_ReturnsTxt()
        {
       var testProcessor = new TestExchangeFixedProcessor();

      string result = testProcessor.GetFileTypePublic("filename");

     Assert.AreEqual("txt", result);
        }

        [TestMethod]
public void GetFileType_EmptyString_ReturnsTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.GetFileTypePublic("");

            Assert.AreEqual("txt", result);
      }

        [TestMethod]
 public void GetFileType_MultipleDots_ReturnsLastExtension()
        {
      var testProcessor = new TestExchangeFixedProcessor();

   string result = testProcessor.GetFileTypePublic("file.name.pdf");

            Assert.AreEqual("pdf", result);
        }

        #endregion

  #region Special Character Removal Tests

[TestMethod]
        public void RemoveSpecialChars_ColonRemoved()
        {
            var testProcessor = new TestExchangeFixedProcessor();

        string result = testProcessor.RemoveSpecialCharsPublic("5R01:CA123456");

            Assert.IsFalse(result.Contains(":"));
        }

        [TestMethod]
        public void RemoveSpecialChars_SlashRemoved()
        {
            var testProcessor = new TestExchangeFixedProcessor();

   string result = testProcessor.RemoveSpecialCharsPublic("5R01/CA123456");

          Assert.IsFalse(result.Contains("/"));
    }

  [TestMethod]
        public void RemoveSpecialChars_BackslashRemoved()
        {
 var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.RemoveSpecialCharsPublic("5R01\\CA123456");

       Assert.IsFalse(result.Contains("\\"));
 }

        [TestMethod]
  public void RemoveSpecialChars_SpacesRemoved()
        {
     var testProcessor = new TestExchangeFixedProcessor();

   string result = testProcessor.RemoveSpecialCharsPublic("5R01 CA 123456");

            Assert.IsFalse(result.Contains(" "));
 }

      [TestMethod]
        public void RemoveSpecialChars_EmptyString_ReturnsEmpty()
        {
var testProcessor = new TestExchangeFixedProcessor();

   string result = testProcessor.RemoveSpecialCharsPublic("");

            Assert.AreEqual("", result);
      }

     #endregion

        #region Email Validation Tests

        [TestMethod]
    public void IsValidEmailForProcessing_WithGrantNumber_ReturnsTrue()
 {
      var testProcessor = new TestExchangeFixedProcessor();
         string subject = "grantnumber=5R01CA123456, category=Funding";

Assert.IsTrue(testProcessor.IsValidEmailForProcessing(subject));
        }

        [TestMethod]
    public void IsValidEmailForProcessing_WithApplId_ReturnsTrue()
  {
      var testProcessor = new TestExchangeFixedProcessor();
            string subject = "applid=12345678, category=Correspondence";

            Assert.IsTrue(testProcessor.IsValidEmailForProcessing(subject));
        }

        [TestMethod]
  public void IsValidEmailForProcessing_NoIdentifier_ReturnsFalse()
        {
            var testProcessor = new TestExchangeFixedProcessor();
      string subject = "category=Funding, sub=NoA";

  Assert.IsFalse(testProcessor.IsValidEmailForProcessing(subject));
        }

 [TestMethod]
    public void IsValidEmailForProcessing_RandomSubject_ReturnsFalse()
     {
  var testProcessor = new TestExchangeFixedProcessor();
        string subject = "RE: Please review this document";

            Assert.IsFalse(testProcessor.IsValidEmailForProcessing(subject));
        }

        #endregion

        #region Single Email Processing Tests

   [TestMethod]
     public void ProcessSingleEmail_ValidGrantNumber_CreatesRecord()
        {
      var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding";

var result = testProcessor.TestProcessSingleEmail(subject);

    Assert.IsNotNull(result);
      Assert.AreEqual("5R01CA123456", result.GrantNumber);
        }

      [TestMethod]
        public void ProcessSingleEmail_ValidApplId_CreatesRecord()
        {
       var testProcessor = new TestExchangeFixedProcessor();
            string subject = "applid=12345678, category=Correspondence";

            var result = testProcessor.TestProcessSingleEmail(subject);

       Assert.IsNotNull(result);
     Assert.AreEqual("12345678", result.ApplId);
        }

        [TestMethod]
        public void ProcessSingleEmail_InvalidSubject_ReturnsNull()
        {
            var testProcessor = new TestExchangeFixedProcessor();
    string subject = "RE: Please review";

     var result = testProcessor.TestProcessSingleEmail(subject);

  Assert.IsNull(result);
        }

      [TestMethod]
    public void ProcessSingleEmail_DefaultCategory_IsCorrespondence()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

            var result = testProcessor.TestProcessSingleEmail(subject);

    Assert.AreEqual("Correspondence", result.Category);
     }

        [TestMethod]
        public void ProcessSingleEmail_DefaultExtract_IsOne()
 {
 var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

       var result = testProcessor.TestProcessSingleEmail(subject);

     Assert.AreEqual("1", result.Extract);
        }

        [TestMethod]
        public void ProcessSingleEmail_IncrementsCounter()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            testProcessor.TestProcessSingleEmail("grantnumber=111, category=A");
         testProcessor.TestProcessSingleEmail("grantnumber=222, category=B");
            testProcessor.TestProcessSingleEmail("grantnumber=333, category=C");

     Assert.AreEqual(3, testProcessor.ProcessedCount);
        }

        [TestMethod]
        public void ProcessSingleEmail_CapturesBody()
        {
       var testProcessor = new TestExchangeFixedProcessor();
  string body = "This is the email body content.";

            var result = testProcessor.TestProcessSingleEmail("grantnumber=123", body);

            Assert.AreEqual(body, result.Body);
    }

        #endregion

      #region File Save Operations Tests

        [TestMethod]
        public void ProcessSingleEmail_Extract1_SavesEmailBody()
 {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

  Assert.IsTrue(testProcessor.FileSaveOperations.Exists(op => op.SaveType == "EmailBody"));
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract2WithAttachment_SavesAttachment()
    {
            var testProcessor = new TestExchangeFixedProcessor();
       string subject = "grantnumber=123, extract=2";

   testProcessor.TestProcessSingleEmail(subject, attachmentCount: 1);

            Assert.IsTrue(testProcessor.FileSaveOperations.Exists(op => op.SaveType == "Attachment"));
        }

    [TestMethod]
        public void ProcessSingleEmail_Extract2NoAttachment_NoAttachmentSave()
   {
 var testProcessor = new TestExchangeFixedProcessor();
 string subject = "grantnumber=123, extract=2";

  testProcessor.TestProcessSingleEmail(subject, attachmentCount: 0);

          Assert.IsFalse(testProcessor.FileSaveOperations.Exists(op => op.SaveType == "Attachment"));
      }

        [TestMethod]
     public void ProcessSingleEmail_Extract3WithAttachment_SavesBoth()
 {
            var testProcessor = new TestExchangeFixedProcessor();
   string subject = "grantnumber=123, extract=3";

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 1);

      Assert.IsTrue(testProcessor.FileSaveOperations.Exists(op => op.SaveType == "EmailBody"));
            Assert.IsTrue(testProcessor.FileSaveOperations.Exists(op => op.SaveType == "Attachment"));
        }

        #endregion

   #region Multiple Emails Processing Tests

        [TestMethod]
        public void ProcessSimulatedEmails_ProcessesAllValid()
        {
    var testProcessor = new TestExchangeFixedProcessor();
            testProcessor.AddSimulatedEmail("grantnumber=111, category=A");
            testProcessor.AddSimulatedEmail("grantnumber=222, category=B");
 testProcessor.AddSimulatedEmail("grantnumber=333, category=C");

        int result = testProcessor.TestProcessSimulatedEmails();

      Assert.AreEqual(3, result);
     Assert.AreEqual(3, testProcessor.EmailsProcessedThisSession.Count);
     }

        [TestMethod]
        public void ProcessSimulatedEmails_SkipsInvalid()
        {
       var testProcessor = new TestExchangeFixedProcessor();
            testProcessor.AddSimulatedEmail("grantnumber=111, category=A");
            testProcessor.AddSimulatedEmail("RE: Invalid email");
       testProcessor.AddSimulatedEmail("grantnumber=222, category=B");

            int result = testProcessor.TestProcessSimulatedEmails();

     Assert.AreEqual(2, result);
        }

        [TestMethod]
  public void ProcessSimulatedEmails_NoEmails_ReturnsZero()
        {
            var testProcessor = new TestExchangeFixedProcessor();

         int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(0, result);
        }

        #endregion

        #region Reset Tests

        [TestMethod]
     public void Reset_ClearsAllData()
        {
            var testProcessor = new TestExchangeFixedProcessor();
     testProcessor.TestProcessSingleEmail("grantnumber=111");
            testProcessor.TestProcessSingleEmail("grantnumber=222");

            testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.ProcessedCount);
Assert.AreEqual(0, testProcessor.EmailsProcessedThisSession.Count);
  Assert.AreEqual(0, testProcessor.FileSaveOperations.Count);
        }

        [TestMethod]
 public void Reset_ClearsSimulatedEmails()
        {
        var testProcessor = new TestExchangeFixedProcessor();
       testProcessor.AddSimulatedEmail("grantnumber=111");

      testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.SimulatedEmails.Count);
        }

        [TestMethod]
     public void Reset_ClearsErrorState()
        {
 var testProcessor = new TestExchangeFixedProcessor();
            testProcessor.TestProcessSingleEmail("grantnumber=111");

    testProcessor.Reset();

       Assert.IsFalse(testProcessor.ErrorOccurred);
            Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

#region Error Handling Tests

        [TestMethod]
        public void ProcessSingleEmail_NoErrorDuringNormalProcessing()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            testProcessor.TestProcessSingleEmail("grantnumber=123, category=Funding");

       Assert.IsFalse(testProcessor.ErrorOccurred);
      Assert.IsNull(testProcessor.LastErrorMessage);
    }

        #endregion

        #region Add Simulated Email Tests

        [TestMethod]
      public void AddSimulatedEmail_AddsToList()
{
            var testProcessor = new TestExchangeFixedProcessor();

            testProcessor.AddSimulatedEmail("grantnumber=123");

         Assert.AreEqual(1, testProcessor.SimulatedEmails.Count);
        }

        [TestMethod]
        public void AddSimulatedEmail_SetsProperties()
     {
            var testProcessor = new TestExchangeFixedProcessor();
      string subject = "grantnumber=123, category=Funding";
            string body = "Email body";
 string sender = "admin@nih.gov";
            int attachments = 2;

          testProcessor.AddSimulatedEmail(subject, body, sender, attachments);

            var email = testProcessor.SimulatedEmails[0];
       Assert.AreEqual(subject, email.Subject);
     Assert.AreEqual(body, email.Body);
          Assert.AreEqual(sender, email.SenderEmail);
  Assert.AreEqual(attachments, email.AttachmentCount);
        }

        #endregion
    }
}

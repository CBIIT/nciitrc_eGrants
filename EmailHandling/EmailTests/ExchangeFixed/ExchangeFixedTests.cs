using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.ExchangeFixed
{
    [TestClass]
    public class ExchangeFixedTests
    {
        #region Subject Parsing Tests - Basic Fields

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
        public void ParseSubjectLine_WithDocumentDate_ExtractsDocumentDate()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, documentdate=2024-01-15";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.AreEqual("2024-01-15", result.DocumentDate);
        }

        [TestMethod]
        public void ParseSubjectLine_WithDocumentId_ExtractsDocumentId()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, documentid=99999";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.AreEqual("99999", result.DocumentId);
        }

        [TestMethod]
        public void ParseSubjectLine_AllParams_ExtractsAll()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding, sub=NoA, extract=3, documentdate=2024-03-01, documentid=555";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.AreEqual("5R01CA123456", result.GrantNumber);
            Assert.AreEqual("Funding", result.Category);
            Assert.AreEqual("NoA", result.SubCategory);
            Assert.AreEqual("3", result.Extract);
            Assert.AreEqual("2024-03-01", result.DocumentDate);
            Assert.AreEqual("555", result.DocumentId);
        }

        [TestMethod]
        public void ParseSubjectLine_EmptySubject_ReturnsDefaultExtract()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            var result = testProcessor.ParseSubjectLinePublic("");

            Assert.IsNull(result.GrantNumber);
            Assert.IsNull(result.ApplId);
            Assert.AreEqual("1", result.Extract); // Default
        }

        [TestMethod]
        public void ParseSubjectLine_NoValidParams_ReturnsNullIdentifiers()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "RE: Please review this document";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.IsNull(result.GrantNumber);
            Assert.IsNull(result.ApplId);
        }

        [TestMethod]
        public void ParseSubjectLine_DefaultExtract_IsOne()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding";

            var result = testProcessor.ParseSubjectLinePublic(subject);

            Assert.AreEqual("1", result.Extract);
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

        [TestMethod]
        public void ExtractValue_MultipleEquals_ReturnsNull()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            string result = testProcessor.ExtractValuePublic("key=val=ue", "key");

            Assert.IsNull(result);
        }

        #endregion

        #region File Type Extraction Tests

        [TestMethod]
        public void GetFileType_PdfFile_ReturnsPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("pdf", testProcessor.GetFileTypePublic("document.pdf"));
        }

        [TestMethod]
        public void GetFileType_DocxFile_ReturnsDocx()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("docx", testProcessor.GetFileTypePublic("document.docx"));
        }

        [TestMethod]
        public void GetFileType_XlsxFile_ReturnsXlsx()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("xlsx", testProcessor.GetFileTypePublic("spreadsheet.xlsx"));
        }

        [TestMethod]
        public void GetFileType_NoExtension_ReturnsTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("txt", testProcessor.GetFileTypePublic("filename"));
        }

        [TestMethod]
        public void GetFileType_EmptyString_ReturnsTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("txt", testProcessor.GetFileTypePublic(""));
        }

        [TestMethod]
        public void GetFileType_MultipleDots_ReturnsLastExtension()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("pdf", testProcessor.GetFileTypePublic("file.name.pdf"));
        }

        [TestMethod]
        public void GetFileType_NullString_ReturnsTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("txt", testProcessor.GetFileTypePublic(null));
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
        public void RemoveSpecialChars_AmpersandReplacedWithAnd()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveSpecialCharsPublic("R&D");
            Assert.IsTrue(result.Contains("and"));
            Assert.IsFalse(result.Contains("&"));
        }

        [TestMethod]
        public void RemoveSpecialChars_AngleBracketsRemoved()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveSpecialCharsPublic("test<value>here");
            Assert.IsFalse(result.Contains("<"));
            Assert.IsFalse(result.Contains(">"));
        }

        [TestMethod]
        public void RemoveSpecialChars_AtSignRemoved()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveSpecialCharsPublic("user@domain");
            Assert.IsFalse(result.Contains("@"));
        }

        [TestMethod]
        public void RemoveSpecialChars_PercentRemoved()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveSpecialCharsPublic("100%complete");
            Assert.IsFalse(result.Contains("%"));
        }

        [TestMethod]
        public void RemoveSpecialChars_EmptyString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.RemoveSpecialCharsPublic(""));
        }

        [TestMethod]
        public void RemoveSpecialChars_NullString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.RemoveSpecialCharsPublic(null));
        }

        [TestMethod]
        public void RemoveSpecialChars_GrantNumber_CleanedCorrectly()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            // Simulates "5 R01 CA123456" format
            string result = testProcessor.RemoveSpecialCharsPublic("5:R01/CA 123456");
            Assert.AreEqual("5R01CA123456", result);
        }

        #endregion

        #region RemoveJunk Tests

        [TestMethod]
        public void RemoveJunk_ColonReplacedWithSpace()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveJunkPublic("file:name.pdf");
            Assert.IsTrue(result.Contains(" "));
            Assert.IsFalse(result.Contains(":"));
        }

        [TestMethod]
        public void RemoveJunk_AmpersandReplacedWithAnd()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveJunkPublic("R&D Report.pdf");
            Assert.IsTrue(result.Contains("and"));
        }

        [TestMethod]
        public void RemoveJunk_SemicolonReplacedWithSpace()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.RemoveJunkPublic("file;name.pdf");
            Assert.IsFalse(result.Contains(";"));
        }

        [TestMethod]
        public void RemoveJunk_EmptyString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.RemoveJunkPublic(""));
        }

        [TestMethod]
        public void RemoveJunk_NullString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.RemoveJunkPublic(null));
        }

        [TestMethod]
        public void RemoveJunk_NormalFilename_Unchanged()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("report.pdf", testProcessor.RemoveJunkPublic("report.pdf"));
        }

        #endregion

        #region IsQcRequired Tests

        [TestMethod]
        public void IsQcRequired_Pdf_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("pdf"));
        }

        [TestMethod]
        public void IsQcRequired_Txt_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("txt"));
        }

        [TestMethod]
        public void IsQcRequired_Doc_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("doc"));
        }

        [TestMethod]
        public void IsQcRequired_Docx_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("docx"));
        }

        [TestMethod]
        public void IsQcRequired_Xls_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("xls"));
        }

        [TestMethod]
        public void IsQcRequired_Xlsx_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("xlsx"));
        }

        [TestMethod]
        public void IsQcRequired_Ppt_ReturnsNo()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("no", testProcessor.IsQcRequiredPublic("ppt"));
        }

        [TestMethod]
        public void IsQcRequired_Zip_ReturnsYes()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("yes", testProcessor.IsQcRequiredPublic("zip"));
        }

        [TestMethod]
        public void IsQcRequired_Exe_ReturnsYes()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("yes", testProcessor.IsQcRequiredPublic("exe"));
        }

        [TestMethod]
        public void IsQcRequired_EmptyString_ReturnsYes()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("yes", testProcessor.IsQcRequiredPublic(""));
        }

        [TestMethod]
        public void IsQcRequired_NullString_ReturnsYes()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("yes", testProcessor.IsQcRequiredPublic(null));
        }

        [TestMethod]
        public void IsQcRequired_Jpg_ReturnsYes()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("yes", testProcessor.IsQcRequiredPublic("jpg"));
        }

        #endregion

        #region GetAliasFromExAddress Tests

        [TestMethod]
        public void GetAliasFromExAddress_StandardExAddress_ReturnsAlias()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.GetAliasFromExAddressPublic("/o=org/ou=unit/cn=recipients/cn=jsmith");
            Assert.AreEqual("jsmith", result);
        }

        [TestMethod]
        public void GetAliasFromExAddress_SingleEquals_ReturnsAfterEquals()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.GetAliasFromExAddressPublic("cn=jdoe");
            Assert.AreEqual("jdoe", result);
        }

        [TestMethod]
        public void GetAliasFromExAddress_NoEquals_ReturnsFullString()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string result = testProcessor.GetAliasFromExAddressPublic("jsmith");
            Assert.AreEqual("jsmith", result);
        }

        [TestMethod]
        public void GetAliasFromExAddress_EmptyString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.GetAliasFromExAddressPublic(""));
        }

        [TestMethod]
        public void GetAliasFromExAddress_NullString_ReturnsEmpty()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.AreEqual("", testProcessor.GetAliasFromExAddressPublic(null));
        }

        #endregion

        #region Email Validation Tests

        [TestMethod]
        public void IsValidEmailForProcessing_WithGrantNumber_ReturnsTrue()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.IsTrue(testProcessor.IsValidEmailForProcessing("grantnumber=5R01CA123456, category=Funding"));
        }

        [TestMethod]
        public void IsValidEmailForProcessing_WithApplId_ReturnsTrue()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.IsTrue(testProcessor.IsValidEmailForProcessing("applid=12345678, category=Correspondence"));
        }

        [TestMethod]
        public void IsValidEmailForProcessing_NoIdentifier_ReturnsFalse()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.IsFalse(testProcessor.IsValidEmailForProcessing("category=Funding, sub=NoA"));
        }

        [TestMethod]
        public void IsValidEmailForProcessing_RandomSubject_ReturnsFalse()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.IsFalse(testProcessor.IsValidEmailForProcessing("RE: Please review this document"));
        }

        [TestMethod]
        public void IsValidEmailForProcessing_EmptySubject_ReturnsFalse()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            Assert.IsFalse(testProcessor.IsValidEmailForProcessing(""));
        }

        #endregion

        #region NCIOGAPROGESS Sender Handling Tests

        [TestMethod]
        public void ProcessSingleEmail_NciOgaProSender_OverridesCategory()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding";

            var result = testProcessor.TestProcessSingleEmail(subject, senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            Assert.AreEqual("Notification", result.Category);
        }

        [TestMethod]
        public void ProcessSingleEmail_NciOgaProSender_OverridesSubCategory()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

            var result = testProcessor.TestProcessSingleEmail(subject, senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            Assert.AreEqual("Late Progress Report", result.SubCategory);
        }

        [TestMethod]
        public void ProcessSingleEmail_NciOgaProSender_SetsExtractTo1()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, extract=2";

            var result = testProcessor.TestProcessSingleEmail(subject, senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            Assert.AreEqual("1", result.Extract);
        }

        [TestMethod]
        public void ProcessSingleEmail_NciOgaProSender_RenamesSender()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

            var result = testProcessor.TestProcessSingleEmail(subject, senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            Assert.AreEqual("NCIOGAPROGESS", result.SenderEmail);
        }

        [TestMethod]
        public void ProcessSingleEmail_NciOgaProSender_SendsNotification()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

            testProcessor.TestProcessSingleEmail(subject, senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            Assert.AreEqual(1, testProcessor.NotificationEmails.Count);
            Assert.IsTrue(testProcessor.NotificationEmails[0].Contains("Late Progress Report"));
        }

        [TestMethod]
        public void ProcessSingleEmail_NormalSender_NoNotification()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456";

            testProcessor.TestProcessSingleEmail(subject, senderEmail: "user@nih.gov");

            Assert.AreEqual(0, testProcessor.NotificationEmails.Count);
        }

        #endregion

        #region Category-Specific PDF Generation Tests

        [TestMethod]
        public void ProcessSingleEmail_PublicAccess_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=PublicAccess, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_JitInfo_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=JIT Info, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_CtGov_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=CT.gov, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_CloseoutPastDue_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Closeout, sub=past due documents reminder, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_CloseoutFRppr_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Closeout, sub=f-rppr acceptance past due reminder, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_EraNotificationJitSubmitted_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=eRA Notification, sub=JIT Submitted, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_CorrespondenceRppr_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Correspondence, sub=rppr unobligated balance, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_FundingDciInth_GeneratesPdf()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding, sub=dci-inth notice, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_StandardCorrespondence_GeneratesTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Correspondence, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBody"));
            Assert.IsFalse(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
        }

        [TestMethod]
        public void ProcessSingleEmail_StandardFunding_GeneratesTxt()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=5R01CA123456, category=Funding, sub=NoA, extract=1";

            testProcessor.TestProcessSingleEmail(subject);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBody"));
        }

        #endregion

        #region Attachment Processing Tests - ATT Prefix Skipping

        [TestMethod]
        public void ProcessSingleEmail_Extract2_SkipsAttPrefixedFiles()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=2";
            var attachments = new List<string> { "ATT00001.txt", "report.pdf", "ATT00002.gif" };

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 3, attachmentFileNames: attachments);

            // Only report.pdf should be saved (2 ATT files skipped)
            Assert.AreEqual(1, testProcessor.FileSaveOperations.Count(op => op.SaveType == "Attachment"));
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract2_AllAttPrefixed_NoSaves()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=2";
            var attachments = new List<string> { "ATT00001.txt", "ATT00002.gif" };

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 2, attachmentFileNames: attachments);

            Assert.AreEqual(0, testProcessor.FileSaveOperations.Count(op => op.SaveType == "Attachment"));
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract2_MultipleValidAttachments_SavesAll()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=2";
            var attachments = new List<string> { "report.pdf", "data.xlsx", "notes.doc" };

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 3, attachmentFileNames: attachments);

            Assert.AreEqual(3, testProcessor.FileSaveOperations.Count(op => op.SaveType == "Attachment"));
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract2_AttachmentQcFlagging()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=2";
            var attachments = new List<string> { "report.pdf", "image.jpg" };

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 2, attachmentFileNames: attachments);

            var pdfOp = testProcessor.FileSaveOperations.First(op => op.FileType == "pdf");
            var jpgOp = testProcessor.FileSaveOperations.First(op => op.FileType == "jpg");
            Assert.AreEqual("no", pdfOp.QcRequired);
            Assert.AreEqual("yes", jpgOp.QcRequired);
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract3_SavesBodyAndAttachments()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, category=Correspondence, extract=3";
            var attachments = new List<string> { "report.pdf", "ATT00001.gif" };

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 2, attachmentFileNames: attachments);

            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBody"));
            Assert.AreEqual(1, testProcessor.FileSaveOperations.Count(op => op.SaveType == "Attachment"));
        }

        [TestMethod]
        public void ProcessSingleEmail_Extract2NoAttachment_NoAttachmentSave()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string subject = "grantnumber=123, extract=2";

            testProcessor.TestProcessSingleEmail(subject, attachmentCount: 0);

            Assert.IsFalse(testProcessor.FileSaveOperations.Any(op => op.SaveType == "Attachment"));
        }

        #endregion

        #region 30-Item Limit Tests

        [TestMethod]
        public void ProcessSimulatedEmails_StopsAt30Items()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            for (int i = 0; i < 35; i++)
                testProcessor.AddSimulatedEmail($"grantnumber={i + 1000}, category=Correspondence");

            int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(30, result);
            Assert.IsTrue(testProcessor.ItemLimitReached);
        }

        [TestMethod]
        public void ProcessSimulatedEmails_30Items_SendsAdminWarning()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            for (int i = 0; i < 31; i++)
                testProcessor.AddSimulatedEmail($"grantnumber={i + 1000}, category=Correspondence");

            testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(1, testProcessor.AdminNotifications.Count);
            Assert.IsTrue(testProcessor.AdminNotifications[0].Contains("30 items"));
        }

        [TestMethod]
        public void ProcessSimulatedEmails_Under30Items_NoWarning()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            for (int i = 0; i < 10; i++)
                testProcessor.AddSimulatedEmail($"grantnumber={i + 1000}, category=Correspondence");

            testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(0, testProcessor.AdminNotifications.Count);
            Assert.IsFalse(testProcessor.ItemLimitReached);
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

        [TestMethod]
        public void ProcessSimulatedEmails_MixedCategoriesAndExtracts()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            testProcessor.AddSimulatedEmail("grantnumber=111, category=Correspondence, extract=1");
            testProcessor.AddSimulatedEmail("grantnumber=222, category=PublicAccess, extract=1");
            testProcessor.AddSimulatedEmail("grantnumber=333, category=Funding, extract=2", attachmentCount: 2,
                attachmentFileNames: new List<string> { "doc1.pdf", "doc2.xlsx" });

            int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(3, result);
            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBody"));
            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "EmailBodyPDF"));
            Assert.IsTrue(testProcessor.FileSaveOperations.Any(op => op.SaveType == "Attachment"));
        }

        #endregion

        #region Default Category and Extract Tests

        [TestMethod]
        public void ProcessSingleEmail_DefaultCategory_IsCorrespondence()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            var result = testProcessor.TestProcessSingleEmail("grantnumber=5R01CA123456");
            Assert.AreEqual("Correspondence", result.Category);
        }

        [TestMethod]
        public void ProcessSingleEmail_DefaultExtract_IsOne()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            var result = testProcessor.TestProcessSingleEmail("grantnumber=5R01CA123456");
            Assert.AreEqual("1", result.Extract);
        }

        [TestMethod]
        public void ProcessSingleEmail_CapturesBody()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            string body = "This is the email body content.";

            var result = testProcessor.TestProcessSingleEmail("grantnumber=123", body);

            Assert.AreEqual(body, result.Body);
        }

        [TestMethod]
        public void ProcessSingleEmail_CapturesDocumentDate()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            var result = testProcessor.TestProcessSingleEmail("grantnumber=123, documentdate=2024-06-15");
            Assert.AreEqual("2024-06-15", result.DocumentDate);
        }

        [TestMethod]
        public void ProcessSingleEmail_CapturesDocumentId()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            var result = testProcessor.TestProcessSingleEmail("grantnumber=123, documentid=77777");
            Assert.AreEqual("77777", result.DocumentId);
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

        [TestMethod]
        public void Reset_ClearsNotifications()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            testProcessor.TestProcessSingleEmail("grantnumber=111", senderEmail: "FD6862D09E7043D49596358F980D064F-NCI OGA PRO");

            testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.NotificationEmails.Count);
            Assert.AreEqual(0, testProcessor.AdminNotifications.Count);
        }

        [TestMethod]
        public void Reset_ClearsItemLimitFlag()
        {
            var testProcessor = new TestExchangeFixedProcessor();
            for (int i = 0; i < 31; i++)
                testProcessor.AddSimulatedEmail($"grantnumber={i}");
            testProcessor.TestProcessSimulatedEmails();

            testProcessor.Reset();

            Assert.IsFalse(testProcessor.ItemLimitReached);
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

        [TestMethod]
        public void ProcessSingleEmail_InvalidSubject_ReturnsNull()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            var result = testProcessor.TestProcessSingleEmail("RE: Please review");

            Assert.IsNull(result);
        }

        #endregion

        #region Counter Tests

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
        public void ProcessSingleEmail_InvalidDoesNotIncrement()
        {
            var testProcessor = new TestExchangeFixedProcessor();

            testProcessor.TestProcessSingleEmail("grantnumber=111");
            testProcessor.TestProcessSingleEmail("RE: Invalid");
            testProcessor.TestProcessSingleEmail("grantnumber=222");

            Assert.AreEqual(2, testProcessor.ProcessedCount);
        }

        #endregion
    }
}

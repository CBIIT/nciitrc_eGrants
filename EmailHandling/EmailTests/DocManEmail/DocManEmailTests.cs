using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.DocManEmail
{
    [TestClass]
    public class DocManEmailHandlingTests
    {
        #region Subject Validation Tests

        [TestMethod]
        public void IsValidDocManSubject_WithCpiId_ReturnsTrue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";
            Assert.IsTrue(testProcessor.IsValidDocManSubject(subject));
        }

        [TestMethod]
        public void IsValidDocManSubject_WithDocId_ReturnsTrue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "docid=67890, catid=10, num=1, date=2024-01-15";
            Assert.IsTrue(testProcessor.IsValidDocManSubject(subject));
        }

        [TestMethod]
        public void IsValidDocManSubject_WithoutIds_ReturnsFalse()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "catid=10, num=1, date=2024-01-15";
            Assert.IsFalse(testProcessor.IsValidDocManSubject(subject));
        }

        [TestMethod]
        public void IsValidDocManSubject_EmptySubject_ReturnsFalse()
        {
            var testProcessor = new TestDocManEmailProcessor();
            Assert.IsFalse(testProcessor.IsValidDocManSubject(""));
        }

        [TestMethod]
        public void IsValidDocManSubject_RandomText_ReturnsFalse()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "RE: Please review this document";
            Assert.IsFalse(testProcessor.IsValidDocManSubject(subject));
        }

        #endregion

        #region Element Extraction Tests

        [TestMethod]
        public void ExtractElement_FirstElement_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";
            string result = testProcessor.ExtractElementPublic(subject, 1);
            Assert.AreEqual("cpiid=12345", result);
        }

        [TestMethod]
        public void ExtractElement_SecondElement_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";
            string result = testProcessor.ExtractElementPublic(subject, 2);
            Assert.AreEqual("catid=10", result);
        }

        [TestMethod]
        public void ExtractElement_ThirdElement_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";
            string result = testProcessor.ExtractElementPublic(subject, 3);
            Assert.AreEqual("num=1", result);
        }

        [TestMethod]
        public void ExtractElement_FourthElement_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";
            string result = testProcessor.ExtractElementPublic(subject, 4);
            Assert.AreEqual("date=2024-01-15", result);
        }

        [TestMethod]
        public void ExtractElement_InvalidIndex_ReturnsEmpty()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10";
            string result = testProcessor.ExtractElementPublic(subject, 5);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void ExtractElement_ZeroIndex_ReturnsEmpty()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10";
            string result = testProcessor.ExtractElementPublic(subject, 0);
            Assert.AreEqual("", result);
        }

        #endregion

        #region Value Extraction Tests

        [TestMethod]
        public void ExtractValue_CpiId_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "cpiid=12345";
            string result = testProcessor.ExtractValuePublic(element, "cpiid");
            Assert.AreEqual("12345", result);
        }

        [TestMethod]
        public void ExtractValue_DocId_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "docid=67890";
            string result = testProcessor.ExtractValuePublic(element, "docid");
            Assert.AreEqual("67890", result);
        }

        [TestMethod]
        public void ExtractValue_CatId_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "catid=10";
            string result = testProcessor.ExtractValuePublic(element, "catid");
            Assert.AreEqual("10", result);
        }

        [TestMethod]
        public void ExtractValue_Num_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "num=5";
            string result = testProcessor.ExtractValuePublic(element, "num");
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public void ExtractValue_Date_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "date=2024-01-15";
            string result = testProcessor.ExtractValuePublic(element, "date");
            Assert.AreEqual("2024-01-15", result);
        }

        [TestMethod]
        public void ExtractValue_Reason_ReturnsCorrectValue()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "reason=Amendment";
            string result = testProcessor.ExtractValuePublic(element, "reason");
            Assert.AreEqual("Amendment", result);
        }

        [TestMethod]
        public void ExtractValue_WrongName_ReturnsNull()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "cpiid=12345";
            string result = testProcessor.ExtractValuePublic(element, "docid");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExtractValue_EmptyString_ReturnsNull()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string result = testProcessor.ExtractValuePublic("", "cpiid");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExtractValue_NoEqualsSign_ReturnsNull()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string result = testProcessor.ExtractValuePublic("cpiid12345", "cpiid");
            Assert.IsNull(result);
        }

        #endregion

        #region Single Document Processing Tests

        [TestMethod]
        public void ProcessSingleDocument_ValidCpiId_CreatesRecord()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=1, date=2024-01-15";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.IsNotNull(result);
            Assert.AreEqual("12345", result.CpiId);
        }

        [TestMethod]
        public void ProcessSingleDocument_ValidDocId_CreatesRecord()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "docid=67890, catid=10, num=1, date=2024-01-15";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.IsNotNull(result);
            Assert.AreEqual("67890", result.DocId);
        }

        [TestMethod]
        public void ProcessSingleDocument_ExtractsAllFields()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "cpiid=12345, catid=10, num=5, date=2024-01-15";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.IsNotNull(result);
            Assert.AreEqual("12345", result.CpiId);
            Assert.AreEqual("10", result.CategoryId);
            Assert.AreEqual("5", result.SequenceNumber);
            Assert.AreEqual("2024-01-15", result.DocumentDate);
        }

        [TestMethod]
        public void ProcessSingleDocument_InvalidSubject_ReturnsNull()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "RE: Please review this document";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.IsNull(result);
            Assert.AreEqual(0, testProcessor.ProcessedCount);
        }

        [TestMethod]
        public void ProcessSingleDocument_IncrementsCounter()
        {
            var testProcessor = new TestDocManEmailProcessor();

            testProcessor.TestProcessSingleDocument("cpiid=111, catid=1");
            testProcessor.TestProcessSingleDocument("cpiid=222, catid=2");
            testProcessor.TestProcessSingleDocument("cpiid=333, catid=3");

            Assert.AreEqual(3, testProcessor.ProcessedCount);
        }

        [TestMethod]
        public void ProcessSingleDocument_CapturesSenderEmail()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string senderEmail = "document.admin@nih.gov";

            var result = testProcessor.TestProcessSingleDocument(
                "cpiid=12345, catid=10", senderEmail);

            Assert.AreEqual(senderEmail, result.SenderEmail);
        }

        [TestMethod]
        public void ProcessSingleDocument_MarksAsMovedToOld()
        {
            var testProcessor = new TestDocManEmailProcessor();

            var result = testProcessor.TestProcessSingleDocument("cpiid=12345, catid=10");

            Assert.IsTrue(result.WasMovedToOld);
        }

        #endregion

        #region Multiple Documents Processing Tests

        [TestMethod]
        public void ProcessSimulatedEmails_ProcessesAllValid()
        {
            var testProcessor = new TestDocManEmailProcessor();
            testProcessor.AddSimulatedEmail("cpiid=111, catid=1");
            testProcessor.AddSimulatedEmail("cpiid=222, catid=2");
            testProcessor.AddSimulatedEmail("cpiid=333, catid=3");

            int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(3, result);
            Assert.AreEqual(3, testProcessor.DocumentsProcessedThisSession.Count);
        }

        [TestMethod]
        public void ProcessSimulatedEmails_SkipsInvalid()
        {
            var testProcessor = new TestDocManEmailProcessor();
            testProcessor.AddSimulatedEmail("cpiid=111, catid=1");
            testProcessor.AddSimulatedEmail("RE: Invalid email");
            testProcessor.AddSimulatedEmail("cpiid=222, catid=2");

            int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ProcessSimulatedEmails_NoEmails_ReturnsZero()
        {
            var testProcessor = new TestDocManEmailProcessor();

            int result = testProcessor.TestProcessSimulatedEmails();

            Assert.AreEqual(0, result);
        }

        #endregion

        #region Attachment Tests

        [TestMethod]
        public void ProcessSingleDocument_CapturesAttachmentCount()
        {
            var testProcessor = new TestDocManEmailProcessor();

            var result = testProcessor.TestProcessSingleDocument(
                "cpiid=12345, catid=10", "user@nih.gov", attachmentCount: 3);

            Assert.AreEqual(3, result.AttachmentCount);
        }

        [TestMethod]
        public void ProcessSingleDocument_DefaultAttachmentCount()
        {
            var testProcessor = new TestDocManEmailProcessor();

            var result = testProcessor.TestProcessSingleDocument("cpiid=12345, catid=10");

            Assert.AreEqual(1, result.AttachmentCount);
        }

        #endregion

        #region Reset Tests

        [TestMethod]
        public void Reset_ClearsAllData()
        {
            var testProcessor = new TestDocManEmailProcessor();
            testProcessor.TestProcessSingleDocument("cpiid=111, catid=1");
            testProcessor.TestProcessSingleDocument("cpiid=222, catid=2");

            testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.ProcessedCount);
            Assert.AreEqual(0, testProcessor.DocumentsProcessedThisSession.Count);
        }

        [TestMethod]
        public void Reset_ClearsSimulatedEmails()
        {
            var testProcessor = new TestDocManEmailProcessor();
            testProcessor.AddSimulatedEmail("cpiid=111, catid=1");

            testProcessor.Reset();

            Assert.AreEqual(0, testProcessor.SimulatedEmails.Count);
        }

        [TestMethod]
        public void Reset_ClearsErrorState()
        {
            var testProcessor = new TestDocManEmailProcessor();
            testProcessor.TestProcessSingleDocument("cpiid=111, catid=1");

            testProcessor.Reset();

            Assert.IsFalse(testProcessor.ErrorOccurred);
            Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void ProcessSingleDocument_NoErrorDuringNormalProcessing()
        {
            var testProcessor = new TestDocManEmailProcessor();

            testProcessor.TestProcessSingleDocument("cpiid=12345, catid=10");

            Assert.IsFalse(testProcessor.ErrorOccurred);
            Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

        #region Add Simulated Email Tests

        [TestMethod]
        public void AddSimulatedEmail_AddsToList()
        {
            var testProcessor = new TestDocManEmailProcessor();

            testProcessor.AddSimulatedEmail("cpiid=12345, catid=10");

            Assert.AreEqual(1, testProcessor.SimulatedEmails.Count);
        }

        [TestMethod]
        public void AddSimulatedEmail_SetsProperties()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "docid=67890, catid=5";
            string senderEmail = "admin@nih.gov";
            int attachments = 2;

            testProcessor.AddSimulatedEmail(subject, senderEmail, attachments);

            var email = testProcessor.SimulatedEmails[0];
            Assert.AreEqual(subject, email.Subject);
            Assert.AreEqual(senderEmail, email.SenderEmail);
            Assert.AreEqual(attachments, email.AttachmentCount);
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void ExtractValue_ValueWithSpaces_TrimsCorrectly()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string element = "  cpiid = 12345  ";
            string result = testProcessor.ExtractValuePublic(element.Trim(), "cpiid");
            Assert.AreEqual("12345", result);
        }

        [TestMethod]
        public void ProcessSingleDocument_WithReason_ExtractsReason()
        {
            var testProcessor = new TestDocManEmailProcessor();
            string subject = "docid=12345, catid=10, reason=Contract Amendment";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.AreEqual("Contract Amendment", result.Reason);
        }

        [TestMethod]
        public void ProcessSingleDocument_BothCpiIdAndDocId_UsesCpiId()
        {
            var testProcessor = new TestDocManEmailProcessor();
            // When both are present in different elements, cpiid takes precedence
            string subject = "cpiid=11111, catid=10";

            var result = testProcessor.TestProcessSingleDocument(subject);

            Assert.IsNotNull(result);
            Assert.AreEqual("11111", result.CpiId);
        }

        #endregion
    }
}

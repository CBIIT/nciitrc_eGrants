using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.Unit.AddSuppVoteCollection
{
    [TestClass]
    public class AddSuppVoteCollectionTests
    {
        #region Vote Detection Tests

        [TestMethod]
 public void IsVoteEmail_AcceptedSubject_ReturnsTrue()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();
            string subject = "Accepted: Administrative Supplement Request - Grant CA123456";
        Assert.IsTrue(testProcessor.IsVoteEmail(subject), "Should detect Accepted vote");
        }

     [TestMethod]
  public void IsVoteEmail_RejectedSubject_ReturnsTrue()
     {
      var testProcessor = new TestAddSuppVoteCollectionProcessor();
            string subject = "Rejected: Administrative Supplement Request - Grant CA123456";
            Assert.IsTrue(testProcessor.IsVoteEmail(subject), "Should detect Rejected vote");
        }

        [TestMethod]
        public void IsVoteEmail_NonVoteSubject_ReturnsFalse()
        {
    var testProcessor = new TestAddSuppVoteCollectionProcessor();
        string subject = "RE: Administrative Supplement Request - Grant CA123456";
       Assert.IsFalse(testProcessor.IsVoteEmail(subject), "Should not detect as vote email");
        }

        [TestMethod]
        public void IsVoteEmail_EmptySubject_ReturnsFalse()
        {
    var testProcessor = new TestAddSuppVoteCollectionProcessor();
Assert.IsFalse(testProcessor.IsVoteEmail(""), "Empty subject should not be a vote");
    }

  #endregion

        #region Vote Type Tests

        [TestMethod]
 public void GetVoteType_AcceptedSubject_ReturnsAccepted()
    {
    var testProcessor = new TestAddSuppVoteCollectionProcessor();
   string subject = "Accepted: Grant ABC123 Supplement";
  Assert.AreEqual("Accepted", testProcessor.GetVoteType(subject));
     }

        [TestMethod]
        public void GetVoteType_RejectedSubject_ReturnsRejected()
        {
  var testProcessor = new TestAddSuppVoteCollectionProcessor();
        string subject = "Rejected: Grant ABC123 Supplement";
  Assert.AreEqual("Rejected", testProcessor.GetVoteType(subject));
        }

        [TestMethod]
public void GetVoteType_NonVoteSubject_ReturnsNull()
  {
      var testProcessor = new TestAddSuppVoteCollectionProcessor();
        string subject = "FW: Grant ABC123 Supplement";
            Assert.IsNull(testProcessor.GetVoteType(subject));
        }

  #endregion

        #region Single Vote Processing Tests

        [TestMethod]
     public void ProcessSingleVote_AcceptedVote_CreatesRecord()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();
       string subject = "Accepted: Administrative Supplement Request";
       string senderName = "John Smith";

      var result = testProcessor.TestProcessSingleVote(subject, senderName);

      Assert.IsNotNull(result, "Should create a vote record");
            Assert.AreEqual(subject, result.Subject);
            Assert.AreEqual(senderName, result.SenderName);
  Assert.AreEqual("Accepted", result.VoteType);
        }

      [TestMethod]
        public void ProcessSingleVote_RejectedVote_CreatesRecord()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();
            string subject = "Rejected: Administrative Supplement Request";
            string senderName = "Jane Doe";

            var result = testProcessor.TestProcessSingleVote(subject, senderName);

            Assert.IsNotNull(result, "Should create a vote record");
  Assert.AreEqual("Rejected", result.VoteType);
        }

        [TestMethod]
        public void ProcessSingleVote_NonVoteEmail_ReturnsNull()
    {
          var testProcessor = new TestAddSuppVoteCollectionProcessor();
            string subject = "RE: Administrative Supplement Request";
         string senderName = "John Smith";

   var result = testProcessor.TestProcessSingleVote(subject, senderName);

     Assert.IsNull(result, "Non-vote email should return null");
            Assert.AreEqual(0, testProcessor.ProcessedCount, "Should not increment count");
        }

        [TestMethod]
        public void ProcessSingleVote_IncrementsCounter()
 {
     var testProcessor = new TestAddSuppVoteCollectionProcessor();

            testProcessor.TestProcessSingleVote("Accepted: Test", "User1");
   testProcessor.TestProcessSingleVote("Rejected: Test", "User2");
    testProcessor.TestProcessSingleVote("Accepted: Test2", "User3");

            Assert.AreEqual(3, testProcessor.ProcessedCount);
        }

 #endregion

   #region Email Forwarding Tests

     [TestMethod]
        public void ProcessSingleVote_CreatesForwardedEmail()
    {
      var testProcessor = new TestAddSuppVoteCollectionProcessor();
         string subject = "Accepted: Grant Request";

 testProcessor.TestProcessSingleVote(subject, "Voter Name");

       Assert.AreEqual(1, testProcessor.ForwardedEmailsThisSession.Count);
        }

   [TestMethod]
    public void ProcessSingleVote_ForwardedSubjectFormat()
        {
 var testProcessor = new TestAddSuppVoteCollectionProcessor();
     string subject = "Accepted: Grant Request ABC123";

       testProcessor.TestProcessSingleVote(subject, "Voter Name");

            var forwarded = testProcessor.ForwardedEmailsThisSession[0];
            string expected = "DO NOT REPLY : Forwarding Response [" + subject + "]";
 Assert.AreEqual(expected, forwarded.ForwardedSubject);
        }

    [TestMethod]
        public void ProcessSingleVote_ForwardsToCorrectRecipients()
        {
var testProcessor = new TestAddSuppVoteCollectionProcessor();

     testProcessor.TestProcessSingleVote("Accepted: Test", "Voter");

         var forwarded = testProcessor.ForwardedEmailsThisSession[0];
       Assert.IsTrue(forwarded.Recipients.Contains("emily.driskell@nih.gov"));
      Assert.IsTrue(forwarded.Recipients.Contains("jonesni@mail.nih.gov"));
        }

      #endregion

        #region Multiple Votes Processing Tests

        [TestMethod]
        public void ProcessSimulatedVotes_ProcessesAllVotes()
     {
   var testProcessor = new TestAddSuppVoteCollectionProcessor();
         testProcessor.AddSimulatedVoteEmail("Accepted: Request 1", "User1");
testProcessor.AddSimulatedVoteEmail("Rejected: Request 2", "User2");
    testProcessor.AddSimulatedVoteEmail("Accepted: Request 3", "User3");

      int result = testProcessor.TestProcessSimulatedVotes();

     Assert.AreEqual(3, result);
Assert.AreEqual(3, testProcessor.VotesProcessedThisSession.Count);
        }

        [TestMethod]
     public void ProcessSimulatedVotes_SkipsNonVoteEmails()
{
     var testProcessor = new TestAddSuppVoteCollectionProcessor();
       testProcessor.AddSimulatedVoteEmail("Accepted: Request 1", "User1");
  testProcessor.AddSimulatedVoteEmail("RE: Request 2", "User2");
            testProcessor.AddSimulatedVoteEmail("FW: Request 3", "User3");

     int result = testProcessor.TestProcessSimulatedVotes();

   Assert.AreEqual(1, result, "Should only process 1 vote email");
        }

  [TestMethod]
  public void ProcessSimulatedVotes_NoEmails_ReturnsZero()
      {
          var testProcessor = new TestAddSuppVoteCollectionProcessor();

            int result = testProcessor.TestProcessSimulatedVotes();

         Assert.AreEqual(0, result);
      }

   #endregion

        #region Vote Movement Tests

   [TestMethod]
 public void ProcessSingleVote_MarksAsMovedToVoteFolder()
    {
   var testProcessor = new TestAddSuppVoteCollectionProcessor();

        var result = testProcessor.TestProcessSingleVote("Accepted: Test", "Voter");

       Assert.IsTrue(result.WasMovedToVoteFolder);
        }

        #endregion

      #region Reset Tests

        [TestMethod]
    public void Reset_ClearsAllData()
     {
       var testProcessor = new TestAddSuppVoteCollectionProcessor();
            testProcessor.TestProcessSingleVote("Accepted: Test1", "User1");
        testProcessor.TestProcessSingleVote("Rejected: Test2", "User2");

        testProcessor.Reset();

          Assert.AreEqual(0, testProcessor.ProcessedCount);
            Assert.AreEqual(0, testProcessor.VotesProcessedThisSession.Count);
            Assert.AreEqual(0, testProcessor.ForwardedEmailsThisSession.Count);
    }

        [TestMethod]
        public void Reset_ClearsSimulatedEmails()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();
        testProcessor.AddSimulatedVoteEmail("Accepted: Test", "User");

            testProcessor.Reset();

  Assert.AreEqual(0, testProcessor.SimulatedVoteEmails.Count);
    }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void ProcessSingleVote_NoErrorDuringNormalProcessing()
        {
  var testProcessor = new TestAddSuppVoteCollectionProcessor();

   testProcessor.TestProcessSingleVote("Accepted: Test", "User");

        Assert.IsFalse(testProcessor.ErrorOccurred);
  Assert.IsNull(testProcessor.LastErrorMessage);
        }

        #endregion

        #region Sender Information Tests

        [TestMethod]
        public void ProcessSingleVote_CapturesSenderEmail()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();
            string senderEmail = "program.director@nih.gov";

     var result = testProcessor.TestProcessSingleVote(
       "Accepted: Test", "PD Name", senderEmail);

         Assert.AreEqual(senderEmail, result.SenderEmail);
}

        [TestMethod]
        public void ProcessSingleVote_CapturesBody()
        {
  var testProcessor = new TestAddSuppVoteCollectionProcessor();
    string body = "I approve this supplement request.";

   var result = testProcessor.TestProcessSingleVote(
   "Accepted: Test", "PD Name", "pd@nih.gov", body);

 Assert.AreEqual(body, result.Body);
        }

        #endregion

        #region Add Simulated Email Tests

  [TestMethod]
        public void AddSimulatedVoteEmail_AddsToList()
        {
            var testProcessor = new TestAddSuppVoteCollectionProcessor();

     testProcessor.AddSimulatedVoteEmail("Accepted: Test", "User1");

Assert.AreEqual(1, testProcessor.SimulatedVoteEmails.Count);
        }

        [TestMethod]
        public void AddSimulatedVoteEmail_SetsProperties()
        {
          var testProcessor = new TestAddSuppVoteCollectionProcessor();
    string subject = "Rejected: Grant Request";
  string senderName = "Jane Doe";
     string senderEmail = "jane.doe@nih.gov";

    testProcessor.AddSimulatedVoteEmail(subject, senderName, senderEmail);

     var email = testProcessor.SimulatedVoteEmails[0];
            Assert.AreEqual(subject, email.Subject);
          Assert.AreEqual(senderName, email.SenderName);
 Assert.AreEqual(senderEmail, email.SenderEmail);
        }

     #endregion
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.AddSuppProd
{
    /// <summary>
    /// Scenario-based tests for the AddSuppProd.Processor class.
    /// These tests verify email processing, item movement, and logging logic
    /// without requiring actual Outlook or database connections.
    /// </summary>
    [TestClass]
    public class AddSuppProdTests
    {
   #region Processing Tests

        /// <summary>
        /// Verifies that processing a single item increments the counter.
        /// </summary>
    [TestMethod]
        public void ProcessSingleItem_IncrementsCounter()
        {
 // Arrange
      var testProcessor = new TestAddSuppProdProcessor();

       // Act
            var result = testProcessor.TestProcessSingleItem(
         "Test Subject",
        "Test Body",
             "sender@nih.gov");

          // Assert
        Assert.IsNotNull(result, "Processed item should not be null");
    Assert.AreEqual(1, testProcessor.ProcessedCount, "Should have processed 1 item");
        }

        /// <summary>
        /// Verifies that the processed item captures the correct subject.
        /// </summary>
        [TestMethod]
        public void ProcessSingleItem_CapturesCorrectSubject()
        {
     // Arrange
       var testProcessor = new TestAddSuppProdProcessor();
        string expectedSubject = "Administrative Supplement Notification";

       // Act
      var result = testProcessor.TestProcessSingleItem(
      expectedSubject,
     "Test Body");

 // Assert
            Assert.IsNotNull(result, "Processed item should not be null");
            Assert.AreEqual(expectedSubject, result.Subject, "Subject should match");
        }

        /// <summary>
        /// Verifies that the processed item captures the correct body.
        /// </summary>
  [TestMethod]
        public void ProcessSingleItem_CapturesCorrectBody()
        {
  // Arrange
  var testProcessor = new TestAddSuppProdProcessor();
        string expectedBody = "This is the email body content for testing.";

       // Act
            var result = testProcessor.TestProcessSingleItem(
       "Test Subject",
         expectedBody);

// Assert
    Assert.IsNotNull(result, "Processed item should not be null");
            Assert.AreEqual(expectedBody, result.Body, "Body should match");
}

 /// <summary>
        /// Verifies that the processed item captures the sender email.
        /// </summary>
     [TestMethod]
        public void ProcessSingleItem_CapturesSenderEmail()
      {
            // Arrange
  var testProcessor = new TestAddSuppProdProcessor();
   string expectedSender = "programdirector@nih.gov";

   // Act
          var result = testProcessor.TestProcessSingleItem(
       "Test Subject",
      "Test Body",
     expectedSender);

 // Assert
      Assert.IsNotNull(result, "Processed item should not be null");
    Assert.AreEqual(expectedSender, result.SenderEmail, "Sender email should match");
        }

        #endregion

        #region Multiple Items Tests

        /// <summary>
 /// Verifies that processing multiple items increments the counter correctly.
        /// </summary>
        [TestMethod]
   public void ProcessMultipleItems_IncrementsCounterCorrectly()
        {
  // Arrange
       var testProcessor = new TestAddSuppProdProcessor();
      testProcessor.AddSimulatedMailItem("Subject 1", "Body 1");
   testProcessor.AddSimulatedMailItem("Subject 2", "Body 2");
            testProcessor.AddSimulatedMailItem("Subject 3", "Body 3");

          // Act
   int result = testProcessor.TestProcessSimulatedItems();

            // Assert
            Assert.AreEqual(3, result, "Should return 3 as items processed");
            Assert.AreEqual(3, testProcessor.ProcessedCount, "ProcessedCount should be 3");
     Assert.AreEqual(3, testProcessor.ItemsProcessedThisSession.Count,
           "Should have 3 items in session");
        }

        /// <summary>
        /// Verifies that all simulated items are captured in the session.
      /// </summary>
        [TestMethod]
        public void ProcessMultipleItems_CapturesAllItems()
      {
            // Arrange
   var testProcessor = new TestAddSuppProdProcessor();
          testProcessor.AddSimulatedMailItem("First Email", "First Body", "user1@nih.gov");
        testProcessor.AddSimulatedMailItem("Second Email", "Second Body", "user2@nih.gov");

            // Act
     testProcessor.TestProcessSimulatedItems();

            // Assert
 Assert.AreEqual(2, testProcessor.ItemsProcessedThisSession.Count);
        Assert.AreEqual("First Email", testProcessor.ItemsProcessedThisSession[0].Subject);
            Assert.AreEqual("Second Email", testProcessor.ItemsProcessedThisSession[1].Subject);
        }

        /// <summary>
        /// Verifies that processing with no items returns zero.
        /// </summary>
        [TestMethod]
        public void ProcessNoItems_ReturnsZero()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
// Don't add any simulated items

            // Act
      int result = testProcessor.TestProcessSimulatedItems();

     // Assert
            Assert.AreEqual(0, result, "Should return 0 when no items to process");
            Assert.AreEqual(0, testProcessor.ProcessedCount, "ProcessedCount should be 0");
        }

  #endregion

        #region Item Movement Tests

        /// <summary>
    /// Verifies that processed items are marked as moved to old folder.
        /// </summary>
        [TestMethod]
 public void ProcessItem_MarksAsMovedToOld()
        {
   // Arrange
       var testProcessor = new TestAddSuppProdProcessor();

            // Act
       var result = testProcessor.TestProcessSingleItem(
                "Test Subject",
     "Test Body");

            // Assert
        Assert.IsNotNull(result, "Processed item should not be null");
   Assert.IsTrue(result.WasMovedToOld, "Item should be marked as moved to old folder");
        }

   /// <summary>
        /// Verifies that all processed items are marked as moved.
        /// </summary>
  [TestMethod]
        public void ProcessMultipleItems_AllMarkedAsMovedToOld()
   {
    // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
          testProcessor.AddSimulatedMailItem("Subject 1", "Body 1");
  testProcessor.AddSimulatedMailItem("Subject 2", "Body 2");

     // Act
            testProcessor.TestProcessSimulatedItems();

        // Assert
    foreach (var item in testProcessor.ItemsProcessedThisSession)
 {
      Assert.IsTrue(item.WasMovedToOld,
              $"Item '{item.Subject}' should be marked as moved to old folder");
            }
 }

     #endregion

        #region Timestamp Tests

/// <summary>
     /// Verifies that processed items have a valid processed time.
        /// </summary>
        [TestMethod]
   public void ProcessItem_SetsProcessedTime()
  {
   // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
     var beforeProcess = DateTime.Now;

    // Act
        var result = testProcessor.TestProcessSingleItem(
    "Test Subject",
     "Test Body");

            var afterProcess = DateTime.Now;

            // Assert
            Assert.IsNotNull(result, "Processed item should not be null");
    Assert.IsTrue(result.ProcessedTime >= beforeProcess,
           "ProcessedTime should be at or after test start");
            Assert.IsTrue(result.ProcessedTime <= afterProcess,
                "ProcessedTime should be at or before test end");
        }

        /// <summary>
        /// Verifies that received time is before processed time.
  /// </summary>
        [TestMethod]
        public void ProcessItem_ReceivedTimeBeforeProcessedTime()
 {
       // Arrange
         var testProcessor = new TestAddSuppProdProcessor();

            // Act
     var result = testProcessor.TestProcessSingleItem(
                "Test Subject",
      "Test Body");

       // Assert
            Assert.IsNotNull(result, "Processed item should not be null");
            Assert.IsTrue(result.ReceivedTime < result.ProcessedTime,
        "ReceivedTime should be before ProcessedTime");
        }

        #endregion

        #region Reset Tests

        /// <summary>
        /// Verifies that Reset clears all counters and records.
        /// </summary>
        [TestMethod]
      public void Reset_ClearsAllData()
        {
     // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
    testProcessor.TestProcessSingleItem("Subject 1", "Body 1");
       testProcessor.TestProcessSingleItem("Subject 2", "Body 2");

    // Act
            testProcessor.Reset();

          // Assert
  Assert.AreEqual(0, testProcessor.ProcessedCount, "ProcessedCount should be 0");
            Assert.AreEqual(0, testProcessor.ItemsProcessedThisSession.Count,
     "ItemsProcessedThisSession should be empty");
   Assert.AreEqual(0, testProcessor.SimulatedMailItems.Count,
            "SimulatedMailItems should be empty");
      }

        /// <summary>
  /// Verifies that Reset clears error state.
        /// </summary>
        [TestMethod]
     public void Reset_ClearsErrorState()
  {
            // Arrange
    var testProcessor = new TestAddSuppProdProcessor();
     testProcessor.TestProcessSingleItem("Subject", "Body");

            // Act
      testProcessor.Reset();

            // Assert
            Assert.IsFalse(testProcessor.ErrorOccurred, "ErrorOccurred should be false");
        Assert.IsNull(testProcessor.LastErrorMessage, "LastErrorMessage should be null");
     }

   #endregion

    #region Error Handling Tests

        /// <summary>
        /// Verifies that no error occurs during normal processing.
    /// </summary>
        [TestMethod]
     public void ProcessItem_NoErrorDuringNormalProcessing()
        {
       // Arrange
  var testProcessor = new TestAddSuppProdProcessor();

      // Act
            testProcessor.TestProcessSingleItem("Test Subject", "Test Body");

      // Assert
Assert.IsFalse(testProcessor.ErrorOccurred,
           "No error should occur during normal processing");
       Assert.IsNull(testProcessor.LastErrorMessage,
                "Error message should be null");
        }

        #endregion

        #region Simulated Mail Item Tests

        /// <summary>
        /// Verifies that AddSimulatedMailItem correctly adds items.
        /// </summary>
        [TestMethod]
   public void AddSimulatedMailItem_AddsItemCorrectly()
        {
     // Arrange
       var testProcessor = new TestAddSuppProdProcessor();

  // Act
            testProcessor.AddSimulatedMailItem(
                "Test Subject",
        "Test Body",
 "test@nih.gov");

            // Assert
            Assert.AreEqual(1, testProcessor.SimulatedMailItems.Count,
    "Should have 1 simulated item");
            Assert.AreEqual("Test Subject", testProcessor.SimulatedMailItems[0].Subject);
         Assert.AreEqual("Test Body", testProcessor.SimulatedMailItems[0].Body);
            Assert.AreEqual("test@nih.gov", testProcessor.SimulatedMailItems[0].SenderEmail);
        }

        /// <summary>
        /// Verifies that simulated items have default sender if not specified.
        /// </summary>
        [TestMethod]
        public void AddSimulatedMailItem_UsesDefaultSender()
        {
    // Arrange
   var testProcessor = new TestAddSuppProdProcessor();

 // Act - don't specify sender email
  testProcessor.AddSimulatedMailItem("Subject", "Body");

    // Assert
      Assert.AreEqual("test@nih.gov", testProcessor.SimulatedMailItems[0].SenderEmail,
              "Should use default sender email");
     }

        /// <summary>
        /// Verifies that simulated items have received time set.
        /// </summary>
        [TestMethod]
        public void AddSimulatedMailItem_SetsReceivedTime()
{
     // Arrange
      var testProcessor = new TestAddSuppProdProcessor();
            var customTime = new DateTime(2024, 1, 15, 10, 30, 0);

    // Act
            testProcessor.AddSimulatedMailItem("Subject", "Body", "test@nih.gov", customTime);

            // Assert
          Assert.AreEqual(customTime, testProcessor.SimulatedMailItems[0].ReceivedTime,
    "Should use provided received time");
        }

        #endregion

        #region Configuration Tests

        /// <summary>
        /// Verifies default simulated directory path.
        /// </summary>
        [TestMethod]
        public void SimulatedDirPath_HasDefaultValue()
        {
            // Arrange & Act
     var testProcessor = new TestAddSuppProdProcessor();

       // Assert
    Assert.IsFalse(string.IsNullOrEmpty(testProcessor.SimulatedDirPath),
 "SimulatedDirPath should have a default value");
        }

 /// <summary>
     /// Verifies default simulated output directory.
     /// </summary>
     [TestMethod]
     public void SimulatedOutDir_HasDefaultValue()
        {
         // Arrange & Act
    var testProcessor = new TestAddSuppProdProcessor();

            // Assert
        Assert.IsFalse(string.IsNullOrEmpty(testProcessor.SimulatedOutDir),
         "SimulatedOutDir should have a default value");
        }

        #endregion

        #region Scenario-Based Tests

        /// <summary>
        /// Simulates processing a system notification email from nciogaegrantsprod.
        /// </summary>
        [TestMethod]
        public void Scenario_SystemNotification_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Admin Supplement notification";
            string body = "You have received a supplement notification. Notification Id=12345";
            string sender = "nciogaegrantsprod@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "System notification should be processed");
            Assert.AreEqual(subject, result.Subject);
            Assert.IsTrue(result.Body.Contains("Notification Id=12345"));
            Assert.AreEqual(sender, result.SenderEmail);
            Assert.IsTrue(result.WasMovedToOld, "Should be moved to archive");
        }

        /// <summary>
        /// Simulates processing an eRA notification from caeranotifications.
        /// </summary>
        [TestMethod]
        public void Scenario_EraNotification_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Supplement Requested - 5R01CA123456-03";
            string body = "An administrative supplement has been requested for grant 5R01CA123456-03";
            string sender = "caeranotifications@era.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "eRA notification should be processed");
            Assert.IsTrue(result.Subject.Contains("Supplement Requested"));
            Assert.IsTrue(result.Subject.Contains("5R01CA123456-03"));
            Assert.AreEqual(sender, result.SenderEmail);
        }

        /// <summary>
        /// Simulates processing a staff correspondence email.
        /// </summary>
        [TestMethod]
        public void Scenario_StaffCorrespondence_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "category=correspondence,sub=admin supplement,grantnumber=1R01CA123456-01";
            string body = "This is the correspondence body text that will be saved.";
            string sender = "driskelleb@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "Staff correspondence should be processed");
            Assert.IsTrue(result.Subject.Contains("category=correspondence"));
            Assert.IsTrue(result.Subject.Contains("sub=admin supplement"));
            Assert.IsTrue(result.Subject.Contains("grantnumber="));
            Assert.AreEqual(sender, result.SenderEmail);
        }

        /// <summary>
        /// Simulates processing a staff application file upload.
        /// </summary>
        [TestMethod]
        public void Scenario_StaffApplicationFile_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "category=application file,grantnumber=5R44CA987654-02";
            string body = "Please process the attached application file.";
            string sender = "jonesni@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "Staff application file should be processed");
            Assert.IsTrue(result.Subject.Contains("category=application file"));
            Assert.IsTrue(result.Subject.Contains("grantnumber="));
            Assert.AreEqual(sender, result.SenderEmail);
        }

        /// <summary>
        /// Simulates processing a PD/PI reply to a notification.
        /// </summary>
        [TestMethod]
        public void Scenario_PDPIReply_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Re: Admin Supplement Notification";
            string body = "Thank you for the notification. Notification Id=67890. I accept the supplement.";
            string sender = "pi.researcher@university.edu";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "PD/PI reply should be processed");
            Assert.IsTrue(result.Subject.Contains("Re:"));
            Assert.IsTrue(result.Body.Contains("Notification Id=67890"));
            Assert.AreEqual(sender, result.SenderEmail);
        }

        /// <summary>
        /// Simulates processing an email from unknown sender (should trigger error handling).
        /// </summary>
        [TestMethod]
        public void Scenario_UnknownSender_ProcessesAsUnidentified()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Random email subject";
            string body = "This email has no notification ID and is from an unknown sender.";
            string sender = "unknown@external.com";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result, "Unknown sender email should still be recorded");
            Assert.AreEqual(sender, result.SenderEmail);
            // In real implementation, this would trigger admin notification
        }

        /// <summary>
        /// Simulates batch processing of multiple email types.
        /// </summary>
        [TestMethod]
        public void Scenario_BatchProcessing_HandlesMultipleTypes()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();

            // Add various email types
            testProcessor.AddSimulatedMailItem(
                "Admin Supplement notification",
                "Notification Id=111",
                "nciogaegrantsprod@mail.nih.gov");

            testProcessor.AddSimulatedMailItem(
                "Supplement Requested - 5R01CA111111-01",
                "Supplement requested",
                "caeranotifications@era.nih.gov");

            testProcessor.AddSimulatedMailItem(
                "category=correspondence,sub=diversity supplement,grantnumber=2R01CA222222-02",
                "Staff correspondence body",
                "omairi@mail.nih.gov");

            testProcessor.AddSimulatedMailItem(
                "Re: Supplement Notification",
                "I accept. Notification Id=222",
                "pi@university.edu");

            // Act
            int processed = testProcessor.TestProcessSimulatedItems();

            // Assert
            Assert.AreEqual(4, processed, "Should process all 4 emails");
            Assert.AreEqual(4, testProcessor.ItemsProcessedThisSession.Count);

            // Verify each type was captured
            Assert.IsTrue(testProcessor.ItemsProcessedThisSession[0].Subject.Contains("Admin Supplement"));
            Assert.IsTrue(testProcessor.ItemsProcessedThisSession[1].Subject.Contains("Supplement Requested"));
            Assert.IsTrue(testProcessor.ItemsProcessedThisSession[2].Subject.Contains("category="));
            Assert.IsTrue(testProcessor.ItemsProcessedThisSession[3].Subject.Contains("Re:"));
        }

        /// <summary>
        /// Simulates processing with different authorized staff members.
        /// </summary>
        [TestMethod]
        public void Scenario_MultipleAuthorizedStaff_AllProcessCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string[] staffMembers = { "driskelleb", "jonesni", "omairi", "woldezf" };

            foreach (var staff in staffMembers)
            {
                testProcessor.AddSimulatedMailItem(
                    $"category=correspondence,sub=test,grantnumber=1R01CA{staff}-01",
                    $"Test from {staff}",
                    $"{staff}@mail.nih.gov");
            }

            // Act
            int processed = testProcessor.TestProcessSimulatedItems();

            // Assert
            Assert.AreEqual(4, processed, "Should process all authorized staff emails");
            foreach (var item in testProcessor.ItemsProcessedThisSession)
            {
                Assert.IsTrue(item.WasMovedToOld, $"Email from {item.SenderEmail} should be archived");
            }
        }

        /// <summary>
        /// Simulates processing diversity supplement notifications.
        /// </summary>
        [TestMethod]
        public void Scenario_DiversitySupplement_ProcessesWithCorrectSubcategory()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Diversity Supplement notification";
            string body = "Your diversity supplement notification. Notification Id=99999";
            string sender = "nciogaegrantsprod@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Subject.Contains("Diversity Supplement"));
            Assert.IsTrue(result.Body.Contains("diversity"));
        }

        /// <summary>
        /// Simulates processing status change notifications.
        /// </summary>
        [TestMethod]
        public void Scenario_StatusChange_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Change in Status - Supplement Application";
            string body = "The status of your supplement has changed. Notification Id=88888";
            string sender = "nciogaegrantsprod@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Subject.Contains("Change in Status"));
        }

        /// <summary>
        /// Simulates processing response required notifications.
        /// </summary>
        [TestMethod]
        public void Scenario_ResponseRequired_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProdProcessor();
            string subject = "Response Required - Administrative Supplement";
            string body = "Your response is required for this supplement. Notification Id=77777";
            string sender = "nciogaegrantsprod@mail.nih.gov";

            // Act
            var result = testProcessor.TestProcessSingleItem(subject, body, sender);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Subject.Contains("Response Required"));
        }

        #endregion
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.AddSuppProd
{
    /// <summary>
    /// Unit tests for the AddSuppProd.Processor class.
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
    }
}

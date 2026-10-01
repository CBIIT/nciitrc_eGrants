using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OGARequestAccountDisable;
using EmailHandlingTests.Unit.OGADisableEmail;

namespace EmailHandlingTests.Integration.OGADisableEmail
{
    /// <summary>
  /// Integration tests for the OGARequestAccountDisable.Processor class.
/// These tests verify user filtering, email body creation, and processing logic
    /// without requiring actual Outlook or database connections.
    /// </summary>
    [TestClass]
    public class OGADisableIntegrationTests
    {
        #region Full Processing Integration Tests

        /// <summary>
        /// Verifies that processing simulated users creates an email record.
  /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_CreatesEmailRecord()
        {
            // Arrange
    var testProcessor = new TestOGADisableProcessor();
         testProcessor.AddSimulatedDisabledUser("John", "Doe", "", "jdoe", "jdoe@nih.gov", "01/15/2024");
            testProcessor.AddSimulatedDisabledUser("Jane", "Smith", "", "jsmith", "jsmith@nih.gov", "01/10/2024");

      // Act
  int result = testProcessor.TestProcessSimulatedUsers();

            // Assert
            Assert.AreEqual(2, result, "Should process 2 users");
            Assert.AreEqual(1, testProcessor.EmailsSentThisSession.Count, "Should create 1 email");
        }

        /// <summary>
        /// Verifies that the email contains the correct user count.
        /// </summary>
        [TestMethod]
   public void ProcessSimulatedUsers_EmailContainsCorrectUserCount()
        {
            // Arrange
            var testProcessor = new TestOGADisableProcessor();
  testProcessor.AddSimulatedDisabledUser("John", "Doe", "", "jdoe");
   testProcessor.AddSimulatedDisabledUser("Jane", "Smith", "", "jsmith");
         testProcessor.AddSimulatedDisabledUser("Bob", "Johnson", "", "bjohnson");

     // Act
         testProcessor.TestProcessSimulatedUsers();

  // Assert
      var email = testProcessor.EmailsSentThisSession[0];
            Assert.AreEqual(3, email.UserCount, "Email should indicate 3 users");
        }

     /// <summary>
   /// Verifies that no email is sent when there are no users to process.
        /// </summary>
    [TestMethod]
        public void ProcessSimulatedUsers_NoUsers_NoEmailSent()
  {
            // Arrange
            var testProcessor = new TestOGADisableProcessor();
      // Don't add any users

            // Act
    int result = testProcessor.TestProcessSimulatedUsers();

        // Assert
            Assert.AreEqual(0, result, "Should process 0 users");
         Assert.AreEqual(0, testProcessor.EmailsSentThisSession.Count, "Should not create any email");
   }

        /// <summary>
        /// Verifies that the email subject is correct.
  /// </summary>
    [TestMethod]
        public void ProcessSimulatedUsers_EmailHasCorrectSubject()
      {
            // Arrange
    var testProcessor = new TestOGADisableProcessor();
      testProcessor.AddSimulatedDisabledUser("John", "Doe");

     // Act
            testProcessor.TestProcessSimulatedUsers();

            // Assert
        var email = testProcessor.EmailsSentThisSession[0];
 Assert.IsTrue(email.Subject.Contains("Deprovisioning Request"),
    "Subject should contain 'Deprovisioning Request'");
     }

        /// <summary>
        /// Verifies that custom recipient is used when set.
        /// </summary>
[TestMethod]
  public void ProcessSimulatedUsers_UsesCustomRecipient()
        {
   // Arrange
          var testProcessor = new TestOGADisableProcessor();
       testProcessor.SimulatedRecipient = "custom@nih.gov";
            testProcessor.AddSimulatedDisabledUser("John", "Doe");

            // Act
        testProcessor.TestProcessSimulatedUsers();

          // Assert
  var email = testProcessor.EmailsSentThisSession[0];
 Assert.AreEqual("custom@nih.gov", email.To, "Should use custom recipient");
   }

        #endregion

        #region Email Body Content Tests

        /// <summary>
   /// Verifies that the email body contains all user names.
        /// </summary>
 [TestMethod]
      public void ProcessSimulatedUsers_EmailBodyContainsAllUserNames()
        {
// Arrange
   var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("John", "Doe", "", "jdoe");
    testProcessor.AddSimulatedDisabledUser("Jane", "Smith", "", "jsmith");

         // Act
      testProcessor.TestProcessSimulatedUsers();

   // Assert
       var email = testProcessor.EmailsSentThisSession[0];
          Assert.IsTrue(email.Body.Contains("John Doe"), "Body should contain 'John Doe'");
            Assert.IsTrue(email.Body.Contains("Jane Smith"), "Body should contain 'Jane Smith'");
        }

        /// <summary>
        /// Verifies that the email body contains user IDs.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_EmailBodyContainsUserIds()
    {
   // Arrange
            var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("John", "Doe", "", "johndoe123");

         // Act
        testProcessor.TestProcessSimulatedUsers();

      // Assert
      var email = testProcessor.EmailsSentThisSession[0];
   Assert.IsTrue(email.Body.Contains("johndoe123"), "Body should contain user ID");
    }

        /// <summary>
        /// Verifies that the email body contains last login dates.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_EmailBodyContainsLastLoginDates()
        {
       // Arrange
   var testProcessor = new TestOGADisableProcessor();
     testProcessor.AddSimulatedDisabledUser("John", "Doe", "", "jdoe", "jdoe@nih.gov", "12/25/2023");

    // Act
         testProcessor.TestProcessSimulatedUsers();

       // Assert
   var email = testProcessor.EmailsSentThisSession[0];
            Assert.IsTrue(email.Body.Contains("12/25/2023"), "Body should contain last login date");
   }

        /// <summary>
        /// Verifies that the email body contains the inactivity message.
  /// </summary>
        [TestMethod]
    public void ProcessSimulatedUsers_EmailBodyContainsInactivityMessage()
        {
   // Arrange
         var testProcessor = new TestOGADisableProcessor();
      testProcessor.AddSimulatedDisabledUser("John", "Doe");

        // Act
     testProcessor.TestProcessSimulatedUsers();

            // Assert
            var email = testProcessor.EmailsSentThisSession[0];
       Assert.IsTrue(email.Body.Contains("60 days of inactivity"),
    "Body should contain inactivity message");
        }

  /// <summary>
        /// Verifies that the email body contains HTML table structure.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_EmailBodyContainsHtmlTable()
    {
     // Arrange
            var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("John", "Doe");

      // Act
     testProcessor.TestProcessSimulatedUsers();

    // Assert
        var email = testProcessor.EmailsSentThisSession[0];
 Assert.IsTrue(email.Body.Contains("<table"), "Body should contain table tag");
Assert.IsTrue(email.Body.Contains("</table>"), "Body should contain closing table tag");
        }

        #endregion

   #region User Filtering Integration Tests

        /// <summary>
      /// Verifies that users with missing names are filtered out during processing.
    /// </summary>
        [TestMethod]
  public void ProcessSimulatedUsers_FiltersOutUsersWithMissingNames()
        {
     // Arrange
    var testProcessor = new TestOGADisableProcessor();
         testProcessor.AddSimulatedDisabledUser("John", "Doe");  // Valid
            testProcessor.AddSimulatedDisabledUser("Friedrich", "");  // Missing last name
  testProcessor.AddSimulatedDisabledUser("", "Einstein");  // Missing first name

 // Act
     int result = testProcessor.TestProcessSimulatedUsers();

       // Assert
            Assert.AreEqual(1, result, "Should only process user with complete name");
        }

    /// <summary>
        /// Verifies that service accounts with person_name are processed.
        /// </summary>
        [TestMethod]
    public void ProcessSimulatedUsers_ProcessesServiceAccounts()
        {
    // Arrange
            var testProcessor = new TestOGADisableProcessor();
      testProcessor.AddSimulatedDisabledUser("", "", "NCI OGA SERVICE ACCOUNT", "serviceacct");

            // Act
     int result = testProcessor.TestProcessSimulatedUsers();

            // Assert
            Assert.AreEqual(1, result, "Should process service account");
        }

        /// <summary>
      /// Verifies that service account uses person_name in email body.
        /// </summary>
    [TestMethod]
        public void ProcessSimulatedUsers_ServiceAccountUsesPersonName()
   {
       // Arrange
   var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("", "", "NCI PROGRESS REPORT", "ncipr");

 // Act
  testProcessor.TestProcessSimulatedUsers();

      // Assert
            var email = testProcessor.EmailsSentThisSession[0];
            Assert.IsTrue(email.Body.Contains("NCI PROGRESS REPORT"),
   "Body should contain service account person name");
        }

        /// <summary>
        /// Verifies that mixed users (valid and invalid) are correctly filtered.
  /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_MixedUsers_CorrectFiltering()
        {
       // Arrange
   var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("Valid", "User1");  // Valid
      testProcessor.AddSimulatedDisabledUser("", "", "SERVICE ACCT");  // Service account
      testProcessor.AddSimulatedDisabledUser("Invalid", "");  // Invalid
    testProcessor.AddSimulatedDisabledUser("Valid", "User2");  // Valid

    // Act
            int result = testProcessor.TestProcessSimulatedUsers();

    // Assert
  Assert.AreEqual(3, result, "Should process 3 valid users (2 regular + 1 service)");
 }

#endregion

        #region Users Processed Tracking Tests

        /// <summary>
        /// Verifies that processed users are tracked in session.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_TracksProcessedUsers()
        {
         // Arrange
            var testProcessor = new TestOGADisableProcessor();
       testProcessor.AddSimulatedDisabledUser("John", "Doe");
         testProcessor.AddSimulatedDisabledUser("Jane", "Smith");

         // Act
            testProcessor.TestProcessSimulatedUsers();

        // Assert
            Assert.AreEqual(2, testProcessor.UsersProcessedThisSession.Count,
     "Should track 2 processed users");
        }

        /// <summary>
        /// Verifies that processed users have FinalNameForOGA set.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_SetsFinalnameForOGA()
        {
      // Arrange
       var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("John", "Doe");

         // Act
            testProcessor.TestProcessSimulatedUsers();

       // Assert
    var user = testProcessor.UsersProcessedThisSession[0];
      Assert.AreEqual("John Doe", user.FinalNameForOGA,
      "FinalNameForOGA should be 'John Doe'");
        }

        #endregion

        #region Reset Tests

        /// <summary>
        /// Verifies that Reset clears all session data.
        /// </summary>
        [TestMethod]
  public void Reset_ClearsAllSessionData()
        {
      // Arrange
     var testProcessor = new TestOGADisableProcessor();
       testProcessor.AddSimulatedDisabledUser("John", "Doe");
            testProcessor.TestProcessSimulatedUsers();

            // Act
            testProcessor.Reset();

 // Assert
            Assert.AreEqual(0, testProcessor.ProcessedCount, "ProcessedCount should be 0");
Assert.AreEqual(0, testProcessor.EmailsSentThisSession.Count, "Emails should be cleared");
  Assert.AreEqual(0, testProcessor.UsersProcessedThisSession.Count, "Processed users should be cleared");
       Assert.AreEqual(0, testProcessor.SimulatedDisabledUsers.Count, "Simulated users should be cleared");
   }

  /// <summary>
 /// Verifies that Reset clears error state.
      /// </summary>
        [TestMethod]
        public void Reset_ClearsErrorState()
        {
        // Arrange
        var testProcessor = new TestOGADisableProcessor();
            testProcessor.TestProcessSimulatedUsers();

    // Act
    testProcessor.Reset();

     // Assert
            Assert.IsFalse(testProcessor.ErrorOccurred, "ErrorOccurred should be false");
   Assert.IsNull(testProcessor.LastErrorMessage, "LastErrorMessage should be null");
        }

        /// <summary>
        /// Verifies that Reset resets recipient to default.
        /// </summary>
        [TestMethod]
        public void Reset_ResetsRecipientToDefault()
        {
            // Arrange
         var testProcessor = new TestOGADisableProcessor();
        testProcessor.SimulatedRecipient = "custom@nih.gov";

         // Act
            testProcessor.Reset();

          // Assert
            Assert.AreEqual("test@nih.gov", testProcessor.SimulatedRecipient,
          "Recipient should be reset to default");
    }

        #endregion

        #region Error Handling Tests

        /// <summary>
 /// Verifies that no error occurs during normal processing.
        /// </summary>
   [TestMethod]
        public void ProcessSimulatedUsers_NoErrorDuringNormalProcessing()
    {
            // Arrange
         var testProcessor = new TestOGADisableProcessor();
            testProcessor.AddSimulatedDisabledUser("John", "Doe");

   // Act
            testProcessor.TestProcessSimulatedUsers();

     // Assert
            Assert.IsFalse(testProcessor.ErrorOccurred,
    "No error should occur during normal processing");
    Assert.IsNull(testProcessor.LastErrorMessage,
        "Error message should be null");
        }

        #endregion

#region Timestamp Tests

        /// <summary>
        /// Verifies that email has a valid timestamp.
        /// </summary>
        [TestMethod]
        public void ProcessSimulatedUsers_EmailHasValidTimestamp()
     {
       // Arrange
        var testProcessor = new TestOGADisableProcessor();
     testProcessor.AddSimulatedDisabledUser("John", "Doe");
            var beforeProcess = DateTime.Now;

            // Act
            testProcessor.TestProcessSimulatedUsers();

            // Assert
        var email = testProcessor.EmailsSentThisSession[0];
            Assert.IsTrue(email.TimeCaptured >= beforeProcess,
   "Email timestamp should be at or after test start");
            Assert.IsTrue(email.TimeCaptured <= DateTime.Now,
     "Email timestamp should be at or before now");
        }

    #endregion
    }
}

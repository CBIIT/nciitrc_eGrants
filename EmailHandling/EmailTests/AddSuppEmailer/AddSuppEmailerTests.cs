using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailTests.AddSuppEmailer
{
    /// <summary>
    /// Unit tests for the AddSuppEmailer.Processor class.
    /// These tests verify email creation, formatting, and processing logic
    /// without actually sending emails.
    /// </summary>
    [TestClass]
    public class AddSuppEmailerTests
    {
        #region Email Creation Tests

        /// <summary>
        /// Verifies that processing a notification creates an email with the correct recipient.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsCorrectRecipient()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "testuser@nih.gov";
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("testuser@nih.gov", result.To, "Recipient should match simulated value");
        }

        /// <summary>
        /// Verifies that the email subject contains the notification ID.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SubjectContainsNotificationId()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 99999;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.Subject.Contains(notifId.ToString()),
                $"Subject '{result.Subject}' should contain notification ID {notifId}");
        }

        /// <summary>
        /// Verifies that custom subject is used when provided.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_UsesCustomSubjectWhenProvided()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedSubject = "Administrative Supplement Request - Grant ABC123";
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("Administrative Supplement Request - Grant ABC123", result.Subject,
                "Subject should match the simulated custom subject");
        }

        #endregion

        #region Voting Options Tests

        /// <summary>
        /// Verifies that emails are created with the correct voting options.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsVotingOptions()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("Accepted;Rejected", result.VotingOptions,
                "Voting options should be 'Accepted;Rejected'");
        }

        /// <summary>
        /// Verifies that voting options contain 'Accepted' option.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_VotingOptionsContainAccepted()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.VotingOptions.Contains("Accepted"),
                "Voting options should contain 'Accepted'");
        }

        /// <summary>
        /// Verifies that voting options contain 'Rejected' option.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_VotingOptionsContainRejected()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.VotingOptions.Contains("Rejected"),
                "Voting options should contain 'Rejected'");
        }

        #endregion

        #region Email Importance Tests

        /// <summary>
        /// Verifies that emails are marked as high importance.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsHighImportance()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("olImportanceHigh", result.Importance,
                "Email should be marked as high importance");
        }

        #endregion

        #region Email Format Tests

        /// <summary>
        /// Verifies that emails use HTML body format.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_UsesHtmlFormat()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("olFormatHTML", result.BodyFormat,
                "Email should use HTML format");
        }

        /// <summary>
        /// Verifies that HTML body contains the notification ID.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_HtmlBodyContainsNotificationId()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 54321;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.Body.Contains(notifId.ToString()),
                $"HTML body should contain notification ID {notifId}");
        }

        /// <summary>
        /// Verifies that custom body is used when provided.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_UsesCustomBodyWhenProvided()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedBody = "<html><body><h1>Custom HTML Content</h1></body></html>";
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.Body.Contains("Custom HTML Content"),
                "Body should contain the custom HTML content");
        }

        #endregion

        #region Processing Counter Tests

        /// <summary>
        /// Verifies that processing counter is incremented.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_IncrementsCounter()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();

            // Act
            testProcessor.TestProcessSingleNotification(1);
            testProcessor.TestProcessSingleNotification(2);
            testProcessor.TestProcessSingleNotification(3);

            // Assert
            Assert.AreEqual(3, testProcessor.NotificationsProcessed,
                "Should have processed 3 notifications");
            Assert.AreEqual(3, testProcessor.EmailsSentThisSession.Count,
                "Should have captured 3 emails");
        }

        /// <summary>
        /// Verifies that Reset clears all counters and records.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsAllCountersAndRecords()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.TestProcessSingleNotification(1);
            testProcessor.TestProcessSingleNotification(2);

            // Act
            testProcessor.Reset();

            // Assert
            Assert.AreEqual(0, testProcessor.NotificationsProcessed,
                "Counter should be reset to 0");
            Assert.AreEqual(0, testProcessor.EmailsSentThisSession.Count,
                "Email records should be cleared");
            Assert.IsFalse(testProcessor.ErrorOccurred,
                "Error flag should be reset");
        }

        #endregion

        #region Multiple Recipients Tests

        /// <summary>
        /// Verifies that multiple recipients can be set.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SupportsMultipleRecipients()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "user1@nih.gov; user2@nih.gov; user3@nih.gov";
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.IsTrue(result.To.Contains("user1@nih.gov"), "Should contain first recipient");
            Assert.IsTrue(result.To.Contains("user2@nih.gov"), "Should contain second recipient");
            Assert.IsTrue(result.To.Contains("user3@nih.gov"), "Should contain third recipient");
        }

        #endregion

        #region Default Values Tests

        /// <summary>
        /// Verifies default recipient when no simulation is provided.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_UsesDefaultRecipientWhenNotSimulated()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            // Don't set SimulatedRecipients - should use default
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("test@nih.gov", result.To,
                "Should use default test recipient");
        }

        /// <summary>
        /// Verifies default subject format when no simulation is provided.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_UsesDefaultSubjectFormatWhenNotSimulated()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            // Don't set SimulatedSubject - should use default format
            int notifId = 77777;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Email record should not be null");
            Assert.AreEqual("Notification 77777", result.Subject,
                "Should use default subject format with notification ID");
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// Verifies that no error occurs during normal processing.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_NoErrorDuringNormalProcessing()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 12345;

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsFalse(testProcessor.ErrorOccurred, "No error should occur during normal processing");
            Assert.IsNull(testProcessor.LastErrorMessage, "Error message should be null");
        }

        #endregion
    }
}

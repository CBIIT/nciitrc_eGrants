using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailHandlingTests.AddSuppEmailer
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
            Assert.AreEqual("High", result.Importance,
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
            Assert.AreEqual("HTML", result.BodyFormat,
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

        #region Scenario-Based Tests

        /// <summary>
        /// Scenario: Processing notification for single PI.
        /// </summary>
        [TestMethod]
        public void Scenario_SinglePI_EmailCreatedCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "pi.researcher@university.edu";
            testProcessor.SimulatedSubject = "Administrative Supplement Notification - Grant 5R01CA123456-03";
            testProcessor.SimulatedBody = "<html><body><p>Your grant has been approved for a supplement.</p></body></html>";

            // Act
            var result = testProcessor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("pi.researcher@university.edu", result.To);
            Assert.IsTrue(result.Subject.Contains("Administrative Supplement"));
            Assert.IsTrue(result.Body.Contains("approved for a supplement"));
            Assert.AreEqual("Accepted;Rejected", result.VotingOptions);
            Assert.AreEqual("High", result.Importance);
        }

        /// <summary>
        /// Scenario: Processing notification with multiple stakeholders.
        /// </summary>
        [TestMethod]
        public void Scenario_MultipleStakeholders_AllRecipientsIncluded()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "pi@university.edu;admin@university.edu;grants@university.edu";
            testProcessor.SimulatedSubject = "Diversity Supplement Opportunity";

            // Act
            var result = testProcessor.TestProcessSingleNotification(54321);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.To.Contains("pi@university.edu"));
            Assert.IsTrue(result.To.Contains("admin@university.edu"));
            Assert.IsTrue(result.To.Contains("grants@university.edu"));
        }

        /// <summary>
        /// Scenario: Batch processing of multiple notifications.
        /// </summary>
        [TestMethod]
        public void Scenario_BatchProcessing_AllNotificationsProcessed()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "test@nih.gov";

            // Act
            testProcessor.TestProcessSingleNotification(100);
            testProcessor.TestProcessSingleNotification(200);
            testProcessor.TestProcessSingleNotification(300);
            testProcessor.TestProcessSingleNotification(400);
            testProcessor.TestProcessSingleNotification(500);

            // Assert
            Assert.AreEqual(5, testProcessor.NotificationsProcessed);
            Assert.AreEqual(5, testProcessor.EmailsSentThisSession.Count);

            // Verify each notification was tracked
            Assert.IsTrue(testProcessor.EmailsSentThisSession[0].Body.Contains("100"));
            Assert.IsTrue(testProcessor.EmailsSentThisSession[1].Body.Contains("200"));
            Assert.IsTrue(testProcessor.EmailsSentThisSession[2].Body.Contains("300"));
            Assert.IsTrue(testProcessor.EmailsSentThisSession[3].Body.Contains("400"));
            Assert.IsTrue(testProcessor.EmailsSentThisSession[4].Body.Contains("500"));
        }

        /// <summary>
        /// Scenario: Processing notification with HTML-formatted body.
        /// </summary>
        [TestMethod]
        public void Scenario_HtmlFormattedBody_PreservesFormatting()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "test@nih.gov";
            testProcessor.SimulatedBody = @"
                <html>
                <head><title>Supplement Notification</title></head>
                <body>
                    <h1>Administrative Supplement Approved</h1>
                    <p>Grant: <strong>5R01CA123456-03</strong></p>
                    <ul>
                        <li>Amount: $50,000</li>
                        <li>Duration: 12 months</li>
                    </ul>
                </body>
                </html>";

            // Act
            var result = testProcessor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("HTML", result.BodyFormat);
            Assert.IsTrue(result.Body.Contains("<h1>"));
            Assert.IsTrue(result.Body.Contains("<strong>"));
            Assert.IsTrue(result.Body.Contains("<li>"));
        }

        /// <summary>
        /// Scenario: Processing notification for diversity supplement.
        /// </summary>
        [TestMethod]
        public void Scenario_DiversitySupplement_CorrectSubjectAndBody()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "pi@university.edu";
            testProcessor.SimulatedSubject = "Diversity Supplement Opportunity - Grant 2R01CA987654-04";
            testProcessor.SimulatedBody = "<html><body><p>We are pleased to inform you about a diversity supplement opportunity.</p></body></html>";

            // Act
            var result = testProcessor.TestProcessSingleNotification(67890);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Subject.Contains("Diversity Supplement"));
            Assert.IsTrue(result.Body.Contains("diversity supplement opportunity"));
            Assert.AreEqual("Accepted;Rejected", result.VotingOptions);
        }

        /// <summary>
        /// Scenario: Processing notification requiring urgent response.
        /// </summary>
        [TestMethod]
        public void Scenario_UrgentNotification_MarkedHighImportance()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "urgent@university.edu";
            testProcessor.SimulatedSubject = "URGENT: Response Required - Administrative Supplement";

            // Act
            var result = testProcessor.TestProcessSingleNotification(11111);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("High", result.Importance, "Urgent notifications should always be high importance");
            Assert.IsTrue(result.Subject.Contains("URGENT"));
        }

        /// <summary>
        /// Scenario: Processing after system restart (state reset).
        /// </summary>
        [TestMethod]
        public void Scenario_AfterSystemRestart_ProcessesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "test@nih.gov";

            // Simulate some processing
            testProcessor.TestProcessSingleNotification(100);
            testProcessor.TestProcessSingleNotification(200);

            // Simulate system restart (reset)
            testProcessor.Reset();

            // Act - Process new notifications after restart
            testProcessor.TestProcessSingleNotification(300);
            testProcessor.TestProcessSingleNotification(400);

            // Assert
            Assert.AreEqual(2, testProcessor.NotificationsProcessed, "Should start fresh after reset");
            Assert.AreEqual(2, testProcessor.EmailsSentThisSession.Count);
            Assert.IsTrue(testProcessor.EmailsSentThisSession[0].Body.Contains("300"));
            Assert.IsTrue(testProcessor.EmailsSentThisSession[1].Body.Contains("400"));
        }

        /// <summary>
        /// Scenario: Processing notification with international characters.
        /// </summary>
        [TestMethod]
        public void Scenario_InternationalCharacters_PreservesContent()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            testProcessor.SimulatedRecipients = "josé.garcía@universidad.edu";
            testProcessor.SimulatedSubject = "Suplemento Administrativo - Subvención";
            testProcessor.SimulatedBody = "<html><body><p>Notificación: Café & Résumé</p></body></html>";

            // Act
            var result = testProcessor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.To.Contains("josé.garcía"));
            Assert.IsTrue(result.Subject.Contains("Suplemento"));
            Assert.IsTrue(result.Body.Contains("Café"));
            Assert.IsTrue(result.Body.Contains("Résumé"));
        }

        /// <summary>
        /// Scenario: Processing with very long recipient list.
        /// </summary>
        [TestMethod]
        public void Scenario_LongRecipientList_HandlesCorrectly()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            var recipients = new System.Text.StringBuilder();
            for (int i = 1; i <= 20; i++)
            {
                if (i > 1) recipients.Append(";");
                recipients.Append($"user{i}@university.edu");
            }
            testProcessor.SimulatedRecipients = recipients.ToString();

            // Act
            var result = testProcessor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.To.Contains("user1@university.edu"));
            Assert.IsTrue(result.To.Contains("user10@university.edu"));
            Assert.IsTrue(result.To.Contains("user20@university.edu"));
        }

        /// <summary>
        /// Scenario: Processing notification with embedded notification ID.
        /// </summary>
        [TestMethod]
        public void Scenario_EmbeddedNotificationId_PreservesInBody()
        {
            // Arrange
            var testProcessor = new TestAddSuppProcessor();
            int notifId = 88888;
            testProcessor.SimulatedRecipients = "test@nih.gov";
            testProcessor.SimulatedBody = $"<html><body><p>Please reference Notification Id={notifId} in your response.</p></body></html>";

            // Act
            var result = testProcessor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Body.Contains($"Notification Id={notifId}"));
        }

        #endregion
    }
}

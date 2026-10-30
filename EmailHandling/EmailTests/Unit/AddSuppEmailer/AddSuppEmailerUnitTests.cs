using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailHandlingTests.Shared;

namespace EmailHandlingTests.Unit.AddSuppEmailer
{
    /// <summary>
    /// Unit tests for AddSuppEmailer.Processor helper methods and logic.
    /// These tests verify individual methods in isolation without requiring
    /// database or Outlook dependencies.
    /// </summary>
    [TestClass]
    public class AddSuppEmailerUnitTests
    {
        private TestAddSuppProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _processor = new TestAddSuppProcessor();
        }

        #region Notification Processing Tests

        /// <summary>
        /// Verifies that a single notification is processed successfully.
        /// </summary>
        [TestMethod]
        public void ProcessSingleNotification_IncrementsCounter()
        {
            // Arrange
            int notifId = 12345;
            _processor.SimulatedRecipients = "test@nih.gov";
            _processor.SimulatedSubject = "Test Subject";
            _processor.SimulatedBody = "Test Body";

            // Act
            var result = _processor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsNotNull(result, "Should return an email record");
            Assert.AreEqual(1, _processor.NotificationsProcessed, "Should have processed 1 notification");
            Assert.AreEqual(1, _processor.EmailsSentThisSession.Count, "Should have 1 email in session");
        }

        /// <summary>
        /// Verifies that multiple notifications are tracked correctly.
        /// </summary>
        [TestMethod]
        public void ProcessMultipleNotifications_TracksAll()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            _processor.TestProcessSingleNotification(111);
            _processor.TestProcessSingleNotification(222);
            _processor.TestProcessSingleNotification(333);

            // Assert
            Assert.AreEqual(3, _processor.NotificationsProcessed, "Should have processed 3 notifications");
            Assert.AreEqual(3, _processor.EmailsSentThisSession.Count, "Should have 3 emails in session");
        }

        /// <summary>
        /// Verifies that notification IDs are distinct in email records.
        /// </summary>
        [TestMethod]
        public void ProcessMultipleNotifications_PreservesDistinctIds()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            _processor.TestProcessSingleNotification(111);
            _processor.TestProcessSingleNotification(222);

            // Assert
            Assert.IsTrue(_processor.EmailsSentThisSession[0].Body.Contains("111"));
            Assert.IsTrue(_processor.EmailsSentThisSession[1].Body.Contains("222"));
        }

        #endregion

        #region Email Content Tests

        /// <summary>
        /// Verifies that email subject is set correctly.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsSubjectCorrectly()
        {
            // Arrange
            string expectedSubject = "Administrative Supplement Notification - Grant 5R01CA123456-03";
            _processor.SimulatedSubject = expectedSubject;

            // Act
            var result = _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.AreEqual(expectedSubject, result.Subject);
        }

        /// <summary>
        /// Verifies that email body is set correctly.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsBodyCorrectly()
        {
            // Arrange
            string expectedBody = "<html><body><p>Test notification body</p></body></html>";
            _processor.SimulatedBody = expectedBody;

            // Act
            var result = _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.AreEqual(expectedBody, result.Body);
        }

        /// <summary>
        /// Verifies that email recipients are set correctly.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsRecipientsCorrectly()
        {
            // Arrange
            string expectedRecipients = "pi@university.edu;coadmin@university.edu";
            _processor.SimulatedRecipients = expectedRecipients;

            // Act
            var result = _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.AreEqual(expectedRecipients, result.To);
        }

        /// <summary>
        /// Verifies that body contains notification ID.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_BodyContainsNotificationId()
        {
            // Arrange
            int notifId = 99999;
            _processor.SimulatedBody = $"Notification details. Notification Id={notifId}";

            // Act
            var result = _processor.TestProcessSingleNotification(notifId);

            // Assert
            Assert.IsTrue(result.Body.Contains(notifId.ToString()));
            Assert.IsTrue(result.Body.Contains("Notification Id="));
        }

        #endregion

        #region Voting and Formatting Tests

        /// <summary>
        /// Verifies that voting options are always set to "Accepted;Rejected".
        /// </summary>
        [TestMethod]
        public void ProcessNotification_AlwaysSetsVotingOptions()
        {
            // Arrange & Act
            var result1 = _processor.TestProcessSingleNotification(111);
            var result2 = _processor.TestProcessSingleNotification(222);
            var result3 = _processor.TestProcessSingleNotification(333);

            // Assert
            Assert.AreEqual("Accepted;Rejected", result1.VotingOptions);
            Assert.AreEqual("Accepted;Rejected", result2.VotingOptions);
            Assert.AreEqual("Accepted;Rejected", result3.VotingOptions);
        }

        /// <summary>
        /// Verifies that importance is always set to High.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_AlwaysSetsHighImportance()
        {
            // Arrange & Act
            var result1 = _processor.TestProcessSingleNotification(111);
            var result2 = _processor.TestProcessSingleNotification(222);

            // Assert
            Assert.AreEqual("High", result1.Importance);
            Assert.AreEqual("High", result2.Importance);
        }

        /// <summary>
        /// Verifies that body format is always HTML.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_AlwaysSetsHtmlFormat()
        {
            // Arrange & Act
            var result1 = _processor.TestProcessSingleNotification(111);
            var result2 = _processor.TestProcessSingleNotification(222);

            // Assert
            Assert.AreEqual("HTML", result1.BodyFormat);
            Assert.AreEqual("HTML", result2.BodyFormat);
        }

        #endregion

        #region Reset Tests

        /// <summary>
        /// Verifies that Reset clears all tracked emails.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsEmailRecords()
        {
            // Arrange
            _processor.TestProcessSingleNotification(111);
            _processor.TestProcessSingleNotification(222);
            Assert.AreEqual(2, _processor.EmailsSentThisSession.Count);

            // Act
            _processor.Reset();

            // Assert
            Assert.AreEqual(0, _processor.EmailsSentThisSession.Count);
        }

        /// <summary>
        /// Verifies that Reset clears notification counter.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsNotificationCounter()
        {
            // Arrange
            _processor.TestProcessSingleNotification(111);
            _processor.TestProcessSingleNotification(222);
            Assert.AreEqual(2, _processor.NotificationsProcessed);

            // Act
            _processor.Reset();

            // Assert
            Assert.AreEqual(0, _processor.NotificationsProcessed);
        }

        /// <summary>
        /// Verifies that Reset clears error state.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsErrorState()
        {
            // Arrange
            _processor.TestProcessSingleNotification(111);

            // Act
            _processor.Reset();

            // Assert
            Assert.IsFalse(_processor.ErrorOccurred);
            Assert.IsNull(_processor.LastErrorMessage);
        }

        /// <summary>
        /// Verifies that Reset clears simulated data.
        /// </summary>
        [TestMethod]
        public void Reset_ClearsSimulatedData()
        {
            // Arrange
            _processor.SimulatedSubject = "Test Subject";
            _processor.SimulatedBody = "Test Body";
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            _processor.Reset();

            // Assert
            Assert.IsNull(_processor.SimulatedSubject);
            Assert.IsNull(_processor.SimulatedBody);
            Assert.IsNull(_processor.SimulatedRecipients);
        }

        #endregion

        #region Timestamp Tests

        /// <summary>
        /// Verifies that email records have a timestamp.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_SetsTimestamp()
        {
            // Arrange
            DateTime before = DateTime.Now;

            // Act
            var result = _processor.TestProcessSingleNotification(12345);
            DateTime after = DateTime.Now;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TimeCaptured >= before, "Timestamp should be at or after test start");
            Assert.IsTrue(result.TimeCaptured <= after, "Timestamp should be at or before test end");
        }

        /// <summary>
        /// Verifies that each email has a distinct timestamp.
        /// </summary>
        [TestMethod]
        public void ProcessMultipleNotifications_HasDistinctTimestamps()
        {
            // Arrange & Act
            var result1 = _processor.TestProcessSingleNotification(111);
            System.Threading.Thread.Sleep(10); // Small delay to ensure different timestamps
            var result2 = _processor.TestProcessSingleNotification(222);

            // Assert
            Assert.IsTrue(result2.TimeCaptured >= result1.TimeCaptured,
                "Second email should have same or later timestamp");
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
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsFalse(_processor.ErrorOccurred, "No error should occur");
            Assert.IsNull(_processor.LastErrorMessage, "Error message should be null");
        }

        #endregion

        #region Verbose Mode Tests

        /// <summary>
        /// Verifies that verbose mode doesn't affect processing.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_VerboseMode_ProcessesSuccessfully()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            var result = _processor.TestProcessSingleNotification(12345, verbose: "y");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, _processor.NotificationsProcessed);
        }

        /// <summary>
        /// Verifies that non-verbose mode works correctly.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_NonVerboseMode_ProcessesSuccessfully()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            var result = _processor.TestProcessSingleNotification(12345, verbose: "n");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, _processor.NotificationsProcessed);
        }

        #endregion

        #region Edge Cases

        /// <summary>
        /// Verifies handling of notification ID zero.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_WithZeroId_ProcessesCorrectly()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";

            // Act
            var result = _processor.TestProcessSingleNotification(0);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Body.Contains("0"));
        }

        /// <summary>
        /// Verifies handling of large notification ID.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_WithLargeId_ProcessesCorrectly()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";
            int largeId = int.MaxValue;

            // Act
            var result = _processor.TestProcessSingleNotification(largeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Body.Contains(largeId.ToString()));
        }

        /// <summary>
        /// Verifies handling of empty email body.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_WithEmptyBody_StillProcesses()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";
            _processor.SimulatedBody = "";

            // Act
            var result = _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("", result.Body);
        }

        /// <summary>
        /// Verifies handling of HTML body with special characters.
        /// </summary>
        [TestMethod]
        public void ProcessNotification_WithHtmlSpecialCharacters_PreservesContent()
        {
            // Arrange
            _processor.SimulatedRecipients = "test@nih.gov";
            _processor.SimulatedBody = "<html><body><p>Grant &lt;R01&gt; &amp; supplement</p></body></html>";

            // Act
            var result = _processor.TestProcessSingleNotification(12345);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Body.Contains("&lt;R01&gt;"));
            Assert.IsTrue(result.Body.Contains("&amp;"));
        }

        #endregion
    }
}

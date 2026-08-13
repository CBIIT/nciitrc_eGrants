using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AddSuppEmailer;
using CommonUtilties;

namespace EmailHandlingTests.Shared
{
    /// <summary>
    /// Test processor that extends AddSuppEmailer.Processor to intercept email sending
    /// and capture email details for test verification.
    /// </summary>
    internal class TestAddSuppProcessor : Processor
    {
        /// <summary>
        /// Tracks all emails that would have been sent during the test session.
        /// </summary>
        public List<TestEmailRecord> EmailsSentThisSession { get; } = new List<TestEmailRecord>();

        /// <summary>
        /// Count of notifications processed during the test.
        /// </summary>
        public int NotificationsProcessed { get; private set; } = 0;

        /// <summary>
        /// Indicates if an error occurred during processing.
        /// </summary>
        public bool ErrorOccurred { get; private set; } = false;

        /// <summary>
        /// Error message if an error occurred.
        /// </summary>
        public string LastErrorMessage { get; private set; } = null;

        /// <summary>
        /// Simulated email subject for testing (when not using database).
        /// </summary>
        public string SimulatedSubject { get; set; } = null;

        /// <summary>
        /// Simulated email body for testing (when not using database).
        /// </summary>
        public string SimulatedBody { get; set; } = null;

        /// <summary>
        /// Simulated email recipients for testing (when not using database).
        /// </summary>
        public string SimulatedRecipients { get; set; } = null;

        /// <summary>
        /// Overrides ProcessNotification to capture email details instead of actually sending.
        /// Uses late-bound COM (dynamic) to match the base class signature.
        /// </summary>
        protected override void ProcessNotification(SqlConnection con,
            int notifId, string verbose, string logDir, string debugEmail,
            string additionalCcRecipients, string errorToRecipients, string errorCcRecipients,
            ref int suppMailsSent)
        {
            try
            {
                NotificationsProcessed++;
                CommonUtilities.ShowDiagnosticIfVerbose($"TEST: Processing notification ID: {notifId}", verbose);

                // Capture the email details without actually sending
                var record = new TestEmailRecord
                {
                    To = SimulatedRecipients ?? "test@nih.gov",
                    Subject = SimulatedSubject ?? $"Notification {notifId}",
                    Body = SimulatedBody ?? $"Notification Id={notifId}",
                    VotingOptions = "Accepted;Rejected",
                    Importance = "High",
                    BodyFormat = "HTML",
                    TimeCaptured = DateTime.Now
                };

                EmailsSentThisSession.Add(record);
                suppMailsSent++;
            }
            catch (Exception ex)
            {
                ErrorOccurred = true;
                LastErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Test method to process a single notification without database access.
        /// </summary>
        public TestEmailRecord TestProcessSingleNotification(int notifId, string verbose = "n")
        {
            try
            {
                int mailsSent = 0;
                // Pass null for outlookApp since we override ProcessNotification and don't use it
                ProcessNotification(null, notifId, verbose, "", "test@nih.gov", "", "", "", ref mailsSent);

                return EmailsSentThisSession.Count > 0
                    ? EmailsSentThisSession[EmailsSentThisSession.Count - 1]
                    : null;
            }
            catch (Exception ex)
            {
                ErrorOccurred = true;
                LastErrorMessage = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Clears all recorded emails and resets counters.
        /// </summary>
        public void Reset()
        {
            EmailsSentThisSession.Clear();
            NotificationsProcessed = 0;
            ErrorOccurred = false;
            LastErrorMessage = null;
            SimulatedSubject = null;
            SimulatedBody = null;
            SimulatedRecipients = null;
        }
    }

    /// <summary>
    /// Record class to store captured email details during testing.
    /// </summary>
    public class TestEmailRecord
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string VotingOptions { get; set; }
        public string Importance { get; set; }
        public string BodyFormat { get; set; }
        public DateTime TimeCaptured { get; set; }
    }
}

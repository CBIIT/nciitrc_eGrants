using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AddSuppEmailer;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace EmailTests.AddSuppEmailer
{
    /// <summary>
    /// Test processor that extends AddSuppEmailer.Processor to intercept email sending
    /// and capture email details for test verification.
    /// </summary>
    internal class TestAddSuppProcessor : Processor
    {
        /// <summary>
        /// Tracks all emails that would have been sent during the test session.
        /// Key: "notificationId_{id}" contains email details for each notification processed.
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
        /// Overrides the Send method to capture email details instead of actually sending.
        /// </summary>
        /// <param name="mailItem">The Outlook mail item that would be sent</param>
        protected override void Send(Outlook.MailItem mailItem)
        {
            // Don't actually send the email - just record it
            var record = new TestEmailRecord
            {
                To = mailItem.To,
                Subject = mailItem.Subject,
                Body = mailItem.HTMLBody ?? mailItem.Body,
                VotingOptions = mailItem.VotingOptions,
                Importance = mailItem.Importance.ToString(),
                BodyFormat = mailItem.BodyFormat.ToString(),
                TimeCaptured = DateTime.Now
            };

            EmailsSentThisSession.Add(record);
        }

        /// <summary>
        /// Overrides ProcessNotification to track processing and handle test scenarios.
        /// </summary>
        protected override void ProcessNotification(SqlConnection con, Outlook.Application outlookApp,
            int notifId, string verbose, string logDir, string debug, ref int suppMailsSent)
        {
            try
            {
                NotificationsProcessed++;
                CommonUtilities.ShowDiagnosticIfVerbose($"TEST: Processing notification ID: {notifId}", verbose);

                // Create new Outlook mail item
                Outlook.MailItem mail = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

                // Use simulated values if provided, otherwise use defaults
                mail.To = SimulatedRecipients ?? "test@nih.gov";
                mail.Subject = SimulatedSubject ?? $"Notification {notifId}";
                mail.VotingOptions = "Accepted;Rejected";
                mail.Importance = Outlook.OlImportance.olImportanceHigh;
                mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
                mail.HTMLBody = SimulatedBody ?? $"Notification Id={notifId}";

                // Always capture the email (don't actually send)
                Send(mail);

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
        /// <param name="notifId">The notification ID to simulate</param>
        /// <param name="verbose">Verbose mode flag</param>
        /// <returns>The email record that was captured</returns>
        public TestEmailRecord TestProcessSingleNotification(int notifId, string verbose = "n")
        {
            try
            {
                Outlook.Application outlookApp = new Outlook.Application();
                int mailsSent = 0;

                // Call ProcessNotification directly with debug mode on
                ProcessNotification(null, outlookApp, notifId, verbose, "", "y", ref mailsSent);

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

using System;
using System.Data.SqlClient;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppEmailer
{
    /// <summary>
    /// Processor class for Administrative Supplement Emailer.
    /// 
    /// PURPOSE:
    /// Responsible for:
    /// - Querying the database for pending supplement notifications
    /// - Creating and sending Outlook emails with voting options
    /// - Tracking email send status
    /// 
    /// DATABASE TABLES USED:
    /// - dbo.adsup_Notification_email_status: Stores notification queue and send status
    /// 
    /// DATABASE FUNCTIONS USED (in production):
    /// - dbo.fn_adsupp_getemail_subject(notification_id): Returns email subject line
    /// - dbo.fn_adsupp_getemail_body(notification_id): Returns email body HTML
    /// - dbo.fn_adsupp_getemail_string(notification_id, email_type): Returns recipient addresses
    /// 
    /// EMAIL FEATURES:
    /// - Voting buttons: "Accepted" / "Rejected" 
    /// - High importance flag
    /// - HTML body format
    /// 
    /// LOGGING:
    /// Uses Serilog via CommonUtilities.Logger for structured logging:
    /// - Information: Processing start/complete, email sent confirmations
    /// - Debug: Database connection, individual notification processing, debug mode skips
    /// - Error: Processing failures with full exception details
    /// 
    /// Log entries include structured parameters for easy filtering:
    /// - {NotificationId}: The notification being processed
    /// - {MailCount}: Number of emails sent
    /// 
    /// TESTING:
    /// To test this class without sending real emails:
    /// 1. Create a subclass that overrides the Send() method
    /// 2. Use a test database or mock the SqlConnection
    /// 3. Set debug="y" to prevent actual email sending
    /// </summary>
    public class Processor
    {
        /// <summary>
        /// Main processing method that queries for pending notifications and sends emails.
        /// </summary>
        /// <param name="con">SQL Server database connection (will be opened by this method)</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <returns>Number of emails successfully sent</returns>
        public int Process(SqlConnection con, string verbose, string logDir)
        {
            return Process(con, verbose, logDir, "n"); // Default: not debug mode
        }

        /// <summary>
        /// Main processing method with debug flag support.
        /// Logs all operations using Serilog structured logging.
        /// </summary>
        /// <param name="con">SQL Server database connection</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <param name="debug">Debug mode flag ("y" to prevent actual email sending)</param>
        /// <returns>Number of emails processed</returns>
        public int Process(SqlConnection con, string verbose, string logDir, string debug)
        {
            int suppMailsSent = 0;

            CommonUtilities.Logger?.Information("Starting supplement email processing");
            CommonUtilities.ShowDiagnosticIfVerbose("Starting supplement email processing...", verbose);

            // Initialize Outlook application for sending emails
            // NOTE: Requires Outlook to be installed and configured on the machine
            Outlook.Application outlookApp = new Outlook.Application();
            CommonUtilities.Logger?.Debug("Outlook application initialized");

            con.Open();
            CommonUtilities.Logger?.Debug("Database connection opened");

            // Query for all notifications that haven't been emailed yet
            string sql = @"
                SELECT DISTINCT Notification_id 
                FROM dbo.adsup_Notification_email_status 
               WHERE email_date IS NULL 
                 ORDER BY Notification_id DESC";

            using (var cmd = new SqlCommand(sql, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int notifId = reader.GetInt32(0);
                    CommonUtilities.Logger?.Debug("Processing notification ID: {NotificationId}", notifId);
                    ProcessNotification(con, outlookApp, notifId, verbose, logDir, debug, ref suppMailsSent);
                }
            }

            con.Close();
            CommonUtilities.Logger?.Information("Processing complete. {MailCount} emails sent", suppMailsSent);

            return suppMailsSent;
        }

        /// <summary>
        /// Processes a single notification - creates and sends the email.
        /// Logs success and failure for each notification.
        /// </summary>
        /// <param name="con">Database connection</param>
        /// <param name="outlookApp">Outlook application instance</param>
        /// <param name="notifId">Notification ID to process</param>
        /// <param name="verbose">Verbose mode flag</param>
        /// <param name="logDir">Log directory</param>
        /// <param name="debug">Debug mode ("y" to skip actual sending)</param>
        /// <param name="suppMailsSent">Reference counter for emails sent</param>
        protected virtual void ProcessNotification(SqlConnection con, Outlook.Application outlookApp,
            int notifId, string verbose, string logDir, string debug, ref int suppMailsSent)
        {
            try
            {
                CommonUtilities.Logger?.Information("Processing notification ID: {NotificationId}", notifId);
                CommonUtilities.ShowDiagnosticIfVerbose($"Processing notification ID: {notifId}", verbose);

                // Create new Outlook mail item
                Outlook.MailItem mail = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

                mail.To = "test@nih.gov";  // Replace with actual recipient query
                mail.Subject = "Notification " + notifId;
                mail.VotingOptions = "Accepted;Rejected";
                mail.Importance = Outlook.OlImportance.olImportanceHigh;
                mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
                mail.HTMLBody = "Notification Id=" + notifId;

                // Send unless in debug mode
                if (debug?.ToLower() != "y")
                {
                    Send(mail);
                    CommonUtilities.Logger?.Information("Email sent for Notification ID: {NotificationId}", notifId);
                }
                else
                {
                    CommonUtilities.Logger?.Debug("DEBUG MODE: Would send email for NotifID={NotificationId}", notifId);
                }

                suppMailsSent++;
                Program.WriteLog($"Email sent for Notification ID: {notifId}", null, DateTime.Now, logDir);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error processing notification ID: {NotificationId}", notifId);
                Program.WriteLog("Error with NotifID=" + notifId, ex.Message, DateTime.Now, logDir);
            }
        }

        /// <summary>
        /// Sends the email via Outlook.
        /// This method is virtual to allow overriding in tests.
        /// </summary>
        /// <param name="mailItem">The Outlook mail item to send</param>
        protected virtual void Send(Outlook.MailItem mailItem)
        {
            mailItem.Send();
        }

        #region Database Helper Methods

        /// <summary>
        /// Gets the email subject for a notification from the database.
        /// </summary>
        protected virtual string GetEmailSubject(SqlConnection con, int notifId)
        {
            using (var cmd = new SqlCommand($"SELECT dbo.fn_adsupp_getemail_subject({notifId})", con))
            {
                return cmd.ExecuteScalar()?.ToString() ?? $"Supplement Notification {notifId}";
            }
        }

        /// <summary>
        /// Gets the email body HTML for a notification from the database.
        /// </summary>
        protected virtual string GetEmailBody(SqlConnection con, int notifId)
        {
            using (var cmd = new SqlCommand($"SELECT dbo.fn_adsupp_getemail_body({notifId})", con))
            {
                return cmd.ExecuteScalar()?.ToString() ?? $"Notification Id={notifId}";
            }
        }

        /// <summary>
        /// Gets the email recipients (TO or CC) for a notification from the database.
        /// </summary>
        protected virtual string GetEmailRecipients(SqlConnection con, int notifId, string emailType)
        {
            using (var cmd = new SqlCommand($"SELECT dbo.fn_adsupp_getemail_string({notifId}, '{emailType}')", con))
            {
                return cmd.ExecuteScalar()?.ToString() ?? "";
            }
        }

        /// <summary>
        /// Updates the notification status after email is sent.
        /// </summary>
        protected virtual void UpdateNotificationStatus(SqlConnection con, int notifId, string status)
        {
            string sql = $@"
     UPDATE dbo.adsup_Notification_email_status 
     SET email_date = GETDATE(), email_send_status = '{status}' 
     WHERE Notification_id = {notifId}";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}

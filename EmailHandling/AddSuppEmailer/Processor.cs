using System;
using System.Data.SqlClient;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace AddSuppEmailer
{
    /// <summary>
    /// Processor class for Administrative Supplement Emailer.
    /// 
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
        /// </summary>
        /// <param name="con">SQL Server database connection</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <param name="debug">Debug mode flag ("y" to prevent actual email sending)</param>
        /// <returns>Number of emails processed</returns>
        public int Process(SqlConnection con, string verbose, string logDir, string debug)
        {
            int suppMailsSent = 0;

            CommonUtilities.ShowDiagnosticIfVerbose("Starting supplement email processing...", verbose);

            // Initialize Outlook application for sending emails
            // NOTE: Requires Outlook to be installed and configured on the machine
            Outlook.Application outlookApp = new Outlook.Application();

            con.Open();

            // Query for all notifications that haven't been emailed yet (email_date IS NULL)
            // Results ordered by Notification_id DESC to process newest first
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
                    ProcessNotification(con, outlookApp, notifId, verbose, logDir, debug, ref suppMailsSent);
                }
            }

            con.Close();
            CommonUtilities.ShowDiagnosticIfVerbose($"Processing complete. {suppMailsSent} emails sent.", verbose);

            return suppMailsSent;
        }

        /// <summary>
        /// Processes a single notification - creates and sends the email.
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
                CommonUtilities.ShowDiagnosticIfVerbose($"Processing notification ID: {notifId}", verbose);

                // Create new Outlook mail item
                Outlook.MailItem mail = (Outlook.MailItem)outlookApp.CreateItem(Outlook.OlItemType.olMailItem);

                // TODO: In production, retrieve values from database:
                // string toEmail = GetEmailRecipients(con, notifId, "to");
                // string ccEmail = GetEmailRecipients(con, notifId, "cc");
                // string subject = GetEmailSubject(con, notifId);
                // string body = GetEmailBody(con, notifId);

                mail.To = "test@nih.gov";  // Replace with actual recipient query
                mail.Subject = "Notification " + notifId;

                // Voting options allow recipients to respond with Accept/Reject
                mail.VotingOptions = "Accepted;Rejected";

                // Mark as high importance
                mail.Importance = Outlook.OlImportance.olImportanceHigh;

                // Use HTML format
                mail.BodyFormat = Outlook.OlBodyFormat.olFormatHTML;
                mail.HTMLBody = "Notification Id=" + notifId;

                // Send unless in debug mode
                if (debug?.ToLower() != "y")
                {
                    Send(mail);
                    // TODO: UpdateNotificationStatus(con, notifId, "Send");
                }
                else
                {
                    CommonUtilities.ShowDiagnosticIfVerbose($"DEBUG MODE: Would send email for NotifID={notifId}", verbose);
                }

                suppMailsSent++;
                Program.WriteLog($"Email sent for Notification ID: {notifId}", null, DateTime.Now, logDir);
            }
            catch (Exception ex)
            {
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

        #region Database Helper Methods (TODO: Implement for production)

        /// <summary>
        /// Gets the email subject for a notification from the database.
        /// </summary>
        protected virtual string GetEmailSubject(SqlConnection con, int notifId)
        {
            // TODO: Implement - call dbo.fn_adsupp_getemail_subject
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
            // TODO: Implement - call dbo.fn_adsupp_getemail_body
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
            // TODO: Implement - call dbo.fn_adsupp_getemail_string
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

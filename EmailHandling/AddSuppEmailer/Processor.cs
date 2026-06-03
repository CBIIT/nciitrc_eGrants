using System;
using System.Data.SqlClient;
using CommonUtilties;

namespace AddSuppEmailer
{
    /// <summary>
    /// Processor class for Administrative Supplement Emailer.
    /// 
    /// PURPOSE:
    /// Responsible for:
    /// - Querying the database for pending supplement notifications
    /// - Creating and sending Outlook emails with voting options via late-bound COM
    /// - Tracking email send status
    /// 
    /// OUTLOOK INTEGRATION:
    /// Uses late-bound COM automation (dynamic/Activator) to control Outlook.
    /// No Primary Interop Assembly (PIA) or NuGet interop package is required at compile time.
    /// Outlook must be installed and configured on the machine where this runs.
    /// 
    /// DATABASE TABLES USED:
    /// - dbo.adsup_Notification_email_status: Stores notification queue and send status
    /// 
    /// DATABASE FUNCTIONS USED:
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
    /// - Debug: Outlook initialization, individual notification processing, debug mode skips
    /// - Error: Processing failures with full exception details
    /// 
    /// TESTING:
    /// To test this class without sending real emails:
    /// 1. Set debug="y" to prevent actual email sending
    /// 2. Use a test database with test notification records
    /// 3. Override ProcessNotification in a subclass for unit testing
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
            return Process(con, verbose, logDir, "n");
        }

        /// <summary>
        /// Main processing method with debug flag support.
        /// Creates an Outlook COM instance via late binding and processes all pending notifications.
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

            // Connect to existing Outlook instance (matches VBS GetObject behavior)
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
            {
                CommonUtilities.Logger?.Error("Outlook is not installed or not registered on this machine");
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");
            }

            dynamic outlookApp = GetRunningOutlook() ?? Activator.CreateInstance(outlookType);

            con.Open();
            CommonUtilities.Logger?.Debug("Database connection opened");

            string sql = @"
                SELECT DISTINCT Notification_id 
                FROM dbo.adsup_Notification_email_status 
                WHERE email_date IS NULL 
                ORDER BY Notification_id DESC";

            // Read all notification IDs into a list first to free the DataReader
            var notificationIds = new System.Collections.Generic.List<int>();
            using (var cmd = new SqlCommand(sql, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    notificationIds.Add(reader.GetInt32(0));
                }
            }

            // Now process each notification (DataReader is closed)
            foreach (var notifId in notificationIds)
            {
                CommonUtilities.Logger?.Debug("Processing notification ID: {NotificationId}", notifId);
                ProcessNotification(con, outlookApp, notifId, verbose, logDir, debug, ref suppMailsSent);
            }

            con.Close();
            CommonUtilities.Logger?.Information("Processing complete. {MailCount} emails sent", suppMailsSent);

            return suppMailsSent;
        }

        /// <summary>
        /// Processes a single notification - creates and sends the email via Outlook COM late binding.
        /// 
        /// Outlook constants used:
        /// - CreateItem(0) = olMailItem
        /// - Importance = 2 = olImportanceHigh
        /// - BodyFormat = 2 = olFormatHTML
        /// </summary>
        /// <param name="con">Database connection</param>
        /// <param name="outlookApp">Outlook COM application instance (dynamic/late-bound)</param>
        /// <param name="notifId">Notification ID to process</param>
        /// <param name="verbose">Verbose mode flag</param>
        /// <param name="logDir">Log directory</param>
        /// <param name="debug">Debug mode ("y" to skip actual sending)</param>
        /// <param name="suppMailsSent">Reference counter for emails sent</param>
        protected virtual void ProcessNotification(SqlConnection con, dynamic outlookApp,
            int notifId, string verbose, string logDir, string debug, ref int suppMailsSent)
        {
            try
            {
                CommonUtilities.Logger?.Information("Processing notification ID: {NotificationId}", notifId);
                CommonUtilities.ShowDiagnosticIfVerbose($"Processing notification ID: {notifId}", verbose);

                var subject = GetEmailSubject(con, notifId);
                var body = GetEmailBody(con, notifId);
#if DEBUG
                var toRecipients = "daryl.dehuff@nih.gov";
#else
                var toRecipients = GetEmailRecipients(con, notifId, "TO");
#endif
                var ccRecipients = GetEmailRecipients(con, notifId, "CC");

                // Create mail item via late-bound COM: 0 = olMailItem
                dynamic mail = outlookApp.CreateItem(0);
                mail.To = toRecipients;
                mail.CC = ccRecipients;
                mail.Subject = subject;
                mail.VotingOptions = "Accepted;Rejected";
                mail.Importance = 2; // olImportanceHigh
                mail.BodyFormat = 2; // olFormatHTML
                mail.HTMLBody = body + "Notification Id=" + notifId;

                if (debug?.ToLower() != "y")
                {
                    mail.Send();
                    UpdateNotificationStatus(con, notifId, "sent");
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

        #region Database Helper Methods

        /// <summary>
        /// Gets the email subject for a notification from the database.
        /// Calls: SELECT dbo.fn_adsupp_getemail_subject({notifId})
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
        /// Calls: SELECT dbo.fn_adsupp_getemail_body({notifId})
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
        /// Calls: SELECT dbo.fn_adsupp_getemail_string({notifId}, '{emailType}')
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
        /// Sets email_date to current time and email_send_status to the provided status.
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

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] Guid clsid,
            IntPtr reserved,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object obj);

        private static dynamic GetRunningOutlook()
        {
            try
            {
                var clsid = new Guid("0006F03A-0000-0000-C000-000000000046"); // Outlook.Application CLSID
                GetActiveObject(clsid, IntPtr.Zero, out object obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }
    }
}

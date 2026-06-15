using System;
using System.Data.SqlClient;
using CommonUtilties;
using System.IO;

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
        private bool _firstEmailSentInDevMode = false;

        /// <summary>
        /// Main processing method that queries for pending notifications and sends emails.
        /// Uses DOTNET_ENVIRONMENT to determine if running in development mode.
        /// In development mode: sends first email, logs all others.
        /// </summary>
        /// <param name="con">SQL Server database connection (will be opened by this method)</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <param name="debugEmail">Email address to use when in development environment</param>
        /// <returns>Number of emails successfully sent</returns>
        public int Process(SqlConnection con, string verbose, string logDir, string debugEmail)
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

            // Validate Outlook session
            try
            {
                dynamic session = outlookApp.Session;
                if (session == null)
                {
                    throw new InvalidOperationException("Outlook session is not available");
                }
                CommonUtilities.Logger?.Debug("Outlook session validated");
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Failed to validate Outlook session");
                throw new InvalidOperationException("Outlook is not properly configured or logged in", ex);
            }

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
                ProcessNotification(con, outlookApp, notifId, verbose, logDir, debugEmail, ref suppMailsSent);
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
        /// <param name="debugEmail">Email address to use when in development environment</param>
        /// <param name="suppMailsSent">Reference counter for emails sent</param>
        protected virtual void ProcessNotification(SqlConnection con, dynamic outlookApp,
            int notifId, string verbose, string logDir, string debugEmail, ref int suppMailsSent)
        {
            try
            {
                CommonUtilities.Logger?.Information("Processing notification ID: {NotificationId}", notifId);
                CommonUtilities.ShowDiagnosticIfVerbose($"Processing notification ID: {notifId}", verbose);

                var subject = GetEmailSubject(con, notifId);
                var body = GetEmailBody(con, notifId);
                var toRecipients = IsDevEnvironment()
                    ? debugEmail
                    : GetEmailRecipients(con, notifId, "TO");

                // In development mode, don't include CC recipients
                var ccRecipients = IsDevEnvironment() 
                    ? null 
                    : GetEmailRecipients(con, notifId, "CC");

                // Validate recipients
                if (string.IsNullOrWhiteSpace(toRecipients))
                {
                    CommonUtilities.Logger?.Warning("No TO recipients for notification {NotificationId}, skipping", notifId);
                    return;
                }

                CommonUtilities.Logger?.Debug("Creating mail item for NotifID={NotificationId}", notifId);

                // Create mail item via late-bound COM: 0 = olMailItem
                dynamic mail = null;
                try
                {
                    mail = outlookApp.CreateItem(0);

                    // Set all properties before sending
                    mail.To = toRecipients;
                    if (!string.IsNullOrWhiteSpace(ccRecipients))
                    {
                        mail.CC = ccRecipients;
                    }

                    // In development mode, prefix subject with [TEST]
                    mail.Subject = IsDevEnvironment() ? $"[TEST] {subject}" : subject;

                    mail.VotingOptions = "Accepted;Rejected";
                    mail.Importance = 2; // olImportanceHigh
                    mail.BodyFormat = 2; // olFormatHTML
                    mail.HTMLBody = body + "Notification Id=" + notifId;

                    // In development mode: send FIRST email, log all others
                    if (IsDevEnvironment())
                    {
                        if (!_firstEmailSentInDevMode)
                        {
                            // Send the FIRST email in development mode
                            CommonUtilities.Logger?.Information("DEVELOPMENT MODE - Sending FIRST email as test");
                            CommonUtilities.Logger?.Information("NotificationId: {NotificationId}", notifId);
                            CommonUtilities.Logger?.Information("To: {To}", toRecipients);
                            CommonUtilities.Logger?.Information("CC: {CC}", ccRecipients ?? "(none)");
                            CommonUtilities.Logger?.Information("Subject: {Subject}", mail.Subject);

                            mail.Send();
                            UpdateNotificationStatus(con, notifId, "sent");
                            _firstEmailSentInDevMode = true;

                            CommonUtilities.Logger?.Information("✓ First email SENT for Notification ID: {NotificationId}", notifId);
                            Program.WriteLog($"DEV MODE: First email sent for Notification ID: {notifId}", null, DateTime.Now, logDir);
                        }
                        else
                        {
                            // Log all SUBSEQUENT emails without sending
                            CommonUtilities.Logger?.Information("DEVELOPMENT MODE - Email #{Count} NOT sent (logged only)", suppMailsSent + 1);
                            CommonUtilities.Logger?.Information("NotificationId: {NotificationId}", notifId);
                            CommonUtilities.Logger?.Information("To: {To}", toRecipients);
                            CommonUtilities.Logger?.Information("CC: {CC}", ccRecipients ?? "(none)");
                            CommonUtilities.Logger?.Information("Subject: {Subject}", mail.Subject);
                            CommonUtilities.Logger?.Debug("Body length: {BodyLength} characters", body?.Length ?? 0);

                            // Do NOT send the email
                            // Do NOT update notification status
                            CommonUtilities.ShowDiagnosticIfVerbose($"DEV MODE: Would send email for NotifID={notifId}", verbose);
                        }
                    }
                    else
                    {
                        // Production mode - actually send all emails
                        CommonUtilities.Logger?.Debug("Sending mail for NotifID={NotificationId} to {Recipients}", 
                            notifId, toRecipients);

                        mail.Send();
                        UpdateNotificationStatus(con, notifId, "sent");
                        CommonUtilities.Logger?.Information("Email sent for Notification ID: {NotificationId}", notifId);
                        Program.WriteLog($"Email sent for Notification ID: {notifId}", null, DateTime.Now, logDir);
                    }

                    suppMailsSent++;
                }
                finally
                {
                    // Release COM object
                    if (mail != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(mail);
                        mail = null;
                    }
                }
            }
            catch (System.Runtime.InteropServices.COMException comEx)
            {
                CommonUtilities.Logger?.Error(comEx, "COM error processing notification ID: {NotificationId}. HRESULT: {HResult}", 
                    notifId, comEx.HResult);
                Program.WriteLog($"COM Error with NotifID={notifId}", 
                    $"HRESULT: {comEx.HResult:X}, Message: {comEx.Message}", DateTime.Now, logDir);
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
            string sql = @"
                UPDATE dbo.adsup_Notification_email_status 
                SET email_date = GETDATE(), email_send_status = @Status 
                WHERE Notification_id = @NotifId";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@NotifId", notifId);
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

        /// <summary>
        /// Checks if the current environment is a development environment.
        /// Looks for DOTNET_ENVIRONMENT variable set to "Development".
        /// </summary>
        /// <returns>True if running in development environment, false otherwise</returns>
        private bool IsDevEnvironment()
        {
            string dotNetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            return string.Equals(dotNetEnv?.Trim(), "Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}

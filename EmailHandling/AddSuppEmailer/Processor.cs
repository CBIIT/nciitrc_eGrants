using System;
using System.Data.SqlClient;
using System.Net.Mail;
using CommonUtilties;
using System.IO;
using Microsoft.Extensions.Configuration;

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
        private SmtpEmailService _smtpService;

        /// <summary>
        /// Main processing method that queries for pending notifications and sends emails.
        /// Uses DOTNET_ENVIRONMENT to determine if running in development mode.
        /// In development mode: sends all emails to the debug recipient.
        /// </summary>
        /// <param name="con">SQL Server database connection (will be opened by this method)</param>
        /// <param name="verbose">Verbose mode flag ("y" for diagnostic output)</param>
        /// <param name="logDir">Directory for log files</param>
        /// <param name="debugEmail">Email address to use when in development environment</param>
        /// <param name="additionalCcRecipients">Additional CC recipients appended to every email</param>
        /// <param name="errorToRecipients">TO recipients for error notification emails when PD address is missing</param>
        /// <param name="errorCcRecipients">CC recipients for error notification emails</param>
        /// <param name="config">Configuration for SMTP settings</param>
        /// <returns>Number of emails successfully sent</returns>
        public int Process(SqlConnection con, string verbose, string logDir, string debugEmail,
            string additionalCcRecipients, string errorToRecipients, string errorCcRecipients,
            IConfiguration config = null)
        {
            int suppMailsSent = 0;

            CommonUtilities.Logger?.Information("Starting supplement email processing");
            CommonUtilities.ShowDiagnosticIfVerbose("Starting supplement email processing...", verbose);

            _smtpService = new SmtpEmailService(config ?? AppConfig.Load());

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
                ProcessNotification(con, notifId, verbose, logDir, debugEmail,
                    additionalCcRecipients, errorToRecipients, errorCcRecipients, ref suppMailsSent);
            }

            con.Close();
            CommonUtilities.Logger?.Information("Processing complete. {MailCount} emails sent", suppMailsSent);

            return suppMailsSent;
        }

        /// <summary>
        /// Processes a single notification - creates and sends the email via Outlook COM late binding.
        /// If TO recipients are empty, sends an error notification email to admin and marks as 'NtSend'.
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
        /// <param name="additionalCcRecipients">Additional CC recipients appended to every email</param>
        /// <param name="errorToRecipients">TO recipients for error notification emails</param>
        /// <param name="errorCcRecipients">CC recipients for error notification emails</param>
        /// <param name="suppMailsSent">Reference counter for emails sent</param>
        protected virtual void ProcessNotification(SqlConnection con,
            int notifId, string verbose, string logDir, string debugEmail,
            string additionalCcRecipients, string errorToRecipients, string errorCcRecipients,
            ref int suppMailsSent)
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

                // If no TO recipients found, send error notification email (matches VBS behavior)
                if (!IsDevEnvironment() && string.IsNullOrWhiteSpace(toRecipients))
                {
                    CommonUtilities.Logger?.Warning("No TO recipients for notification {NotificationId}, sending error email", notifId);
                    SendMissingRecipientErrorEmail(con, notifId, subject,
                        errorToRecipients, errorCcRecipients, logDir);
                    suppMailsSent++;
                    return;
                }

                // Append additional CC recipients (matches VBS: .CC=ccemailstr & "; additional@...")
                if (!IsDevEnvironment() && !string.IsNullOrWhiteSpace(additionalCcRecipients))
                {
                    ccRecipients = string.IsNullOrWhiteSpace(ccRecipients)
                        ? additionalCcRecipients
                        : ccRecipients + ";" + additionalCcRecipients;
                }

                CommonUtilities.Logger?.Debug("Creating email for NotifID={NotificationId}", notifId);

                var fullSubject = GetEnvironmentPrefix() + subject;
                var fullBody = body + "Notification Id=" + notifId;

                if (IsDevEnvironment())
                {
                    CommonUtilities.Logger?.Information("DEVELOPMENT MODE - Sending email");
                    CommonUtilities.Logger?.Information("NotificationId: {NotificationId}", notifId);
                    CommonUtilities.Logger?.Information("To: {To}", toRecipients);
                    CommonUtilities.Logger?.Information("CC: {CC}", ccRecipients ?? "(none)");
                    CommonUtilities.Logger?.Information("Subject: {Subject}", fullSubject);
                }
                else
                {
                    CommonUtilities.Logger?.Debug("Sending mail for NotifID={NotificationId} to {Recipients}", 
                        notifId, toRecipients);
                }

                _smtpService.SendEmailWithVoting(toRecipients, fullSubject, fullBody,
                    "Accepted;Rejected", ccRecipients, MailPriority.High);

                UpdateNotificationStatus(con, notifId, "Send");
                CommonUtilities.Logger?.Information("Email sent for Notification ID: {NotificationId}", notifId);
                Program.WriteLog($"Processed! => Notification_ID: {notifId}; Sent to: {toRecipients}; Subject: {subject}", null, DateTime.Now, logDir);

                suppMailsSent++;
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Error(ex, "Error processing notification ID: {NotificationId}", notifId);
                Program.WriteLog($"Error Occured! => with Notification_ID: {notifId}",
                    $"Error Description: {ex.Message}, Error Source: {ex.Source}", DateTime.Now, logDir);
            }
        }

        /// <summary>
        /// Sends an error notification email when no TO recipients are found for a notification.
        /// Matches VBS behavior: sends to admin recipients with "ERROR Refering : " subject prefix,
        /// explains that the PD email address could not be found in GPMATS,
        /// and marks the notification status as 'NtSend'.
        /// </summary>
        private void SendMissingRecipientErrorEmail(SqlConnection con,
            int notifId, string subject, string errorToRecipients, string errorCcRecipients, string logDir)
        {
            var errorSubject = GetEnvironmentPrefix() + "ERROR Refering : " + subject;
            var errorBody = "ERROR : Some how Admin Suplement Automated WorkFlow emailer system could not find PD email address in GPMATS as main recipient of Grant Number mention in subject. Email could not be sent for Notification_id = " + notifId;

            _smtpService.SendEmail(errorToRecipients, errorSubject, errorBody, errorCcRecipients, MailPriority.High);
            UpdateNotificationStatus(con, notifId, "NtSend");

            CommonUtilities.Logger?.Warning("Error email sent for Notification ID: {NotificationId} - no TO recipients found", notifId);
            Program.WriteLog($"ERROR: No TO recipients for Notification_ID: {notifId}; Error email sent to: {errorToRecipients}", null, DateTime.Now, logDir);
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
        /// Status values: 'Send' (successfully sent), 'NtSend' (error - not sent to intended recipient)
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

        /// <summary>
        /// Returns the environment name in parentheses (e.g. "(Development) ") if not Production.
        /// Returns empty string for Production or if DOTNET_ENVIRONMENT is not set.
        /// </summary>
        private static string GetEnvironmentPrefix()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return $"({env}) ";
        }
    }
}

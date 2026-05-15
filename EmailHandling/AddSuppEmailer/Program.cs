using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace AddSuppEmailer
{
    /// <summary>
    /// AddSuppEmailer - Administrative Supplement Emailer
    /// 
    /// PURPOSE:
    /// This application sends email notifications to Program Directors (PDs) for 
    /// administrative supplement requests. It queries a database for pending notifications
    /// and sends Outlook emails with voting options (Accepted/Rejected).
    /// 
    /// ORIGINAL SOURCE: Migrated from Add_Supp_Emailer.vbs
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from config.csv (connection string, log directory, verbose mode)
    /// 2. Initializes Serilog logging to daily rolling log files
    /// 3. Connects to the EIM database
    /// 4. Queries adsup_Notification_email_status table for unsent notifications
    /// 5. For each notification, creates and sends an Outlook email with voting buttons
    /// 6. Updates the notification status in the database
    /// 7. Logs all activity using Serilog structured logging
    /// 
    /// LOGGING:
    /// Uses Serilog for structured logging with:
    /// - Daily rolling log files: AddSuppEmailer-{date}.log
    /// - Console output with timestamps
    /// - Structured parameters: {NotificationId}, {MailCount}, etc.
    /// - 31-day log retention, 10MB file size limit
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for config reading and Serilog logging)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - SQL Server database access to EIM database
    /// - config.csv file with: logDir, conStr, Verbose settings
    /// 
    /// SCHEDULED TASK:
    /// This is typically run as a Windows Scheduled Task to process pending notifications.
    /// </summary>
    internal class Program
    {
        private const string ApplicationName = "AddSuppEmailer";

        /// <summary>
        /// Main entry point for the AddSuppEmailer application.
        /// Initializes Serilog logging, creates database connection, and invokes the processor.
        /// 
        /// Logging is initialized at the start and properly closed in a finally block
        /// to ensure all log entries are flushed even if an exception occurs.
        /// </summary>
        /// <param name="args">Command line arguments (not currently used)</param>
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
      // Load credentials from local secrets file (not committed to source control)
  CommonUtilities.LoadLocalSecrets("secrets.local.csv");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Emailer");

                // Load configuration values from config.csv
                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                var conStr = CommonUtilities.GetConfigVal("conStr");

                // Initialize Serilog logging - creates daily rolling log files
                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("Start Time: {StartTime}", startTimeStamp);

                // Create database connection and process notifications
                using (var con = new SqlConnection(conStr))
                {
                    CommonUtilities.Logger.Debug("Database connection string configured");

                    var processor = new Processor();
                    var mailsSent = processor.Process(con, verbose, logDir);

                    CommonUtilities.Logger.Information("Task Completed - {MailCount} emails sent", mailsSent);
                }

                CommonUtilities.Logger.Information("=== {ApplicationName} Finished ===", ApplicationName);
            }
            catch (Exception ex)
            {
                // Log fatal errors - Logger may be null if initialization failed
                CommonUtilities.Logger?.Error(ex, "Fatal error in {ApplicationName}", ApplicationName);
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Ensure all logs are flushed before exit
                CommonUtilities.CloseLogging();
            }
        }

        /// <summary>
        /// Writes a log entry to the daily log file.
        /// 
        /// This method logs to both Serilog (if initialized) and the legacy text file
        /// for backward compatibility. New code should use CommonUtilities.Logger directly.
        /// 
        /// Log file naming: Suppl-Emailer-Log-{yyyy-M-d}.txt (legacy format)
        /// Serilog file: AddSuppEmailer-{yyyy-MM-dd}.log
        /// </summary>
        /// <param name="message">The main log message</param>
        /// <param name="errorInfo">Optional error details (appended on new line if provided)</param>
        /// <param name="timeStamp">Timestamp for the log entry</param>
        /// <param name="logDir">Directory where log files are stored</param>
        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            // Use Serilog for structured logging
            if (string.IsNullOrEmpty(errorInfo))
            {
                CommonUtilities.Logger?.Information("{Message}", message);
            }
            else
            {
                CommonUtilities.Logger?.Error("{Message} - {ErrorInfo}", message, errorInfo);
            }

            // Legacy file logging for backward compatibility
            var fileName = $"Suppl-Emailer-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                 ? $"{timeStamp}-\t{message}"
           : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";

            try
            {
                File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Warning(ex, "Failed to write to legacy log file");
            }
        }
    }
}


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
    /// 2. Connects to the EIM database
    /// 3. Queries adsup_Notification_email_status table for unsent notifications
    /// 4. For each notification, creates and sends an Outlook email with voting buttons
    /// 5. Updates the notification status in the database
    /// 6. Logs all activity to a daily log file
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for config reading and logging helpers)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - SQL Server database access to EIM database
    /// - config.csv file with: logDir, conStr, Verbose settings
    /// 
    /// SCHEDULED TASK:
    /// This is typically run as a Windows Scheduled Task to process pending notifications.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point for the AddSuppEmailer application.
        /// Initializes configuration, creates database connection, and invokes the processor.
        /// </summary>
        /// <param name="args">Command line arguments (not currently used)</param>
        static void Main(string[] args)
        {
            try
            {
                var startTimeStamp = DateTime.Now;
                Console.WriteLine("AddSuppEmailer - Administrative Supplement Emailer");

                // Load configuration values from config.csv
                // Verbose: "y" or "n" - controls diagnostic output
                var verbose = CommonUtilities.GetConfigVal("Verbose");

                // logDir: Directory path where log files will be written
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;

                // conStr: SQL Server connection string for EIM database
                var conStr = CommonUtilities.GetConfigVal("conStr");

                // Log task start
                WriteLog("Task Started", null, startTimeStamp, logDir);

                // Create database connection and process notifications
                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var mailsSent = processor.Process(con, verbose, logDir);
                    WriteLog($"Task Completed - {mailsSent} emails sent", null, DateTime.Now, logDir);
                }
            }
            catch (Exception ex)
            {
                // Log fatal errors to console (log file may not be accessible)
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Writes a log entry to the daily log file.
        /// Log files are named: Suppl-Emailer-Log-{yyyy-M-d}.txt
        /// </summary>
        /// <param name="message">The main log message</param>
        /// <param name="errorInfo">Optional error details (appended on new line if provided)</param>
        /// <param name="timeStamp">Timestamp for the log entry</param>
        /// <param name="logDir">Directory where log files are stored</param>
        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = $"Suppl-Emailer-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                ? $"{timeStamp}-\t{message}"
                : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}


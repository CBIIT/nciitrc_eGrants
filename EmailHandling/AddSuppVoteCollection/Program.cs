using System;
using System.IO;
using CommonUtilties;

namespace AddSuppVoteCollection
{
    /// <summary>
    /// AddSuppVoteCollection - Administrative Supplement Vote Collection Processor
    /// 
    /// PURPOSE:
    /// This application collects voting responses (Accepted/Rejected) from supplement
    /// notification emails and forwards them to the designated OGA staff members.
    /// It monitors an Outlook folder for voting response emails and processes them.
    /// 
    /// ORIGINAL SOURCE: Migrated from VBS supplement vote collection scripts
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from config.csv (folder path, log directory, verbose mode)
    /// 2. Initializes Serilog logging to daily rolling log files
    /// 3. Connects to the configured Outlook public folder
    /// 4. Scans for emails with "Accepted:" or "Rejected:" in the subject line
    /// 5. Forwards matching emails to emily.driskell@nih.gov and jonesni@mail.nih.gov
    /// 6. Moves processed emails to the "AddSupp_Vote" archive folder
    /// 7. Logs all activity using Serilog structured logging
    /// 
    /// VOTING RESPONSES:
    /// When PDs respond to supplement notification emails using Outlook's voting
    /// buttons, the response emails have subjects like:
    /// - "Accepted: [Original Subject]"
    /// - "Rejected: [Original Subject]"
    /// 
    /// LOGGING:
    /// Uses Serilog for structured logging with:
    /// - Daily rolling log files: AddSuppVoteCollection-{date}.log
    /// - Console output with timestamps
    /// - Structured parameters for filtering (VoteType, Sender, Subject, etc.)
    /// - 31-day log retention
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for config reading and Serilog logging)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - config.csv file with: logDir, Verbose, dirpathVoteCollection
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task to collect voting responses.
    /// </summary>
    internal class Program
    {
        private const string ApplicationName = "AddSuppVoteCollection";

        /// <summary>
        /// Main entry point for the AddSuppVoteCollection application.
        /// Initializes Serilog logging, loads configuration, and invokes the processor.
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
                Console.WriteLine($"{ApplicationName} - Vote Collection Processor");

                // Load configuration values from config.csv
                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
     
                // Initialize Serilog logging - creates daily rolling log files
                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("Start Time: {StartTime}", startTimeStamp);

                // dirpathVoteCollection: Outlook folder path where voting responses arrive
                var dirPath = CommonUtilities.GetConfigVal("dirpathVoteCollection");
                CommonUtilities.Logger.Debug("Using folder path: {FolderPath}", dirPath);

                var processor = new Processor();
                var itemsProcessed = processor.Process(dirPath, verbose, logDir);

                CommonUtilities.Logger.Information("Task Completed - {ItemCount} votes processed", itemsProcessed);
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
        /// Log file naming: Supp-VoteColl-Log-{yyyy-M-d}.txt (legacy format)
        /// Serilog file: AddSuppVoteCollection-{yyyy-MM-dd}.log
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
            var fileName = $"Supp-VoteColl-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                ? $"{timeStamp}  -\t{message}"
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

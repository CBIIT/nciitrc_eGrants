using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace AddSuppProd
{
    /// <summary>
    /// AddSuppProd - Administrative Supplement Production Processor
    /// 
    /// PURPOSE:
    /// This application processes administrative supplement emails from a designated
    /// Outlook public folder and moves them to an archive folder after processing.
    /// It handles emails related to supplement requests and responses.
    /// 
    /// ORIGINAL SOURCE: Migrated from add_supp_prod.vbs
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from config.csv (connection string, folder paths, verbose mode)
    /// 2. Initializes Serilog logging to daily rolling log files
    /// 3. Connects to the configured Outlook public folder (dirpathSupplement)
    /// 4. Processes each email item in the folder
    /// 5. Moves processed items to the "old" archive subfolder
    /// 6. Logs all activity using Serilog structured logging
    /// 
    /// EMAIL SENDERS HANDLED:
    /// - nciogaegrantsprod: Internal eGrants supplement notifications
    /// - caeranotifications: eRA supplement requested notifications
    /// - Authorized users (driskelleb, jonesni, etc.): Manual supplement uploads
    /// - PD/PI responses: Replies to supplement notifications
    /// 
    /// LOGGING:
    /// Uses Serilog for structured logging with:
    /// - Daily rolling log files: AddSuppProd-{date}.log
    /// - Console output with timestamps
    /// - Structured parameters: {FolderPath}, {ItemCount}, {Subject}, etc.
    /// - 31-day log retention, 10MB file size limit
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for config reading and Serilog logging)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - SQL Server database access to EIM database
    /// - config.csv file with: logDir, conStr, Verbose, dirpathSupplement, OutDir
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task to process supplement emails.
    /// </summary>
    internal class Program
    {
        private const string ApplicationName = "AddSuppProd";

        /// <summary>
        /// Main entry point for the AddSuppProd application.
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
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Production Processor");

                // Load configuration values from config.csv
                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                var conStr = CommonUtilities.GetConfigVal("conStr");
                var dirPath = CommonUtilities.GetConfigVal("dirpathSupplement");
                var outDir = CommonUtilities.GetConfigVal("OutDir");

                // Initialize Serilog logging - creates daily rolling log files
                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("Start Time: {StartTime}", startTimeStamp);
                CommonUtilities.Logger.Debug("Folder path: {FolderPath}", dirPath);
                CommonUtilities.Logger.Debug("Output directory: {OutDir}", outDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var itemsProcessed = processor.Process(con, dirPath, outDir, verbose, logDir);
                    CommonUtilities.Logger.Information("Task Completed - {ItemCount} items processed", itemsProcessed);
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
        /// Log file naming: AddSupp-Prod-Log-{yyyy-M-d}.txt (legacy format)
        /// Serilog file: AddSuppProd-{yyyy-MM-dd}.log
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
            var fileName = $"AddSupp-Prod-Log-{timeStamp:yyyy-M-d}.txt";
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

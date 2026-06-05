using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace LoadPfr
{
    /// <summary>
    /// Load PFR (Progress/Final Report) Application
    /// 
    /// PURPOSE:
    /// Processes XML metadata files containing Progress or Final Report information and loads
    /// the associated PDF documents into the eGrants document management system.
    /// 
    /// ORIGINAL SOURCE: Migrated from Load_PFR.vbs
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from appsettings.json (paths, connection string, logging)
    /// 2. Scans the source directory for XML metadata files
    /// 3. For each XML file:
    ///    - Parses metadata (applid, filename, date, file type, creator)
    ///    - Calls Create_PFR stored procedure to register the document
    ///    - Copies the PDF to the final destination with the assigned file number
    ///    - Moves both XML and PDF files to the backup directory
    /// 4. Logs all processing activities and errors
    /// 
    /// XML FILE FORMAT:
    /// The XML metadata files contain one or more document entries with these fields:
    /// - APPLID: Application ID in the eGrants system
    /// - FILENAME: Name of the PDF file to be loaded
    /// - DATE: Document date (when the report was created)
    /// - FILE_TYPE: File extension (typically "pdf")
    /// - UID: User ID of the person who created/uploaded the report
    /// 
    /// CONFIGURATION:
    /// Uses appsettings.json with environment-specific overrides:
    /// - appsettings.json: Base configuration
    /// - appsettings.Development.json: Development environment settings
    /// - appsettings.Production.json: Production environment settings
    /// 
    /// DEPENDENCIES:
    /// - SQL Server database with Create_PFR stored procedure
    /// - File system access to source, backup, and final destination directories
    /// - CommonUtilities project for logging and diagnostics
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task on a regular interval to process
    /// incoming PFR documents that have been placed in the source directory.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point for the LoadPfr application.
        /// Initializes configuration, processes PFR files, and handles top-level errors.
        /// </summary>
        /// <param name="args">Command line arguments (currently not used)</param>
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                // In debug builds, default to Development environment for local testing
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                // Load credentials from shared secrets file in the solution root (if present)
                // This file is not committed to source control and contains sensitive data
                var secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "secrets.local.csv");
                CommonUtilities.LoadLocalSecrets(secretsPath);

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("LoadPfr - Progress/Final Report Loader");

                // Build configuration from appsettings.json files
                // The configuration system loads base settings from appsettings.json,
                // then overlays environment-specific settings from appsettings.{Environment}.json
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                // Load configuration values from appsettings
                // Environment variables in the format %VARIABLE_NAME% are automatically expanded
                var verbose = configuration["AppSettings:Verbose"] ?? "n";
                var logDir = Environment.ExpandEnvironmentVariables(configuration["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\");
                var conStr = Environment.ExpandEnvironmentVariables(configuration["ConnectionStrings:EIM"]);
                var docSrcPath = Environment.ExpandEnvironmentVariables(configuration["PfrPaths:DocSrcPath"]);
                var bakDstPath = Environment.ExpandEnvironmentVariables(configuration["PfrPaths:BakDstPath"]);
                var finalDstPath = Environment.ExpandEnvironmentVariables(configuration["PfrPaths:FinalDstPath"]);

                // Set the global log directory for CommonUtilities logging
                CommonUtilities.LogDir = logDir;

                // Log the start of processing
                WriteLog(".........Task Started!........", null, startTimeStamp, logDir);
                CommonUtilities.ShowDiagnosticIfVerbose("LoadPfr task is starting", verbose);

                // Process all PFR files in the source directory
                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var filesProcessed = processor.Process(con, docSrcPath, bakDstPath, finalDstPath, verbose, logDir);
                    WriteLog($"******* Task Completed! ******* {filesProcessed} files processed.", null, DateTime.Now, logDir);
                }

                CommonUtilities.ShowDiagnosticIfVerbose("Done", verbose);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions to console and log file
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                try
                {
                    // Attempt to write the error to the log file
                    var logDir = Environment.ExpandEnvironmentVariables(@"C:\egrants\apps\log\");
                    WriteLog("Fatal Error in LoadPfr", $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}", DateTime.Now, logDir);
                }
                catch { }
            }
        }

        /// <summary>
        /// Writes a log entry to a daily log file.
        /// Creates the log directory if it doesn't exist.
        /// Log files are named PFR-Log-{date}.txt and contain timestamped entries.
        /// </summary>
        /// <param name="message">Main log message describing the event</param>
        /// <param name="errorInfo">Optional error details (exception message, stack trace, etc.)</param>
        /// <param name="timeStamp">Timestamp for the log entry</param>
        /// <param name="logDir">Directory where log files are stored</param>
        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            try
            {
                // Ensure log directory exists before writing
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // Create daily log file name (one file per day)
                var fileName = $"PFR-Log-{timeStamp:yyyy-M-d}.txt";
                
                // Format log entry with timestamp and optional error details
                var content = string.IsNullOrEmpty(errorInfo)
                    ? $"{timeStamp}  -\t{message}"
                    : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";
                
                // Append to the log file (creates file if it doesn't exist)
                File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // If logging fails, output to console as fallback
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}

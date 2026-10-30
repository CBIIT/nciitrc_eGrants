using System;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

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
    /// 1. Reads configuration from appsettings.json (folder path, log directory, verbose mode)
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
    /// CONFIGURATION:
    /// Uses shared appsettings.json with environment-specific overrides.
    /// Set DOTNET_ENVIRONMENT=Development for local dev.
    /// Defaults to Production if not set.
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for Serilog logging and AppConfig)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - appsettings.json with AppSettings, FolderPaths, and VoteCollection sections
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
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine($"{ApplicationName} - Vote Collection Processor");

                // Diagnostic: Check what environment variable is set
                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                Console.WriteLine($"DOTNET_ENVIRONMENT: {dotnetEnv ?? "(not set)"}");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                // If DOTNET_ENVIRONMENT=Development, this will also load appsettings.Development.json
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var dirPath = config["FolderPaths:dirpathVoteCollection"] ?? "";

                // Initialize Serilog logging
                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("DOTNET_ENVIRONMENT: {DotnetEnv}", dotnetEnv ?? "(not set)");
                CommonUtilities.Logger.Information("Resolved Environment: {Environment}", dotnetEnv ?? "Production");

                // Log which config files were loaded
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var envConfigFile = Path.Combine(baseDir, $"appsettings.{dotnetEnv ?? "Production"}.json");
                CommonUtilities.Logger.Information("Base config file: appsettings.json");
                CommonUtilities.Logger.Information("Environment config file: appsettings.{Environment}.json (exists: {Exists})", 
                    dotnetEnv ?? "Production", File.Exists(envConfigFile));

                CommonUtilities.Logger.Information("Start Time: {StartTime}", startTimeStamp);
                CommonUtilities.Logger.Debug("Folder path: {FolderPath}", dirPath);

                var processor = new Processor();
                var itemsProcessed = processor.Process(dirPath, verbose, config);

                CommonUtilities.Logger.Information("Task Completed - {ItemCount} votes processed", itemsProcessed);
                CommonUtilities.Logger.Information("=== {ApplicationName} Finished ===", ApplicationName);
            }
            catch (Exception ex)
            {
                // Log fatal errors - Logger may be null if initialization failed
                CommonUtilities.Logger?.Fatal(ex, "Fatal error in {ApplicationName}", ApplicationName);
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
            finally
            {
                // Ensure all logs are flushed before exit
                CommonUtilities.CloseLogging();
            }
        }
    }
}

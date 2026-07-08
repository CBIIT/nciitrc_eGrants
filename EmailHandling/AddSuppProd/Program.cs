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
    /// Processes administrative supplement emails from a designated Outlook public folder
    /// and moves them to an archive folder after processing.
    /// 
    /// ORIGINAL SOURCE: Migrated from add_supp_prod.vbs
    /// 
    /// CONFIGURATION:
    /// Uses shared appsettings.json with environment-specific overrides.
    /// Set DOTNET_ENVIRONMENT=Development for local dev.
    /// Defaults to Production if not set.
    /// 
    /// CREDENTIALS:
    /// Database credentials are resolved from environment variables:
    /// - DB_USER
    /// - DB_PASSWORD
    /// Set these as user-level or machine-level environment variables.
    /// </summary>
    internal class Program
    {
        private const string ApplicationName = "AddSuppProd";

        static void Main()
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Production Processor");

                // Diagnostic: Check what environment variable is set
                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                Console.WriteLine($"DOTNET_ENVIRONMENT: {dotnetEnv ?? "(not set)"}");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                // If DOTNET_ENVIRONMENT=Development, this will also load appsettings.Development.json
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "EIM");
                var dirPath = config["FolderPaths:dirpathSupplement"] ?? "";
                var outDir = config["AppSettings:OutDir"] ?? @"C:\egrants\watch\out\";
                var serverDstPath = config["AppSettings:ServerDstPath"];
                var adminEmails = config["AppSettings:AdminEmailRecipients"] ?? "leul.ayana@nih.gov;guillermo.choy-leon@nih.gov";

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
                CommonUtilities.Logger.Debug("Output directory: {OutDir}", outDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor(adminEmails);  // Pass admin emails to constructor
                    var itemsProcessed = processor.Process(con, dirPath, outDir, serverDstPath, verbose, logDir);
                    CommonUtilities.Logger.Information("Task Completed - {ItemCount} items processed", itemsProcessed);
                }

                CommonUtilities.Logger.Information("=== {ApplicationName} Finished ===", ApplicationName);
            }
            catch (Exception ex)
            {
                CommonUtilities.Logger?.Fatal(ex, "Fatal error in {ApplicationName}", ApplicationName);
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
            finally
            {
                CommonUtilities.CloseLogging();
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timestamp, string logDir)
        {
            CommonUtilities.Logger?.Information("{Message} {ErrorInfo}", message, errorInfo ?? "");
        }
    }
}

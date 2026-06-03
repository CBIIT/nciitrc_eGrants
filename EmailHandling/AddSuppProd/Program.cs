using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

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
    /// </summary>
    internal class Program
    {
        private const string ApplicationName = "AddSuppProd";

        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

                // Load credentials from shared secrets file at solution root
                var secretsPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "secrets.local.csv"));
                CommonUtilities.LoadLocalSecrets(secretsPath);
#endif
                var startTimeStamp = DateTime.Now;
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Production Processor");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "EIM");
                var dirPath = config["FolderPaths:dirpathSupplement"] ?? "";
                var outDir = config["AppSettings:OutDir"] ?? @"C:\egrants\watch\out\";

                // Initialize Serilog logging
                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("Environment: {Environment}",
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production");
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
                CommonUtilities.Logger?.Error(ex, "Fatal error in {ApplicationName}", ApplicationName);
                Console.WriteLine("Error: " + ex.Message);
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

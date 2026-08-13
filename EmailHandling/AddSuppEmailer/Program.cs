using System;
using System.IO;
using System.Data.SqlClient;
using CommonUtilties;

namespace AddSuppEmailer
{
    class Program
    {
        private const string ApplicationName = "AddSuppEmailer";

        static void Main()
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Emailer");

                // Diagnostic: Check what environment variable is set
                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                Console.WriteLine($"DOTNET_ENVIRONMENT: {dotnetEnv ?? "(not set)"}");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                // If DOTNET_ENVIRONMENT=Development, this will also load appsettings.Development.json
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var debugEmail = config["AppSettings:DebugEmail"];
                var additionalCc = config["AppSettings:AdditionalCcRecipients"] ?? "";
                var errorTo = config["AppSettings:ErrorToRecipients"] ?? "";
                var errorCc = config["AppSettings:ErrorCcRecipients"] ?? "";
                var conStr = AppConfig.GetConnectionString(config, "EIM");

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

                using (var con = new SqlConnection(conStr))
                {
                    CommonUtilities.Logger.Debug("Database connection string configured");

                    var processor = new Processor();
                    var mailsSent = processor.Process(con, verbose, logDir, debugEmail, additionalCc, errorTo, errorCc, config);

                    CommonUtilities.Logger.Information("Task Completed - {MailCount} emails sent", mailsSent);
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

        /// <summary>
        /// Legacy WriteLog method for backward compatibility.
        /// Writes to the daily log file and uses Serilog if initialized.
        /// </summary>
        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            CommonUtilities.WriteLog(8, message, errorInfo, timeStamp);
        }
    }
}


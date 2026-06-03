using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace AddSuppEmailer
{
    internal class Program
    {
        private const string ApplicationName = "AddSuppEmailer";

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
                Console.WriteLine($"{ApplicationName} - Administrative Supplement Emailer");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var debug = config["AppSettings:Debug"] ?? "n";
                var conStr = AppConfig.GetConnectionString(config, "EIM");

                CommonUtilities.InitializeLogging(ApplicationName, logDir);
                CommonUtilities.Logger.Information("=== {ApplicationName} Started ===", ApplicationName);
                CommonUtilities.Logger.Information("Environment: {Environment}",
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production");
                CommonUtilities.Logger.Information("Start Time: {StartTime}", startTimeStamp);

                using (var con = new SqlConnection(conStr))
                {
                    CommonUtilities.Logger.Debug("Database connection string configured");

                    var processor = new Processor();
                    var mailsSent = processor.Process(con, verbose, logDir, debug);

                    CommonUtilities.Logger.Information("Task Completed - {MailCount} emails sent", mailsSent);
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


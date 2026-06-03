using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace DocManEmail
{
    /// <summary>
    /// Document Management Email Processor - Migrated from DocMan_email_2008_Prod.vbs
    /// Processes emails from eContracts public folder for document management.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                // Load credentials from shared secrets file in the solution root (if present)
                var secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "secrets.local.csv");
                CommonUtilities.LoadLocalSecrets(secretsPath);

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("DocManEmail - Document Management Email Processor");

                // Build configuration from appsettings.json files
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                // Load configuration values from appsettings
                var verbose = configuration["AppSettings:Verbose"] ?? "n";
                var logDir = configuration["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = Environment.ExpandEnvironmentVariables(configuration["ConnectionStrings:DocMan"]);
                var dirPath = configuration["FolderPaths:dirpathDocMan"];
                var outDir = configuration["DocMan:OutDir"] ?? @"C:\egrants\watch\out\docman\";

                // Expand environment variables in config values
                logDir = Environment.ExpandEnvironmentVariables(logDir);

                CommonUtilities.LogDir = logDir;

                WriteLog("...........Task Started!...........", null, startTimeStamp, logDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var itemsProcessed = processor.Process(con, dirPath, outDir, verbose, logDir);
                    WriteLog($"******* Task Completed! ******* {itemsProcessed} Mail Items Processed", null, DateTime.Now, logDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = $"DocMan-Email-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                ? $"{timeStamp}: {message}"
                : $"{timeStamp}: {message}\n{errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
